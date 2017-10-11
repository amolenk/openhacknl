using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftTelemetryCollector.Models
{
    public class Tenant
    {
        public string Name { get; set; }
        public Endpoints Endpoints { get; set; }
    }
}
