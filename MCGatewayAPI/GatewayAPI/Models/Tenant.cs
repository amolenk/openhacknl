using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatewayAPI.Models
{
    public class Tenant
    {
        public string Name { get; set; }
        public Endpoints Endpoints { get; set; }
    }
}
