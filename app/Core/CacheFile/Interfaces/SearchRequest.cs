using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{
    [Serializable]
    public class SearchRequest
    {
        public int PageSize { set; get; }
        public int PageNumber { set; get; }

        public string Predicate { private set; get; }
        public object[] Values { private set; get; }

        public int getCacheCode()
        {
            StringBuilder bi = new StringBuilder(Predicate);
            if (Values != null && Values.Length > 0)
                foreach (var si in Values)
                    if (si != null)
                        bi.Append(si.ToString());
            return bi.ToString().GetHashCode();
        }

        public SearchRequest(string predicate, params object[] values)
        {
            Predicate = predicate;
            Values = values;
        }

        public SearchRequest(int pageSize, int pageNumber, string predicate, params object[] values)
        {
            Predicate = predicate;
            Values = values;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public class EqualityComparer : IEqualityComparer<SearchRequest>
        {
            public bool Equals(SearchRequest x, SearchRequest y)
            {
                return x.getCacheCode() == y.getCacheCode();
            }

            public int GetHashCode(SearchRequest x)
            {
                int v = x.getCacheCode();
                return v;
            }
        }
    }
}
