using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{

    [Serializable]
    public enum dbAction
    {
        NONE = 0,
        DB_REG_MODEL = 1,
        DB_ADD = 2,
        DB_REMOVE = 3,
        DB_UPDATE = 4,
        DB_SELECT = 5,
        DB_RESULT = 6,
    }

}
