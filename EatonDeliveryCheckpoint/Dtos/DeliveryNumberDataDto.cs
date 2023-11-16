using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryNumberDataDto
    {
        public string delivery { get; set; }
        public string material { get; set; }
        public int product_count { get; set; }
        public int pallet_count { get; set; }
        public int realtime_product_count { get; set; }
        public int realtime_pallet_count { get; set; }
        public int alert { get; set; }
        public List<DeliveryNumberDataRecordDto> records { get; set; }
    }
}
