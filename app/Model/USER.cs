using System;
using System.Collections.Generic;
using System.Text;
using app.Core;

namespace app.Model
{
    public enum LOGIN_STATUS
    {
        MODEL_USER_IS_EMPTY = 0,
        USERNAME_PASS_WRONG = 1,
        LOGIN_SUCCESSFULLY = 2
    }

    [DB_MODEL(CAPTION = "User", TAG = "System")]
    [Serializable]
    public class USER
    {
        [FieldInfo(IS_KEY_AUTO = true, IS_KEY_SYNC_EDIT = true)]
        public long ID { set; get; }

        [FieldInfo(IS_INDEX = true, IS_NOT_DUPLICATE = true)]
        public string USERNAME { set; get; }

        [FieldInfo(IS_ENCRYPT = true, KIT = ControlKit.TEXT_PASS, ONLY_SHOW_IN_DETAIL = true)]
        public string PASSWORD { set; get; }

        public string FULLNAME { set; get; }
    }
}
