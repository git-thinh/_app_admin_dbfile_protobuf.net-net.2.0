
using System;
using System.Collections;
using System.Reflection;

namespace ProtoBuf
{
    /// <summary>
    /// Not all frameworks are created equal (fx1.1 vs fx2.0,
    /// micro-framework, compact-framework,
    /// silverlight, etc). This class simply wraps up a few things that would
    /// otherwise make the real code unnecessarily messy, providing fallback
    /// implementations if necessary.
    /// </summary>
    internal sealed class Helpers
    {
        private Helpers() { }

        public static System.Text.StringBuilder AppendLine(System.Text.StringBuilder builder)
        {
            return builder.AppendLine();
        }
        public static bool IsNullOrEmpty(string value)
        { // yes, FX11 lacks this!
            return value == null || value.Length == 0;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugWriteLine(string message, object obj)
        {
            string suffix;
            try
            {
                suffix = obj == null ? "(null)" : obj.ToString();
            }
            catch
            {
                suffix = "(exception)";
            }
            DebugWriteLine(message + ": " + suffix);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugWriteLine(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
        [System.Diagnostics.Conditional("TRACE")]
        public static void TraceWriteLine(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugAssert(bool condition, string message)
        {
            if (!condition)
            {
                System.Diagnostics.Debug.Assert(false, message);
            }
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugAssert(bool condition, string message, params object[] args)
        {
            if (!condition) DebugAssert(false, string.Format(message, args));
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugAssert(bool condition)
        {
            if(!condition && System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
            System.Diagnostics.Debug.Assert(condition);
        }
        public static void Sort(int[] keys, object[] values)
        {
            // bubble-sort; it'll work on MF, has small code,
            // and works well-enough for our sizes. This approach
            // also allows us to do `int` compares without having
            // to go via IComparable etc, so win:win
            bool swapped;
            do {
                swapped = false;
                for (int i = 1; i < keys.Length; i++) {
                    if (keys[i - 1] > keys[i]) {
                        int tmpKey = keys[i];
                        keys[i] = keys[i - 1];
                        keys[i - 1] = tmpKey;
                        object tmpValue = values[i];
                        values[i] = values[i - 1];
                        values[i - 1] = tmpValue;
                        swapped = true;
                    }
                }
            } while (swapped);
        }
        public static void BlockCopy(byte[] from, int fromIndex, byte[] to, int toIndex, int count)
        {
            Buffer.BlockCopy(from, fromIndex, to, toIndex, count);
        }
        public static bool IsInfinity(float value)
        {
            return float.IsInfinity(value);
        }

        internal static MethodInfo GetInstanceMethod(Type declaringType, string name)
        {
            return declaringType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        internal static MethodInfo GetStaticMethod(Type declaringType, string name)
        {
            return declaringType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
        internal static MethodInfo GetInstanceMethod(Type declaringType, string name, Type[] types)
        {
            if(types == null) types = EmptyTypes;
            return declaringType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, types, null);
        }

        internal static bool IsSubclassOf(Type type, Type baseClass)
        {
            return type.IsSubclassOf(baseClass);
        }

        public static bool IsInfinity(double value)
        {
            return double.IsInfinity(value);
        }
        public readonly static Type[] EmptyTypes =
            Type.EmptyTypes;


        public static ProtoTypeCode GetTypeCode(System.Type type)
        {
            TypeCode code = System.Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Empty:
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return (ProtoTypeCode)code;
            }
            if (type == typeof(TimeSpan)) return ProtoTypeCode.TimeSpan;
            if (type == typeof(Guid)) return ProtoTypeCode.Guid;
            if (type == typeof(Uri)) return ProtoTypeCode.Uri;
            if (type == typeof(byte[])) return ProtoTypeCode.ByteArray;
            if (type == typeof(System.Type)) return ProtoTypeCode.Type;

            return ProtoTypeCode.Unknown;
        }

      

        internal static System.Type GetUnderlyingType(System.Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        internal static bool IsValueType(Type type)
        {
            return type.IsValueType;
        }

        internal static bool IsEnum(Type type)
        {
            return type.IsEnum;
        }

        internal static MethodInfo GetGetMethod(PropertyInfo property, bool nonPublic, bool allowInternal)
        {
            if (property == null) return null;
            MethodInfo method = property.GetGetMethod(nonPublic);
            if (method == null && !nonPublic && allowInternal)
            { // could be "internal" or "protected internal"; look for a non-public, then back-check
                method = property.GetGetMethod(true);
                if (method == null && !(method.IsAssembly || method.IsFamilyOrAssembly))
                {
                    method = null;
                }
            }
            return method;
        }
        internal static MethodInfo GetSetMethod(PropertyInfo property, bool nonPublic, bool allowInternal)
        {
            if (property == null) return null;

            MethodInfo method = property.GetSetMethod(nonPublic);
            if (method == null && !nonPublic && allowInternal)
            { // could be "internal" or "protected internal"; look for a non-public, then back-check
                method = property.GetGetMethod(true);
                if (method == null && !(method.IsAssembly || method.IsFamilyOrAssembly))
                {
                    method = null;
                }
            }
            return method;

        }
        

        internal static ConstructorInfo GetConstructor(Type type, Type[] parameterTypes, bool nonPublic)
        {
            return type.GetConstructor(
                nonPublic ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                          : BindingFlags.Instance | BindingFlags.Public,
                    null, parameterTypes, null);

        }
        internal static ConstructorInfo[] GetConstructors(Type type, bool nonPublic)
        {
            return type.GetConstructors(
                nonPublic ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                          : BindingFlags.Instance | BindingFlags.Public);
        }
        internal static PropertyInfo GetProperty(Type type, string name, bool nonPublic)
        {
            return type.GetProperty(name,
                nonPublic ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                          : BindingFlags.Instance | BindingFlags.Public);
        }


        internal static object ParseEnum(Type type, string value)
        {
            return Enum.Parse(type, value, true);
        }


        internal static MemberInfo[] GetInstanceFieldsAndProperties(Type type, bool publicOnly)
        {
            BindingFlags flags = publicOnly ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            PropertyInfo[] props = type.GetProperties(flags);
            FieldInfo[] fields = type.GetFields(flags);
            MemberInfo[] members = new MemberInfo[fields.Length + props.Length];
            props.CopyTo(members, 0);
            fields.CopyTo(members, props.Length);
            return members;
        }

        internal static Type GetMemberType(MemberInfo member)
        {            switch(member.MemberType)
            {
                case MemberTypes.Field: return ((FieldInfo) member).FieldType;
                case MemberTypes.Property: return ((PropertyInfo) member).PropertyType;
                default: return null;
            }
        }

        internal static bool IsAssignableFrom(Type target, Type type)
        {
            return target.IsAssignableFrom(type);
        }

    }
    /// <summary>
    /// Intended to be a direct map to regular TypeCode, but:
    /// - with missing types
    /// - existing on WinRT
    /// </summary>
    internal enum ProtoTypeCode
    {
        Empty = 0,
        Unknown = 1, // maps to TypeCode.Object
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 15,
        DateTime = 16,
        String = 18,

        // additions
        TimeSpan = 100,
        ByteArray = 101,
        Guid = 102,
        Uri = 103,
        Type = 104
    }
}
