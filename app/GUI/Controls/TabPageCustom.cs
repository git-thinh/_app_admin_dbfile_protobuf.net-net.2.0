using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Linq.Dynamic;
using app.Core;
using System.Linq;

namespace app.GUI
{
    public class PanelCustom : Panel
    {
        public FieldInfo[] Fields { set; get; }
        public Type TypeModel { set; get; }
        public SearchRequest SearchRequest { set; get; }
        public SearchResult SearchResult { set; get; }
        public int PageCurrent { set; get; }

        public PanelCustom() { PageCurrent = 1; }

        public PanelCustom(FieldInfo[] fields)
            : this()
        {
            Fields = fields;
            var _fields = fields.Select(x => new DynamicProperty(x.NAME, x.Type)).ToArray();
            TypeModel = DynamicExpression.CreateClass(_fields);
        }
    }

    public class TabPageCustom : TabPage
    {
        public delegate void EventLoadData();
        public EventLoadData OnLoadData;

        public FieldInfo[] Fields { set; get; }
        public Type TypeModel { set; get; }
        public SearchRequest SearchRequest { set; get; }
        public SearchResult SearchResult { set; get; }
        public int PageCurrent { set; get; }

        public TabPageCustom() { PageCurrent = 1; }

        public TabPageCustom(FieldInfo[] fields)
            : this()
        {
            Fields = fields;
            var _fields = fields.Select(x => new DynamicProperty(x.NAME, x.Type)).ToArray();
            TypeModel = DynamicExpression.CreateClass(_fields);
        }

        public void UpdateFields(FieldInfo[] fields) 
        {
            Fields = fields;
            var _fields = fields.Select(x => new DynamicProperty(x.NAME, x.Type)).ToArray();
            TypeModel = DynamicExpression.CreateClass(_fields);
        }
    }
}
