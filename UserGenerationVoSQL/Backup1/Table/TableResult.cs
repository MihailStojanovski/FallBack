using System;
using System.Collections.Generic;
using System.Text;
using FastReport.Engine;
using FastReport.Preview;
using System.Drawing;

namespace FastReport.Table
{
  /// <summary>
  /// Represents a result table.
  /// </summary>
  /// <remarks>
  /// Do not use this class directly. It is used by the <see cref="TableObject"/> and 
  /// <see cref="FastReport.Matrix.MatrixObject"/> objects to render a result.
  /// </remarks>
  public class TableResult : TableBase
  {
    private bool FSkip;
    
    /// <summary>
    /// Occurs after calculation of table bounds.
    /// </summary>
    /// <remarks>
    /// You may use this event to change automatically calculated rows/column sizes. It may be useful
    /// if you need to fit dynamically printed table on a page.
    /// </remarks>
    public event EventHandler AfterCalcBounds;
    
    internal bool Skip
    {
      get { return FSkip; }
      set { FSkip = value; }
    }

    private float GetRowsHeight(int startRow, int count)
    {
      float height = 0;

      // include row header
      if (startRow != 0 && RepeatHeaders)
      {
        for (int i = 0; i < FixedRows; i++)
        {
          height += Rows[i].Height;
        }
      }

      for (int i = 0; i < count; i++)
      {
        height += Rows[startRow + i].Height;
      }
      
      return height;
    }

    private float GetColumnsWidth(int startColumn, int count)
    {
      float width = 0;

      // include column header
      if (startColumn != 0 && RepeatHeaders)
      {
        for (int i = 0; i < FixedColumns; i++)
        {
          width += Columns[i].Width;
        }
      }

      for (int i = 0; i < count; i++)
      {
        width += Columns[startColumn + i].Width;
      }

      return width;
    }

    private int GetRowsFit(int startRow, float freeSpace)
    {
      int rowsFit = 0;
      
      while (startRow + rowsFit < Rows.Count && 
        (rowsFit == 0 || !Rows[startRow + rowsFit].PageBreak) && 
        GetRowsHeight(startRow, rowsFit + 1) <= freeSpace)
      {
        rowsFit++;
      }

      return rowsFit;
    }

    private int GetColumnsFit(int startColumn, float freeSpace)
    {
      int columnsFit = 0;
      
      while (startColumn + columnsFit < Columns.Count && 
        (columnsFit == 0 || !Columns[startColumn + columnsFit].PageBreak) &&
        GetColumnsWidth(startColumn, columnsFit + 1) <= freeSpace)
      {
        columnsFit++;
      }
      
      return columnsFit;
    }

    internal void GeneratePages(object sender, EventArgs e)
    {
      if (Skip)
      {
        Skip = false;
        return;
      }
      
      // check if band contains several tables
      if (sender is BandBase)
      {
        SortedList<float, TableBase> tables = new SortedList<float,TableBase>();
        foreach (Base obj in (sender as BandBase).Objects)
        {
          TableBase table = obj as TableBase;
          if (table != null && table.ResultTable != null)
            tables.Add(table.Left, table);
        }

        // render tables side-by-side
        if (tables.Count > 1)
        {
          ReportEngine engine = Report.Engine;
          TableLayoutInfo info = new TableLayoutInfo();
          info.StartPage = engine.CurPage;
          info.TableSize = new Size(1, 1);
          info.StartX = tables.Values[0].Left;
          
          int firstTableHeight = 0;
          int startPage = info.StartPage;
          float firstTableCurY = 0;
          float saveCurY = engine.CurY;
          
          for (int i = 0; i < tables.Count; i++)
          {
            TableBase table = tables.Values[i];
            
            // do not allow table to render itself in the band.AfterPrint event
            table.ResultTable.Skip = true;
            // render using the down-then-across mode
            table.Layout = TableLayout.DownThenAcross;
            
            engine.CurPage = info.StartPage + (info.TableSize.Width - 1) * info.TableSize.Height;
            engine.CurY = saveCurY;
            float addLeft = 0;
            if (i > 0)
              addLeft = table.Left - tables.Values[i - 1].Right;
            table.ResultTable.Left = info.StartX + addLeft;
            Report.PreparedPages.AddPageAction = AddPageAction.WriteOver;
            info = table.ResultTable.GeneratePagesDownThenAcross();
            
            if (i == 0)
            {
              firstTableHeight = info.TableSize.Height;
              firstTableCurY = engine.CurY;
            }  
          }
          
          engine.CurPage = startPage + firstTableHeight - 1;
          engine.CurY = firstTableCurY;
          
          Skip = false;
          return;
        }
      }
      
      // calculate cells' bounds
      CalcHeight();
      
      // fire AfterCalcBounds event
      if (AfterCalcBounds != null)
        AfterCalcBounds(this, EventArgs.Empty);
      
      if (Layout == TableLayout.AcrossThenDown)
        GeneratePagesAcrossThenDown();
      else if (Layout == TableLayout.DownThenAcross)
        GeneratePagesDownThenAcross();
      else
        GeneratePagesWrapped();  
    }

    private void GeneratePagesAcrossThenDown()
    {
      ReportEngine engine = Report.Engine;
      PreparedPages preparedPages = Report.PreparedPages;
      preparedPages.CanUploadToCache = false;

      List<Rectangle> spans = GetSpanList();
      int startRow = 0;
      bool addNewPage = false;
      float freeSpace = engine.FreeSpace;
      Top = 0;

      while (startRow < Rows.Count)
      {
        if (addNewPage)
        {
          engine.StartNewPage();
          freeSpace = engine.FreeSpace;
        }

        int startColumn = 0;
        int rowsFit = GetRowsFit(startRow, freeSpace);
        // avoid the infinite loop if there is not enough space for one row
        if (startRow > 0 && rowsFit == 0)
          rowsFit = 1;

        int saveCurPage = engine.CurPage;
        float saveLeft = Left;
        float saveCurY = engine.CurY;
        float curY = engine.CurY;

        if (rowsFit != 0)
        {
          while (startColumn < Columns.Count)
          {
            int columnsFit = GetColumnsFit(startColumn, engine.PageWidth - Left);
            // avoid the infinite loop if there is not enough space for one column
            if (startColumn > 0 && columnsFit == 0)
              columnsFit = 1;

            engine.CurY = saveCurY;
            curY = GeneratePage(startColumn, startRow, columnsFit, rowsFit,
              new RectangleF(0, 0, engine.PageWidth, freeSpace), spans) + saveCurY;

            Left = 0;
            startColumn += columnsFit;
            if (startColumn < Columns.Count)
            {
              // if we have something to print, start a new page
              engine.StartNewPage();
            }  
            else if (engine.CurPage > saveCurPage)
            {
              // finish the last printed page in case it is not a start page
              engine.EndPage(false);
            }  
          }
        }

        startRow += rowsFit;
        Left = saveLeft;
        engine.CurPage = saveCurPage;
        engine.CurY = curY;
        preparedPages.AddPageAction = AddPageAction.Add;
        addNewPage = true;
      }
    }

    private TableLayoutInfo GeneratePagesDownThenAcross()
    {
      ReportEngine engine = Report.Engine;
      PreparedPages preparedPages = Report.PreparedPages;
      preparedPages.CanUploadToCache = false;

      TableLayoutInfo info = new TableLayoutInfo();
      info.StartPage = engine.CurPage;
      List<Rectangle> spans = GetSpanList();
      int startColumn = 0;
      bool addNewPage = false;
      float saveCurY = engine.CurY;
      float lastCurY = 0;
      int lastPage = 0;
      Top = 0;
      
      while (startColumn < Columns.Count)
      {
        if (addNewPage)
          engine.StartNewPage();

        int startRow = 0;
        int columnsFit = GetColumnsFit(startColumn, engine.PageWidth - Left);
        // avoid the infinite loop if there is not enough space for one column
        if (startColumn > 0 && columnsFit == 0)
          columnsFit = 1;

        engine.CurY = saveCurY;
        info.TableSize.Width++;
        info.TableSize.Height = 0;
        
        if (columnsFit > 0)
        {
          while (startRow < Rows.Count)
          {
            int rowsFit = GetRowsFit(startRow, engine.FreeSpace);
            // avoid the infinite loop if there is not enough space for one row
            if (startRow > 0 && rowsFit == 0)
              rowsFit = 1;
            
            engine.CurY += GeneratePage(startColumn, startRow, columnsFit, rowsFit,
              new RectangleF(0, 0, engine.PageWidth, engine.FreeSpace), spans);
            info.TableSize.Height++;

            startRow += rowsFit;
            if (startRow < Rows.Count)
            {
              // if we have something to print, start a new page
              engine.StartNewPage();
            }
            else if (startColumn > 0)
            {
              // finish the last printed page in case it is not a start page
              engine.EndPage(false);
            }
          }
        }

        info.StartX = Left + GetColumnsWidth(startColumn, columnsFit);
        startColumn += columnsFit;
        Left = 0;
        preparedPages.AddPageAction = AddPageAction.Add;
        addNewPage = true;

        if (lastPage == 0)
        {
          lastPage = engine.CurPage;
          lastCurY = engine.CurY;
        }
      }
      
      engine.CurPage = lastPage;
      engine.CurY = lastCurY;
      return info;
    }

    private void GeneratePagesWrapped()
    {
      ReportEngine engine = Report.Engine;
      PreparedPages preparedPages = Report.PreparedPages;
      preparedPages.CanUploadToCache = false;

      List<Rectangle> spans = GetSpanList();
      int startColumn = 0;
      Top = 0;

      while (startColumn < Columns.Count)
      {
        int startRow = 0;
        int columnsFit = GetColumnsFit(startColumn, engine.PageWidth - Left);
        // avoid the infinite loop if there is not enough space for one column
        if (startColumn > 0 && columnsFit == 0)
          columnsFit = 1;

        while (startRow < Rows.Count)
        {
          int rowsFit = GetRowsFit(startRow, engine.FreeSpace);
          if (rowsFit == 0)
          {
            engine.StartNewPage();
            rowsFit = GetRowsFit(startRow, engine.FreeSpace);
          }
          
          engine.CurY += GeneratePage(startColumn, startRow, columnsFit, rowsFit,
            new RectangleF(0, 0, engine.PageWidth, engine.FreeSpace), spans);

          startRow += rowsFit;
        }

        startColumn += columnsFit;
        if (startColumn < Columns.Count)
          engine.CurY += WrappedGap;
      }
    }

    private float GeneratePage(int startColumn, int startRow, int columnsFit, int rowsFit,
      RectangleF bounds, List<Rectangle> spans)
    {
      // break spans
      foreach (Rectangle span in spans)
      {
        TableCellData spannedCell = GetCellData(span.Left, span.Top);
        TableCellData newSpannedCell = null;
        if (span.Left < startColumn && span.Right > startColumn)
        {
          if (RepeatHeaders && span.Left < FixedColumns)
            spannedCell.ColSpan = Math.Min(span.Right, startColumn + columnsFit) - startColumn + FixedColumns;
          else
          {
            newSpannedCell = GetCellData(startColumn, span.Top);
            newSpannedCell.RunTimeAssign(spannedCell.Cell, true);
            newSpannedCell.ColSpan = Math.Min(span.Right, startColumn + columnsFit) - startColumn;
            newSpannedCell.RowSpan = spannedCell.RowSpan;
          }
        }
        if (span.Left < startColumn + columnsFit && span.Right > startColumn + columnsFit)
        {
          spannedCell.ColSpan = startColumn + columnsFit - span.Left;
        }
        if (span.Top < startRow && span.Bottom > startRow)
        {
          if (RepeatHeaders && span.Top < FixedRows)
            spannedCell.RowSpan = Math.Min(span.Bottom, startRow + rowsFit) - startRow + FixedRows;
        }
        if (span.Top < startRow + rowsFit && span.Bottom > startRow + rowsFit)
        {
          spannedCell.RowSpan = startRow + rowsFit - span.Top;

          newSpannedCell = GetCellData(span.Left, startRow + rowsFit);
          newSpannedCell.RunTimeAssign(spannedCell.Cell, true);
          newSpannedCell.ColSpan = spannedCell.ColSpan;
          newSpannedCell.RowSpan = span.Bottom - (startRow + rowsFit);

          // break the cell text
          TableCell cell = spannedCell.Cell;
          using (TextObject tempObject = new TextObject())
          {
            if (!cell.Break(tempObject))
              cell.Text = "";
            if (cell.CanBreak)
              newSpannedCell.Text = tempObject.Text;
          }
          
          // fix the row height
          float textHeight = newSpannedCell.Cell.CalcHeight();
          float rowsHeight = 0;
          for (int i = 0; i < newSpannedCell.RowSpan; i++)
          {
            rowsHeight += Rows[i + startRow + rowsFit].Height;
          }
          
          if (rowsHeight < textHeight)
          {
            // fix the last row's height
            Rows[startRow + rowsFit + newSpannedCell.RowSpan - 1].Height += textHeight - rowsHeight;
          }
        }
      }

      // set visible columns
      for (int i = 0; i < Columns.Count; i++)
      {
        if (!RepeatHeaders || i >= FixedColumns)
          Columns[i].Visible = i >= startColumn && i < startColumn + columnsFit;
      }

      // set visible rows
      for (int i = 0; i < Rows.Count; i++)
      {
        if (!RepeatHeaders || i >= FixedRows)
          Rows[i].Visible = i >= startRow && i < startRow + rowsFit;
      }

      DataBand band = new DataBand();
      band.Bounds = bounds;
      band.Objects.Add(this);
      Report.Engine.AddToPreparedPages(band);
      
      return GetRowsHeight(startRow, rowsFit);
    }
    
    
    private class TableLayoutInfo
    {
      public int StartPage;
      public Size TableSize;
      public float StartX;
    }
  }
}
