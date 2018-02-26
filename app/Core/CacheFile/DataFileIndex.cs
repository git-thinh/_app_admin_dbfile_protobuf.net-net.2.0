using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Linq;

namespace app.Core
{
    [Serializable]
    public class KeyIndex
    {
        public string Field { set; get; }
        public object Value { set; get; }

        public int getCacheCode()
        {
            return string.Format("{0}{1}", Field, Value).GetHashCode();
        }

        public KeyIndex(string field, object value)
        {
            Field = field;
            Value = value;
        }

        public class EqualityComparer : IEqualityComparer<KeyIndex>
        {
            public bool Equals(KeyIndex x, KeyIndex y)
            {
                return x.getCacheCode() == y.getCacheCode();
            }

            public int GetHashCode(KeyIndex x)
            {
                int v = x.getCacheCode();
                return v;
            }
        }
    }

    /// <summary>
    /// This is a generic cache that is thread safe and uses a read/write lock access for performance.
    /// The cache itself is a string key based dictionary.
    /// </summary>
    /// <typeparam name=”T”>The type that we want to keep in the cache</typeparam>
    public class DataFileIndex
    {
        private readonly ReaderWriterLockSlim cacheLock;
        private readonly Dictionary<KeyIndex, List<int>> cacheStore;

        public DataFileIndex()
        {
            cacheLock = new ReaderWriterLockSlim(); // mutex
            cacheStore = new Dictionary<KeyIndex, List<int>>(new KeyIndex.EqualityComparer()); // the cache itself 
        }

    }
}