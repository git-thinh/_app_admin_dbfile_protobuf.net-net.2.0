using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{ 
    [ProtoContract]
    public interface IFieldInfo
    {
        [ProtoMember(1)]
        string NAME { set; get; }

        [ProtoMember(2)]
        string TYPE_NAME { set; get; }

        [ProtoMember(3)]
        bool IS_DATE_TIME { set; get; }

        [ProtoMember(4)]
        bool IS_KEY_AUTO { set; get; }

        [ProtoMember(5)]
        ControlKit KIT { set; get; }

        [ProtoMember(6)]
        bool IS_ALLOW_NULL { set; get; }

        [ProtoMember(7)]
        bool IS_INDEX { set; get; }

        [ProtoMember(8)]
        bool IS_ENCRYPT { set; get; }

        [ProtoMember(9)]
        bool IS_NOT_DUPLICATE { set; get; }

        [ProtoMember(10)]
        bool IS_FULL_TEXT_SEARCH { set; get; }

        [ProtoMember(11)]
        bool IS_KEY_SYNC_EDIT { set; get; }

        //////////////////////////////////////////

        [ProtoMember(12)]
        string CAPTION { set; get; }

        [ProtoMember(13)]
        string CAPTION_SHORT { set; get; }

        [ProtoMember(14)]
        string DESCRIPTION { set; get; }

        //////////////////////////////////////////

        [ProtoMember(15)]
        JoinType JOIN_TYPE { set; get; }

        [ProtoMember(16)]
        string JOIN_MODEL { set; get; }

        [ProtoMember(17)]
        string JOIN_FIELD { set; get; }

        [ProtoMember(18)]
        string JOIN_VIEW { set; get; }

        [ProtoMember(19)]
        string[] VALUE_DEFAULT { set; get; }

        //////////////////////////////////////////

        [ProtoMember(20)]
        bool MOBI { set; get; }

        [ProtoMember(21)]
        bool TABLET { set; get; }

        //////////////////////////////////////////

        [ProtoMember(22)]
        int ORDER_EDIT { set; get; }

        [ProtoMember(23)]
        int ORDER_VIEW { set; get; }

        [ProtoMember(24)]
        string FUNC_EDIT { set; get; }

        [ProtoMember(25)]
        string FUNC_BEFORE_UPDATE { set; get; }

        [ProtoMember(26)]
        int ORDER_KEY_URL { set; get; }

        [ProtoMember(27)]
        bool ONLY_SHOW_IN_DETAIL { set; get; }
    }
}
