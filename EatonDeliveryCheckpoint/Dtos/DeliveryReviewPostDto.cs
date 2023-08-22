using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryReviewPostDto
    {
        [JsonProperty(Required = Required.Always)]
        public string No { get; set; }
    }
}
