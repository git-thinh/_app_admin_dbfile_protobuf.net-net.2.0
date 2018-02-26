https://www.codeproject.com/articles/25471/webcontrols/webcontrols/?fid=1244083&df=90&mpp=50&prof=true&sort=position&view=expanded&spc=relaxed&fr=51

// Dynamically created controls.

// Create grid view control.
DataGridView gridView = new DataGridView();
gridView.BorderStyle = BorderStyle.None;
gridView.Columns.Add("Column1", "Column 1");
gridView.Columns.Add("Column2", "Column 2");
gridView.Columns.Add("Column3", "Column 3");
gridView.Columns.Add("Column4", "Column 4");
gridView.Columns.Add("Column5", "Column 5");
this.customComboBox1.DropDownControl = gridView;

// Create user control.
UserControl1 userControl = new UserControl1();
userControl.BorderStyle = BorderStyle.None;
this.customComboBox2.DropDownControl = userControl;

// Create rich textbox control.
RichTextBox richTextBox = new RichTextBox();
richTextBox.BorderStyle = BorderStyle.None;
this.customComboBox3.DropDownControl = richTextBox;