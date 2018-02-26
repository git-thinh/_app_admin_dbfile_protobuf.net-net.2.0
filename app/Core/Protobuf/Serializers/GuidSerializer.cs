using System;



namespace ProtoBuf.Serializers
{
    sealed class GuidSerializer : IProtoSerializer
    {
        static readonly Type expectedType = typeof(Guid);
        public GuidSerializer(ProtoBuf.Meta.TypeModel model)
        {
        }

        public Type ExpectedType { get { return expectedType; } }

        bool IProtoSerializer.RequiresOldValue { get { return false; } }
        bool IProtoSerializer.ReturnsValue { get { return true; } }

        public void Write(object value, ProtoWriter dest)
        {
            BclHelpers.WriteGuid((Guid)value, dest);
        }
        public object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return BclHelpers.ReadGuid(source);
        }
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitWrite(ctx.MapType(typeof(BclHelpers)), "WriteGuid", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead(ctx.MapType(typeof(BclHelpers)), "ReadGuid", ExpectedType);
        }

    }
}
