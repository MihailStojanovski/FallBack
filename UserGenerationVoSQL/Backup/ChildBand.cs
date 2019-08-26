using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using FastReport.Utils;
using DevComponents.DotNetBar;

namespace FastReport
{
  /// <summary>
  /// This class represents a child band.
  /// </summary>
  /// <remarks>
  /// Typical use of child band is to print several objects that can grow or shrink. It also can be done
  /// using the shift feature (via <see cref="ShiftMode"/> property), but in some cases it's not possible.
  /// </remarks>
  public class ChildBand : BandBase
  {
    private bool FFillUnusedSpace;
    private int FCompleteToNRows;
    
    /// <summary>
    /// Gets or sets a value indicating that band will be used to fill unused space on a page.
    /// </summary>
    /// <remarks>
    /// If you set this property to <b>true</b>, the band will be printed several times to fill 
    /// unused space on a report page.
    /// </remarks>
    [Category("Behavior")]
    [DefaultValue(false)]
    public bool FillUnusedSpace
    {
      get { return FFillUnusedSpace; }
      set { FFillUnusedSpace = value; }
    }

    /// <summary>
    /// Gets or sets a value that determines the overall number of data rows printed by the data band.
    /// </summary>
    /// <remarks>
    /// Using this property, you may complete the data band upto N data rows.
    /// If the data band has less number of rows, this band will be used to print empty rows.
    /// </remarks>
    [Category("Behavior")]
    [DefaultValue(0)]
    public int CompleteToNRows
    {
      get { return FCompleteToNRows; }
      set { FCompleteToNRows = value; }
    }

    internal BandBase GetTopParentBand
    {
      get
      {
        BandBase band = this;
        while (band is ChildBand)
        {
          band = band.Parent as BandBase;
        }
        
        return band;
      }
    }

    /// <inheritdoc/>
    public override void Assign(Base source)
    {
      base.Assign(source);
      ChildBand src = source as ChildBand;
      FillUnusedSpace = src.FillUnusedSpace;
      CompleteToNRows = src.CompleteToNRows;
    }

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
      ChildBand c = writer.DiffObject as ChildBand;
      base.Serialize(writer);

      if (FillUnusedSpace != c.FillUnusedSpace)
        writer.WriteBool("FillUnusedSpace", FillUnusedSpace);
      if (CompleteToNRows != c.CompleteToNRows)
        writer.WriteInt("CompleteToNRows", CompleteToNRows);
    }

    /// <inheritdoc/>
    public override ContextMenuBar GetContextMenu()
    {
      return new ChildBandMenu(Report.Designer);
    }

    /// <inheritdoc/>
    public override void Delete()
    {
      if (!CanDelete)
        return;
      // remove only this band, keep its subbands
      if (Parent is BandBase)
        (Parent as BandBase).Child = Child;
      Dispose();
    }
  }
}