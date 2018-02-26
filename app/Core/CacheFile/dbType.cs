using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{
    public class dbTypeConvert
    {
        public bool OK { set; get; }
        public object Value { set; get; }

        public dbTypeConvert(bool _ok, object _value)
        {
            OK = _ok;
            Value = _value;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", OK, Value);
        }
    }

    public class dbType
    {
        //private string[] aType = "String,Byte,SByte,Int32,UInt32,Int16,UInt16,Int64,UInt64,Single,Double,Char,Boolean,Object,Decimal".Split(',');
        public static readonly string[] Types = "String,Int32,Int64,DateTime,Byte,Boolean".Split(',');

        public static Type GetByName(string nameType)
        {
            Type type = typeof(String);
            switch (nameType)
            {
                case "Int32":
                    type = typeof(Int32);
                    break;
                case "Int64":
                    type = typeof(Int64);
                    break;
                case "String":
                    type = typeof(String);
                    break;
                case "DateTime":
                    type = typeof(Int64);
                    break;
                case "Byte":
                    type = typeof(Byte);
                    break;
                case "Boolean":
                    type = typeof(Byte);
                    break;
            }
            return type;
        }

        public static dbTypeConvert Convert(string nameType, object value)
        {
            if (value.GetType().Name == nameType) return new dbTypeConvert(true, value);

            object val = null;
            bool ok = false;
            try
            {
                string _value = value.ToString();
                switch (nameType)
                {
                    case "Int32":
                        val = Int32.Parse(_value);
                        ok = true;
                        break;
                    case "Int64":
                        val = long.Parse(_value);
                        ok = true;
                        break;
                    case "String":
                        val = _value;
                        ok = true;
                        break;
                    case "DateTime":
                        val = long.Parse(_value);
                        ok = true;
                        break;
                    case "Byte":
                        switch (_value) 
                        {
                            case "True": case "1": val = (byte)1; break;
                            default:
                                byte bi = 0;
                                byte.TryParse(_value, out bi);
                                val = bi;
                                break; 
                        } 
                        //val = Byte.Parse(_value);
                        ok = true;
                        break;
                    case "Boolean": 
                        val = Byte.Parse(_value);
                        ok = true;
                        break;
                }
            }
            catch { }

            return new dbTypeConvert(ok, val);
        }


        public static object CreateValueRandom(string nameType)
        {
            object val = null;
            switch (nameType)
            {
                case "Int32":
                    val = new Random().Next(0, int.MaxValue);
                    break;
                case "Int64":
                    val = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff")) +  new Random().Next(0, int.MaxValue);
                    break;
                case "String":
                    val = Guid.NewGuid().ToString().ToUpper().Replace('-', ' ');
                    break;
                case "DateTime":
                    val = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    break;
                case "Byte":
                    val = (Byte)(new Random().Next(1, 255));
                    break;
                case "Boolean":
                    val = (Byte)(new Random().Next(1, 255));
                    break;
            }
            return val;
        }
    }
}
