using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class DeliveryCargoContext
    {
        public int id { get; set; }
        public int f_delivery_file_id { get; set; }
        public string no { get; set; }
        public int material_quantity { get; set; }
        public int product_quantity { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public int valid_pallet_quantity { get; set; }
        public int invalid_pallet_quantity { get; set; }
        public int state { get; set; } // -1: ready to delivery, 0: deliverying, 1: did delivery
    }
}
