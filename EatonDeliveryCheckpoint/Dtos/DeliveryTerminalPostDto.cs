using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryTerminalPostDto
    {
        public int epc_raw_id { get; set; }
        public int epc_data_id { get; set; }
        public string wo { get; set; }
        public int qty { get; set; }
        public string pn { get; set; }
        public string line { get; set; }
        public string pallet_id { get; set; }
    }
}
