using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;

namespace Kartverket.Metadatakatalog.Service
{
    public class MemoryCacher
    {
        static readonly object addLock = new object();

        public object GetValue(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Get(key);
        }

        public bool Add(string key, object value, DateTimeOffset absExpiration)
        {
            lock (addLock)
            {
                CacheItemPolicy policy =
                new CacheItemPolicy { AbsoluteExpiration = absExpiration, Priority = CacheItemPriority.NotRemovable };
                MemoryCache memoryCache = MemoryCache.Default;
                memoryCache.Remove(key);
                memoryCache.Set(key, value, policy);
            }

            return true;
        }

        public void Delete(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(key))
            {
                memoryCache.Remove(key);
            }
        }

        public void DeleteAll()
        {
            List<string> cacheKeys = MemoryCache.Default.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                MemoryCache.Default.Remove(cacheKey);
            }
        }

    }
}