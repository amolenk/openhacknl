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
        public IEnumerable<Tenant> Get()
        {
            string clusterUri = "ohnlt3sfv2.westeurope.cloudapp.azure.com";

            List<Tenant> tenants = new List<Tenant>()
            {
                new Tenant { Name = "Server1", Endpoints = new Endpoints { Minecraft = $"{clusterUri}:9000", RCON = $"{clusterUri}:9001" }  },
                new Tenant { Name = "Server2", Endpoints = new Endpoints { Minecraft = $"{clusterUri}:9002", RCON = $"{clusterUri}:9003" }  },
            };

            return tenants;
        }
    }
}
