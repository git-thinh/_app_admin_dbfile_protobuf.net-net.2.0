using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace System
{

    /// <summary>
    /// This is a generic cache that is thread safe and uses a read/write lock access for performance.
    /// The cache itself is a string key based dictionary.
    /// </summary>
    /// <typeparam name=”T”>The type that we want to keep in the cache</typeparam>
    public class ThreadSafeCache<K, V>
    { 
        protected readonly ReaderWriterLockSlim cacheLock; // = new ReaderWriterLockSlim(); // mutex
        protected readonly Dictionary<K, V> innerCache; // = new Dictionary<string, T>(); // the cache itself

        public ThreadSafeCache()
        {
            cacheLock = new ReaderWriterLockSlim(); // mutex
            innerCache = new Dictionary<K, V>(); // the cache itself
        }

        // This method will replace existing item or add if it does not already exist
        public void Add(K key, V val)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache[key] = val;
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        // This is to get an item from the cache by its key, it will return null if not found.
        public V Get(K key)
        {
            cacheLock.EnterReadLock();
            try
            {
                V val;
                innerCache.TryGetValue(key, out val); // So there is no exception if not found
                return val;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        // This method is to remove an item from the cache by its key
        public void Delete(K key)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Remove(key);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        // This method empty the whole cache.
        public void Clear()
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Clear();
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        // This method validates that a key exists in the cache.
        public bool Exist(K key)
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache.ContainsKey(key);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        // This methods gets the number of items saved in the cache.
        public int Count
        {
            get
            {
                cacheLock.EnterReadLock();
                try
                {
                    return innerCache.Count;
                }
                finally
                {
                    cacheLock.ExitReadLock();
                }
            }
        }
    }
}