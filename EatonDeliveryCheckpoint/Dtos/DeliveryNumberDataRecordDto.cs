using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryNumberDataRecordDto
    {
        public string epc { get; set; }
        public string reader_id { get; set; }
        public string timestamp { get; set; }
        public string wo { get; set; }
        public string qty { get; set; }
        public string pn { get; set; }
        public string line { get; set; }
        public string pallet_id { get; set; }
    }
}
