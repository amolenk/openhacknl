using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GatewayAPI.Models;
using Microsoft.ServiceFabric.Services.Client;
using System.Fabric;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace GatewayAPI.Controllers
{
    [Route("api/[controller]")]
    public class MinecraftServersController : Controller
    {
        // GET api/minecraftservers
        [HttpGet]
        public async Task<IEnumerable<Tenant>> Get()
        {
            ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

            ResolvedServicePartition partition =
                 await resolver.ResolveAsync(new Uri("fabric:/MineCraft/MineCraft"), new ServicePartitionKey(), CancellationToken.None);

            List<Tenant> tenants = new List<Tenant>();
            foreach (var endpoint in partition.Endpoints)
            {
                JObject addressDeserialized = JObject.Parse(endpoint.Address);
                var x = addressDeserialized["Endpoints"][""].Value<string>();
                string address = new Uri(x).Host;
                tenants.Add(new Tenant
                {
                    Name = address,
                    Endpoints = new Endpoints { Minecraft = $"{address}:25565", RCON = $"{address}:25575" } 
                });
            }

            return tenants;
        }
    }
}
