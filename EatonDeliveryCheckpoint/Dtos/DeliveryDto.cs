using EatonDeliveryCheckpoint.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryDto
    {
        public DeliveryFileContext excelFile { get; set; }
        public DeliveryWorkContext operation { get; set; }
        public List<DeliveryFileDataContext> data { get; set; }
    }
}
