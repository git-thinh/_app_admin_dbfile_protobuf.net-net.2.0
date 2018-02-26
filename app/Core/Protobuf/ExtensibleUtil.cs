using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf.Meta;

namespace ProtoBuf
{
    /// <summary>
    /// This class acts as an internal wrapper allowing us to do a dynamic
    /// methodinfo invoke; an't put into Serializer as don't want on public
    /// API; can't put into Serializer&lt;T&gt; since we need to invoke
    /// accross classes, which isn't allowed in Silverlight)
    /// </summary>
    internal
    static
        class ExtensibleUtil
    {

        /// <summary>
        /// All this does is call GetExtendedValuesTyped with the correct type for "instance";
        /// this ensures that we don't get issues with subclasses declaring conflicting types -
        /// the caller must respect the fields defined for the type they pass in.
        /// </summary>
        internal static IEnumerable<TValue> GetExtendedValues<TValue>(IExtensible instance, int tag, DataFormat format, bool singleton, bool allowDefinedTag)
        {
            foreach (TValue value in GetExtendedValues(RuntimeTypeModel.Default, typeof(TValue), instance, tag, format, singleton, allowDefinedTag))
            {
                yield return value;
            }
        }
        /// <summary>
        /// All this does is call GetExtendedValuesTyped with the correct type for "instance";
        /// this ensures that we don't get issues with subclasses declaring conflicting types -
        /// the caller must respect the fields defined for the type they pass in.
        /// </summary>
        internal static IEnumerable GetExtendedValues(TypeModel model, Type type, IExtensible instance, int tag, DataFormat format, bool singleton, bool allowDefinedTag)
        {


            if (instance == null) throw new ArgumentNullException("instance");
            if (tag <= 0) throw new ArgumentOutOfRangeException("tag");
            IExtension extn = instance.GetExtensionObject(false);

            if (extn == null)
            {
                yield break;
            }

            Stream stream = extn.BeginQuery();
            object value = null;
            ProtoReader reader = null;
            try {
                SerializationContext ctx = new SerializationContext();
                reader = ProtoReader.Create(stream, model, ctx, ProtoReader.TO_EOF);
                while (model.TryDeserializeAuxiliaryType(reader, format, tag, type, ref value, true, false, false, false) && value != null)
                {
                    if (!singleton)
                    {
                        yield return value;
                        value = null; // fresh item each time
                    }
                }
                if (singleton && value != null)
                {
                    yield return value;
                }
            } finally {
                ProtoReader.Recycle(reader);
                extn.EndQuery(stream);
            }
        }

        internal static void AppendExtendValue(TypeModel model, IExtensible instance, int tag, DataFormat format, object value)
        {

            if(instance == null) throw new ArgumentNullException("instance");
            if(value == null) throw new ArgumentNullException("value");

            // TODO
            //model.CheckTagNotInUse(tag);

            // obtain the extension object and prepare to write
            IExtension extn = instance.GetExtensionObject(true);
            if (extn == null) throw new InvalidOperationException("No extension object available; appended data would be lost.");
            bool commit = false;
            Stream stream = extn.BeginAppend();
            try {
                using(ProtoWriter writer = new ProtoWriter(stream, model, null)) {
                    model.TrySerializeAuxiliaryType(writer, null, format, tag, value, false);
                    writer.Close();
                }
                commit = true;
            }
            finally {
                extn.EndAppend(stream, commit);
            }
        } 
    }

}