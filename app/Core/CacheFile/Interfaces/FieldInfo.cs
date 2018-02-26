using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{

    [Serializable, ProtoContract]
    public class FieldInfo : Attribute, IFieldInfo
    {
        [ProtoMember(1)]
        public string NAME
        {
            set
            {
                _name = value;
                if (_name == null) _name = string.Empty;
                else _name = _name.Replace(' ', '_').ToUpper().Trim();
            }
            get
            {
                return _name;
            }
        }

        [ProtoMember(2)]
        public string TYPE_NAME
        {
            set
            {
                IS_DATE_TIME = value == typeof(DateTime).Name;
                Type = dbType.GetByName(value);
                _type = Type.Name;
            }
            get
            {
                return _type;
            }
        }

        [ProtoMember(3)]
        public bool IS_DATE_TIME { set; get; }

        [ProtoMember(4)]
        public bool IS_KEY_AUTO { set; get; }

        [ProtoMember(5)]
        public ControlKit KIT { set; get; }

        [ProtoMember(6)]
        public bool IS_ALLOW_NULL { set; get; }

        [ProtoMember(7)]
        public bool IS_INDEX { set; get; }

        [ProtoMember(8)]
        public bool IS_ENCRYPT { set; get; }

        [ProtoMember(9)]
        public bool IS_NOT_DUPLICATE { set; get; }

        [ProtoMember(10)]
        public bool IS_FULL_TEXT_SEARCH { set; get; }

        [ProtoMember(11)]
        public bool IS_KEY_SYNC_EDIT { set; get; }

        //////////////////////////////////////////

        [ProtoMember(12)]
        public string CAPTION { set; get; }

        [ProtoMember(13)]
        public string CAPTION_SHORT { set; get; }

        [ProtoMember(14)]
        public string DESCRIPTION { set; get; }

        //////////////////////////////////////////

        [ProtoMember(15)]
        public JoinType JOIN_TYPE { set; get; }

        [ProtoMember(16)]
        public string JOIN_MODEL { set; get; }

        [ProtoMember(17)]
        public string JOIN_FIELD { set; get; }

        [ProtoMember(18)]
        public string JOIN_VIEW { set; get; }

        [ProtoMember(19)]
        public string[] VALUE_DEFAULT { set; get; }

        //////////////////////////////////////////

        [ProtoMember(20)]
        public bool MOBI { set; get; }

        [ProtoMember(21)]
        public bool TABLET { set; get; }

        //////////////////////////////////////////

        [ProtoMember(22)]
        public int ORDER_EDIT { set; get; }

        [ProtoMember(23)]
        public int ORDER_VIEW { set; get; }

        [ProtoMember(24)]
        public string FUNC_EDIT { set; get; }

        [ProtoMember(25)]
        public string FUNC_BEFORE_UPDATE { set; get; }

        [ProtoMember(26)]
        public int ORDER_KEY_URL { set; get; }

        [ProtoMember(27)]
        public bool ONLY_SHOW_IN_DETAIL { set; get; }

        //////////////////////////////////////////

        [ProtoIgnore]
        public object Value = null;

        [ProtoIgnore]
        private string _name = string.Empty;
        [ProtoIgnore]
        private string _type = typeof(String).Name;
        [ProtoIgnore]
        public Type Type { private set; get; }

        [ProtoIgnore]
        public dbFieldChange FieldChange { set; get; }

        public FieldInfo()
        {
            FieldChange = dbFieldChange.NONE;
            JOIN_TYPE = JoinType.NONE;
            KIT = ControlKit.TEXT;
            IS_ENCRYPT = false;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", NAME, TYPE_NAME);
        }

        //////////////////////////////////////////
    }
}
