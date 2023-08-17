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


        // CacheDidChange: detect new dn uploaded or updated
        public bool ReadCacheDidChange()
        {
            if (!_cache.TryGetValue(LocalMemoryCacheKey.CacheDidChange, out bool cacheEntry))
            {
                cacheEntry = true;
            }
            return cacheEntry;
        }

        public void SaveCacheDidChange(bool changed)
        {
            _cache.Set(LocalMemoryCacheKey.CacheDidChange, changed, TimeSpan.FromHours(1));
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

        public void DeleteDeliveryingCargoDto()
        {
            _cache.Remove(LocalMemoryCacheKey.DeliveryingCargoDto);

            SaveCacheDidChange(true);
        }
    }
}
