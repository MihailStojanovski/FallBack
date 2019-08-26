using System;
using System.Collections.Generic;
using System.Text;
using FastReport.Preview;

namespace FastReport.Engine
{
  public partial class ReportEngine
  {
    private void RenderInnerSubreports(BandBase parentBand)
    {
      int originalObjectsCount = parentBand.Objects.Count;
      for (int i = 0; i < originalObjectsCount; i++)
      {
        SubreportObject subreport = parentBand.Objects[i] as SubreportObject;
        if (subreport != null && subreport.Visible && subreport.PrintOnParent)
          RenderInnerSubreport(parentBand, subreport);
      }
    }
    
    private void RenderOuterSubreports(BandBase parentBand)
    {
      float saveCurX = CurX;
      float saveCurY = CurY;
      float saveOriginX = FOriginX;
      int saveCurPage = CurPage;
      
      float maxY = CurY;
      int maxPage = CurPage;
      
      try
      {
        for (int i = 0; i < parentBand.Objects.Count; i++)
        {
          SubreportObject subreport = parentBand.Objects[i] as SubreportObject;
          if (subreport != null && subreport.Visible && !subreport.PrintOnParent)
          {
            // restore start position
            CurPage = saveCurPage;
            CurY = saveCurY - subreport.Height;
            FOriginX = saveOriginX + subreport.Left;
            // do not upload generated pages to the file cache
            PreparedPages.CanUploadToCache = false;
            
            RenderSubreport(subreport);
            
            // find maxY. We will continue from maxY when all subreports finished.
            if (CurPage == maxPage)
            {
              if (CurY > maxY)
                maxY = CurY;
            }
            else if (CurPage > maxPage)
            {
              maxPage = CurPage;
              maxY = CurY;
            }
          }  
        }
      }
      finally
      {
        CurPage = maxPage;
        CurY = maxY;
        FOriginX = saveOriginX;
        PreparedPages.CanUploadToCache = true;
      }
    }

    private void RenderInnerSubreport(BandBase parentBand, SubreportObject subreport)
    {
      BandBase saveOutputBand = FOutputBand;
      float saveCurX = CurX;
      float saveCurY = CurY;
      
      try
      {
        FOutputBand = parentBand;
        CurX = subreport.Left;
        CurY = subreport.Top;
        
        RenderSubreport(subreport);
      }
      finally
      {  
        FOutputBand = saveOutputBand;
        CurX = saveCurX;
        CurY = saveCurY;
      }  
    }

    private void RenderSubreport(SubreportObject subreport)
    {
      RunBands(subreport.ReportPage.Bands);
    }
  }
}
