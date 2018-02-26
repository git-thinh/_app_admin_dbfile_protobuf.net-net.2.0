using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Drawing;
using app.Model;

namespace app.Core
{
    public class dbFunc
    {
        private static ConcurrentDictionary<string, Func<object, bool>> cacheFuncValidate; 
        private static ConcurrentDictionary<string, Func<DB_MODEL, string, Dictionary<string,object>, object>> cacheFuncBeforeUpdate; 
        static dbFunc()
        {
            cacheFuncBeforeUpdate = new ConcurrentDictionary<string, Func<DB_MODEL, string, Dictionary<string, object>, object>>(Environment.ProcessorCount * 2, 101);
            cacheFuncValidate = new ConcurrentDictionary<string, Func<object, bool>>(Environment.ProcessorCount * 2, 101);

            #region [ === FUNC BEFORE UPDATE === ]

            cacheFuncBeforeUpdate.TryAdd("LOAD_FILE_PC_TO_BASE64", delegate (DB_MODEL model, string colField, Dictionary<string, object> data)
            {
                string val = string.Empty;
                ////if (item == null) return val;
                ////string file = item.ToString();
                ////if (file == string.Empty) return val;
                ////if (File.Exists(file)) 
                ////{
                ////    using (Image image = Image.FromFile(file))
                ////    {
                ////        using (MemoryStream m = new MemoryStream())
                ////        {
                ////            image.Save(m, image.RawFormat);
                ////            byte[] imageBytes = m.ToArray(); 
                ////            val = Convert.ToBase64String(imageBytes); 
                ////        }
                ////    }
                ////}
                return val;
            });

            cacheFuncBeforeUpdate.TryAdd("GENERATE_KEY_URI", delegate (DB_MODEL model, string colField, Dictionary<string, object> data)
            {
                string val = string.Empty;
                var fData = model.FIELDS.Where(x => x.ORDER_KEY_URL > 0 && x.NAME != colField).OrderBy(x => x.ORDER_KEY_URL).ToArray();
                if (fData.Length > 0)
                {
                    var dt = data.Where(kv => fData.Any(x => x.NAME == kv.Key)).Select(kv => kv.Value == null ? "" : kv.Value.ToString()).ToArray();
                    if (dt.Length > 0)
                        val = string.Join("-", dt).ToAscii().Replace(' ', '-');
                }
                return val;
            });

            #endregion

            #region [ === FUNC VALIDATE === ]

            cacheFuncValidate.TryAdd(VALIDATE_EMPTY, delegate(object item)
            {
                if (item == null) return false;
                if (item.ToString() == string.Empty) return false;
                return true;
            });
            cacheFuncValidate.TryAdd("VALIDATE_IS_EMAIL", delegate(object item)
            {
                if (item == null) return false;
                if (item.ToString() == string.Empty) return false;
                string s = item.ToString();
                if (s.IndexOf('@') == -1) return false;
                return true;
            });
            cacheFuncValidate.TryAdd("VALIDATE_IS_PHONE_NUMBER", delegate(object item)
            {
                if (item == null) return false;
                if (item.ToString() == string.Empty) return false;
                return true;
            });
            cacheFuncValidate.TryAdd("VALIDATE_IS_URL_HTTP", delegate(object item)
            {
                if (item == null) return false;
                if (item.ToString() == string.Empty) return false;
                string s = item.ToString().ToLower().Trim();
                if (s.IndexOf("http") != 0) return false;
                return true;
            });

            #endregion
        }

        #region [ === METHOD === ]

        public static string[] GetFuncValidate() { return cacheFuncValidate.Keys.OrderBy(x => x).ToArray(); }
        public static string[] GetFuncBeforeAddOrUpdate() { return cacheFuncBeforeUpdate.Keys.OrderBy(x => x).ToArray(); }

        public static object ExeFuncBeforeAddOrUpdate(string fun_name, DB_MODEL model, string colField, Dictionary<string, object> data, object val) 
        {
            object rs = val;
            if (fun_name != title_FUNC_BEFORE_ADD_OR_UPDATE && fun_name != title_FUNC_VALIDATE_ON_FORM)
            {
                Func<DB_MODEL, string, Dictionary<string, object>, object> exe;
                if (cacheFuncBeforeUpdate.TryGetValue(fun_name, out exe))
                    rs = exe(model, colField, data);
            }
            return rs; 
        }

        public const string VALIDATE_EMPTY = "VALIDATE_EMPTY";
        public const string title_FUNC_BEFORE_ADD_OR_UPDATE = "Func before add or update";
        public const string title_FUNC_VALIDATE_ON_FORM = "Func validate on form";

        #endregion

    }
}
