using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class CargoDataContext
    {
        public int id { get; set; }
        public int f_delivery_number_id { get; set; }
        public string delivery { get; set; }
        public string item { get; set; }
        public string material { get; set; }
        public int quantity { get; set; }
    }
}
