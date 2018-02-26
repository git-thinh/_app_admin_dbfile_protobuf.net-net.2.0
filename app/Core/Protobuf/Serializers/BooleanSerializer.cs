using System;





namespace ProtoBuf.Serializers
{
    sealed class BooleanSerializer : IProtoSerializer
    {
        static readonly Type expectedType = typeof(bool);
        public BooleanSerializer(ProtoBuf.Meta.TypeModel model)
        {
        }
        public Type ExpectedType { get { return expectedType; } }

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteBoolean((bool)value, dest);
        }
        public object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return source.ReadBoolean();
        }
        bool IProtoSerializer.RequiresOldValue { get { return false; } }
        bool IProtoSerializer.ReturnsValue { get { return true; } }
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteBoolean", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead("ReadBoolean", ExpectedType);
        }
    }
}
