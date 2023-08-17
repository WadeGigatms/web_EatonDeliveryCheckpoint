using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class EpcDataContext
    {
        public int id { get; set; }
        public string f_epc_raw_ids { get; set; }
        public string wo { get; set; }
        public string qty { get; set; }
        public string pn { get; set; }
        public string pallet_id { get; set; }
    }
}
