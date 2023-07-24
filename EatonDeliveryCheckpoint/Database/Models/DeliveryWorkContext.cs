using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class DeliveryWorkContext
    {
        public int id { get; set; }
        public int f_delivery_file_id { get; set; }
        public string f_delivery_file_data_ids { get; set; }
        public string no { get; set; }
        public string start_timestamp { get; set; }
        public string end_timestamp { get; set; }
        public string duration { get; set; }
        public int valid_pallet_quantity { get; set; }
        public int invalid_pallet_quantity { get; set; }
        public int state { get; set; }
    }
}
