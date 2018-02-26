using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading;

namespace app.Core
{

    /// <summary>
    /// This is a generic cache that is thread safe and uses a read/write lock access for performance.
    /// The cache itself is a string key based dictionary.
    /// </summary>
    /// <typeparam name=”T”>The type that we want to keep in the cache</typeparam>
    public class DataFileSearch
    {
        private readonly ReaderWriterLockSlim cacheLock;
        private readonly Dictionary<SearchRequest, SearchResult> cacheStore;

        public DataFileSearch()
        {
            cacheLock = new ReaderWriterLockSlim(); // mutex
            cacheStore = new Dictionary<SearchRequest, SearchResult>(new SearchRequest.EqualityComparer()); // the cache itself
        }


        public void Initialize(IList list)
        { 
        }

        public void Truncate()
        {
            using (cacheLock.WriteLock())
            {
                cacheStore.Clear();
            }
        }




        public SearchResult SearchGetIDs_(IList list, SearchRequest search)
        {
            bool ok = false;
            int[] ids = new int[] { };
            string text = string.Empty;
            try
            {
                IQueryable lo = null;
                    if (search.Values == null)
                        lo = list.AsQueryable().Where(search.Predicate);
                    else
                        lo = list.AsQueryable().Where(search.Predicate, search.Values); ;

                if (lo != null)
                {
                    List<int> li = new List<int>();
                    foreach (var o in lo)
                        li.Add(list.IndexOf(o));
                    ids = li.ToArray();
                }

                ok = true;
            }
            catch (Exception ex) { text = ex.Message; }

            SearchResult rs = new SearchResult(ok, ids.Length, ids, text); 
            return rs;
        }

        public void BuildCache(IList list, ItemEditType type, int[] ids = null)
        {
            using (cacheLock.WriteLock())
            {
                SearchRequest[] keys = cacheStore.Keys.ToArray();
                foreach (var ki in keys)
                {
                    SearchResult sr = null;
                    switch (type)
                    {
                        case ItemEditType.ADD_NEW_ITEM:
                            sr = SearchGetIDs_(list, ki);
                            break;
                        case ItemEditType.REMOVE_ITEM:
                            sr = cacheStore[ki];
                            if (sr.IDs != null && sr.IDs.Length > 0 && ids != null && ids.Length > 0)
                            {
                                int[] val = sr.IDs.Where(x => !ids.Any(o => o == x)).ToArray();
                                sr.IDs = val;
                                sr.Total = val.Length;
                            }
                            break;
                    }
                    cacheStore[ki] = sr;
                }
            }
        }






























        // This method will replace existing item or add if it does not already exist
        public void Add(SearchRequest key, SearchResult val)
        {
            using (cacheLock.WriteLock())
                if (cacheStore.ContainsKey(key))
                    cacheStore[key] = val;
                else
                    cacheStore.Add(key, val);
        }

        // This is to get an item from the cache by its key, it will return null if not found.
        public SearchResult Get(SearchRequest key)
        {
            SearchResult val;
            using (cacheLock.ReadLock())
                cacheStore.TryGetValue(key, out val); // So there is no exception if not found
            return val;
        }

        // This method is to remove an item from the cache by its key
        public void Remove(SearchRequest key)
        {
            using (cacheLock.WriteLock())
                if (cacheStore.ContainsKey(key))
                    cacheStore.Remove(key);
        }

        // This method empty the whole cache.
        public void Clear()
        {
            using (cacheLock.WriteLock())
                cacheStore.Clear();
        }

        // This method validates that a key exists in the cache.
        public bool Exist(SearchRequest key)
        {
            using (cacheLock.ReadLock())
                return cacheStore.ContainsKey(key);
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