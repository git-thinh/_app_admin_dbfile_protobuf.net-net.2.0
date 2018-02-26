using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{
    [Serializable]
    public enum EditStatus
    {
        NONE = 0,
        FAIL_ITEM_EXIST = 1,
        FAIL_ITEM_NOT_EXIST = 2,
        FAIL_MAX_LEN_IS_255_BYTE = 3,
        FAIL_EXCEPTION_IS_NULL = 4,
        FAIL_EXCEPTION_CONVERT_TO_DYNAMIC_OBJECT = 5,
        FAIL_EXCEPTION_SERIALIZE_DYNAMIC_OBJECT = 6,
        FAIL_EXCEPTION_WRITE_ARRAY_BYTE_TO_FILE = 7,
        FAIL_EXCEPTION = 8,
        SUCCESS = 9,
    }
}
