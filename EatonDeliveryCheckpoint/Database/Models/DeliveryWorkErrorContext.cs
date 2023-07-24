using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class DeliveryWorkErrorContext
    {
        public int id { get; set; }
        public int f_delivery_work_id { get; set; }
        public int error_code { get; set; }
        public string timestamp { get; set; }
        public int f_epc_raw_id { get; set; }
        public int f_epc_data_id { get; set; }
    }
}
