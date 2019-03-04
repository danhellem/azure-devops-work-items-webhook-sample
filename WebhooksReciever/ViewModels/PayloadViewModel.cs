using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebhooksReceiver.ViewModels
{
    public class PayloadViewModel : BaseViewModel
    {
        public int id { get; set;}
        public string eventType { get; set; }
        public string createdBy {  get; set; }
        public int rev { get; set; }
        public string teamProject { get; set; }
        public string url { get; set; }
        public string assignedTo { get; set; }
    }
}
