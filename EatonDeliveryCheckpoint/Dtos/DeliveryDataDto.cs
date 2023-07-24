using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryDataDto
    {
        public string material { get; set; }
        public string quantity { get; set; }
        public bool valid { get; set; }
    }
}
