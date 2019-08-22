using System;
using System.Collections.Generic;
using System.Text;
using FastReport.Data;
using System.Collections;

namespace FastReport.Engine
{
  public partial class ReportEngine
  {
    private void RunDataBand(DataBand dataBand)
    {
      dataBand.InitDataSource();
      dataBand.DataSource.First();

      int rowCount = dataBand.DataSource.RowCount;
      bool keepFirstRow = NeedKeepFirstRow(dataBand);
      bool keepLastRow = NeedKeepLastRow(dataBand);

      RunDataBand(dataBand, rowCount, keepFirstRow, keepLastRow);
    }

    private void RunDataBand(DataBand dataBand, int rowCount, bool keepFirstRow, bool keepLastRow)
    {
      if (dataBand.IsHierarchical)
      {
        ShowHierarchy(dataBand, rowCount);
        return;
      }

      bool isFirstRow = true;
      bool someRowsPrinted = false;
      dataBand.RowNo = 0;
      dataBand.IsFirstRow = false;
      dataBand.IsLastRow = false;

      // check if we have only one data row that should be kept with both header and footer
      bool oneRow = rowCount == 1 && keepFirstRow && keepLastRow;

      // cycle through records
      for (int i = 0; i < rowCount; i++)
      {
        if (!dataBand.IsDetailEmpty())
        {
          dataBand.RowNo++;
          dataBand.AbsRowNo++;
          bool isLastRow = i == rowCount - 1;
          dataBand.IsFirstRow = isFirstRow;
          dataBand.IsLastRow = isLastRow;

          // keep header
          if (isFirstRow && keepFirstRow)
            StartKeep(dataBand);

          // keep together
          if (isFirstRow && dataBand.KeepTogether)
            StartKeep(dataBand);

          // keep detail
          if (dataBand.KeepDetail)
            StartKeep(dataBand);  
          
          // show header
          if (isFirstRow)
            ShowDataHeader(dataBand);

          // keep footer
          if (isLastRow && keepLastRow && dataBand.IsDeepmostDataBand)
            StartKeep(dataBand);

          // start block event
          if (isFirstRow)
            OnStateChanged(dataBand, EngineState.BlockStarted);

          // show band
          ShowDataBand(dataBand, rowCount);

          // end keep header
          if (isFirstRow && keepFirstRow && !oneRow)
            EndKeep();

          // end keep footer
          if (isLastRow && keepLastRow && dataBand.IsDeepmostDataBand)
            CheckKeepFooter(dataBand);

          // show sub-bands
          RunBands(dataBand.Bands);

          // up the outline
          OutlineUp(dataBand);

          // end keep detail
          if (dataBand.KeepDetail)
            EndKeep();

          isFirstRow = false;
          someRowsPrinted = true;
          
          if (dataBand.Columns.Count > 1)
            break;
        }
        
        dataBand.DataSource.Next();
        if (Report.Aborted)
          break;
      }

      // complete upto N rows
      ChildBand child = dataBand.Child;
      if (child != null && child.CompleteToNRows > rowCount)
      {
        for (int i = 0; i < child.CompleteToNRows - rowCount; i++)
        {
          child.RowNo = rowCount + i + 1;
          child.AbsRowNo = rowCount + i + 1;
          ShowBand(child);
        }
      }

      if (someRowsPrinted)
      {
        // finish block event
        OnStateChanged(dataBand, EngineState.BlockFinished);

        // show footer
        ShowDataFooter(dataBand);
        
        // end KeepTogether
        if (dataBand.KeepTogether)
          EndKeep();

        // end KeepLastRow
        if (keepLastRow)
          EndKeep();
      }
    }

    private void ShowDataBand(DataBand dataBand, int rowCount)
    {
      if (dataBand.Columns.Count > 1)
      {
        dataBand.Width = dataBand.Columns.ActualWidth;
        RenderMultiColumnBand(dataBand, rowCount);
      }
      else
      {
        ShowBand(dataBand);
      }  
    }

    private void RenderMultiColumnBand(DataBand dataBand, int rowCount)
    {
      if (dataBand.Columns.Layout == ColumnLayout.AcrossThenDown)
        RenderBandAcrossThenDown(dataBand, rowCount);
      else
      {
        DataSourceBase dataSource = dataBand.DataSource;
        int saveRow = dataSource.CurrentRowNo;

        // calc height of each data row. This list is shared across RenderBandDownThenAcross calls. 
        Hashtable heights = new Hashtable();
        for (int i = 0; i < rowCount; i++)
        {
          dataSource.CurrentRowNo = i + saveRow;
          heights[i + saveRow] = CalcHeight(dataBand);
        }

        dataSource.CurrentRowNo = saveRow;
        while (rowCount > 0)
        {
          rowCount = RenderBandDownThenAcross(dataBand, rowCount, heights);
        }  
      }  
    }
    
    private void RenderBandAcrossThenDown(DataBand dataBand, int rowCount)
    {
      DataSourceBase dataSource = dataBand.DataSource;
      int saveRow = dataSource.CurrentRowNo;

      // create output band
      using (DataBand outputBand = new DataBand())
      {
        outputBand.SetReport(Report);

        int columnNo = 0;
        for (int i = 0; i < rowCount; i++)
        {
          dataSource.CurrentRowNo = i + saveRow;
          if (columnNo == 0)
          {
            outputBand.Clear();
            outputBand.Assign(dataBand);
            outputBand.OutlineExpression = "";
            outputBand.Border = new Border();
            outputBand.Fill = new SolidFill();
          }

          // write to the output band
          ShowBand(dataBand, outputBand, dataBand.Columns.Positions[columnNo], 0);
          // add outline
          AddBandOutline(dataBand);
          // outline up
          OutlineUp(dataBand);

          dataBand.RowNo++;
          dataBand.AbsRowNo++;
          columnNo++;
          if (columnNo == dataBand.Columns.Count)
          {
            columnNo = 0;
            // show output band itself
            ShowBand(outputBand, false);
          }
        }

        // show last portion
        if (columnNo != 0)
          ShowBand(outputBand, false);
      }

      dataSource.CurrentRowNo = saveRow + rowCount;
    }

    private int RenderBandDownThenAcross(DataBand dataBand, int rowCount, Hashtable heights)
    {
      DataSourceBase dataSource = dataBand.DataSource;
      int saveRow = dataSource.CurrentRowNo;

      // determine number of rows in a column. Do not take the height into account - it's too complex.
      int rowsPerColumn = (int)Math.Ceiling((float)rowCount / dataBand.Columns.Count);
      if (rowsPerColumn < dataBand.Columns.MinRowCount)
        rowsPerColumn = dataBand.Columns.MinRowCount;

      // calculate max height of all columns to check the free space
      float maxHeight = 0;
      for (int i = 0; i < dataBand.Columns.Count; i++)
      {
        // calculate column height
        float columnHeight = 0;
        for (int j = 0; j < rowsPerColumn; j++)
        {
          int rowIndex = j + i * rowsPerColumn + saveRow;
          if (heights[rowIndex] == null)
            break;
          columnHeight += (float)heights[rowIndex];
        }

        if (columnHeight > maxHeight)
          maxHeight = columnHeight;
      }

      float saveCurX = CurX;
      float startColumnY = CurY;
      int columnNo = 0;

      if (maxHeight > FreeSpace)
      {
        // not enough free space. Render rows down then across. After finishing the page,
        // run this method again to render remaining rows.
        
        for (int i = 0; i < rowCount; i++)
        {
          dataSource.CurrentRowNo = i + saveRow;
          CurX = dataBand.Columns.Positions[columnNo] + saveCurX;

          // check free space. 
          if ((float)heights[i + saveRow] > FreeSpace && i != 0)
          {
            columnNo++;
            if (columnNo == dataBand.Columns.Count)
            {
              // start the new page
              columnNo = 0;
              CurX = saveCurX;
              EndPage();
              // decrease number of available rows and call this method again
              rowCount -= i;
              return rowCount;
            }
            else
            {
              // start the new column
              CurY = startColumnY;
            }
            // check free space again before show a band - we may be at a page end
            i--;
            continue;
          }

          // show a band
          CurX = dataBand.Columns.Positions[columnNo] + saveCurX;
          ShowBand(dataBand);
          dataBand.RowNo++;
          dataBand.AbsRowNo++;
        }
        // we shouldn't go here...
      }
      else
      {
        // we have enough free space to render all rows.
        float maxY = CurY;
        int rowNo = 0;

        for (int i = 0; i < rowCount; i++)
        {
          dataSource.CurrentRowNo = i + saveRow;
          
          // show a band
          CurX = dataBand.Columns.Positions[columnNo] + saveCurX;
          ShowBand(dataBand);
          // outline up
          OutlineUp(dataBand);
          if (CurY > maxY)
            maxY = CurY;

          dataBand.RowNo++;
          dataBand.AbsRowNo++;
          rowNo++;
          // check if we need to start a new column
          if (rowNo >= rowsPerColumn)
          {
            columnNo++;
            rowNo = 0;
            CurY = startColumnY;
          }
        }

        CurX = saveCurX;
        CurY = maxY;
      }  

      dataSource.CurrentRowNo = saveRow + rowCount;
      return 0;
    }
    
    private float GetColumnHeight(Hashtable heights, int startIndex, int rowCount, int rowNo)
    {
      float result = 0;
      for (int i = 0; i < rowCount; i++)
      {
        if (i + startIndex >= heights.Count)
          break;
        result += (float)heights[i + startIndex + rowNo];
      }
      return result;
    }

    private HierarchyItem MakeHierarchy(DataBand dataBand, int rowCount)
    {
      Dictionary<string, HierarchyItem> items = new Dictionary<string, HierarchyItem>();

      Column idColumn = DataHelper.GetColumn(Report.Dictionary, dataBand.IdColumn);
      if (idColumn == null)
        return null;

      Column parentIdColumn = DataHelper.GetColumn(Report.Dictionary, dataBand.ParentIdColumn);
      if (parentIdColumn == null)
        return null;

      for (int i = 0; i < rowCount; i++)
      {
        object idColumnValue = dataBand.DataSource[idColumn];
        object parentIdColumnValue = dataBand.DataSource[parentIdColumn];
        string id = idColumnValue == null || idColumnValue is DBNull ? "" : idColumnValue.ToString();
        string parentId = parentIdColumnValue == null || parentIdColumnValue is DBNull ? "" : parentIdColumnValue.ToString();

        HierarchyItem item = null;
        if (items.ContainsKey(id))
          item = items[id];
        else
        {
          item = new HierarchyItem();
          items.Add(id, item);
        }
        item.RowNo = dataBand.DataSource.CurrentRowNo;

        HierarchyItem parentItem = null;
        if (items.ContainsKey(parentId))
          parentItem = items[parentId];
        else
        {
          parentItem = new HierarchyItem();
          items.Add(parentId, parentItem);
        }
        parentItem.Add(item);

        dataBand.DataSource.Next();
      }

      // create the root item
      HierarchyItem rootItem = new HierarchyItem();
      foreach (HierarchyItem item in items.Values)
      {
        if (item.Parent == null)
        {
          foreach (HierarchyItem childItem in item.Items)
          {
            rootItem.Add(childItem);
          }
        }
      }

      return rootItem;
    }

    private void ShowHierarchy(DataBand dataBand, HierarchyItem rootItem, int level)
    {
      int saveLevel = FHierarchyLevel;
      float saveIndent = FHierarchyIndent;
      FHierarchyLevel = level;
      FHierarchyIndent = dataBand.Indent * (level - 1);

      try
      {
        if (rootItem.Items.Count > 0)
        {
          ShowBand(dataBand.Header);

          dataBand.RowNo = 0;
          foreach (HierarchyItem item in rootItem.Items)
          {
            dataBand.RowNo++;
            dataBand.AbsRowNo++;
            dataBand.DataSource.CurrentRowNo = item.RowNo;
            
            // show the main hierarchy band
            ShowBand(dataBand);

            // show sub-bands if any
            RunBands(dataBand.Bands);
            
            ShowHierarchy(dataBand, item, level + 1);
          }

          ShowBand(dataBand.Footer);
        }
      }
      finally
      {
        FHierarchyLevel = saveLevel;
        FHierarchyIndent = saveIndent;
      }
    }
    
    private void ShowHierarchy(DataBand dataBand, int rowCount)
    {
      HierarchyItem rootItem = MakeHierarchy(dataBand, rowCount);
      if (rootItem == null)
        return;

      ShowHierarchy(dataBand, rootItem, 1);
    }
    
    private void ShowDataHeader(DataBand dataBand)
    {
      DataHeaderBand header = dataBand.Header;
      if (header != null)
      {
        ShowBand(header);
        if (header.RepeatOnEveryPage)
          AddReprint(header);
      }
      
      DataFooterBand footer = dataBand.Footer;
      if (footer != null)
      {
        if (footer.RepeatOnEveryPage)
          AddReprint(footer);
      }
    }

    private void ShowDataFooter(DataBand dataBand)
    {
      DataFooterBand footer = dataBand.Footer;
      RemoveReprint(footer);
      ShowBand(footer);
      RemoveReprint(dataBand.Header);
    }

    
    private class HierarchyItem
    {
      public int RowNo;
      public List<HierarchyItem> Items;
      public HierarchyItem Parent;

      public void Add(HierarchyItem item)
      {
        Items.Add(item);
        item.Parent = this;
      }

      public HierarchyItem()
      {
        Items = new List<HierarchyItem>();
      }
    }
  }
}
