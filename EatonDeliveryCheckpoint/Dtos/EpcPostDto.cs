using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class EpcPostDto
    {
        [JsonProperty(Required = Required.Always)]
        public string Epc { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ReaderId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TransTime { get; set; }
    }
}
