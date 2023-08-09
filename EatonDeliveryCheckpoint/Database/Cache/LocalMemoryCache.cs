using EatonDeliveryCheckpoint.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database
{
    public class LocalMemoryCache
    {
        private readonly IMemoryCache _cache;

        public LocalMemoryCache(IMemoryCache cache)
        {
            _cache = cache;
        }


        // CargoNoCount: detect new dn uploaded
        public int ReadCargoNoCount()
        {
            if (!_cache.TryGetValue(LocalMemoryCacheKey.CargoNoCount, out int cacheEntry))
            {
                cacheEntry = -1;
            }
            return cacheEntry;
        }

        public void SaveCargoNoCount(int count)
        {
            _cache.Set(LocalMemoryCacheKey.CargoNoCount, count, TimeSpan.FromHours(1));
        }

        // DeliveryCargoDtos: Cache the context sent to client
        public List<DeliveryCargoDto> ReadDeliveryCargoDtos()
        {
            if (!_cache.TryGetValue(LocalMemoryCacheKey.DeliveryCargoDtos, out List<DeliveryCargoDto> cacheEntry))
            {
                cacheEntry = null;
            }
            return cacheEntry;
        }

        public void SaveDeliveryCargoDtos(List<DeliveryCargoDto> dtos)
        {
            _cache.Set(LocalMemoryCacheKey.DeliveryCargoDtos, dtos, TimeSpan.FromHours(1));
        }

        // DeliveryingCargoDto: Cache the deliverying context sent to client

        public DeliveryCargoDto ReadDeliveryingCargoDto()
        {
            if (!_cache.TryGetValue(LocalMemoryCacheKey.DeliveryingCargoDto, out DeliveryCargoDto cacheEntry))
            {
                cacheEntry = null;
            }
            return cacheEntry;
        }

        public void SaveDeliveryingCargoDto(DeliveryCargoDto dto)
        {
            _cache.Set(LocalMemoryCacheKey.DeliveryingCargoDto, dto, TimeSpan.FromHours(1));
        }
    }
}
