using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FastReport.Utils;
using DevComponents.DotNetBar;

namespace FastReport.Matrix
{
  internal class MatrixCellMenu : MatrixCellMenuBase
  {
    private ButtonItem miFunction;
    private ButtonItem miFunctionNone;
    private ButtonItem miFunctionSum;
    private ButtonItem miFunctionMin;
    private ButtonItem miFunctionMax;
    private ButtonItem miFunctionAvg;
    private ButtonItem miFunctionCount;

    private void Function_Click(object sender, EventArgs e)
    {
      MatrixAggregateFunction function = (MatrixAggregateFunction)(sender as ButtonItem).Tag;
      (Descriptor as MatrixCellDescriptor).Function = function;
      Change();
    }

    public MatrixCellMenu(MatrixObject matrix, MatrixElement element, MatrixDescriptor descriptor)
      : base(matrix, element, descriptor)
    {
      MyRes res = new MyRes("Forms,TotalEditor");

      miFunction = CreateMenuItem(Res.GetImage(132), Res.Get("ComponentMenu,MatrixCell,Function"), null);
      miFunctionNone = CreateMenuItem(null, Res.Get("Misc,None"), new EventHandler(Function_Click));
      miFunctionNone.AutoCheckOnClick = true;
      miFunctionNone.Tag = MatrixAggregateFunction.None;
      miFunctionSum = CreateMenuItem(null, res.Get("Sum"), new EventHandler(Function_Click));
      miFunctionSum.AutoCheckOnClick = true;
      miFunctionSum.Tag = MatrixAggregateFunction.Sum;
      miFunctionMin = CreateMenuItem(null, res.Get("Min"), new EventHandler(Function_Click));
      miFunctionMin.AutoCheckOnClick = true;
      miFunctionMin.Tag = MatrixAggregateFunction.Min;
      miFunctionMax = CreateMenuItem(null, res.Get("Max"), new EventHandler(Function_Click));
      miFunctionMax.AutoCheckOnClick = true;
      miFunctionMax.Tag = MatrixAggregateFunction.Max;
      miFunctionAvg = CreateMenuItem(null, res.Get("Avg"), new EventHandler(Function_Click));
      miFunctionAvg.AutoCheckOnClick = true;
      miFunctionAvg.Tag = MatrixAggregateFunction.Avg;
      miFunctionCount = CreateMenuItem(null, res.Get("Count"), new EventHandler(Function_Click));
      miFunctionCount.AutoCheckOnClick = true;
      miFunctionCount.Tag = MatrixAggregateFunction.Count;

      miFunction.SubItems.AddRange(new BaseItem[] {
        miFunctionNone, miFunctionSum, miFunctionMin, miFunctionMax, miFunctionAvg, miFunctionCount });
      
      int insertIndex = Items.IndexOf(miDelete);
      Items.Insert(insertIndex, miFunction);
      
      MatrixAggregateFunction function = (Descriptor as MatrixCellDescriptor).Function;
      foreach (BaseItem item in miFunction.SubItems)
      {
        if ((MatrixAggregateFunction)item.Tag == function)
        {
          (item as ButtonItem).Checked = true;
          break;
        }
      }
    }
  }
}
