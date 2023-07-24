using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class DeliveryUploadPostDto
    {
        [JsonProperty(Required = Required.Always)]
        public string FileName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public List<FileDto> FileData { get; set; }
    }
}
