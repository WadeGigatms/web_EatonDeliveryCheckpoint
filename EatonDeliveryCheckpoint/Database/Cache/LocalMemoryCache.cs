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
        public List<DeliveryNumberDto> ReadDeliveryNumberDtos()
        {
            if (!_cache.TryGetValue(LocalMemoryCacheKey.DeliveryNumberDtos, out List<DeliveryNumberDto> cacheEntry))
            {
                cacheEntry = null;
            }
            return cacheEntry;
        }

        public void SaveDeliveryNumberDtos(List<DeliveryNumberDto> dtos)
        {
            _cache.Set(LocalMemoryCacheKey.DeliveryNumberDtos, dtos, TimeSpan.FromHours(1));
        }

        public void RemoveDeliveryNumberDtos()
        {
            _cache.Remove(LocalMemoryCacheKey.DeliveryNumberDtos);

            SaveCacheDidChange(true);
        }

        // DeliveryingCargoDto: Cache the deliverying context sent to client

        public DeliveryNumberDto ReadDeliveryingNumberDto()
        {
            if (!_cache.TryGetValue(LocalMemoryCacheKey.DeliveryingNumberDto, out DeliveryNumberDto cacheEntry))
            {
                cacheEntry = null;
            }
            return cacheEntry;
        }

        public void SaveDeliveryingNumberDto(DeliveryNumberDto dto)
        {
            _cache.Set(LocalMemoryCacheKey.DeliveryingNumberDto, dto, TimeSpan.FromHours(1));
        }

        public void RemoveDeliveryingNumberDto()
        {
            _cache.Remove(LocalMemoryCacheKey.DeliveryingNumberDto);

            SaveCacheDidChange(true);
        }
    }
}
