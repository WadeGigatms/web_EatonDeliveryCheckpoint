using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class DeliveryFileDataContext
    {
        public int id { get; set; }
        public int f_delivery_file_id { get; set; }
        public string delivery { get; set; }
        public string item { get; set; }
        public string material { get; set; }
        public string quantity { get; set; }
        public string no { get; set; }
    }
}
