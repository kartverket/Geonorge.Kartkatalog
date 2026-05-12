using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kartverket.Metadatakatalog.Service
{
    public class MemoryCacher
    {
        private readonly IMemoryCache _memoryCache;
        static readonly object addLock = new object();

        public MemoryCacher(IMemoryCache memoryCache = null)
        {
            // For backwards compatibility, create a default instance if not injected
            _memoryCache = memoryCache ?? new MemoryCache(new MemoryCacheOptions());
        }

        public object GetValue(string key)
        {
            _memoryCache.TryGetValue(key, out object value);
            return value;
        }

        public bool Add(string key, object value, DateTimeOffset absExpiration)
        {
            lock (addLock)
            {
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = absExpiration,
                    Priority = CacheItemPriority.High
                };
                _memoryCache.Remove(key);
                _memoryCache.Set(key, value, options);
            }

            return true;
        }

        public void Delete(string key)
        {
            _memoryCache.Remove(key);
        }

        public void DeleteAll()
        {
            // Note: IMemoryCache doesn't expose all keys, so we need to track them manually
            // For now, dispose and recreate the cache (this is a breaking change but necessary for ASP.NET Core)
            if (_memoryCache is MemoryCache memCache)
            {
                // Use reflection to clear all entries (not ideal but necessary for migration)
                var field = typeof(MemoryCache).GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field?.GetValue(memCache) is IDictionary<object, object> cache)
                {
                    cache.Clear();
                }
            }
        }
    }
}