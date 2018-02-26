using System;

namespace ProtoBuf.Serializers
{
    sealed class DateTimeSerializer : IProtoSerializer
    {
        static readonly Type expectedType = typeof(DateTime);
        public Type ExpectedType { get { return expectedType; } }

        bool IProtoSerializer.RequiresOldValue { get { return false; } }
        bool IProtoSerializer.ReturnsValue { get { return true; } }

        public DateTimeSerializer(ProtoBuf.Meta.TypeModel model)
        {
        }
        public object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return BclHelpers.ReadDateTime(source);
        }
        public void Write(object value, ProtoWriter dest)
        {
            BclHelpers.WriteDateTime((DateTime)value, dest);
        }
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitWrite(ctx.MapType(typeof(BclHelpers)), "WriteDateTime", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead(ctx.MapType(typeof(BclHelpers)), "ReadDateTime", ExpectedType);
        }

    }
}
