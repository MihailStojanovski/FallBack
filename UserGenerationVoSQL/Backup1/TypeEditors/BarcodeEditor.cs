using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using FastReport.Controls;
using FastReport.Barcode;

namespace FastReport.TypeEditors
{
  internal class BarcodeEditor : UITypeEditor
  {
    private IWindowsFormsEditorService edSvc;

    private void lb_Click(object sender, EventArgs e)
    {
      edSvc.CloseDropDown();
    }

    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.DropDown;
    }

    public override object EditValue(ITypeDescriptorContext context,
      IServiceProvider provider, object Value)
    {
      edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
      string name = (Value as BarcodeBase).Name;
      
      ListBox lb = new ListBox();
      lb.Items.AddRange(Barcodes.GetItems());
      lb.SelectedItem = name;
      lb.BorderStyle = BorderStyle.None;
      lb.Height = lb.ItemHeight * lb.Items.Count;
      lb.Click += new EventHandler(lb_Click);
      edSvc.DropDownControl(lb);
      
      string selectedName = (string)lb.SelectedItem;
      if (selectedName != name)
        return Activator.CreateInstance(Barcodes.GetType(selectedName));
      return Value;
    }

  }
}
