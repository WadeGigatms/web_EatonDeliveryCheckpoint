using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class CargoDataRecordContext
    {
        public int id { get; set; }
        public int f_cargo_data_info_id { get; set; }
        public int f_epc_raw_id { get; set; }
        public int f_epc_data_id { get; set; }
        public int state { get; set; }
    }
}
