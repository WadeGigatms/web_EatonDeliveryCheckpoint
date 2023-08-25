using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public static class LocalMemoryCacheKey
    {
        public static readonly string CacheDidChange = "CacheDidChange";
        public static readonly string DeliveryNumberDtos = "DeliveryNumberDtos";
        public static readonly string DeliveryingNumberDto = "DeliveryingNumberDto";
        public static readonly string InvalidDeliveryNumberDataDto = "InvalidDeliveryNumberDataDto";
    }
}
