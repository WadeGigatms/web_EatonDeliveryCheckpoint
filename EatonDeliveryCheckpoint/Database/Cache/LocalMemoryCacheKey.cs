using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public static class LocalMemoryCacheKey
    {
        public static readonly string CacheDidChange = "CacheDidChange";
        public static readonly string DeliveryCargoDtos = "DeliveryCargoDtos";
        public static readonly string DeliveryingCargoDto = "DeliveryingCargoDto";
        public static readonly string InvalidDeliveryCargoDataDto = "InvalidDeliveryingCargoDataDto";
    }
}
