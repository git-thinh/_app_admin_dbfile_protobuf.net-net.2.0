using System;
using System.Collections.Generic;
using System.Text;
using app.Core;

namespace app.Model
{
    [DB_MODEL(CAPTION = "Content", TAG = "Article")]
    public class CNDATA
    {
        [FieldInfo(IS_KEY_AUTO = true, IS_KEY_SYNC_EDIT = true)]
        public long ID { set; get; }

        [FieldInfo(TYPE_NAME = "String", KIT = ControlKit.LABEL, FUNC_BEFORE_UPDATE = "GENERATE_KEY_URI")]
        public string KEY_URI { set; get; }

        [FieldInfo(ORDER_KEY_URL = 1, IS_NOT_DUPLICATE = false)]
        public string TITLE { set; get; }

        [FieldInfo(KIT = ControlKit.PICTURE)]
        public string IMG_DEFAULT { set; get; }

        [FieldInfo(KIT = ControlKit.PICTURES)]
        public string IMG_SLIDE { set; get; }

        [FieldInfo(KIT = ControlKit.TAGS)]
        public string TAG_INDEX { set; get; }

        [FieldInfo(KIT = ControlKit.TEXTAREA)]
        public string DESCRIPTION { set; get; }

        [FieldInfo(KIT = ControlKit.HTML)]
        public string CONTENT { set; get; }
         
        public string URL { set; get; }

        [FieldInfo(KIT = ControlKit.CHECK)]
        public byte ACTIVE { set; get; }
    }
}
