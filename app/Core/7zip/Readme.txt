http://www.7-zip.org/sdk.html

http://www.nullskull.com/a/768/7zip-lzma-inmemory-compression-with-c.aspx

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using SevenZip;
using System.Core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;
using System.Collections;

namespace ConsoleApplication1
{
    [Serializable]
    public class Test
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public long Level { set; get; }
    }
    
    public class ItemList
    {
        public string Name { set; get; }
        public object Data { set; get; }
    }

    class Program
    { 
        static void Main(string[] args)
        {
            var l0 = new List<Test>() {
                new Test() { Id = 1, Level = 10, Name = "Nguyễn Văn Thịnh" },
                new Test() { Id = 2, Level = 20, Name = "Nguyễn Văn Trường" },
                new Test() { Id = 3, Level = 30, Name = "Nguyễn Văn Tuấn" },
                new Test() { Id = 4, Level = 40, Name = "Nguyễn Văn Việt" },
            };


            var ls = l0.AsQueryable().Where("Id > 0");//.Cast<Test>().ToArray();
              

            ItemList item = new ItemList() { Name = "Albert", Data = ls };

            DynamicProperty[] props = new DynamicProperty[]
            {
                new DynamicProperty("Name", typeof(string)),
                new DynamicProperty("Data", typeof(Test[]))
            };

            Type type = DynamicExpression.CreateClass(props);
            //object obj = Activator.CreateInstance(type);
            //type.GetProperty("Name").SetValue(obj, "Albert", null);
            //type.GetProperty("Data").SetValue(obj, l0, null);
            //string json = JsonConvert.SerializeObject(obj, Formatting.Indented);

            byte[] buf;
            object vall;
            using (var ms = new MemoryStream())
            {
                var serializer = new JsonSerializer();
                var writer = new BsonWriter(ms);
                serializer.Serialize(writer, item);
                buf = ms.ToArray();
            }

            using (var ms = new MemoryStream(buf))
            {
                var serializer = new JsonSerializer();
                //Reset the stream back to the beginning
                //stream.Seek(0, SeekOrigin.Begin);

                //Create the BSON Reader and Deserialize
                var reader = new BsonReader(ms);
                vall = serializer.Deserialize(reader, type);
            }

        }
    }
}

