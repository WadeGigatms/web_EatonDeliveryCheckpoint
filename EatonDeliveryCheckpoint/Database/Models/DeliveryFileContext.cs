using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class DeliveryFileContext
    {
        public int id { get; set; }
        public string name { get; set; }
        public string upload_timestamp { get; set; }
        public string json { get; set; }
    }
}
