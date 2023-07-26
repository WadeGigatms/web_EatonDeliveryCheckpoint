using EatonDeliveryCheckpoint.Database;
using EatonDeliveryCheckpoint.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class CargoResultDto : ResultDto, IResultDto
    {
        public List<DeliveryCargoDto> CargoNos { get; set; }
    }
}
