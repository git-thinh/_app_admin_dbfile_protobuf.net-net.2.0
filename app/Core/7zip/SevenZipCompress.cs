using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SevenZip
{
    public class SevenZipCompress
    {
        public static byte[] SerializeBson(object obj, bool Compress = true)
        {
            byte[] buf = null;
            using (var ms = new MemoryStream())
            {
                using (var writer = new BsonWriter(ms))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, new { Value = obj });
                    byte[] bytes = ms.ToArray();
                    if (Compress)
                        buf = Compression.LZMA.SevenZipHelper.Compress(bytes);
                    else
                        return bytes;
                }
            }
            return buf;
        }

        public static object DeserializeBson(byte[] bytes, Type type, bool Compress = true)
        {
            object val = null;
            byte[] buf = null;
            if (Compress)
                buf = Compression.LZMA.SevenZipHelper.Compress(bytes);
            else
                buf = bytes;
            using (var ms = new MemoryStream(buf))
            {
                using (var reader = new BsonReader(ms))
                {
                    var serializer = new JsonSerializer();
                    val = serializer.Deserialize(reader, type);
                }
            }
            return val;
        }

        public static object DeserializeBson(byte[] bytes, bool Compress = true)
        {
            object val = null;
            byte[] buf = null;
            if (Compress)
                buf = Compression.LZMA.SevenZipHelper.Compress(bytes);
            else
                buf = bytes;
            using (var ms = new MemoryStream(buf))
            {
                using (var reader = new BsonReader(ms))
                {
                    var serializer = new JsonSerializer();
                    val = serializer.Deserialize(reader);
                }
            }
            return val;
        }

        public static byte[] SerializeBinary(object obj, bool Compress = true)
        {
            byte[] buf = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, obj);
                    byte[] bytes = ms.ToArray();
                    if (Compress)
                        buf = Compression.LZMA.SevenZipHelper.Compress(bytes);
                    else
                        return bytes;
                }
            }
            catch
            {
                throw;
            }
            return buf;
        }

        public static object DeserializeBinary(byte[] buf, bool Decompress = true)
        {
            object val = null;
            try
            {
                byte[] bytes = null;
                if (Decompress)
                    bytes = Compression.LZMA.SevenZipHelper.Compress(buf);
                else
                    bytes = buf;

                using (var ms = new MemoryStream(bytes))
                {
                    ms.Seek(0, 0);
                    BinaryFormatter formatter = new BinaryFormatter();
                    val = (object)formatter.Deserialize(ms, null);

                    ////IFormatter ft = new BinaryFormatter();
                    ////val = ft.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return val;
        }
    }
}
