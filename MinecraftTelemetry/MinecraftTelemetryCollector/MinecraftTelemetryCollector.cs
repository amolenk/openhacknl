using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MinecraftServerRCON;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using MinecraftTelemetryCollector.Models;

namespace MinecraftTelemetryCollector
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class MinecraftTelemetryCollector : StatelessService
    {
        private TelemetryClient _telemetryClient;

        public MinecraftTelemetryCollector(StatelessServiceContext context)
            : base(context)
        {

        }

        public ITelemetryChannel TelemetryChannel { get; private set; }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            TelemetryConfiguration config = new TelemetryConfiguration("0ae11727-3b91-4eea-bbce-6cf19781a877");
            config.TelemetryChannel.DeveloperMode = true;
            _telemetryClient = new TelemetryClient(config);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach(Tenant tenant in GetTenants("http://ohnlt3sfv2.westeurope.cloudapp.azure.com:5000/api/minecraftservers"))
                {
                    ProbeServer(tenant.Name, new Uri(tenant.Endpoints.RCON), "cheesesteakjimmys");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        private IEnumerable<Tenant> GetTenants(string apiUri)
        {
            // TODO: call API

            string clusterUri = "ohnlt3sfv2.westeurope.cloudapp.azure.com";

            List<Tenant> tenants = new List<Tenant>()
            {
                new Tenant { Name = "Server1", Endpoints = new Endpoints { Minecraft = $"{clusterUri}:9000", RCON = $"{clusterUri}:9001" }  },
                new Tenant { Name = "Server2", Endpoints = new Endpoints { Minecraft = $"{clusterUri}:9002", RCON = $"{clusterUri}:9003" }  },
            };

            return tenants;
        }

        private void ProbeServer(string tenantName, Uri uri, string password)
        {
            using (var rcon = RCONClient.INSTANCE)
            {
                try
                {
                    // get telemetry from Minecraft server
                    rcon.setupStream(uri.Host, uri.Port, "cheesesteakjimmys");
                    var answer = rcon.sendMessage(RCONMessageType.Command, "list");
                    if (!string.IsNullOrEmpty(answer))
                    {
                        Regex regex = new Regex(@"[a-z]*(?<TotalPlayers>\d+)\/(?<TotalCapacity>\d+).*:(?<PlayerList>.*)");
                        var match = regex.Match(answer);
                        var totalPlayers = match.Groups[1];
                        var maxCapacity = match.Groups[2];
                        var population = match.Groups[3];

                        string logmessage = $"Server: {tenantName}, #Players: {totalPlayers}, Max. Capacity: {maxCapacity}, Population: {population}";
                        ServiceEventSource.Current.ServiceMessage(this.Context, logmessage);

                        // publish telemetry
                        _telemetryClient.TrackTrace(logmessage);
                        _telemetryClient.TrackMetric(new MetricTelemetry($"{tenantName}PlayerCount", Convert.ToDouble(totalPlayers.Value)));
                        _telemetryClient.TrackMetric(new MetricTelemetry($"{tenantName}MaxPlayerCapacity", Convert.ToDouble(maxCapacity.Value)));
                    }
                    else
                    {
                        string message = "No result from server.";
                        ServiceEventSource.Current.ServiceMessage(this.Context, message);
                        _telemetryClient.TrackTrace(message);
                    }
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, ex.ToString());
                    _telemetryClient.TrackException(ex);
                }
            }
        }
    }
}
