﻿using System;
using System.Reflection;

namespace ProtoBuf.Serializers
{
    sealed class UInt64Serializer : IProtoSerializer
    {
        static readonly Type expectedType = typeof(ulong);
        public UInt64Serializer(ProtoBuf.Meta.TypeModel model)
        {
#if FEAT_IKVM
            expectedType = model.MapType(typeof(ulong));
#endif
        }
        public Type ExpectedType { get { return expectedType; } }

        bool IProtoSerializer.RequiresOldValue { get { return false; } }
        bool IProtoSerializer.ReturnsValue { get { return true; } }

        public object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return source.ReadUInt64();
        }
        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteUInt64((ulong)value, dest);
        }

        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteUInt64", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead("ReadUInt64", ExpectedType);
        }
    }
}
