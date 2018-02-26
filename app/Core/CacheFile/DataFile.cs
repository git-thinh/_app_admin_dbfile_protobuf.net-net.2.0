using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using app.Model;

namespace app.Core
{
    public class DataFile
    {
        const string _NotOpened = "File not open";

        #region [ === VARIABLE === ]

        // HEADER = [8 byte id] + [4 bytes capacity] + [4 bytes count] + [984 byte fields] = 1,000
        const int m_HeaderSize = 1000;
        private long m_FileID = 0;
        private int m_FileSize = 0;
        private MemoryMappedFile m_mapFile;
        private readonly int m_BlobGrowSize = 1000;
        private const int m_BlobSizeMax = 255;
        private int m_BlobLEN = 0;
        private string m_FileName = string.Empty;
        private string m_FilePath = string.Empty;

        public Type TypeDynamic = null;

        private IList m_listItems;
        private MemoryMappedFile m_mapPort;
        private int m_Count = 0;
        private int m_Capacity = 0;

        private const int initialCapacity = 101;
        private readonly DataFileBuffer m_buffer;
        private readonly DataFileSearch m_search;
        private readonly DataFileIndex m_index;
        private readonly object _lockWriteFile = new object();
        private readonly object _lockListItem = new object();

        private string Field_SyncEdit = "";
        ///////////////////////////////////////////////

        #endregion

        #region [ === MEMBER === ]

        public DB_MODEL Model { private set; get; }
        public bool Opened = false;
        public int Count
        {
            get { return m_Count; }
        }

        #endregion

        #region [ === OPEN - CLOSE === ]

        public void Close()
        {
            if (m_mapFile != null) m_mapFile.Close();
            if (m_mapPort != null) m_mapPort.Close();
            if (listener != null) listener.Abort();
        }

        public static DataFile Open(Type _typeModel)
        {
            return new DataFile(new DB_MODEL(_typeModel));
        }

        public DataFile()
        {
            Opened = false;
            m_buffer = new DataFileBuffer();
            m_search = new DataFileSearch();
            m_index = new DataFileIndex();
        }

        public DataFile(DB_MODEL model)
            : this()
        {
            Model = model;

            m_mapPort = MemoryMappedFile.Create(MapProtection.PageReadWrite, 4, Model.NAME);
            m_FileName = Model.NAME + ".df";
            string m_PathData = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");
            if (!Directory.Exists(m_PathData)) Directory.CreateDirectory(m_PathData);
            m_FilePath = Path.Combine(m_PathData, m_FileName);

            if (OpenOrCreateFile())
            {
                init_Component();
                Opened = true;

                if (m_listItems == null)
                    m_listItems = (IList)typeof(List<>).MakeGenericType(TypeDynamic).GetConstructor(Type.EmptyTypes).Invoke(null);
            }
            else Close();
        }

        public DataFile(string pathFile)
            : this()
        {
            m_FilePath = pathFile;

            if (OpenOrCreateFile())
            {
                init_Component();
                Opened = true;

                if (m_listItems == null)
                    m_listItems = (IList)typeof(List<>).MakeGenericType(TypeDynamic).GetConstructor(Type.EmptyTypes).Invoke(null);
            }
            else Close();
        }

        private void init_Component()
        {
            if (Model.NAME == "CNSPLIT")
                Field_SyncEdit = "";
            Field_SyncEdit = Model.FIELDS.Where(x => x.IS_KEY_SYNC_EDIT).Select(x => x.NAME).Take(1).SingleOrDefault();
            http_Init();
        }

        private bool OpenOrCreateFile()
        {
            try
            {
                if (File.Exists(m_FilePath))
                {
                    /////////////////////////////////////////////////
                    // OPEN m_FilePath

                    FileInfo fi = new FileInfo(m_FilePath);
                    m_FileSize = (int)fi.Length;
                    m_mapFile = MemoryMappedFile.Create(m_FilePath, MapProtection.PageReadWrite, m_FileSize);
                    if (m_FileSize > m_HeaderSize)
                    {
                        byte[] buf = new byte[m_FileSize];
                        using (Stream view = m_mapFile.MapView(MapAccess.FileMapRead, 0, m_FileSize))
                            view.Read(buf, 0, m_FileSize);

                        bool val = bindHeader(buf);
                        if (val)
                        {
                            m_mapPort = MemoryMappedFile.Create(MapProtection.PageReadWrite, 4, Model.NAME);
                            m_FileName = Model.NAME + ".df";
                            return true;
                        }
                    }
                }
                else
                {
                    /////////////////////////////////////////////////
                    // CREATE NEW m_FilePath

                    m_Capacity = m_BlobGrowSize;
                    m_FileSize = m_HeaderSize + (m_Capacity * m_BlobSizeMax) + 1;
                    m_mapFile = MemoryMappedFile.Create(m_FilePath, MapProtection.PageReadWrite, m_FileSize);

                    //string fs = _model.Name + ";" + string.Join(",", _model.Fields.Select(x => ((int)x.Type).ToString() + x.Name).ToArray());
                    writeHeaderBlank();

                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        #endregion

        #region [ === SEARCH === ]

        public IList GetComboboxItem(FieldInfo field)
        {
            //Func<object, string> func = delegate(object item)
            //{
            //    return item.ToString();
            //};
            IList lo = null;
            //string wh = string.Format(@"new @out ({0} as Value, @0({1}) as Text)", field.JoinField, field.JoinView);
            string wh = string.Format(@"new ({0}, {1})", field.JOIN_FIELD, field.JOIN_VIEW);
            //try
            //{
            lock (_lockListItem)
                lo = m_listItems.AsQueryable().Select(wh).ToListDynamicClass();
            //}
            //catch (Exception e1)
            //{
            //}
            return lo;
        }

        public IList GetSelectPage(int pageNumber, int pageSize)
        {
            IQueryable lo = null;
            lock (_lockListItem)
                lo = m_listItems.AsQueryable().Skip((pageNumber - 1) * pageSize).Take(pageSize);

            IList returnList = (IList)typeof(List<>)
                .MakeGenericType(TypeDynamic)
                .GetConstructor(Type.EmptyTypes)
                .Invoke(null);
            if (lo != null)
                foreach (var elem in lo)
                    returnList.Add(elem);

            return returnList;
        }

        public InfoSelectTop GetInfoSelectTop(int selectTop)
        {
            IQueryable lo = null;
            lock (_lockListItem)
                lo = m_listItems.AsQueryable().Take(selectTop);

            IList returnList = (IList)typeof(List<>)
                .MakeGenericType(TypeDynamic)
                .GetConstructor(Type.EmptyTypes)
                .Invoke(null);
            if (lo != null)
                foreach (var elem in lo)
                    returnList.Add(elem);

            return new InfoSelectTop()
            {
                PortHTTP = Port,
                TotalRecord = m_Count,
                DataSelectTop = returnList,
                Fields = Model.FIELDS,
            };
        }

        public bool Exist(object item)
        {
            bool val = false;
            object it = convertToDynamicObject(item, false);
            if (it != null)
            {
                lock (_lockListItem)
                {
                    int index = m_listItems.IndexOf(it);
                    if (index != -1) val = true;
                }
            }
            return val;
        }

        public bool ExistItemDynamic(object item)
        {
            bool val = false;
            if (item != null)
            {
                lock (_lockListItem)
                {
                    int index = m_listItems.IndexOf(item);
                    if (index != -1) val = true;
                }
            }
            return val;
        }
        public bool Exist(string predicate, params object[] values)
        {
            bool val = false;
            int k = 0;
            lock (_lockListItem)
            {
                if (values == null)
                    k = m_listItems.AsQueryable().Where(predicate).Count();
                else
                    k = m_listItems.AsQueryable().Where(predicate, values).Count();
            }
            if (k > 0) val = true;
            return val;
        }

        public SearchResult SearchGetIDs(SearchRequest search)
        {
            SearchResult sr = m_search.Get(search);

            if (sr == null)
            {
                lock (_lockListItem)
                    sr = m_search.SearchGetIDs_(m_listItems, search);

                sr.FieldSyncEdit = Field_SyncEdit;
                m_search.Add(search, sr);
                sr.IsCache = false;
            }
            else
                sr.IsCache = true;

            if (sr.Status)
            {
                sr.FieldSyncEdit = Field_SyncEdit;

                IList lo = (IList)typeof(List<>).MakeGenericType(TypeDynamic).GetConstructor(Type.EmptyTypes).Invoke(null);
                int[] a = sr.IDs.Page(search.PageNumber, search.PageSize).ToArray();
                if (a.Length > 0)
                    lock (_lockListItem)
                        for (int k = 0; k < a.Length; k++)
                            if (m_listItems.Count > a[k])
                                lo.Add(m_listItems[a[k]]);
                if (lo.Count > sr.Total) sr.Total = lo.Count;
                sr.Message = lo;// JsonConvert.SerializeObject(lo);
            }

            sr.PageNumber = search.PageNumber;
            sr.PageSize = search.PageSize;
            return sr.Clone();
        }

        //private SearchResult SearchGetIDs_(SearchRequest search)
        //{
        //    if (Opened == false) return null; ;
        //    bool ok = false;
        //    int[] ids = new int[] { };
        //    string text = string.Empty;
        //    if (Opened == false)
        //        text = _NotOpened;
        //    else
        //    {
        //        try
        //        {
        //            IQueryable lo = null;
        //            lock (_lockListItem)
        //            {
        //                if (search.Values == null)
        //                    lo = m_listItems.AsQueryable().Where(search.Predicate);
        //                else
        //                    lo = m_listItems.AsQueryable().Where(search.Predicate, search.Values); ;
        //            }

        //            if (lo != null)
        //            {
        //                List<int> li = new List<int>();
        //                foreach (var o in lo)
        //                    li.Add(m_listItems.IndexOf(o));
        //                ids = li.ToArray();
        //            }

        //            ok = true;
        //        }
        //        catch (Exception ex) { text = ex.Message; }
        //    }
        //    return new SearchResult(ok, ids.Length, ids, text);
        //}

        //private void SearchExecuteUpdateCache(ItemEditType type, int[] ids = null)
        //{
        //    //new Thread(new ParameterizedThreadStart((obj) =>
        //    //{
        //    //Tuple<ItemEditType, int[]> tu = (Tuple<ItemEditType, int[]>)obj;
        //    //using (m_lockCache.WriteLock())
        //    //{
        //    SearchRequest[] keys = m_cacheSR.Keys.ToArray();
        //    foreach (var ki in keys)
        //    {
        //        SearchResult sr = null;
        //        switch (type)
        //        {
        //            case ItemEditType.ADD_NEW_ITEM:
        //                sr = SearchGetIDs_(ki);
        //                break;
        //            case ItemEditType.REMOVE_ITEM:
        //                sr = m_cacheSR[ki];
        //                //int[] idr = tu.Item2;
        //                if (sr.IDs != null && sr.IDs.Length > 0 && ids != null && ids.Length > 0)
        //                {
        //                    int[] val = sr.IDs.Where(x => !ids.Any(o => o == x)).ToArray();
        //                    sr.IDs = val;
        //                    sr.Total = val.Length;
        //                }
        //                break;
        //        }
        //        m_cacheSR[ki] = sr;
        //    }
        //    //}
        //    //})).Start(new Tuple<ItemEditType, int[]>(type, ids));
        //}

        #endregion

        #region [ === ADD - UPDATE - REMOVE - TRUNCATE === ]

        public bool UpdateModel(DB_MODEL m, bool hasRemoveField)
        {
            if (!Opened || m == null || m.FIELDS == null || m.FIELDS.Length == 0) return false;

            List<byte> _byteFields = new List<byte>();
            //////////////////////////////////////////////////////
            //////// HEADER = [4 bytes blob len] + [8 byte ID] + [4 bytes Capacity] + [4 bytes Count] + [980 byte fields] = 1,000
            #region [ === MODEL 980 byte Model, Fields === ]

            byte[] bm;
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize<DB_MODEL>(ms, m);
                bm = ms.ToArray();
            }
            _byteFields.AddRange(BitConverter.GetBytes(bm.Length));
            _byteFields.AddRange(bm);
            if (_byteFields.Count > 980)
                return false;
            _byteFields.AddRange(new byte[980 - _byteFields.Count]);

            #endregion

            ////////////////////////////////////////////////
            int kAdd = m.FIELDS.Where(x => x.FieldChange == dbFieldChange.ADD).Count();
            //int kRemove = m.Fields.Where(x => x.FieldChange == dbFieldChange.REMOVE).Count();
            if (kAdd == 0 && hasRemoveField == false)
            {
                #region [ === UPDATE CONFIG FIELDS === ]
                try
                {
                    lock (_lockWriteFile)
                    {
                        using (Stream view = m_mapFile.MapView(MapAccess.FileMapWrite, 0, m_HeaderSize))
                        {
                            view.Seek(20, SeekOrigin.Begin);
                            view.Write(_byteFields.ToArray(), 0, _byteFields.Count);
                        }
                    }

                    Model = m;
                    TypeDynamic = buildTypeDynamic(m);
                }
                catch
                {
                    return false;
                }
                #endregion
            }
            else
            {
                #region [ === ALTER ADD NEW OR REMOVE FIELDS === ]
                try
                {
                    //dbModel mComit = m_buffer.CloneByBinaryFormatter(m) as dbModel;
                    //dbField[] fieldComit = mComit.Fields.Where(x => x.FieldChange != dbFieldChange.REMOVE).ToArray();
                    //mComit.Fields = fieldComit;

                    //m.Fields = m.Fields.Where(x => x.FieldChange != dbFieldChange.REMOVE).ToArray();
                    Type _typeDynamic = buildTypeDynamic(m);
                    Type _typeList = typeof(List<>).MakeGenericType(_typeDynamic);

                    string[] colSelect = m.FIELDS
                        .Where(x => x.FieldChange != dbFieldChange.ADD && x.FieldChange != dbFieldChange.REMOVE)
                        .Select(x => x.NAME + " as " + x.NAME).ToArray();
                    string selector = "new @out (" + string.Join(",", colSelect) + ")";
                    var list = m_listItems.AsQueryable().Select(_typeDynamic, selector);
                    //var l1 = list.ToListDynamicClass();

                    IList listNEW = (IList)_typeList.GetConstructor(Type.EmptyTypes).Invoke(null);
                    foreach (var it in list)
                        listNEW.Add(it);

                    ///////////////////////////////////////////////////////
                    // BYTES DATA BODY
                    List<byte> lsByte = new List<byte>();
                    byte[] blen = m_buffer.Serialize(m_Count, typeof(int));
                    int lenHeadData = blen.Length + 1; // 0 + bytes
                    if (lenHeadData < 4)
                        lsByte.AddRange(new byte[4 - lenHeadData]);

                    byte[] byteData = m_buffer.Serialize(listNEW, _typeList);
                    lsByte.AddRange(byteData);

                    ///////////////////////////////////////////////////////
                    // [4 bytes blob LEN]
                    int _blobLEN = lsByte.Count;
                    byte[] _byteBlobLEN = BitConverter.GetBytes(_blobLEN);

                    ///////////////////////////////////////////////////////
                    // WRITE BYTES
                    lock (_lockWriteFile)
                    {
                        using (Stream view = m_mapFile.MapView(MapAccess.FileMapWrite, 0, m_FileSize))
                        {
                            //view.Seek(0, SeekOrigin.Begin);
                            view.Write(_byteBlobLEN, 0, 4);

                            view.Seek(20, SeekOrigin.Begin);
                            view.Write(_byteFields.ToArray(), 0, _byteFields.Count);

                            //view.Seek(m_HeaderSize, SeekOrigin.Begin);
                            view.Write(lsByte.ToArray(), 0, lsByte.Count);
                        }
                    }

                    ///////////////////////////////////////////////////////
                    // RESET CACHE 
                    lock (_lockListItem)
                    {
                        m_listItems.Clear();
                        m_listItems = listNEW;
                    }

                    m_BlobLEN = _blobLEN;
                    Model = m;
                    TypeDynamic = _typeDynamic;
                }
                catch //(Exception e1)
                {
                    return false;
                }
                #endregion
            }
            return true;
        }

        public object AddItem(Dictionary<string, object> data)
        {
            object obj = Activator.CreateInstance(TypeDynamic);
            try
            {
                foreach (KeyValuePair<string, object> kv in data)
                {
                    var po = TypeDynamic.GetProperty(kv.Key);
                    if (po != null)
                    {
                        var rs = dbType.Convert(po.PropertyType.Name, kv.Value);
                        if (rs.OK)
                            //po.SetValue(obj, kv.Value, null);
                            //po.SetValue(obj, new DateTime(1879, 3, 14), null);
                            po.SetValue(obj, rs.Value, null);
                        else
                            return EditStatus.FAIL_EXCEPTION_CONVERT_TO_DYNAMIC_OBJECT;
                    }
                }

                var fields = Model.FIELDS.Where(x => x.IS_KEY_AUTO).ToArray();
                foreach (IFieldInfo fi in fields)
                {
                    object val = getKeyAuto(fi.TYPE_NAME, obj);
                    if (val == null)
                        return EditStatus.FAIL_EXCEPTION_CONVERT_TO_DYNAMIC_OBJECT;
                    else
                    {
                        if (data.ContainsKey(fi.NAME))
                            data[fi.NAME] = val;
                        else
                            data.Add(fi.NAME, val);
                    }
                    TypeDynamic.GetProperty(fi.NAME).SetValue(obj, val, null);
                }

                var fieldBeforeUpdate = Model.FIELDS.Where(x => !string.IsNullOrEmpty(x.FUNC_BEFORE_UPDATE)).ToArray();
                var fieldBeforeUpdateData = Model.FIELDS.Where(x => x.ORDER_KEY_URL > 0).ToArray();
                if (fieldBeforeUpdate.Length > 0 && fieldBeforeUpdateData.Length > 0)
                {
                    for (int k = 0; k < fieldBeforeUpdate.Length; k++)
                    {
                        FieldInfo fo = fieldBeforeUpdate[k];
                        object val = null;
                        val = dbFunc.ExeFuncBeforeAddOrUpdate(fo.FUNC_BEFORE_UPDATE, Model, fo.NAME, data, val);
                        TypeDynamic.GetProperty(fo.NAME).SetValue(obj, val, null);
                    }
                }
            }
            catch //(Exception ex)
            {
                return EditStatus.FAIL_EXCEPTION_CONVERT_TO_DYNAMIC_OBJECT;
            }
            return AddItems(new object[] { obj }, false)[0];
        }

        private object AddItem(object item)
        {
            return AddItems(new object[] { item })[0];
        }

        public object[] AddItems(object[] items, bool convertToDynamic = true)
        {
            EditStatus[] rs = new EditStatus[items.Length];
            if (Opened == false || items == null || items.Length == 0) return null;

            bool ok = false;
            int itemConverted = 0;
            /////////////////////////////////////////
            var lsDynObject = new List<object>();

            #region [ convert items[] to array dynamic object ]

            if (convertToDynamic == false)
                lsDynObject.AddRange(items);
            else
                //lsDynObject = (IList)typeof(List<>).MakeGenericType(TypeDynamic).GetConstructor(Type.EmptyTypes).Invoke(null);
                lsDynObject = items
                    .Select((x, k) => convertToDynamicObject(x, true))
                    .ToList();

            if (lsDynObject.Count > 0)
            {
                int[] ids;
                lock (_lockListItem)
                    // performance very bad !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    ids = lsDynObject.Select(x => x == null ? -2 : m_listItems.IndexOf(x)).ToArray();

                for (int k = ids.Length - 1; k >= 0; k--)
                {
                    switch (ids[k])
                    {
                        case -2:
                            rs[k] = EditStatus.FAIL_EXCEPTION_CONVERT_TO_DYNAMIC_OBJECT;
                            lsDynObject.RemoveAt(k);
                            break;
                        case -1:
                            itemConverted++;
                            break;
                        default:
                            rs[k] = EditStatus.FAIL_ITEM_EXIST;
                            lsDynObject.RemoveAt(k);
                            break;
                    }
                }
            }

            #endregion

            if (itemConverted == 0) return new object[] { };

            /////////////////////////////////////////  
            Dictionary<int, byte[]> dicBytes = new Dictionary<int, byte[]>();
            List<byte> lsByte = new List<byte>() { };
            List<int> lsIndexItemNew = new List<int>();
            int itemCount = 0;

            if (lsDynObject.Count > 0)
            {
                //using (m_lockFile.WriteLock())
                //{
                #region [ === CONVERT DYNAMIC OBJECT - SERIALIZE === ]

                for (int k = 0; k < lsDynObject.Count; k++)
                {
                    rs[k] = EditStatus.NONE;
                    int index_ = m_Count + itemCount;

                    byte[] buf = m_buffer.Serialize(lsDynObject[k], TypeDynamic);

                    if (buf == null || buf.Length == 0)
                    {
                        rs[k] = EditStatus.FAIL_EXCEPTION_SERIALIZE_DYNAMIC_OBJECT;
                        continue;
                    }
                    else
                    //    if (buf.Length > m_BlobSizeMax)
                    //{
                    //    rs[k] = EditStatus.FAIL_MAX_LEN_IS_255_BYTE;
                    //    continue;
                    //}
                    //else
                    {
                        lsByte.AddRange(buf);
                        dicBytes.Add(index_, buf);
                        rs[k] = EditStatus.SUCCESS;
                        itemCount++;
                        lsIndexItemNew.Add(k);
                    }
                } // end for each items


                #endregion

                #region [ === TEST === ]

                ////////////////////var o1 = convertToDynamicObject(items[lsDynamicObject.Count - 1], 0);
                ////////////////////byte[] b2;
                ////////////////////using (var ms = new MemoryStream())
                ////////////////////{
                ////////////////////    ProtoBuf.Serializer.Serialize(ms, o1);
                ////////////////////    b2 = ms.ToArray();
                ////////////////////}
                ////////////////////byte[] b3 = serializeDynamicObject(o1);
                ////////////////////string j3 = string.Join(" ", b2.Select(x => x.ToString()).ToArray());
                ////////////////////string j4 = string.Join(" ", b3.Select(x => x.ToString()).ToArray());
                ////////////////////if (j3 == j4)
                ////////////////////    b2 = null;

                //////////lsDynamicObject.Add(convertToDynamicObject(items[0], 1));
                //////////lsDynamicObject.Add(convertToDynamicObject(items[1], 2));
                //////////lsDynamicObject.Add(convertToDynamicObject(items[2], 3));
                //////////lsDynamicObject.Add(convertToDynamicObject(items[3], 4));

                //////////byte[] b1;
                //////////using (var ms = new MemoryStream())
                //////////{
                //////////    ProtoBuf.Serializer.Serialize(ms, lsDynamicObject);
                //////////    b1 = ms.ToArray();
                //////////}
                ////////////string j1 = string.Join(" ", b1.Select(x => x.ToString()).ToArray());
                //////////////string j2 = string.Join(" ", lsByte.Select(x => x.ToString()).ToArray());
                //////////////if (j1 == j2)
                //////////////    b1 = null;
                //////////////byte[] bs = "10 9 8 0 18 5 16 1 82 1 49 10 2 8 1 0 0 0 0 10 2 8 2 10 9 8 3 18 5 16 2 82 1 51".Split(' ').Select(x => byte.Parse(x)).ToArray();
                //////////////object vs;
                //////////////Type typeList = typeof(List<>).MakeGenericType(m_typeDynamic);
                //////////////using (var ms = new MemoryStream(bs))
                //////////////    vs = (IList)ProtoBuf.Serializer.NonGeneric.Deserialize(typeList, ms);

                //////////return rs;

                #endregion

                #region [ === RESIZE GROW === ]

                int freeStore = m_FileSize - (m_BlobLEN + m_HeaderSize);
                if (freeStore < lsByte.Count + 1)
                {
                    m_mapFile.Close();
                    FileStream fs = new FileStream(m_FilePath, FileMode.OpenOrCreate);
                    long fileSize = fs.Length + lsByte.Count + (m_BlobGrowSize * m_BlobSizeMax);
                    fs.SetLength(fileSize);
                    fs.Close();
                    m_FileSize = (int)fileSize;
                    m_mapFile = MemoryMappedFile.Create(m_FilePath, MapProtection.PageReadWrite, m_FileSize);
                }

                #endregion

                #region [ === WRITE FILE === ]

                bool w = writeData(lsByte.ToArray(), itemCount);
                if (w)
                {
                    //string j1 = string.Join(" ", lsByte.Select(x => x.ToString()).ToArray());
                    lock (_lockListItem)
                    {
                        for (int k = 0; k < itemCount; k++)
                        {
                            Interlocked.Increment(ref m_Count);
                            Interlocked.Increment(ref m_Capacity);
                            m_listItems.Add(lsDynObject[lsIndexItemNew[k]]);
                        }
                    }

                    m_buffer.AddOrUpdate(dicBytes);
                    ok = true;
                }
                if (w == false)
                {
                    for (int k = 0; k < rs.Length; k++)
                        if (rs[k] == EditStatus.SUCCESS) rs[k] = EditStatus.FAIL_EXCEPTION_WRITE_ARRAY_BYTE_TO_FILE;
                }

                #endregion
                //}// end lock
            }

            if (ok)
            {
                IList listClone = null;
                lock (_lockListItem)
                    listClone = m_buffer.Clone(m_listItems);
                m_search.BuildCache(listClone, ItemEditType.ADD_NEW_ITEM);
            }

            return lsDynObject.ToArray();
        }

        public EditStatus Update(object oCurrent, object oUpdate)
        {
            EditStatus ok = EditStatus.NONE;
            if (Opened == false) return ok;

            var o1 = convertToDynamicObject_GetIndex(oCurrent, false);
            var o2 = convertToDynamicObject_GetIndex(oUpdate, false);

            if (o1 == null || o2 == null)
                return EditStatus.FAIL_EXCEPTION_CONVERT_TO_DYNAMIC_OBJECT;
            else
            {
                if (o1.Index == -1) return EditStatus.FAIL_ITEM_NOT_EXIST;
                if (o2.Index != -1) return EditStatus.FAIL_ITEM_EXIST;

                int index = o1.Index;
                object val = o2.Item;

                byte[] buf = m_buffer.Serialize(val, TypeDynamic);
                if (buf == null || buf.Length == 0) return EditStatus.FAIL_EXCEPTION_SERIALIZE_DYNAMIC_OBJECT;

                if (index != -1)
                {
                    lock (_lockListItem)
                        m_listItems[index] = val;

                    m_buffer.AddOrUpdate(index, buf);
                    byte[] bAll = m_buffer.GetAll();
                    //string j1 = string.Join(" ", bAll.Select(x => x.ToString()).ToArray());

                    lock (_lockWriteFile)
                    {
                        m_BlobLEN = 0;
                        writeData(bAll, 0);
                        m_BlobLEN = bAll.Length + 4;
                        ok = EditStatus.SUCCESS;
                    }
                }
            }

            return ok;
        }

        public EditStatus Remove(object item)
        {
            EditStatus ok = EditStatus.NONE;
            if (Opened == false) return ok;

            var it = convertToDynamicObject_GetIndex(item, false);

            if (it == null)
                return EditStatus.FAIL_EXCEPTION_CONVERT_TO_DYNAMIC_OBJECT;
            else
                ok = Remove(it.Index);

            return ok;
        }

        public bool RemoveBySearh(SearchRequest search)
        {
            SearchResult sr = null;
            lock (_lockListItem)
                sr = m_search.SearchGetIDs_(m_listItems, search);
            if (sr.Status)
            {
                if (sr.Total == 0)
                    return true;
                else
                    return RemoveIndexs(sr.IDs);
            }
            else
                return false;
        }

        public bool RemoveItemDynamic(object item)
        {
            int index = -1;

            lock (_lockListItem)
                index = m_listItems.IndexOf(item);

            if (index != -1)
                return RemoveIndexs(new int[] { index });

            return false;
        }

        private bool RemoveIndexs(int[] index)
        {
            if (index == null) return false;

            if (index.Length > 0)
            {
                var a = index.OrderByDescending(x => x).ToArray();
                lock (_lockListItem)
                {
                    foreach (int i in a)
                        m_listItems.RemoveAt(i);
                    m_Count = m_listItems.Count;
                }

                lock (_lockWriteFile)
                {
                    byte[] buf = m_buffer.Remove(index);

                    m_BlobLEN = 0;
                    writeData(buf, 0);
                    m_BlobLEN = buf.Length;
                }
                return true;
            }
            return false;
        }

        public bool RemoveByFieldSyncEdit(object value_FieldSyncEdit)
        {
            if (string.IsNullOrEmpty(Field_SyncEdit) || value_FieldSyncEdit == null || string.IsNullOrEmpty(value_FieldSyncEdit.ToString())) return false;

            int[] index = new int[]{};
            try
            {
                lock (m_listItems)
                    index = m_listItems.AsQueryable()
                        .Select<object>(Field_SyncEdit)
                        .Select((x, k) => new { index = k, item = x })
                        .Where(x => x.item.Equals(value_FieldSyncEdit))
                        .Select(x => x.index).ToArray();
            }
            catch { }

            if (index.Length > 0)
                return RemoveIndexs(index);

            return false;
        }

        public EditStatus Truncate()
        {
            EditStatus ok = EditStatus.NONE;
            try
            {
                m_BlobLEN = 0;
                m_Count = 0;
                m_Capacity = m_FileSize - m_HeaderSize;

                lock (_lockWriteFile)
                    writeHeaderBlank();

                lock (_lockListItem)
                    m_listItems.Clear();

                m_buffer.Truncate();
                m_search.Truncate();

                ok = EditStatus.SUCCESS;
            }
            catch
            {
                ok = EditStatus.FAIL_EXCEPTION;
            }

            return ok;
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
            if (listener.IsListening == false) return;
            HttpListenerContext context = listener.EndGetContext(result);

            string method = context.Request.HttpMethod;
            string path = context.Request.Url.LocalPath;
            switch (method)
            {
                case "POST":
                    #region [ === POST === ]
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        dbMsg m = formatter.Deserialize(context.Request.InputStream) as dbMsg;
                        if (m != null)
                        {
                            switch (m.Action)
                            {
                                case dbAction.DB_SELECT:
                                    SearchRequest sr = (SearchRequest)m.Data;
                                    var rs = SearchGetIDs(sr);
                                    formatter.Serialize(context.Response.OutputStream, rs);
                                    break;
                            }
                        }
                    }
                    catch //(Exception ex)
                    {
                        //Serializer.NonGeneric.Serialize(context.Response.OutputStream, 500);
                        //context.Response.Close();
                    }

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

        #region [ === FUNCTION PRIVATE === ]

        private object getKeyAuto(string type, object item = null)
        {
            object val = null;
            switch (type)
            {
                case "Int32":
                    Int32 _int = Int32.Parse(DateTime.Now.ToString("yMMddHHmmssfff"));
                    if (item != null) _int += item.GetHashCode();
                    val = _int;
                    break;
                case "Int64":
                    long _long = long.Parse(DateTime.Now.ToString("yyMMddHHmmssfff"));
                    if (item != null) _long += item.GetHashCode();
                    val = _long;
                    break;
                case "String":
                    val = Guid.NewGuid().ToString();
                    break;
            }
            return val;
        }

        private bool bindHeader(byte[] buf)
        {
            if (buf.Length < m_HeaderSize) return false;
            try
            {
                ////////////////////////////////////////////////
                // HEADER = [4 bytes blob len] + [8 byte ID] + [4 bytes Capacity] + [4 bytes Count] + [980 byte fields] = 1,000

                // [4 bytes blob LEN]
                m_BlobLEN = BitConverter.ToInt32(buf.Take(4).ToArray(), 0);

                // [8 byte ID]
                m_FileID = BitConverter.ToInt64(buf.Skip(4).Take(8).ToArray(), 0);

                // [4 bytes Capacity]
                m_Capacity = BitConverter.ToInt32(buf.Skip(12).Take(4).ToArray(), 0);

                // [4 bytes Count]
                m_Count = BitConverter.ToInt32(buf.Skip(16).Take(4).ToArray(), 0);

                // [980 byte fields]
                int lenModel = BitConverter.ToInt32(buf.Skip(20).Take(4).ToArray(), 0);
                byte[] _fields = buf.Skip(24).Take(lenModel).ToArray();
                using (var ms = new MemoryStream(_fields))
                    Model = ProtoBuf.Serializer.Deserialize<DB_MODEL>(ms);

                TypeDynamic = buildTypeDynamic(Model);

                if (m_BlobLEN > 0)
                {
                    int lenBlank = 4 - m_buffer.Serialize(m_Count, typeof(int)).Length;
                    if (lenBlank > 0) lenBlank--;

                    byte[] items = buf.Skip(m_HeaderSize + lenBlank).Take(m_BlobLEN - lenBlank).ToArray();

                    IList clone = null;
                    Type type = typeof(List<>).MakeGenericType(TypeDynamic);
                    try
                    {
                        object[] ds = m_buffer.DeserializeAndClone(items, type);
                        m_listItems = (IList)ds[0];
                        clone = (IList)ds[1];
                    }
                    catch //(Exception e1)
                    {
                        string j1 = string.Join(" ", items.Select(x => x.ToString()).ToArray());
                    }

                    if (clone != null && clone.Count > 0)
                    {
                        m_buffer.Initialize(clone, TypeDynamic);
                        m_search.Initialize(clone);
                    }
                }

                return true;
            }
            catch //(Exception exx)
            {
            }
            return false;
        }

        private void writeHeaderBlank()
        {
            ////////////////////////////////////////////////
            // HEADER = [4 bytes blob len] + [8 byte ID] + [4 bytes Capacity] + [4 bytes Count] + [980 byte fields] = 1,000

            // [4 bytes blob LEN]
            byte[] _byteBlobLEN = BitConverter.GetBytes(m_BlobLEN);

            // [8 byte ID]
            m_FileID = long.Parse(DateTime.Now.ToString("yyMMddHHmmssfff"));
            byte[] _byteFileID = BitConverter.GetBytes(m_FileID).ToArray();

            // [4 bytes Capacity]
            byte[] _byteCapacity = BitConverter.GetBytes(m_Capacity);

            // [4 bytes Count]
            byte[] _byteCount = BitConverter.GetBytes(m_Count);

            // [980 byte fields]
            byte[] bm;
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize<DB_MODEL>(ms, Model);
                bm = ms.ToArray();
            }

            List<byte> _byteFields = new List<byte>();
            _byteFields.AddRange(BitConverter.GetBytes(bm.Length));
            _byteFields.AddRange(bm);
            _byteFields.AddRange(new byte[980 - _byteFields.Count]);
            TypeDynamic = buildTypeDynamic(Model);

            List<byte> ls = new List<byte>();
            ls.AddRange(_byteBlobLEN);
            ls.AddRange(_byteFileID);
            ls.AddRange(_byteCapacity);
            ls.AddRange(_byteCount);
            ls.AddRange(_byteFields);

            using (Stream view = m_mapFile.MapView(MapAccess.FileMapWrite, 0, ls.Count))
                view.Write(ls.ToArray(), 0, ls.Count);
        }

        private bool writeData(byte[] item, int countItem)
        {
            if (countItem < 0 || item == null || item.Length == 0) return false;
            int blobLEN = (m_BlobLEN == 0 ? 4 : m_BlobLEN) + item.Length;
            try
            {
                ////////////////////////////////////////////////
                // HEADER = [4 bytes blob len] + [8 byte ID] + [4 bytes Capacity] + [4 bytes Count] + [980 byte fields] = 1,000

                // [4 bytes blob LEN]
                byte[] _byteBlobLEN = BitConverter.GetBytes(blobLEN);

                // [8 byte ID] 

                // [4 bytes Capacity] 

                // [4 bytes Count]
                byte[] _byteCount = BitConverter.GetBytes(m_Count + countItem);

                int offset = m_HeaderSize + m_BlobLEN + (m_BlobLEN == 0 ? 4 : 0);
                if (offset < m_FileSize)
                {
                    /////////////////////////
                    // HEADER BODY DATA  
                    List<byte> lsHeaderBody = new List<byte>(4);
                    byte[] lb = m_buffer.Serialize(m_Count + countItem, typeof(int));
                    int ln = lb.Length;
                    if (ln < 4)
                        lsHeaderBody.AddRange(new byte[4 - ln]);
                    lsHeaderBody.AddRange(lb);

                    //byte[] all = m_buffer.GetAll();
                    //string j1 = string.Join(" ", item.Select(x => x.ToString()).ToArray());
                    //string jall = string.Join(" ", all.Select(x => x.ToString()).ToArray());

                    using (Stream view = m_mapFile.MapView(MapAccess.FileMapWrite, 0, m_FileSize))
                    {
                        //view.Seek(0, SeekOrigin.Begin);
                        view.Write(_byteBlobLEN, 0, 4);
                        view.Seek(16, SeekOrigin.Begin);
                        view.Write(_byteCount, 0, 4);

                        /////////////////////////
                        // WRITE BODY DATA  
                        view.Seek(m_HeaderSize, SeekOrigin.Begin);
                        view.Write(lsHeaderBody.ToArray(), 0, 4);

                        /////////////////////////
                        // WRITE BODY DATA  
                        view.Seek(offset, SeekOrigin.Begin);
                        view.Write(item, 0, item.Length);

                        /////////////////////////
                        // UPDATE ITEMS ///////
                        if (countItem == 0)
                        {
                            int lenBlank = m_FileSize - (item.Length + m_HeaderSize + 1);
                            byte[] bb = new byte[lenBlank];
                            offset = offset + item.Length;
                            view.Seek(offset, SeekOrigin.Begin);
                            view.Write(bb, 0, lenBlank);
                        }
                    }
                }
                else
                {
                    // Grow resize file after write item
                }
            }
            catch //(Exception ex)
            {
                return false;
            }
            m_BlobLEN = blobLEN;
            return true;
        }

        /// <summary>
        /// Convert object class to dynamic class. If add new then createNewKeyAuto = true else it be update then createNewKeyAuto = false
        /// </summary>
        /// <param name="item"></param>
        /// <param name="createNewKeyAuto">If add new then createNewKeyAuto = true else it be update then createNewKeyAuto = false</param>
        /// <returns></returns>
        public object convertToDynamicObject(object item, bool createNewKeyAuto)
        {
            if (item == null) return null;
            bool ex = false;
            var fieldKeyAutos = Model.FIELDS.Where(x => x.IS_KEY_AUTO).ToArray();
            var colKeyAutos = Model.FIELDS.Where(x => x.IS_KEY_AUTO).Select(x => x.NAME).ToList();

            var o = Activator.CreateInstance(TypeDynamic);
            foreach (PropertyInfo pi in item.GetType().GetProperties())
            {
                object val = null;
                try
                {
                    // ADD NEW ITEM
                    if (createNewKeyAuto)
                    {
                        int pos = colKeyAutos.IndexOf(pi.Name);
                        if (pos == -1)
                            val = pi.GetValue(item, null);
                        else
                        {
                            var fi = fieldKeyAutos[pos];
                            val = getKeyAuto(fi.TYPE_NAME, item);
                        }
                    }
                    else // UPDATE ITEM
                        val = pi.GetValue(item, null);
                    var po = o.GetType().GetProperty(pi.Name);
                    if (po.PropertyType.Name != pi.PropertyType.Name)
                    {
                        var oc = dbType.Convert(po.PropertyType.Name, val);
                        if (oc.OK) val = oc.Value; else return null;
                    }
                    po.SetValue(o, val, null);
                }
                catch //(Exception exx)
                {
                    ex = true;
                }
            }
            if (ex) return null;

            return o;
        }

        /// <summary>
        /// Convert object class to dynamic class. If add new then createNewKeyAuto = true else it be update then createNewKeyAuto = false
        /// </summary>
        /// <param name="item"></param>
        /// <param name="createNewKeyAuto">If add new then createNewKeyAuto = true else it be update then createNewKeyAuto = false</param>
        /// <returns></returns>
        private IndexDynamic convertToDynamicObject_GetIndex(object item, bool createNewKeyAuto)
        {
            if (item == null) return null;
            bool ex = false;
            var fieldKeyAutos = Model.FIELDS.Where(x => x.IS_KEY_AUTO).ToArray();
            var colKeyAutos = Model.FIELDS.Where(x => x.IS_KEY_AUTO).Select(x => x.NAME).ToList();

            var o = Activator.CreateInstance(TypeDynamic);
            foreach (PropertyInfo pi in item.GetType().GetProperties())
            {
                object val = null;
                try
                {
                    // ADD NEW ITEM
                    if (createNewKeyAuto)
                    {
                        int pos = colKeyAutos.IndexOf(pi.Name);
                        if (pos == -1)
                            val = pi.GetValue(item, null);
                        else
                        {
                            var fi = fieldKeyAutos[pos];
                            val = getKeyAuto(fi.TYPE_NAME, item);
                        }
                    }
                    else // UPDATE ITEM
                        val = pi.GetValue(item, null);
                    o.GetType().GetProperty(pi.Name).SetValue(o, val, null);
                }
                catch
                {
                    ex = true;
                }
            }
            if (ex) return null;

            int index = -1;
            lock (_lockListItem)
                index = m_listItems.IndexOf(o);

            return new IndexDynamic(index, o);
        }

        private Type buildTypeDynamic(DB_MODEL m)
        {
            if (m == null || string.IsNullOrEmpty(m.NAME) || m.FIELDS == null || m.FIELDS.Length == 0) return null;

            var fields = m.FIELDS
                //.Where(x => x.FieldChange != dbFieldChange.REMOVE)
                .Select(x => new DynamicProperty(x.NAME, x.Type)).OrderBy(x => x.Name).ToArray();
            Type type = DynamicExpression.CreateClass(fields);
            //DynamicProperty[] at = new DynamicProperty[]
            //{
            //    new DynamicProperty("Name", typeof(string)),
            //    new DynamicProperty("Birthday", typeof(DateTime))
            //};
            //object obj = Activator.CreateInstance(type);
            //t.GetProperty("Name").SetValue(obj, "Albert", null);
            //t.GetProperty("Birthday").SetValue(obj, new DateTime(1879, 3, 14), null);

            ///////////////////////////////////////////////////////////////////////////////////////////////
            // REGISTRY CLASS WITH PROPERTIES WITH PROTOBUF
            m_buffer.RegistryType(type);

            ///////////////////////////////////////////////////////////////////////////////////////////////
            // REGISTRY CLASS WITH PROPERTIES WITH PROTOBUF
            ////////////var model = ProtoBuf.Meta.RuntimeTypeModel.Default;
            ////////////// Obtain all serializable types having no explicit proto contract
            ////////////var serializableTypes = Assembly.GetExecutingAssembly()
            ////////////    .GetTypes()
            ////////////    .Where(t => t.IsSerializable && !Attribute.IsDefined(t, typeof(ProtoBuf.ProtoContractAttribute)));

            ////////////var metaType = model.Add(type, false);
            ////////////metaType.AsReferenceDefault = true;
            ////////////metaType.UseConstructor = false;

            ////////////// Add contract for all the serializable fields
            ////////////var serializableFields = type
            ////////////    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ////////////    .Where(fi => !Attribute.IsDefined(fi, typeof(NonSerializedAttribute)))
            ////////////    .OrderBy(fi => fi.Name)  // it's important to keep the same fields order in all the AppDomains
            ////////////    .Select((fi, k) => new { info = fi, index = k })
            ////////////    .ToArray();
            ////////////foreach (var field in serializableFields)
            ////////////{
            ////////////    var metaField = metaType.AddField(field.index + 1, field.info.Name);
            ////////////    metaField.AsReference = !field.info.FieldType.IsValueType;       // cyclic references support
            ////////////    metaField.DynamicType = field.info.FieldType == typeof(object);  // any type support
            ////////////}
            ////////////// Compile model in place for better performance, .Compile() can be used if all types are known beforehand
            ////////////model.CompileInPlace();

            return type;
        }

        public static string textEncrypt(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            string key = "Abc123@";

            //System.Windows.Forms.MessageBox.Show(key);
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string textDecrypt(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            //Get your key from config file to open the lock!
            string key = "Abc123@";

            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //release any resource held by the MD5CryptoServiceProvider

                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        #endregion
    }
}
