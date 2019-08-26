using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.ComponentModel;
using FastReport.Utils;
using FastReport.TypeEditors;
using FastReport.Design.PageDesigners.Page;

namespace FastReport
{
  /// <summary>
  /// Specifies a symbol that will be displayed when a <see cref="CheckBoxObject"/> is in the checked state.
  /// </summary>
  public enum CheckedSymbol
  {
    /// <summary>
    /// Specifies a check symbol.
    /// </summary>
    Check,

    /// <summary>
    /// Specifies a diagonal cross symbol.
    /// </summary>
    Cross,

    /// <summary>
    /// Specifies a plus symbol.
    /// </summary>
    Plus,

    /// <summary>
    /// Specifies a filled rectangle.
    /// </summary>
    Fill
  }

  /// <summary>
  /// Specifies a symbol that will be displayed when a <see cref="CheckBoxObject"/> is in the unchecked state.
  /// </summary>
  public enum UncheckedSymbol
  {
    /// <summary>
    /// Specifies no symbol.
    /// </summary>
    None,

    /// <summary>
    /// Specifies a diagonal cross symbol.
    /// </summary>
    Cross,

    /// <summary>
    /// Specifies a minus symbol.
    /// </summary>
    Minus
  }
  
  /// <summary>
  /// Represents a check box object.
  /// </summary>
#if (!Basic)
  public 
#endif
  class CheckBoxObject : ReportComponentBase
  {
    #region Fields
    private bool FChecked;
    private CheckedSymbol FCheckedSymbol;
    private UncheckedSymbol FUncheckedSymbol;
    private Color FCheckColor;
    private string FDataColumn;
    private string FExpression;
    private float FCheckWidthRatio;
    private bool FHideIfUnchecked;
    #endregion
    
    #region Properties
    /// <summary>
    /// Gets or set a value indicating whether the check box is in the checked state.
    /// </summary>
    [DefaultValue(true)]
    [Category("Data")]
    public bool Checked
    {
      get { return FChecked; }
      set { FChecked = value; }
    }
    
    /// <summary>
    /// Gets or sets a symbol that will be displayed when the check box is in the checked state.
    /// </summary>
    [DefaultValue(CheckedSymbol.Check)]
    [Category("Appearance")]
    public CheckedSymbol CheckedSymbol
    {
      get { return FCheckedSymbol; }
      set { FCheckedSymbol = value; }
    }

    /// <summary>
    /// Gets or sets a symbol that will be displayed when the check box is in the unchecked state.
    /// </summary>
    [DefaultValue(UncheckedSymbol.None)]
    [Category("Appearance")]
    public UncheckedSymbol UncheckedSymbol
    {
      get { return FUncheckedSymbol; }
      set { FUncheckedSymbol = value; }
    }

    /// <summary>
    /// Gets or sets a color of the check symbol.
    /// </summary>
    [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
    [Category("Appearance")]
    public Color CheckColor
    {
      get { return FCheckColor; }
      set { FCheckColor = value; }
    }

    /// <summary>
    /// Gets or sets a data column name bound to this control.
    /// </summary>
    /// <remarks>
    /// Value must be in the form "[Datasource.Column]".
    /// </remarks>
    [Editor(typeof(DataColumnEditor), typeof(UITypeEditor))]
    [Category("Data")]
    public string DataColumn
    {
      get { return FDataColumn; }
      set { FDataColumn = value; }
    }

    /// <summary>
    /// Gets or sets an expression that determines whether to show a check.
    /// </summary>
    [Editor(typeof(ExpressionEditor), typeof(UITypeEditor))]
    [Category("Data")]
    public string Expression
    {
      get { return FExpression; }
      set { FExpression = value; }
    }
    
    /// <summary>
    /// Gets or sets the check symbol width ratio.
    /// </summary>
    /// <remarks>
    /// Valid values are from 0.2 to 2.
    /// </remarks>
    [DefaultValue(1f)]
    [Category("Appearance")]
    public float CheckWidthRatio
    {
      get { return FCheckWidthRatio; }
      set 
      { 
        if (value <= 0.2f)
          value = 0.2f;
        if (value > 2)
          value = 2;  
        FCheckWidthRatio = value; 
      }
    }
    
    /// <summary>
    /// Gets or sets a value determines whether to hide the checkbox if it is in the unchecked state.
    /// </summary>
    [DefaultValue(false)]
    [Category("Behavior")]
    public bool HideIfUnchecked
    {
      get { return FHideIfUnchecked; }
      set { FHideIfUnchecked = value; }
    }
    #endregion
    
    #region Private Methods
    private bool ShouldSerializeCheckColor()
    {
      return CheckColor != Color.Black;
    }
    
    private void DrawCheck(FRPaintEventArgs e)
    {
      RectangleF drawRect = new RectangleF(AbsLeft * e.ScaleX, AbsTop * e.ScaleY, 
        Width * e.ScaleX, Height * e.ScaleY);
      
      float ratio = Width / (Units.Millimeters * 5);
      drawRect.Inflate(-4 * ratio * e.ScaleX, -4 * ratio * e.ScaleY);
      Pen pen = e.Cache.GetPen(CheckColor, 1.6f * ratio * CheckWidthRatio * e.ScaleX, DashStyle.Solid);
      Graphics g = e.Graphics;
      SmoothingMode saveSmoothing = g.SmoothingMode;
      g.SmoothingMode = SmoothingMode.AntiAlias;
      
      if (Checked)
      {
        switch (CheckedSymbol)
        {
          case CheckedSymbol.Check:
            g.DrawLines(pen, new PointF[] {
              new PointF(drawRect.Left, drawRect.Top + drawRect.Height / 10 * 5),
              new PointF(drawRect.Left + drawRect.Width / 10 * 4, drawRect.Bottom - drawRect.Height / 10),
              new PointF(drawRect.Right, drawRect.Top + drawRect.Height / 10) });
            break;

          case CheckedSymbol.Cross:
            g.DrawLine(pen, drawRect.Left, drawRect.Top, drawRect.Right, drawRect.Bottom);
            g.DrawLine(pen, drawRect.Left, drawRect.Bottom, drawRect.Right, drawRect.Top);
            break;

          case CheckedSymbol.Plus:
            g.DrawLine(pen, drawRect.Left, drawRect.Top + drawRect.Height / 2, drawRect.Right, drawRect.Top + drawRect.Height / 2);
            g.DrawLine(pen, drawRect.Left + drawRect.Width / 2, drawRect.Top, drawRect.Left + drawRect.Width / 2, drawRect.Bottom);
            break;

          case CheckedSymbol.Fill:
            Brush brush = e.Cache.GetBrush(CheckColor);
            g.FillRectangle(brush, drawRect);
            break;
        }
      }
      else
      {
        switch (UncheckedSymbol)
        {
          case UncheckedSymbol.Cross:
            g.DrawLine(pen, drawRect.Left, drawRect.Top, drawRect.Right, drawRect.Bottom);
            g.DrawLine(pen, drawRect.Left, drawRect.Bottom, drawRect.Right, drawRect.Top);
            break;

          case UncheckedSymbol.Minus:
            g.DrawLine(pen, drawRect.Left, drawRect.Top + drawRect.Height / 2, drawRect.Right, drawRect.Top + drawRect.Height / 2);
            break;
        }
      }
      
      g.SmoothingMode = saveSmoothing;
    }
    #endregion
    
    #region Public Methods
    /// <inheritdoc/>
    public override void Assign(Base source)
    {
      base.Assign(source);
      
      CheckBoxObject src = source as CheckBoxObject;
      Checked = src.Checked;
      CheckedSymbol = src.CheckedSymbol;
      UncheckedSymbol = src.UncheckedSymbol;
      CheckColor = src.CheckColor;
      DataColumn = src.DataColumn;
      Expression = src.Expression;
      CheckWidthRatio = src.CheckWidthRatio;
      HideIfUnchecked = src.HideIfUnchecked;
    }

    /// <inheritdoc/>
    public override void Draw(FRPaintEventArgs e)
    {
      base.Draw(e);
      DrawCheck(e);
      DrawMarkers(e);
      Border.Draw(e, new RectangleF(AbsLeft, AbsTop, Width, Height));
    }

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
      CheckBoxObject c = writer.DiffObject as CheckBoxObject;
      base.Serialize(writer);
      
      if (Checked != c.Checked)
        writer.WriteBool("Checked", Checked);
      if (CheckedSymbol != c.CheckedSymbol)
        writer.WriteValue("CheckedSymbol", CheckedSymbol);
      if (UncheckedSymbol != c.UncheckedSymbol)
        writer.WriteValue("UncheckedSymbol", UncheckedSymbol);
      if (CheckColor != c.CheckColor)
        writer.WriteValue("CheckColor", CheckColor);
      if (DataColumn != c.DataColumn)
        writer.WriteStr("DataColumn", DataColumn);
      if (Expression != c.Expression)
        writer.WriteStr("Expression", Expression);
      if (CheckWidthRatio != c.CheckWidthRatio)
        writer.WriteFloat("CheckWidthRatio", CheckWidthRatio);
      if (HideIfUnchecked != c.HideIfUnchecked)
        writer.WriteBool("HideIfUnchecked", HideIfUnchecked);
    }

    /// <inheritdoc/>
    public override SizeF GetPreferredSize()
    {
      if ((Page as ReportPage).IsImperialUnitsUsed)
        return new SizeF(Units.Inches * 0.2f, Units.Inches * 0.2f);
      return new SizeF(Units.Millimeters * 5, Units.Millimeters * 5);
    }

    /// <inheritdoc/>
    public override SmartTagBase GetSmartTag()
    {
      return new CheckBoxSmartTag(this);
    }
    #endregion
    
    #region Report Engine
    /// <inheritdoc/>
    public override string[] GetExpressions()
    {
      List<string> expressions = new List<string>();
      expressions.AddRange(base.GetExpressions());

      if (!String.IsNullOrEmpty(DataColumn))
        expressions.Add(DataColumn);
      if (!String.IsNullOrEmpty(Expression))
        expressions.Add(Expression);
      return expressions.ToArray();
    }

    /// <inheritdoc/>
    public override void GetData()
    {
      base.GetData();
      if (!String.IsNullOrEmpty(DataColumn))
      {
        object value = Report.GetColumnValue(DataColumn);
        Variant varValue = value == null ? new Variant(0) : new Variant(value);
        Checked = varValue == true || (varValue.IsNumeric && varValue != 0);
      }
      else if (!String.IsNullOrEmpty(Expression))
      {
        object value = Report.Calc(Expression);
        Checked = value is bool && (bool)value == true;
      }
      if (!Checked && HideIfUnchecked)
        Visible = false;
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <b>CheckBoxObject</b> class with default settings. 
    /// </summary>
    public CheckBoxObject()
    {
      FCheckColor = Color.Black;
      FDataColumn = "";
      FExpression = "";
      FChecked = true;
      FCheckedSymbol = CheckedSymbol.Check;
      FUncheckedSymbol = UncheckedSymbol.None;
      FCheckWidthRatio = 1;
      SetFlags(Flags.HasSmartTag, true);
    }
  }
}
