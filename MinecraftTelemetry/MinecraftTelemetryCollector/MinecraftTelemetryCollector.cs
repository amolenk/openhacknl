﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MinecraftServerRCON;
using System.Text.RegularExpressions;

namespace MinecraftTelemetryCollector
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class MinecraftTelemetryCollector : StatelessService
    {
        public MinecraftTelemetryCollector(StatelessServiceContext context)
            : base(context)
        { }

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
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using (var rcon = RCONClient.INSTANCE)
                    {
                        rcon.setupStream("ohnlt3vm.westeurope.cloudapp.azure.com", password: "cheesesteakjimmys");
                        var answer = rcon.sendMessage(RCONMessageType.Command, "list");
                        if (!string.IsNullOrEmpty(answer))
                        {
                            Regex regex = new Regex(@"[a-z]*(?<TotalPlayers>\d+)\/(?<TotalCapacity>\d+).*:(?<PlayerList>.*)");
                            var match = regex.Match(answer);
                            var totalPlayers = match.Groups[1];
                            var maxCapacity = match.Groups[2];
                            var population = match.Groups[3];

                            string logmessage = $"#Players: {totalPlayers}, Max. Capacity: {maxCapacity}, Population: {population}";
                            ServiceEventSource.Current.ServiceMessage(this.Context, logmessage);

                            // TODO publish telemetry
                        }
                        else
                        {
                            ServiceEventSource.Current.ServiceMessage(this.Context, "No result from server.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, ex.ToString());
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}
