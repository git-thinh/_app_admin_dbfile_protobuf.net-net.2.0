﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace app.GUI
{
    public partial class TabControlCustom : TabControl
    {
        public TabControlCustom()
        {
            if (!this.DesignMode) this.Multiline = true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328 && !this.DesignMode)
                m.Result = new IntPtr(1);
            else
                base.WndProc(ref m);
        }
    }
}
