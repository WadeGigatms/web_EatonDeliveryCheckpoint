using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class DeliveryCargoRecordContext
    {
        public int id { get; set; }
        public int f_delivery_cargo_data_realtime_id { get; set; }
        public int f_epc_raw_id { get; set; }
        public int f_epc_data_id { get; set; }
        public int state { get; set; }
    }
}
