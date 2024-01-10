using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class DeliveryNumberContext
    {
        public int id { get; set; }
        public int f_delivery_file_id { get; set; }
        public string no { get; set; }
        public int material_quantity { get; set; }
        public int product_quantity { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public int pallet_quantity { get; set; }
        public int miss_pallet_quantity { get; set; }
        public int valid_pallet_quantity { get; set; }
        public int invalid_pallet_quantity { get; set; }
        public string state { get; set; }
    }
}
