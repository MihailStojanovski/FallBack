using System;
using System.Collections.Generic;
using System.Text;
using FastReport.Utils;

namespace FastReport.Engine
{
  public partial class ReportEngine
  {
    private BandBase FKeepBand;
    private bool FKeeping;
    private int FKeepPosition;
    private XmlItem FKeepOutline;
    private int FKeepBookmarks;
    private float FKeepCurY;
    private float FKeepDeltaY;
    
    private void StartKeep(BandBase band)
    {
      // do not keep the first row on a page, avoid empty first page
      if (FKeeping || band.AbsRowNo == 1)
        return;
      FKeeping = true;

      FKeepBand = band;
      FKeepPosition = PreparedPages.CurPosition;
      FKeepOutline = PreparedPages.Outline.CurPosition;
      FKeepBookmarks = PreparedPages.Bookmarks.CurPosition;
      FKeepCurY = CurY;
      Report.Dictionary.Totals.StartKeep();
      StartKeepReprint();
    }
    
    private void EndKeep()
    {
      if (FKeeping)
      {
        Report.Dictionary.Totals.EndKeep();
        EndKeepReprint();
        FKeepBand = null;
        FKeeping = false;
      }
    }
    
    private void CutObjects()
    {
      FKeepDeltaY = CurY - FKeepCurY;
      PreparedPages.CutObjects(FKeepPosition);
      CurY = FKeepCurY;
    }
    
    private void PasteObjects()
    {
      PreparedPages.PasteObjects(FOriginX + CurX, CurY);
      PreparedPages.Outline.Shift(FKeepOutline, CurY);
      PreparedPages.Bookmarks.Shift(FKeepBookmarks, CurY);
      EndKeep();
      CurY += FKeepDeltaY;
    }
  }
}
