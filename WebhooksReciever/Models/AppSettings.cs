using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebhooksReceiver.Models
{  
    public class AppSettings
    {
        public string AzureDevOpsOrgUrl { get; set; }
        public string AzureDevOpsToken { get; set; }
    }
}
