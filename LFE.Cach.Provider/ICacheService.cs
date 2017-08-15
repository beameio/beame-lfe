using System;

namespace LFE.Cach.Provider
{
    public interface ICacheService
    {
        T Get<T>( string key ) where T : class;
        object Get( string key );
        void Remove( string key );
        void RemoveByPattern( string pattern );
        bool Contains( string key );
        void Add( string key, object item, DateTimeOffset expiration );
    }
}
