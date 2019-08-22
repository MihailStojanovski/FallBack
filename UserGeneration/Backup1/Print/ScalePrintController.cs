using System;
using System.Drawing.Printing;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using FastReport.Utils;
using System.Drawing;

namespace FastReport.Print
{
  internal class ScalePrintController : PrintControllerBase
  {
    #region Fields
    private float FSheetWidth;
    private float FSheetHeight;
    private bool FFirstTime = true;
    #endregion

    public override void QueryPageSettings(object sender, QueryPageSettingsEventArgs e)
    {
      if (FFirstTime)
        Page = GetNextPage();
      FFirstTime = false;

      if (Page != null)
      {
        SetPaperSize(FSheetWidth, FSheetHeight, e);

        // rotate page if print 2 or 8 pages per sheet
        bool rotate = Report.PrintSettings.PagesOnSheet == PagesOnSheet.Two ||
          Report.PrintSettings.PagesOnSheet == PagesOnSheet.Eight;
        e.PageSettings.Landscape = rotate ? !Page.Landscape : Page.Landscape;

        SetPaperSource(Page, e);
        Duplex duplex = Page.Duplex;
        if (duplex != Duplex.Default)
          e.PageSettings.PrinterSettings.Duplex = duplex;
      }
    }

    public override void PrintPage(object sender, PrintPageEventArgs e)
    {
      StartPage(e);

      if (Page != null)
      {
        int countX = 0;
        int countY = 0;
        float scale = 0;
        
        // switch dimensions because FSheetWidth, FSheetHeight is a portrait dimensions
        float sheetWidth = FSheetWidth;
        float sheetHeight = FSheetHeight;
        if (Page.Landscape && Page.PaperWidth > Page.PaperHeight)
        {
          sheetWidth = FSheetHeight;
          sheetHeight = FSheetWidth;
        }

        switch (Report.PrintSettings.PagesOnSheet)
        {
          case PagesOnSheet.One:
            countX = 1;
            countY = 1;
            scale = Math.Min(sheetWidth / Page.PaperWidth, sheetHeight / Page.PaperHeight);
            break;

          case PagesOnSheet.Two:
            countX = 2;
            countY = 1;
            scale = Math.Min(sheetHeight / Page.PaperWidth, sheetWidth / Page.PaperHeight);
            break;

          case PagesOnSheet.Four:
            countX = 2;
            countY = 2;
            scale = Math.Min(sheetWidth / Page.PaperWidth, sheetHeight / Page.PaperHeight) / 2;
            break;

          case PagesOnSheet.Eight:
            countX = 4;
            countY = 2;
            scale = Math.Min(sheetHeight / Page.PaperWidth, sheetWidth / Page.PaperHeight) / 2;
            break;
        }
        
        float pieceX = Page.PaperWidth * scale;
        float pieceY = Page.PaperHeight * scale;
        float leftMargin = e.PageSettings.HardMarginX / 100f * 25.4f;
        float topMargin = e.PageSettings.HardMarginY / 100f * 25.4f;

        float offsY = -topMargin;

        for (int y = 0; y < countY; y++)
        {
          float offsX = -leftMargin;

          for (int x = 0; x < countX; x++)
          {
            Graphics g = e.Graphics;
            GraphicsState state = g.Save();
            try
            {
              g.PageUnit = GraphicsUnit.Pixel;
              g.TranslateTransform(offsX * Units.Millimeters * g.DpiX / 96, offsY * Units.Millimeters * g.DpiY / 96);
              g.ScaleTransform(scale, scale);
              FRPaintEventArgs paintArgs = new FRPaintEventArgs(g, g.DpiX / 96, g.DpiY / 96, Report.GraphicCache);
              Page.Print(paintArgs);
#if Demo
      using (Watermark trialWatermark = new Watermark())
      {
        trialWatermark.Text = typeof(Duplex).Name[0].ToString() + typeof(Exception).Name[0].ToString() +
          typeof(Margins).Name[0].ToString() + typeof(Object).Name[0].ToString() + " " +
          typeof(ValueType).Name[0].ToString() + typeof(Exception).Name[0].ToString() +
          typeof(Rectangle).Name[0].ToString() + typeof(ShapeKind).Name[0].ToString() +
          typeof(ICloneable).Name[0].ToString() + typeof(Object).Name[0].ToString() +
          typeof(NonSerializedAttribute).Name[0].ToString();
        trialWatermark.DrawText(paintArgs, new RectangleF(0, 0, 
          Page.PaperWidth * Units.Millimeters, Page.PaperHeight * Units.Millimeters), Report, true);
      }
#endif
            }
            finally
            {
              g.Restore(state);
            }

            offsX += pieceX;
            Page.Dispose();
            Page = GetNextPage();
            if (Page == null) 
              break;
          }

          if (Page == null) 
            break;
          offsY += pieceY;
        }
      }

      FinishPage(e);
      e.HasMorePages = Page != null;
    }

    public ScalePrintController(Report report, PrintDocument doc, int curPage) : base(report, doc, curPage)
    {
      FSheetWidth = Report.PrintSettings.PrintOnSheetWidth;
      FSheetHeight = Report.PrintSettings.PrintOnSheetHeight;
    }
  }
}