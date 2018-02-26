using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using app.Model;

namespace app.Core
{
    public interface IDataFile
    {
        IList GetComboboxItem(FieldInfo field);
        bool ModelUpdate(DB_MODEL model, bool hasRemoveField);
        bool CreateDb(DB_MODEL model);
        bool ExistItemDynamic(string model, object item);
        bool RemoveItemDynamic(string model, object item);
        object AddItem(object item);
        object AddItem(string dbName, Dictionary<string, object> data);
        bool RemoveItem(object item, string fields);
        bool RemoveItemByKeyFieldSyncEdit(string model, object key);
        SearchResult FindItemByContainFieldValue(object item, string fields, int pageSize, int pageNumber);
        IndexDynamic FindItemFirstByContainFieldValue(object item, string fields);

        LOGIN_STATUS Login(string user, string pass);
        FieldInfo[] GetFields(string dbName);
        DB_MODEL GetModel(string dbName);
        Type GetTypeDynamic(string dbName);
        bool ExistModel(string dbName);

        InfoSelectTop GetInfoSelectTop(string dbName, int selectTop);
        IList GetSelectPage(string dbName, int pageNumber, int pageSize);
        string[] GetListDB();
        SearchResult Search(string dbName, SearchRequest sr);
        EditStatus ModelTruncate(string dbName);

        string[] menu_GetTAG();
        MENU[] menu_Find(Func<MENU, bool> condition);
    }
}
