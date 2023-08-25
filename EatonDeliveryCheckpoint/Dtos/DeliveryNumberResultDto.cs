using EatonDeliveryCheckpoint.Database;
using EatonDeliveryCheckpoint.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryNumberResultDto : ResultDto, IResultDto
    {
        public List<DeliveryNumberDto> DeliveryNumberDtos { get; set; }
    }
}
