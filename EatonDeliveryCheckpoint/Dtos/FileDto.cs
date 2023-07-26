﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Dtos
{
    public class FileDto
    {
        [JsonProperty(Required = Required.Always)]
        public string Delivery { get; set; }
        //[JsonProperty(Required = Required.Always)]
        public string Item { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Material { get; set; }
        [JsonProperty(Required = Required.Always, PropertyName = " Delivery quantity ")]
        public string Quantity { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string No { get; set; }
    }
}
