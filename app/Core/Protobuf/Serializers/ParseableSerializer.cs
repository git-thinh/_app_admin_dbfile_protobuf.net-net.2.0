using System;
using System.Net;
using ProtoBuf.Meta;
using System.Reflection;

namespace ProtoBuf.Serializers
{
    sealed class ParseableSerializer : IProtoSerializer
    {
        private readonly MethodInfo parse;
        public static ParseableSerializer TryCreate(Type type, TypeModel model)
        {
            if (type == null) throw new ArgumentNullException("type");
            MethodInfo method = type.GetMethod("Parse",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                null, new Type[] { model.MapType(typeof(string)) }, null);
            if (method != null && method.ReturnType == type)
            {
                if (Helpers.IsValueType(type))
                {
                    MethodInfo toString = GetCustomToString(type);
                    if (toString == null || toString.ReturnType != model.MapType(typeof(string))) return null; // need custom ToString, fools
                }
                return new ParseableSerializer(method);
            }
            return null;
        }
        private static MethodInfo GetCustomToString(Type type)
        {

            return type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                        null, Helpers.EmptyTypes, null);
        }
        private ParseableSerializer(MethodInfo parse)
        {
            this.parse = parse;
        }
        public Type ExpectedType { get { return parse.DeclaringType; } }

        bool IProtoSerializer.RequiresOldValue { get { return false; } }
        bool IProtoSerializer.ReturnsValue { get { return true; } }

        public object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return parse.Invoke(null, new object[] { source.ReadString() });
        }
        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteString(value.ToString(), dest);
        }
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            Type type = ExpectedType;
            if (type.IsValueType)
            {   // note that for structs, we've already asserted that a custom ToString
                // exists; no need to handle the box/callvirt scenario
                
                // force it to a variable if needed, so we can take the address
                using (Compiler.Local loc = ctx.GetLocalWithValue(type, valueFrom))
                {
                    ctx.LoadAddress(loc, type);
                    ctx.EmitCall(GetCustomToString(type));
                }
            }
            else {
                ctx.EmitCall(ctx.MapType(typeof(object)).GetMethod("ToString"));
            }
            ctx.EmitBasicWrite("WriteString", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead("ReadString", ctx.MapType(typeof(string)));
            ctx.EmitCall(parse);
        }

    }
}
