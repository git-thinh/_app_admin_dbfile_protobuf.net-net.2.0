﻿using System;

using System.Reflection;


namespace ProtoBuf.Serializers
{
    sealed class UInt32Serializer : IProtoSerializer
    {
        static readonly Type expectedType = typeof(uint);
        public UInt32Serializer(ProtoBuf.Meta.TypeModel model)
        {

        }
        public Type ExpectedType { get { return expectedType; } }

        bool IProtoSerializer.RequiresOldValue { get { return false; } }
        bool IProtoSerializer.ReturnsValue { get { return true; } }
        public object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return source.ReadUInt32();
        }
        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteUInt32((uint)value, dest);
        }

        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteUInt32", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead("ReadUInt32", ctx.MapType(typeof(uint)));
        }
    }
}
