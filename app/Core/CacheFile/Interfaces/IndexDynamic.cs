
using System; 

namespace app.Core
{  
    [Serializable]
    public class IndexDynamic
    {
        public int Index { set; get; }
        public object Item { set; get; }
        public IndexDynamic(int index, object item)
        {
            Index = index;
            Item = item;
        }
    } 
}
