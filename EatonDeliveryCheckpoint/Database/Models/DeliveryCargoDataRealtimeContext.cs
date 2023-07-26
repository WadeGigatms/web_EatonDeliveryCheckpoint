using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class DeliveryCargoDataRealtimeContext
    {
        public int id { get; set; }
        public int f_delivery_cargo_data_id { get; set; }
        public int product_quantity { get; set; }
        public int pallet_quantity { get; set; }
    }
}
