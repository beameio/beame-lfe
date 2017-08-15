using System;

namespace LFE.Cach.Provider
{
    public static class CacheHelper
    {
      
        public static T GetCachedResult<T>( this ICacheService cache, string key, DateTimeOffset expiry, Func<T> action, bool bypassCache = true ) where T : class {
            var result = cache.Get<T>( key );

            if (result != null && !bypassCache) return result;

            result = action();
            
            if ( result != null )
                cache.Add( key, result, expiry );

            return result;
        }

    }
}
