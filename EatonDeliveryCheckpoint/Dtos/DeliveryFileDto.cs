using EatonDeliveryCheckpoint.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryFileDto
    {
        public string file_name { get; set; }
        public string upload_timestamp { get; set; }
        public string no { get; set; }
        public string start_timestamp { get; set; }
        public string end_timestamp { get; set; }
        public string duration { get; set; }
        public int valid_pallet_quantity { get; set; }
        public int invalid_pallet_quantity { get; set; }

        public List<DeliveryDataDto> datas { get; set; }
    }
}
