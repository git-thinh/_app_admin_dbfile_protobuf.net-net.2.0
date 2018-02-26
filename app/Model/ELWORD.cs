using System;
using System.Collections.Generic;
using System.Text;
using app.Core;
using System.Linq;

namespace app.Model
{

    [Serializable]
    public enum ELTYPE
    {
        NONE = 0,
        V = 1,
        N = 2,
        ADJ = 3,
        ARTICLE = 4,
        PREP = 5,
    }

    [DB_MODEL(CAPTION = "Word", TAG = "English")]
    [Serializable]
    public class ELWORD
    {
        [FieldInfo(IS_KEY_AUTO = true, IS_KEY_SYNC_EDIT = true)]
        public long ID { set; get; }

        [FieldInfo(IS_NOT_DUPLICATE = true, IS_INDEX = true)]
        public string WORD { set; get; }

        public string PRONOUNCE { set; get; }

        [FieldInfo(TYPE_NAME = "Byte", KIT = ControlKit.SELECT, VALUE_DEFAULT = new string[] { "NONE", "V", "N", "ADJ", "ARTICLE", "PREP" })]
        public byte TYPE { set; get; }

        public string MEANING { set; get; }

        public override string ToString()
        {
            return string.Format("{0} {1} [{2}] {3}", WORD, PRONOUNCE, TYPE, MEANING);
        }
    }

    [DB_MODEL(CAPTION = "Example", TAG = "English")]
    [Serializable]
    public class ELEXAMPLE
    {
        [FieldInfo(IS_KEY_AUTO = true, IS_KEY_SYNC_EDIT = true)]
        public long ID { set; get; }

        [FieldInfo(IS_NOT_DUPLICATE = false, IS_INDEX = true)]
        public string WORD { set; get; }

        [FieldInfo(IS_NOT_DUPLICATE = false)]
        public string EXAMPLE { set; get; }

        public string MEANING { set; get; }

        public override string ToString()
        {
            return string.Format("- {0}\n- {1}\n- {2}", WORD, EXAMPLE, MEANING);
        }
    }
}
