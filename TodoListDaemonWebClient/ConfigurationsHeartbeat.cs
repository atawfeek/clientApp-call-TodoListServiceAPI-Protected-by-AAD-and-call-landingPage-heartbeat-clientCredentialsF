using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListDaemonWebClient
{
    public class ConfigurationsHeartbeat
    {
        public string Instance { get; set; }
        public string Domain { get; set; }
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string AppKey { get; set; }
        public string ResourceId { get; set; }
        public string ApiUrl { get; set; }
    }
}
