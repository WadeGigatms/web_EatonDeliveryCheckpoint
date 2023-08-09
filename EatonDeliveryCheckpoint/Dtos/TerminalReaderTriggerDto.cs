using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class TerminalReaderTriggerDto
    {
        public int DODurationTime { get; set; }
        public int[] DOPortList { get; set; }
    }
}
