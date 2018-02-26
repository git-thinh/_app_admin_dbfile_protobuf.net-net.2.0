using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{

    [Serializable]
    public class InfoSelectTop
    {
        public int PortHTTP { set; get; }

        public int TotalRecord { set; get; }

        public IList DataSelectTop { set; get; }

        public FieldInfo[] Fields { set; get; }
    }
}
