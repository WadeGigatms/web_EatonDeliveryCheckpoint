using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class TerminalReaderTriggerDto
    {
        public string DODurationTime { get; set; }
        public string[] DOPortList { get; set; }
    }
}
