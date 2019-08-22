using System;
using System.Collections.Generic;
using System.Text;
using FastReport.Table;

namespace FastReport.Matrix
{
  internal class MatrixHeaderItem : MatrixDescriptor
  {
    private MatrixHeaderItem FParent;
    private List<MatrixHeaderItem> FItems;
    private object FValue;
    private int FIndex;
    private bool FIsTotal;
    private int FDataRowNo;
    private bool FPageBreak;

    public MatrixHeaderItem Parent
    {
      get { return FParent; }
    }

    public List<MatrixHeaderItem> Items
    {
      get { return FItems; }
    }

    public object Value
    {
      get { return FValue; }
      set { FValue = value; }
    }
    
    public int Index
    {
      get { return FIndex; }
      set { FIndex = value; }
    }
    
    public int Span
    {
      get 
      { 
        List<MatrixHeaderItem> terminalItems = new List<MatrixHeaderItem>();
        GetTerminalItems(terminalItems);
        return terminalItems.Count;
      }
    }
    
    public bool IsTotal
    {
      get { return FIsTotal; }
      set { FIsTotal = value; }
    }
    
    public object[] Values
    {
      get
      {
        int count = 0;
        MatrixHeaderItem item = this;
        
        while (item.Parent != null)
        {
          count++;
          item = item.Parent;
        }
        
        object[] values = new object[count];
        item = this;
        int index = count - 1;

        while (item.Parent != null)
        {
          values[index] = item.Value;
          index--;
          item = item.Parent;
        }
        
        return values;
      }
    }
    
    public int DataRowNo
    {
      get { return FDataRowNo; }
      set { FDataRowNo = value; }
    }
    
    public bool PageBreak
    {
      get { return FPageBreak; }
      set { FPageBreak = value; }
    }

    public int Find(object value, SortOrder sort)
    {
      if (Items.Count == 0)
        return -1;

      if (sort == SortOrder.None)
      {
        for (int i = 0; i < Items.Count; i++)
        {
          IComparable i1 = Items[i].Value as IComparable;

          int result = 0;
          if (i1 != null)
            result = i1.CompareTo(value);
          else if (value != null)
            result = -1;
            
          if (result == 0)
            return i;  
        }
        return ~Items.Count;
      }
      else
      {
        MatrixHeaderItem header = new MatrixHeaderItem(null);
        header.Value = value;
        return Items.BinarySearch(header, new HeaderComparer(sort));
      }  
    }

    public void Clear()
    {
      Items.Clear();
    }
    
    private void GetTerminalItems(List<MatrixHeaderItem> list)
    {
      if (Items.Count == 0)
        list.Add(this);
      else
      {
        foreach (MatrixHeaderItem item in Items)
        {
          item.GetTerminalItems(list);
        }
      }
    }

    public List<MatrixHeaderItem> GetTerminalItems()
    {
      List<MatrixHeaderItem> result = new List<MatrixHeaderItem>();
      GetTerminalItems(result);
      return result;
    }
    
    public MatrixHeaderItem(MatrixHeaderItem parent)
    {
      FParent = parent;
      FItems = new List<MatrixHeaderItem>();
    }


    private class HeaderComparer : IComparer<MatrixHeaderItem>
    {
      private SortOrder FSort;

      public int Compare(MatrixHeaderItem x, MatrixHeaderItem y)
      {
        int result = 0;
        IComparable i1 = x.Value as IComparable;
        IComparable i2 = y.Value as IComparable;

        if (i1 != null)
          result = i1.CompareTo(i2);
        else if (i2 != null)
          result = -1;
        if (FSort == SortOrder.Descending)
          result = -result;

        return result;
      }

      public HeaderComparer(SortOrder sort)
      {
        FSort = sort;
      }
    }
  }
}
