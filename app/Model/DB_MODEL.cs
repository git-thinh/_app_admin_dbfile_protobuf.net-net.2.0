using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using app.Core;

namespace app.Model
{
    [ProtoContract]
    public interface IDbModel
    {
        [ProtoMember(1)]
        string NAME { set; get; }

        [ProtoMember(2)]
        string CAPTION { set; get; }

        [ProtoMember(3)]
        string TAG { set; get; }
    }

    [Serializable, ProtoContract]
    public class DB_MODEL : Attribute, IDbModel
    {
        [ProtoMember(1)]
        public string NAME { set; get; }

        [ProtoMember(2)]
        public string CAPTION { set; get; }

        [ProtoMember(3)]
        public string TAG { set; get; }

        /////////////////////////////////////////////////////////////////////

        [ProtoMember(4)]
        public FieldInfo[] FIELDS { set; get; }

        public DB_MODEL() { }
        public DB_MODEL(Type model)
        {

            List<FieldInfo> lf = new List<FieldInfo>() { };

            foreach (var prop in model.GetProperties())
            {
                var at = (FieldInfo[])prop.GetCustomAttributes(typeof(FieldInfo), false);
                if (at.Length > 0)
                {
                    FieldInfo o = at[0];
                    o.NAME = prop.Name;
                    o.TYPE_NAME = prop.PropertyType.Name;
                    lf.Add(o);
                }
                else
                    lf.Add(new FieldInfo() { NAME = prop.Name, TYPE_NAME = prop.PropertyType.Name });
            }


            NAME = model.Name;
            FIELDS = lf.ToArray();
            //model.GetProperties().Select(x => new dbField() { Name = x.Name, TypeName = x.PropertyType.Name }).ToArray();
        }

        public override string ToString()
        {
            return this.NAME + ";" + string.Join(",", this.FIELDS.Select(x => x.TYPE_NAME + "." + x.NAME).ToArray());
        }
    }
}
