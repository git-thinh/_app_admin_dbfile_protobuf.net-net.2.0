
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using app.Model;

namespace app.Core
{
    public class DataHost : IDataFile
    {
        private readonly MemoryMappedFile m_mapPort;
        private readonly ConcurrentDictionary<string, DataFile> m_dataFile;
        private readonly string m_PathData;
        private List<string> m_listDataName;
        private List<MENU> m_listMenu;

        public bool Open = false;
        public delegate void EventOpen(string[] fs);
        public EventOpen OnOpen;

        public DataHost()
        {
            m_listMenu = new List<MENU>();
            m_mapPort = MemoryMappedFile.Create(MapProtection.PageReadWrite, 4, "datafile");
            m_listDataName = new List<string>();
            m_dataFile = new ConcurrentDictionary<string, DataFile>();

            m_PathData = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "Data").Replace(@"file:\", string.Empty);
            if (!Directory.Exists(m_PathData)) Directory.CreateDirectory(m_PathData);
        }

        #region [ === START - CLOSE === ]

        public void Start()
        {
            string dbUSER = typeof(USER).Name;
            DataFile dfUSER = null;

            string[] fs = Directory.GetFiles(m_PathData, "*.df");
            if (fs.Length > 0)
            {
                for (int k = 0; k < fs.Length; k++)
                {
                    DataFile df = new DataFile(fs[k]);
                    if (df.Opened)
                    {
                        if (df.Model.NAME == dbUSER) dfUSER = df;

                        string name = df.Model.NAME;
                        if (m_dataFile.TryAdd(name, df))
                        {
                            m_listMenu.Add(new MENU(df.Model));
                            m_listDataName.Add(name);
                        }
                    }
                }
            }

            modelBinding();

            ////////if (m_listDataName.IndexOf(typeof(CNDATA).Name) == -1)
            ////////{
            ////////    var df = DataFile.Open(typeof(CNDATA));
            ////////    if (df.Opened)
            ////////        if (m_dataFile.TryAdd(df.Model.Name, df))
            ////////            m_listDataName.Add(df.Model.Name);
            ////////}

            ////////if (m_listDataName.IndexOf(typeof(CNSPLIT).Name) == -1)
            ////////{
            ////////    var df = DataFile.Open(typeof(CNSPLIT));
            ////////    if (df.Opened)
            ////////        if (m_dataFile.TryAdd(df.Model.Name, df))
            ////////            m_listDataName.Add(df.Model.Name);
            ////////}

            ////////if (dfUSER == null)
            ////////{
            ////////    dfUSER = DataFile.Open(typeof(USER));
            ////////    if (dfUSER.Opened)
            ////////    {
            ////////        if (m_dataFile.TryAdd(dbUSER, dfUSER))
            ////////            m_listDataName.Add(dbUSER);
            ////////    }
            ////////}

            ////////if (dfUSER != null && dfUSER.Count == 0)
            ////////{
            ////////    var ra = dfUSER.AddItem(new USER() { FULLNAME = "Admin", PASSWORD = "12345", USERNAME = "admin" }.ToDictionary());
            ////////    if (ra != EditStatus.SUCCESS)
            ////////    {
            ////////        if (OnOpen != null) OnOpen(m_listDataName.ToArray());
            ////////        return;
            ////////    }
            ////////}

            Open = true;
            if (OnOpen != null) OnOpen(m_listDataName.ToArray());
        }

        private void registryModel(DB_MODEL m)
        {
            var df = new DataFile(m);
            if (df.Opened)
            {
                if (m_dataFile.TryAdd(m.NAME, df))
                    m_listDataName.Add(m.NAME);
            }
        }

        private void modelBinding()
        {
            string _dbModel = typeof(DB_MODEL).Name;

            var ms = Assembly.GetExecutingAssembly().GetTypes().Cast<Type>()
                .Where(x => x.Namespace == "app.Model"
                    && !m_listDataName.Any(o => o == x.Name))
                .ToArray();
            foreach (var info in ms)
            {
                //System.Reflection.MemberInfo info = typeof(MyClass);
                object[] _mcs = info.GetCustomAttributes(true).Where(x => x.GetType().Name == _dbModel).ToArray();
                if (_mcs.Length > 0)
                {
                    DB_MODEL mo = (DB_MODEL)_mcs[0];
                    mo.NAME = info.Name;
                    mo.FIELDS = new DB_MODEL(info).FIELDS;

                    registryModel(mo);

                    int index = m_listMenu.FindIndex(x => x.NAME == mo.NAME);
                    if (index == -1) m_listMenu.Add(new MENU(mo));
                }
            }
        }

        public object[] UpdateOrAdd(object[] items, bool convertToDynamic = true)
        {
            if (items == null || items.Length == 0) return null;
            string dbName = items[0].GetType().Name;

            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.AddItems(items, convertToDynamic);
            return null;
        }

        public void Close()
        {
            m_mapPort.Close();
            foreach (var db in m_dataFile.Values)
                db.Close();
        }

        #endregion

        #region [ === HTTP === ]

        public int Port = 0;
        private HttpListener listener;
        private void http_Init()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            Port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();

            using (Stream view = m_mapPort.MapView(MapAccess.FileMapWrite, 0, 4))
                view.Write(BitConverter.GetBytes(Port), 0, 4);

            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + Port.ToString() + "/");

            listener.Start();
            listener.BeginGetContext(ProcessRequest, listener);
        }

        private void ProcessRequest(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);

            string method = context.Request.HttpMethod;
            string path = context.Request.Url.LocalPath;
            switch (method)
            {
                case "POST":
                    #region [ === POST === ]
                    //try
                    //{
                    BinaryFormatter formatter = new BinaryFormatter();
                    dbMsg m = formatter.Deserialize(context.Request.InputStream) as dbMsg;
                    if (m != null)
                    {
                        switch (m.Action)
                        {
                            case dbAction.DB_SELECT:
                                SearchRequest sr = (SearchRequest)m.Data;
                                SearchResult val = Search(m.Name, sr);
                                if (val != null)
                                    formatter.Serialize(context.Response.OutputStream, val);
                                break;
                        }
                    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    //Serializer.NonGeneric.Serialize(context.Response.OutputStream, 500);
                    //    //context.Response.Close();
                    //}

                    context.Response.Close();
                    //context.Response.Abort();

                    break;
                    #endregion
                case "GET":
                    #region [ === GET === ]

                    switch (path)
                    {
                        case "/favicon.ico":
                            context.Response.Close();
                            return;
                        case "/ping":
                            byte[] buffer = Encoding.UTF8.GetBytes("OK");
                            context.Response.ContentLength64 = buffer.Length;
                            Stream output = context.Response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                            break;
                    }

                    break;
                    #endregion
            }

            listener.BeginGetContext(ProcessRequest, listener);
        }

        #endregion

        #region [ === IDATAFILE === ]

        private DataFile GetDF(string dbName)
        {
            //string predicate, params object[] values
            DataFile db = null;
            m_dataFile.TryGetValue(dbName, out db);
            return db;
        }

        public SearchResult Search(string dbName, SearchRequest sr)
        {
            SearchResult val = null;
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                val = db.SearchGetIDs(sr);
            return val;
        }

        public string[] GetListDB()
        {
            return m_listDataName.ToArray();
        }

        public LOGIN_STATUS Login(string user, string pass)
        {
            LOGIN_STATUS ok = LOGIN_STATUS.USERNAME_PASS_WRONG;
            DataFile db = GetDF(typeof(USER).Name);
            if (db != null && db.Opened)
                if (db.Count == 0)
                    ok = LOGIN_STATUS.MODEL_USER_IS_EMPTY;
                else
                {
                    bool v = db.Exist("Username == @0 && Password == @1", user, pass);
                    if (v) ok = LOGIN_STATUS.LOGIN_SUCCESSFULLY;
                }
            return ok;
        }

        public FieldInfo[] GetFields(string dbName)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.Model.FIELDS;
            return new FieldInfo[] { };
        }

        public InfoSelectTop GetInfoSelectTop(string dbName, int selectTop)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.GetInfoSelectTop(selectTop);
            return null;
        }
        public IList GetSelectPage(string dbName, int pageNumber, int pageSize)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.GetSelectPage(pageNumber, pageSize);
            return null;
        }

        public bool ExistModel(string dbName)
        {
            dbName = dbName.ToUpper();
            return m_listDataName.FindIndex(x => x == dbName) != -1;
        }

        public bool CreateDb(DB_MODEL model)
        {
            DataFile df = new DataFile(model);
            if (df.Opened)
            {
                string name = df.Model.NAME;
                if (m_dataFile.TryAdd(name, df))
                    m_listDataName.Add(name);
                return true;
            }
            return false;
        }

        public DB_MODEL GetModel(string dbName)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.Model;
            return null;
        }

        public Type GetTypeDynamic(string dbName)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.TypeDynamic;
            return null;
        }

        public object AddItem(string dbName, Dictionary<string, object> data)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.AddItem(data);
            return null;
        }

        public IList GetComboboxItem(FieldInfo field)
        {
            DataFile db = GetDF(field.JOIN_MODEL);
            if (db != null && db.Opened)
                return db.GetComboboxItem(field);
            return null;
        }

        public bool ModelUpdate(DB_MODEL model, bool hasRemoveField)
        {
            DataFile db = GetDF(model.NAME);
            if (db != null && db.Opened)
            {
                //int kAdd = model.Fields.Where(x => x.FieldChange == dbFieldChange.ADD).Count();
                //int kRemove = model.Fields.Where(x => x.FieldChange == dbFieldChange.REMOVE).Count();
                //if (kAdd > 0 && kRemove > 0) return false;

                return db.UpdateModel(model, hasRemoveField);
            }
            return false;
        }

        public EditStatus ModelTruncate(string dbName)
        {
            DataFile db = GetDF(dbName);
            if (db != null && db.Opened)
                return db.Truncate();
            return EditStatus.NONE;
        }

        public object AddItem(object item)
        {
            DataFile df;
            Type type = item.GetType();
            string name = type.Name;
            if (m_listDataName.IndexOf(name) == -1)
            {
                df = DataFile.Open(type);
                if (df.Opened)
                {
                    if (m_dataFile.TryAdd(name, df))
                        m_listDataName.Add(name);
                }
            }
            else
                df = GetDF(name);
            if (df != null && df.Opened)
                return df.AddItem(item.ToDictionary());
            return EditStatus.FAIL_ITEM_NOT_EXIST;
        }

        public IndexDynamic FindItemFirstByContainFieldValue(object item, string fields)
        {
            object val = null;
            int index = -1;
            string name = item.GetType().Name;
            string[] a = fields.Split(',').Select(x => x.Trim().ToUpper()).ToArray();
            if (a.Length > 0 && m_listDataName.IndexOf(name) != -1)
            {
                string[] fd = item.GetType().GetProperties().Select(x => new { Name = x.Name, Char = (x.PropertyType.Name == "String" ? "\"" : string.Empty), Value = x.GetGetMethod().Invoke(item, null) })
                 .Where(x => a.Any(o => o == x.Name) && x.Value != null && x.Value.ToString() != string.Empty)
                 .Select((x, k) => x.Name + " = @" + k.ToString())
                 .ToArray();
                object[] vd = item.GetType().GetProperties().Where(x => a.Any(o => o == x.Name)).Select(x => x.GetGetMethod().Invoke(item, null))
                 .Where(x => x != null && x.ToString() != string.Empty)
                 .ToArray();
                DataFile db = GetDF(name);
                if (db != null && db.Opened)
                {
                    SearchResult rs = db.SearchGetIDs(new SearchRequest(1, 1, string.Join(" && ", fd), vd));
                    if (rs.Status && rs.Total > 0)
                    {
                        index = rs.IDs[0];
                        var li = (IList)rs.Message;
                        if (li.Count > 0) val = li[0];
                    }
                }
            }
            return new IndexDynamic(index, val);
        }

        public SearchResult FindItemByContainFieldValue(object item, string fields, int pageSize, int pageNumber)
        {
            SearchResult val = null;
            string name = item.GetType().Name;
            string[] a = fields.Split(',').Select(x => x.Trim().ToUpper()).ToArray();
            if (a.Length > 0 && m_listDataName.IndexOf(name) != -1)
            {
                string[] fd = item.GetType().GetProperties().Select(x => new { Name = x.Name, Char = (x.PropertyType.Name == "String" ? "\"" : string.Empty), Value = x.GetGetMethod().Invoke(item, null) })
                 .Where(x => a.Any(o => o == x.Name) && x.Value != null && x.Value.ToString() != string.Empty)
                 .Select((x, k) => x.Name + " = @" + k.ToString())
                 .ToArray();
                object[] vd = item.GetType().GetProperties().Where(x => a.Any(o => o == x.Name)).Select(x => x.GetGetMethod().Invoke(item, null))
                 .Where(x => x != null && x.ToString() != string.Empty)
                 .ToArray();
                DataFile db = GetDF(name);
                if (db != null && db.Opened)
                    val = db.SearchGetIDs(new SearchRequest(string.Join(" && ", fd), vd) { PageSize = pageSize, PageNumber = pageNumber });
            }
            return val;
        }

        public bool RemoveItem(object item, string fields)
        {
            string name = item.GetType().Name;
            string[] a = fields.Split(',').Select(x => x.Trim().ToUpper()).ToArray();
            if (a.Length > 0 && m_listDataName.IndexOf(name) != -1)
            {
                string[] fd = item.GetType().GetProperties().Select(x => new { Name = x.Name, Char = (x.PropertyType.Name == "String" ? "\"" : string.Empty), Value = x.GetGetMethod().Invoke(item, null) })
                 .Where(x => a.Any(o => o == x.Name) && x.Value != null && x.Value.ToString() != string.Empty)
                 .Select((x, k) => x.Name + " = @" + k.ToString())
                 .ToArray();
                object[] vd = item.GetType().GetProperties().Where(x => a.Any(o => o == x.Name)).Select(x => x.GetGetMethod().Invoke(item, null))
                 .Where(x => x != null && x.ToString() != string.Empty)
                 .ToArray();
                DataFile db = GetDF(name);
                if (db != null && db.Opened)
                {
                    SearchResult rs = db.SearchGetIDs(new SearchRequest(1, 1, string.Join(" && ", fd), vd));
                    if (rs.Status && rs.Total > 0)
                        return db.RemoveBySearh(new SearchRequest(string.Join(" && ", fd), vd));
                }
            }
            return false;
        }

        public bool RemoveItemByKeyFieldSyncEdit(string model, object key)
        {
            DataFile db = GetDF(model);
            if (db != null && db.Opened) 
                return db.RemoveByFieldSyncEdit(key); 
            return false;
        }

        public bool ExistItemDynamic(string model, object item)
        {
            DataFile db = GetDF(model);
            if (db != null && db.Opened)
                return db.ExistItemDynamic(item);
            return false;
        }

        public bool RemoveItemDynamic(string model, object item)
        {
            DataFile db = GetDF(model);
            if (db != null && db.Opened)
                return db.RemoveItemDynamic(item);
            return false;
        }

        #endregion


        #region [ === MENU === ]

        public string[] menu_GetTAG()
        {
            string[] a = new string[] { };
            lock (m_listMenu)
                a = m_listMenu.Select(x => x.TAG).Distinct().ToArray();
            return a;
        }

        public MENU[] menu_Find(Func<MENU, bool> condition)
        {
            MENU[] a = new MENU[] { };
            lock (m_listMenu)
                a = m_listMenu.Where(condition).ToArray();
            return a;
        }

        #endregion

    }

}
