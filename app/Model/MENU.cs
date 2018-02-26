using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using app.Core;

namespace app.Model
{
    [DB_MODEL(CAPTION = "Menu", TAG = "System")]
    [Serializable]
    public class MENU
    {
        [FieldInfo(IS_KEY_AUTO = true, IS_KEY_SYNC_EDIT = true)]
        public long ID { set; get; }

        [FieldInfo(IS_INDEX = true, IS_NOT_DUPLICATE = true)]
        public string NAME { set; get; }

        public string TITLE { set; get; }

        [FieldInfo(IS_INDEX = true)]
        public string TAG { set; get; }

        public string DESCRIPTION { set; get; }

        [FieldInfo(IS_INDEX = true, KIT = ControlKit.USERS)]
        public string USER { set; get; }

        [FieldInfo(IS_INDEX = true, KIT = ControlKit.MODELS)]
        public string MODELS { set; get; }

        [FieldInfo(KIT = ControlKit.CHECK)]
        public byte ACTIVE { set; get; }

        public MENU() { }
        public MENU(DB_MODEL m)
        {
            NAME = m.NAME;
            TITLE = m.CAPTION;
            TAG = m.TAG;
            MODELS = m.NAME;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2} - {3}", TAG, NAME, TITLE, MODELS);
        }
    }
}
