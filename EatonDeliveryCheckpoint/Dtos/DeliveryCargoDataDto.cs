using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryCargoDataDto
    {
        public string delivery { get; set; }
        public string item { get; set; }
        public string material { get; set; }
        public string quantity { get; set; }
        public string realtime_product_quantity { get; set; }
        public string realtime_pallet_quantity { get; set; }
    }
}
