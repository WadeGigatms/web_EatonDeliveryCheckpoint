using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryCargoDataDto
    {
        public string material { get; set; }
        public int count { get; set; }
        public int realtime_product_count { get; set; }
        public int realtime_pallet_count { get; set; }
    }
}
