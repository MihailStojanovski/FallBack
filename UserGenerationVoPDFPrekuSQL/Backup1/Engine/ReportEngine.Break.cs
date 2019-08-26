using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FastReport.Engine
{
  public partial class ReportEngine
  {
    private void BreakBand(BandBase band)
    {
      BandBase cloneBand = Activator.CreateInstance(band.GetType()) as BandBase;
      cloneBand.Assign(band);
      cloneBand.FlagMustBreak = band.FlagMustBreak;
      
      // clone band objects:
      // - remove bands, convert them to Text objects if necessary
      // - skip subreports
      foreach (Base c in band.Objects)
      {
        if (c is BandBase)
        {
          BandBase b = c as BandBase;
          if (b.HasBorder || b.HasFill)
          {
            TextObject textObj = new TextObject();
            textObj.Bounds = b.Bounds;
            textObj.Border = b.Border.Clone();
            textObj.Fill = b.Fill.Clone();
            cloneBand.Objects.Add(textObj);
          }

          foreach (ReportComponentBase obj in b.Objects)
          {
            if (!(obj is BandBase))
            {
              ReportComponentBase cloneObj = Activator.CreateInstance(obj.GetType()) as ReportComponentBase;
              cloneObj.AssignAll(obj);
              cloneObj.Anchor = AnchorStyles.Left | AnchorStyles.Top;
              cloneObj.Dock = DockStyle.None;
              cloneObj.Left = obj.AbsLeft;
              cloneObj.Top = obj.AbsTop;
              if (cloneObj is TextObject)
                (cloneObj as TextObject).Highlight.Clear();
              cloneBand.Objects.Add(cloneObj);
            }
          }
        }
        else if (!(c is SubreportObject))
        {
          Base cloneObj = Activator.CreateInstance(c.GetType()) as Base;
          cloneObj.AssignAll(c);
          cloneBand.Objects.Add(cloneObj);
        }
      }
      
      BandBase breakTo = Activator.CreateInstance(band.GetType()) as BandBase;
      breakTo.Assign(band);
      breakTo.Child = null;
      breakTo.CanGrow = true;
      breakTo.BeforePrintEvent = "";
      breakTo.BeforeLayoutEvent = "";
      breakTo.AfterPrintEvent = "";
      breakTo.AfterLayoutEvent = "";
      // breakTo must be breaked because it will print on a new page.
      breakTo.FlagMustBreak = true;  
      
      // to allow clone and breaked bands to access Report
      cloneBand.SetReport(Report);
      breakTo.SetReport(Report);
      
      try
      {
        cloneBand.Height = FreeSpace;
        if (cloneBand.Break(breakTo))
        {
          AddToPreparedPages(cloneBand);
          EndColumn();
          ShowBand(breakTo, false);
        }  
        else
        {
          if (cloneBand.FlagMustBreak)
          {
            // show band as is
            breakTo.FlagCheckFreeSpace = false;
            AddToPreparedPages(breakTo);
          }
          else
          {
            EndColumn();
            ShowBand(breakTo, false);
          }
        }
      }
      finally
      {
        cloneBand.Dispose();
        breakTo.Dispose();
      }  
    }
  }
}
