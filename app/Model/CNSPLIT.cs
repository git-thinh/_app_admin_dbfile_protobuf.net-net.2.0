using System;
using System.Collections.Generic;
using System.Text;
using app.Core;

namespace app.Model
{
    [DB_MODEL(CAPTION = "Format", TAG = "Article")]
    public class CNSPLIT
    {
        [FieldInfo(IS_KEY_AUTO = true, IS_KEY_SYNC_EDIT = true)]
        public long ID { set; get; }

        public string SITE { set; get; }

        public string TEXT_FIRST { set; get; }
        public string TEXT_LAST { set; get; }
         
        public byte SKIP_LINE_TOP { set; get; }
        public byte SKIP_LINE_BOTTOM { set; get; }
    }
}
