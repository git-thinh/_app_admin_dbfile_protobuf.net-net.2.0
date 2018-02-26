using System;
using System.Collections.Generic;
using System.Text;

namespace app.GUI
{

    public interface IDataFieldEvent
    {
        void BindDataUC(IDictionary<string, object> data);
    }
}
