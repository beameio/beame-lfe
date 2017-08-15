using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace LFE.Cach.Provider
{
    public class InMemoryCacheService : ICacheService
    {
        private static readonly MemoryCache _cache = new MemoryCache( "LFE.Caching" );

        public T Get<T>( string key ) where T : class {
            var item = _cache.Get( key ) as T;
            return item;
        }

        public object Get( string key ) {
            return _cache.Get( key );
        }

        public void Remove( string key ) {
            _cache.Remove( key );
        }

        public void RemoveByPattern( string pattern ) {
            var regex = new Regex( pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase );
            var keysToRemove = new List<String>();

            foreach ( var item in _cache )
                if ( regex.IsMatch( item.Key ) )
                    keysToRemove.Add( item.Key );

            foreach ( var key in keysToRemove ) {
                Remove( key );
            }
        }

        public bool Contains( string key ) {
            return _cache.Contains( key );
        }

        public void Add( string key, object item, DateTimeOffset expiration ) {
            var cachePolicy = new CacheItemPolicy {
                AbsoluteExpiration = expiration
            };

            _cache.Add( key, item, cachePolicy );
        }
    }
}
