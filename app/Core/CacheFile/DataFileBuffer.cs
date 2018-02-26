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
    

    /// <summary>
    /// This is a generic cache that is thread safe and uses a read/write lock access for performance.
    /// The cache itself is a string key based dictionary.
    /// </summary>
    /// <typeparam name=”T”>The type that we want to keep in the cache</typeparam>
    public class DataFileBuffer
    {
        private readonly ReaderWriterLockSlim cacheLock;
        private Dictionary<int, byte[]> cacheStore;
        private readonly app.Core.CacheFile.Serializer serializer;

        public DataFileBuffer()
        {
            cacheLock = new ReaderWriterLockSlim(); // mutex
            cacheStore = new Dictionary<int, byte[]>(); // the cache itself
            serializer = new app.Core.CacheFile.Serializer();
        }

        public void Initialize(IList list, Type typeItem)
        {
            using (cacheLock.WriteLock())
            {
                cacheStore.Clear();
                for (int k = 0; k < list.Count; k++)
                    cacheStore.Add(k, Serialize(list[k], typeItem));
            }
        }

        public void Truncate()
        {
            using (cacheLock.WriteLock())
            {
                cacheStore.Clear();
            }
        }

        public void RegistryType(Type type)
        {
            serializer.Initialize(type);
        }

        public byte[] Serialize(object obj, Type type)
        {
            byte[] buf = null;
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(obj, type, ms);
                buf = ms.ToArray();
            }
            return buf;
        }

        public object[] DeserializeAndClone(byte[] buf, Type type)
        {
            object[] val = new object[2];
            using (var ms = new MemoryStream(buf))
            {
                val[0] = serializer.Deserialize(type, ms);
                ms.Seek(0, SeekOrigin.Begin);
                val[1] = serializer.Deserialize(type, ms);
            }
            return val;
        }

        public object CloneByBinaryFormatter(object item)
        {
            object val = null;
            using (var ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, item);
                ms.Seek(0, SeekOrigin.Begin);
                val = formatter.Deserialize(ms);
            }
            return val;
        }

        public object Deserialize(byte[] buf, Type type)
        {
            object val = null;
            using (var ms = new MemoryStream(buf))
                val = serializer.Deserialize(type, ms);
            return val;
        }

        public IList Clone(IList list)
        {
            Type type = list.GetType();
            IList rs = null;
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(list, type, ms);
                ms.Position = 0;
                rs = (IList)serializer.Deserialize(type, ms);
            }
            return rs;
        }

        public byte[] this[int index]
        {
            get { return Get(index); }
            set { AddOrUpdate(index, value); }
        }

        #region [ === ADD === ]

        // This method will replace existing item or add if it does not already exist
        public void AddOrUpdate(int index, byte[] buf)
        {
            using (cacheLock.WriteLock())
            {
                if (cacheStore.ContainsKey(index))
                    cacheStore[index] = buf;
                else
                    cacheStore.Add(index, buf);
            }
        }

        public void AddOrUpdate(IDictionary<int, byte[]> data)
        {
            if (data == null) return;
            using (cacheLock.WriteLock())
            {
                foreach (KeyValuePair<int, byte[]> kv in data)
                    if (cacheStore.ContainsKey(kv.Key))
                        cacheStore[kv.Key] = kv.Value;
                    else
                        cacheStore.Add(kv.Key, kv.Value);
            }
        }

        public void AddOrUpdate(IndexDynamic it, Type type)
        {
            byte[] buf;
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(it, type, ms);
                buf = ms.ToArray();
            }
            using (cacheLock.WriteLock())
            {
                if (cacheStore.ContainsKey(it.Index))
                    cacheStore[it.Index] = buf;
                else
                    cacheStore.Add(it.Index, buf);
            }
        }

        public void AddOrUpdate(IList list, Type type)
        {
            int index = Count;
            byte[] buf;
            using (cacheLock.WriteLock())
            {
                using (var ms = new MemoryStream())
                {
                    for (int k = 0; k < list.Count; k++)
                    {
                        index++;
                        serializer.Serialize(list[k], type, ms);
                        buf = ms.ToArray();
                        ms.Seek(0, SeekOrigin.Begin);

                        if (cacheStore.ContainsKey(index))
                            cacheStore[index] = buf;
                        else
                            cacheStore.Add(index, buf);
                    }
                }
            }
        }

        #endregion

        // This is to get an item from the cache by its key, it will return null if not found.
        public byte[] GetAll()
        {
            List<byte> li = new List<byte>();
            using (cacheLock.ReadLock())
            {
                for (int k = 0; k < Count; k++)
                {
                    byte[] buf;
                    if (cacheStore.TryGetValue(k, out buf) && buf != null)
                        li.AddRange(buf);
                }
            }
            return li.ToArray();
        }

        // This is to get an item from the cache by its key, it will return null if not found.
        public byte[] Get(int index)
        {
            byte[] val = null;
            using (cacheLock.ReadLock())
                cacheStore.TryGetValue(index, out val); // So there is no exception if not found
            return val;
        }

        // This method is to remove an item from the cache by its key
        public void Remove(int index)
        {
            using (cacheLock.WriteLock())
                if (cacheStore.ContainsKey(index))
                    cacheStore.Remove(index);
        }

        // This method is to remove an item from the cache by its key
        public byte[] Remove(int[] index)
        {
            List<byte> ls = new List<byte>();
            using (cacheLock.WriteLock())
            {
                foreach (int i in index)
                    if (cacheStore.ContainsKey(i))
                        cacheStore.Remove(i);
                Dictionary<int, byte[]> dic = new Dictionary<int, byte[]>() { };
                int k = 0;
                foreach (var kv in cacheStore)
                {
                    dic.Add(k, kv.Value);
                    ls.AddRange(kv.Value);
                    k++;
                }
                cacheStore = dic;
            }
            return ls.ToArray();
        }

        // This method empty the whole cache.
        public void Clear()
        {
            using (cacheLock.WriteLock())
                cacheStore.Clear();
        }

        // This method validates that a key exists in the cache.
        public bool Exist(int index)
        {
            using (cacheLock.ReadLock())
                return cacheStore.ContainsKey(index);
        }

        // This methods gets the number of items saved in the cache.
        public int Count
        {
            get
            {
                using (cacheLock.ReadLock())
                    return cacheStore.Count;
            }
        }
    }
}