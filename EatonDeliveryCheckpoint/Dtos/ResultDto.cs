using EatonDeliveryCheckpoint.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class ResultDto: IResultDto
    {
        public bool Result { get; set; }
        public string Error { get; set; }
    }
}
