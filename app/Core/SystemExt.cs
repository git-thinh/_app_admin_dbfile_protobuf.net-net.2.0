using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    public static class SystemExt
    {

        /// <summary>
        /// Chuyển chuỗi unicode sang ascii (lọc bỏ dấu tiếng việt) 
        /// </summary>
        /// <param name="unicode"></param>
        /// <returns></returns>
        static public String ToAscii(this string unicode)
        {
            if (string.IsNullOrEmpty(unicode)) return "";

            unicode = Regex.Replace(unicode, "[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            unicode = Regex.Replace(unicode, "[óòỏõọôồốổỗộơớờởỡợ]", "o");
            unicode = Regex.Replace(unicode, "[éèẻẽẹêếềểễệ]", "e");
            unicode = Regex.Replace(unicode, "[íìỉĩị]", "i");
            unicode = Regex.Replace(unicode, "[úùủũụưứừửữự]", "u");
            unicode = Regex.Replace(unicode, "[ýỳỷỹỵ]", "y");
            unicode = Regex.Replace(unicode, "[đ]", "d");
            unicode = Regex.Replace(unicode, "[-\\s+/]+", " ");
            unicode = Regex.Replace(unicode, "\\W+", " "); //Nếu bạn muốn thay dấu khoảng trắng thành dấu "_" hoặc dấu cách " " thì thay kí tự bạn muốn vào đấu "-"
            return unicode;
        }

    }
}
