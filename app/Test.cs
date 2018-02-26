using app.Core;
using ProtoBuf;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace app
{
    class Program
    {
        static Program()
        {
        }

        static List<TEST> list = new List<TEST>();
        static void Main(string[] args)
        {

            // 128; 16384
            for (int k = 1; k < 20000; k++)
                //list.Add(new Test() { Id = k, Level = k * 10, Name = k.ToString() });
                list.Add(new TEST() { ID = k, LEVEL = k * 10, NAME = k.ToString() + "-" + Guid.NewGuid().ToString() });

            test_add();
            //test_SEARCH();
            // test_update();
            // test_remove();

            // test_002();
            // test_003();
            // test_004(); 
        }

        static void test_update()
        {
            var ab = DataFile.Open(typeof(TEST));

            var rs = ab.Update(
                new TEST() { ID = 1, LEVEL = 10, NAME = "1" },
                new TEST() { ID = 1, NAME = "update item 1" });
        }

        static void test_remove()
        {
            var ab = DataFile.Open(typeof(TEST));

            var r0 = ab.AddItems(new TEST[] {
                new TEST() { ID = 1, NAME = "item 1" },
                new TEST() { ID = 2, NAME = "item 2" },
                new TEST() { ID = 3, NAME = "item 3" },
            });
            var rs = ab.Remove(new TEST() { ID = 2, NAME = "item 2" });

        }

        static void test_add()
        {
            //var db = DataFile.Open(typeof(USER));
            //var u1 = new USER() { PASSWORD = "12345", USERNAME = "admin" };
            //var u2 = new USER() { PASSWORD = "12345", USERNAME = "user" };

            var db = DataFile.Open(typeof(TEST));
            var u1 = new TEST() { ID = 1, LEVEL = 10, NAME = "admin" };
            var u2 = new TEST() { ID = 2, LEVEL = 20, NAME = "user" };
            var u3 = new TEST() { ID = 3, LEVEL = 30, NAME = "free" };

            var o1 = db.convertToDynamicObject(u1, false);
            var o2 = db.convertToDynamicObject(u2, false);
            var o3 = db.convertToDynamicObject(u3, false);

            IList ls = (IList)typeof(List<>).MakeGenericType(db.TypeDynamic).GetConstructor(Type.EmptyTypes).Invoke(null);
            Type listType = ls.GetType();
            app.Core.CacheFile.Serializer _boisSerializer = new app.Core.CacheFile.Serializer();
            _boisSerializer.Initialize(listType, db.TypeDynamic, typeof(TEST), list.GetType());
            byte[] b1;
            byte[] b2;
            byte[] bs;

            //using (var ms = new MemoryStream())
            //{
            //    ProtoBuf.Serializer.Serialize(ms, o1);
            //    b1 = ms.ToArray();
            //}
            using (var ms = new MemoryStream())
            {
                _boisSerializer.Serialize(o1, db.TypeDynamic, ms);
                b1 = ms.ToArray();
            }
            //b1 = System.Binary.BinarySerializer.Serialize(db.TypeDynamic, o1);

            //using (var ms = new MemoryStream())
            //{
            //    ProtoBuf.Serializer.Serialize(ms, o2);
            //    b2 = ms.ToArray();
            //}
            using (var ms = new MemoryStream())
            {
                _boisSerializer.Serialize(o2, db.TypeDynamic, ms);
                b2 = ms.ToArray();
            }
            //b2 = System.Binary.BinarySerializer.Serialize(db.TypeDynamic, o2);

            ls.Add(o1);
            ls.Add(o2);
            ls.Add(o3);
            //using (var ms = new MemoryStream())
            //{
            //    ProtoBuf.Serializer.Serialize(ms, ls);
            //    bs = ms.ToArray();
            //}

            ls.Clear();
            for (int k = 1; k < 2; k++)
                //list.Add(new TEST() { ID = k, LEVEL = k * 10, NAME = k.ToString() + "-" + Guid.NewGuid().ToString() });
                ls.Add(db.convertToDynamicObject(new TEST() { ID = k, LEVEL = k * 10, NAME = k % 9 == 0 ? null : k.ToString() + "-" + Guid.NewGuid().ToString() }, false));

            using (var ms = new MemoryStream())
            {
                _boisSerializer.Serialize(ls, listType, ms);
                bs = ms.ToArray();
            }

            string j1 = string.Join(" ", b1.Select(x => x.ToString()).ToArray());
            string j2 = string.Join(" ", b2.Select(x => x.ToString()).ToArray());
            string js = string.Join(" ", bs.Select(x => x.ToString()).ToArray());

            byte[] bc;
            using (var ms = new MemoryStream())
            {
                //63//126//189
                _boisSerializer.Serialize(100000000, typeof(int), ms);
                bc = ms.ToArray();
            }

            string s = j1 + Environment.NewLine + j2 + Environment.NewLine + js;
            Console.WriteLine(s);


            //////////object v1 = _boisSerializer.Deserialize(b1, db.TypeDynamic, 0, b1.Length);
            //////////object v2 = _boisSerializer.Deserialize(b2, db.TypeDynamic, 0, b2.Length);
            //////////object vs = _boisSerializer.Deserialize(bs, listType, 0, bs.Length);


            //////////int mx = 1000000;
            //////////for (int k = 1; k < mx; k++)
            //////////    //list.Add(new TEST() { ID = k, LEVEL = k * 10, NAME = k.ToString() + "-" + Guid.NewGuid().ToString() });
            //////////    ls.Add(db.convertToDynamicObject(new TEST() { ID = k, LEVEL = k * 10, NAME = k % 9 == 0 ? null : k.ToString() + "-" + Guid.NewGuid().ToString() }));

            //////////string count = ls.Count.ToString();
            //////////Console.WriteLine("BEGIN TEST ...............");

            //////////byte[] bg; TimeSpan ts; string elapsedTime;
            //////////Stopwatch stopWatch = new Stopwatch();

            //////////stopWatch.Start();
            //////////using (var ms = new MemoryStream())
            //////////{
            //////////    _boisSerializer.Serialize(ls, ls.GetType(), ms);
            //////////    bg = ms.ToArray();
            //////////}
            //////////stopWatch.Stop();
            //////////ts = stopWatch.Elapsed;
            //////////elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            //////////Console.WriteLine("BOIS Serialize: " + count.ToString() + " = " + elapsedTime + " = " + bg.Length.ToString());

            //////////stopWatch.Start();
            //////////var listB = _boisSerializer.Deserialize(bg, ls.GetType(), 0, bg.Length);
            //////////stopWatch.Stop();
            //////////ts = stopWatch.Elapsed;
            //////////elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            //////////Console.WriteLine("BOIS Deserialize: " + count.ToString() + " = " + elapsedTime + " = " + bg.Length.ToString());



            //////////stopWatch.Start();
            //////////using (var ms = new MemoryStream())
            //////////{
            //////////    ProtoBuf.Serializer.Serialize(ms, ls);
            //////////    bg = ms.ToArray();
            //////////}
            //////////stopWatch.Stop();
            //////////ts = stopWatch.Elapsed;
            //////////elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            //////////Console.WriteLine("P Serialize: " + count.ToString() + " = " + elapsedTime + " = " + bg.Length.ToString());

            //////////stopWatch.Start();
            //////////using (var ms = new MemoryStream(bg))
            //////////{
            //////////    var listP = ProtoBuf.Serializer.NonGeneric.Deserialize(ls.GetType(), ms);
            //////////}
            //////////stopWatch.Stop();
            //////////ts = stopWatch.Elapsed;
            //////////elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            //////////Console.WriteLine("P Deserialize: " + count.ToString() + " = " + elapsedTime + " = " + bg.Length.ToString());



            //////////string sss = "";
            ////////////////if (db.Opened)
            ////////////////{
            ////////////////    var ra = db.AddItems(new USER[] {
            ////////////////        new USER() { PASSWORD = "12345", USERNAME = "admin" },
            ////////////////        new USER() { PASSWORD = "12345", USERNAME = "user" },
            ////////////////        new USER() { PASSWORD = "12345", USERNAME = "free" },
            ////////////////    });
            ////////////////}

            //////////////var ab = DataFile.Open(typeof(TEST));
            //////////////if (ab.Opened)
            //////////////{
            //////////////    var r0 = ab.AddItems(new TEST[] {
            //////////////        new TEST() { ID = 1, NAME = "item 1" },
            //////////////        new TEST() { ID = 2, NAME = "item 2" },
            //////////////        new TEST() { ID = 3, NAME = "item 3" },
            //////////////    });
            //////////////}

            ////////////var r1 = ab.AddItems(new Test[] { new Test() { Id = ab.Count, Name = ab.Count.ToString() } });
            ////////////var r2 = ab.AddItems(new Test[] { new Test() { Id = 1, Name = "1" }, new Test(), new Test(), new Test() { Id = 3, Name = "3" } });
            ////////////var r3 = ab.AddItems(new Test[] { new Test() { Id = 2, Name = "item 2" } });
            ////////////var r4 = ab.AddItem(list[1]);
            ////////////var r5 = ab.AddItems(list.ToArray());
        }

        static void test_SEARCH()
        {
            var ab = DataFile.Open(typeof(TEST));
            //var r6 = ab.AddItems(list.ToArray());

            var m = new dbMsg(typeof(TEST))
            {
                Action = dbAction.DB_SELECT,
                Data = new SearchRequest(10, 1, "Name.Contains(@0)", "22"),
            };

            var r0 = m.Post();
            var r1 = m.Post();
            var r2 = m.GoPage(2).Post();

        }

        static void test_004()
        {
            //////////////////////////////////////////////////////
            //////// bytes len item (1 len = 4 bytes)
            //////byte[] _byteLenData = null;
            //////List<byte> li = new List<byte>(2000000);
            //////for (int k = 0; k < 2000000; k++)
            //////    //li.Add(0);
            //////    //li.Add(new Random().Next(k, 2000000 * 2 - k));
            //////    li.Add(255);

            //////using (var ms = new MemoryStream())
            //////{
            //////    ProtoBuf.Serializer.Serialize<List<byte>>(ms, li);
            //////    _byteLenData = ms.ToArray();
            //////}
            //////var _byteLen = BitConverter.GetBytes(_byteLenData.Length);
            //////byte[] b111 = System.Core.BinarySerializer.Serialize(li);


            //////DynamicProperty[] at = new DynamicProperty[]
            //////{
            //////    new DynamicProperty("Id", typeof(int)),
            //////    new DynamicProperty("Level", typeof(long)),
            //////    new DynamicProperty("Name", typeof(string)),
            //////};
            //////Type typeItem = DynamicExpression.CreateClass(at);
            //////Type typeList = typeof(List<>).MakeGenericType(typeItem);

            //////var item = list[0];
            //////object obj = Activator.CreateInstance(typeItem);
            //////foreach (PropertyInfo pi in item.GetType().GetProperties())
            //////    try
            //////    {
            //////        obj.GetType().GetProperty(pi.Name).SetValue(obj, pi.GetValue(item, null), null);
            //////    }
            //////    catch { }

            //////var ls1 = (IList)Activator.CreateInstance(typeList);
            //////ls1.Add(obj);

            //////var ls2 = (IList)typeof(List<>)
            //////    .MakeGenericType(typeItem)
            //////    .GetConstructor(Type.EmptyTypes)
            //////    .Invoke(null);
            //////ls2.Add(obj);

            //////byte[] b1 = System.Core.BinarySerializer.Serialize(obj);
            //////byte[] b2 = System.Core.BinarySerializer.Serialize(typeList, ls2);
            //////string j1 = string.Join(" ", b1.Select(x => x.ToString()).ToArray());
            //////string j2 = string.Join(" ", b2.Select(x => x.ToString()).ToArray());

            //////object v1 = System.Core.BinarySerializer.Deserialize(typeItem, b1);
            //////object v2 = System.Core.BinarySerializer.Deserialize(typeList, b2);

        }

        static void test_002()
        {
            //////var ls = list.AsQueryable().Where("Id > 0");//.Cast<Test>().ToArray();

            //////ItemList item = new ItemList() { Name = "Albert", Data = ls };

            //////DynamicProperty[] props = new DynamicProperty[]
            //////{
            //////    new DynamicProperty("Name", typeof(string)),
            //////    new DynamicProperty("Data", typeof(Test[]))
            //////};
            //////Type type = DynamicExpression.CreateClass(props);
            ////////object obj = Activator.CreateInstance(type);
            ////////type.GetProperty("Name").SetValue(obj, "Albert", null);
            ////////type.GetProperty("Data").SetValue(obj, l0, null);
            ////////string json = JsonConvert.SerializeObject(obj, Formatting.Indented);

            //////byte[] buf;
            //////object vall;
            ////////////////using (var ms = new MemoryStream())
            ////////////////{
            ////////////////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            ////////////////    buf = ms.ToArray();
            ////////////////}
            ////////////////using (var ms = new MemoryStream(buf))
            ////////////////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);

            //////using (var ms = new MemoryStream())
            //////{
            //////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            //////    buf = ms.ToArray();
            //////}
            //////buf = SevenZip.Compression.LZMA.SevenZipHelper.Compress(buf);
            //////buf = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(buf);
            //////using (var ms = new MemoryStream(buf))
            //////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);

            //////int imax = 1000000;
            //////int[] a1m = new int[imax];
            //////for (int k = 0; k < imax; k++)
            //////    a1m[k] = new Random().Next(k, imax * 2 - k);

            ////////byte[] bi1 = System.Core.BinarySerializer.Serialize(a1m);
            //////byte[] bi2 = null;
            //////using (var ms = new MemoryStream())
            //////{
            //////    ProtoBuf.Serializer.Serialize(ms, a1m);
            //////    bi2 = ms.ToArray();
            //////}
            ////////byte[] bi11 = SevenZip.Compression.LZMA.SevenZipHelper.Compress(bi1);
            ////////byte[] bi22 = SevenZip.Compression.LZMA.SevenZipHelper.Compress(bi2);



        }

        static void test_001()
        {
            ////////var ls = list.AsQueryable().Where("Id > 0");//.Cast<Test>().ToArray();


            ////////ItemList item = new ItemList() { Name = "Albert", Data = ls };

            ////////DynamicProperty[] props = new DynamicProperty[]
            ////////{
            ////////    new DynamicProperty("Name", typeof(string)),
            ////////    new DynamicProperty("Data", typeof(Test[]))
            ////////};
            ////////Type type = DynamicExpression.CreateClass(props);
            //////////object obj = Activator.CreateInstance(type);
            //////////type.GetProperty("Name").SetValue(obj, "Albert", null);
            //////////type.GetProperty("Data").SetValue(obj, l0, null);
            //////////string json = JsonConvert.SerializeObject(obj, Formatting.Indented);

            ////////byte[] buf;
            ////////object vall;
            //////////////////using (var ms = new MemoryStream())
            //////////////////{
            //////////////////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            //////////////////    buf = ms.ToArray();
            //////////////////}
            //////////////////using (var ms = new MemoryStream(buf))
            //////////////////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);

            ////////using (var ms = new MemoryStream())
            ////////{
            ////////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            ////////    buf = ms.ToArray();
            ////////}
            ////////buf = SevenZip.Compression.LZMA.SevenZipHelper.Compress(buf);
            ////////buf = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(buf);
            ////////using (var ms = new MemoryStream(buf))
            ////////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);


            //////////////////using (var ms = new MemoryStream())
            //////////////////{
            //////////////////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            //////////////////    buf = ms.ToArray();
            //////////////////}
            //////////////////buf = Snappy.SnappyCodec.Compress(buf);
            //////////////////buf = Snappy.SnappyCodec.Uncompress(buf);
            //////////////////using (var ms = new MemoryStream(buf))
            //////////////////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);


            ////////////////////using (var ms = new MemoryStream())
            ////////////////////{
            ////////////////////    new JsonSerializer().Serialize(new BsonWriter(ms), item);
            ////////////////////    buf = ms.ToArray();
            ////////////////////}
            ////////////////////int compressedSize = new Snappy.Sharp.SnappyCompressor().MaxCompressedLength(buf.Length);
            ////////////////////byte[] bytes = new byte[compressedSize];
            ////////////////////int k = new Snappy.Sharp.SnappyCompressor().Compress(buf, 0, buf.Length, bytes);

            ////////////////////bytes = bytes.Take(k).ToArray();
            ////////////////////buf = new Snappy.Sharp.SnappyDecompressor().Decompress(bytes, 0, bytes.Length);
            ////////////////////using (var ms = new MemoryStream(buf))
            ////////////////////    vall = new JsonSerializer().Deserialize(new BsonReader(ms), type);

        }
    }

    [Serializable]
    public class DB
    {
        public IList L { set; get; }
    }

    [Serializable]
    [ProtoContract]
    public class TEST
    {
        [ProtoMember(1)]
        public int ID { set; get; }

        [ProtoMember(2)]
        public long LEVEL { set; get; }

        [ProtoMember(3)]
        public string NAME { set; get; }
    }

    public class ItemList
    {
        public string Name { set; get; }
        public object Data { set; get; }
    }

    [Serializable]
    public class USER
    {

        //[dbField(IsKeyAuto = true)]
        //public long KEY { set; get; }

        //[dbField(IsDuplicate = false)]
        public string USERNAME { set; get; }

        //[dbField(IsEncrypt = true)]
        public string PASSWORD { set; get; }
    }






    //[Serialiser(typeof(FieldCollectionSerialiser))]
    public class FieldCollection : IList
    {
        public object this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsFixedSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSynchronized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }







}
