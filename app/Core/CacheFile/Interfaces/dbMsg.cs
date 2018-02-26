using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace app.Core
{

    [Serializable]
    public class dbMsg
    {
        public dbMsg(Type typeDB)
        {
            Name = typeDB.Name;
        }

        public string Name { set; get; }

        public dbAction Action { set; get; }

        public object Data { set; get; }

        public dbMsg GoPage(int pageNumber)
        {
            if (Action == dbAction.DB_SELECT)
            {
                SearchRequest sr = (SearchRequest)this.Data;
                sr.PageNumber = pageNumber;
                this.Data = sr;
            }
            return this;
        }

        public object Post()
        {
            int port = 0;
            MemoryMappedFile map = MemoryMappedFile.Create(MapProtection.PageReadWrite, 4, this.Name);
            byte[] buf = new byte[4];
            using (Stream view = map.MapView(MapAccess.FileMapRead, 0, 4))
                view.Read(buf, 0, 4);
            port = BitConverter.ToInt32(buf, 0);

            return Post("http://127.0.0.1:" + port.ToString());
        }

        public object Post(int PortHttp)
        {
            return Post("http://127.0.0.1:" + PortHttp.ToString());
        }

        public object Post(string url)
        {
            object val = null;
            using (var client = new WebClient() { BaseAddress = url })
            {
                byte[] buf;
                BinaryFormatter formatter = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, this);
                    buf = ms.ToArray();
                }
                buf = client.UploadData("/", buf);
                using (var ms = new MemoryStream(buf))
                    val = formatter.Deserialize(ms);
            }
            return val;
        }
    }
}
