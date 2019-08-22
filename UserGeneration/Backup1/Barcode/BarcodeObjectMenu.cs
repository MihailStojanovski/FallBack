using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FastReport.Design;
using FastReport.Utils;
using DevComponents.DotNetBar;

namespace FastReport.Barcode
{
  internal class BarcodeObjectMenu : ReportComponentBaseMenu
  {
    private SelectedObjectCollection FSelection;
    public ButtonItem miBarcodeType;
    public ButtonItem miAutoSize;
    public ButtonItem miShowText;

    private List<BarcodeObject> ModifyList
    {
      get
      {
        List<BarcodeObject> list = new List<BarcodeObject>();
        foreach (Base c in FSelection)
        {
          if (c is BarcodeObject && !c.HasRestriction(Restrictions.DontModify))
            list.Add(c as BarcodeObject);
        }
        return list;
      }
    }

    #region Private Methods
    private void CreateBarcodeTypes(BarcodeBase b)
    {
      foreach (string barcode in Barcodes.GetItems())
      {
        ButtonItem mi = CreateMenuItem(barcode, new EventHandler(miBarcodeType_Click));
        mi.Checked = barcode == b.Name;
        miBarcodeType.SubItems.Add(mi);
      }
    }

    private void miBarcodeType_Click(object sender, EventArgs e)
    {
      foreach (BarcodeObject c in ModifyList)
      {
        c.SymbologyName = (sender as ButtonItem).Text;
      }
      Change();
    }
    
    private void miAutoSize_Click(object sender, EventArgs e)
    {
      foreach (BarcodeObject c in ModifyList)
      {
        c.AutoSize = miAutoSize.Checked;
      }
      Change();
    }

    private void miShowText_Click(object sender, EventArgs e)
    {
      foreach (BarcodeObject c in ModifyList)
      {
        c.ShowText = miShowText.Checked;
      }
      Change();
    }
    #endregion

    public BarcodeObjectMenu(Designer designer) : base(designer)
    {
      FSelection = Designer.SelectedObjects;

      MyRes res = new MyRes("ComponentMenu,Barcode");
      miBarcodeType = CreateMenuItem(res.Get("BarcodeType"));
      miBarcodeType.BeginGroup = true;
      miAutoSize = CreateMenuItem(res.Get("AutoSize"), new EventHandler(miAutoSize_Click));
      miAutoSize.BeginGroup = true;
      miAutoSize.AutoCheckOnClick = true;
      miShowText = CreateMenuItem(res.Get("ShowText"), new EventHandler(miShowText_Click));
      miShowText.AutoCheckOnClick = true;

      int insertPos = Items.IndexOf(miCut);
      Items.Insert(insertPos, miBarcodeType);
      Items.Insert(insertPos + 1, miAutoSize);
      Items.Insert(insertPos + 2, miShowText);

      BarcodeObject barcode = FSelection[0] as BarcodeObject;
      CreateBarcodeTypes(barcode.Barcode);

      bool enabled = !barcode.HasRestriction(Restrictions.DontModify);
      miBarcodeType.Enabled = enabled;
      miAutoSize.Enabled = enabled;
      miShowText.Enabled = enabled;
      if (enabled)
      {
        miAutoSize.Checked = barcode.AutoSize;
        miShowText.Checked = barcode.ShowText;
      }
    }
  }
}
