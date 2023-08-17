using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class CargoDataInfoContext
    {
        public int id { get; set; }
        public int f_delivery_cargo_id { get; set; }
        public string material { get; set; }
        public int count { get; set; }
        public int realtime_product_count { get; set; }
        public int realtime_pallet_count { get; set; }
        public int alert { get; set; }
    }
}
