using EatonDeliveryCheckpoint.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryCargoDto
    {
        public string file_name { get; set; }
        public string upload_timestamp { get; set; }
        public string no { get; set; }
        public int material_quantity { get; set; }
        public int product_quantity { get; set; }
        public string date { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string duration { get; set; }
        public int valid_pallet_quantity { get; set; }
        public int invalid_pallet_quantity { get; set; }
        public int pallet_rate { get; set; }
        public int state { get; set; }
        public List<DeliveryCargoDataDto> datas { get; set; }

    }
}
