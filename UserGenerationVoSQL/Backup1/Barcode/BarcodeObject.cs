using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using FastReport.Utils;
using FastReport.TypeConverters;
using FastReport.TypeEditors;
using DevComponents.DotNetBar;

namespace FastReport.Barcode
{
  /// <summary>
  /// Represents a barcode object.
  /// </summary>
  /// <remarks>
  /// The instance of this class represents a barcode. Here are some common 
  /// actions that can be performed with this object:
  /// <list type="bullet">
  ///   <item>
  ///     <description>To select the type of barcode, use the <see cref="Barcode"/> property.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <description>To specify a static barcode data, use the <see cref="Text"/> property. 
  ///       You also may use the <see cref="DataColumn"/> or <see cref="Expression"/> properties 
  ///       to specify dynamic value for a barcode.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <description>To set a barcode orientation, use the <see cref="Angle"/> property.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <description>To specify the size of barcode, set the <see cref="AutoSize"/> property
  ///       to <b>true</b> and use the <see cref="Zoom"/> property to zoom the barcode. 
  ///       If <see cref="AutoSize"/> property is set to <b>false</b>, you need to specify the
  ///       size using the <see cref="ComponentBase.Width">Width</see> and 
  ///       <see cref="ComponentBase.Height">Height</see> properties.
  ///     </description>
  ///   </item>
  /// </list>  
  /// </remarks>
  /// <example>This example shows how to configure the BarcodeObject to display PDF417 barcode.
  /// <code>
  /// BarcodeObject barcode;
  /// ...
  /// barcode.Barcode = new BarcodePDF417();
  /// (barcode.Barcode as BarcodePDF417).CompactionMode = CompactionMode.Text;
  /// </code>
  /// </example>
#if (!Basic)
  public 
#endif
  class BarcodeObject : ReportComponentBase
  {
    #region Fields
    private int FAngle;
    private bool FAutoSize;
    private BarcodeBase FBarcode;
    private string FDataColumn;
    private string FExpression;
    private string FText;
    private bool FShowText;
    private System.Windows.Forms.Padding FPadding;
    private float FZoom;
    private bool FHideIfNoData;
    private string FNoDataText;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the barcode type.
    /// </summary>
    [Category("Appearance")]
    [Editor(typeof(BarcodeEditor), typeof(UITypeEditor))]
    public BarcodeBase Barcode
    {
      get { return FBarcode; }
      set 
      { 
        if (value == null)
          value = new Barcode39();
        FBarcode = value; 
      }
    }

    /// <summary>
    /// Gets or sets the symbology name.
    /// </summary>
    /// <remarks>
    /// The following symbology names are supported:
    /// <list type="bullet">
    ///   <item><description>"2/5 Interleaved"</description></item>
    ///   <item><description>"2/5 Industrial"</description></item>
    ///   <item><description>"2/5 Matrix"</description></item>
    ///   <item><description>"Codabar"</description></item>
    ///   <item><description>"Code128"</description></item>
    ///   <item><description>"Code39"</description></item>
    ///   <item><description>"Code39 Extended"</description></item>
    ///   <item><description>"Code93"</description></item>
    ///   <item><description>"Code93 Extended"</description></item>
    ///   <item><description>"EAN8"</description></item>
    ///   <item><description>"EAN13"</description></item>
    ///   <item><description>"MSI"</description></item>
    ///   <item><description>"PostNet"</description></item>
    ///   <item><description>"UPC-A"</description></item>
    ///   <item><description>"UPC-E0"</description></item>
    ///   <item><description>"UPC-E1"</description></item>
    ///   <item><description>"Supplement 2"</description></item>
    ///   <item><description>"Supplement 5"</description></item>
    ///   <item><description>"PDF417"</description></item>
    ///   <item><description>"Datamatrix"</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// barcode.SymbologyName = "PDF417";
    /// (barcode.Barcode as BarcodePDF417).CompactionMode = CompactionMode.Text;
    /// </code>
    /// </example>
    [Browsable(false)]
    public string SymbologyName
    {
      get
      {
        return Barcode.Name;
      }
      set
      {
        if (SymbologyName != value)
        {
          Type bartype = Barcodes.GetType(value);
          Barcode = Activator.CreateInstance(bartype) as BarcodeBase;
        }
      }
    }

    /// <summary>
    /// Gets or sets the angle of barcode, in degrees.
    /// </summary>
    [DefaultValue(0)]
    [Category("Appearance")]
    public int Angle
    {
      get { return FAngle; }
      set { FAngle = value; }
    }

    /// <summary>
    /// Gets or sets a value that determines whether the barcode should handle its width automatically.
    /// </summary>
    [DefaultValue(true)]
    [Category("Behavior")]
    public bool AutoSize
    {
      get { return FAutoSize; }
      set { FAutoSize = value; }
    }

    /// <summary>
    /// Gets or sets a data column name bound to this control.
    /// </summary>
    /// <remarks>
    /// Value must be in the form "Datasource.Column".
    /// </remarks>
    [Editor(typeof(DataColumnEditor), typeof(UITypeEditor))]
    [Category("Data")]
    public string DataColumn
    {
      get { return FDataColumn; }
      set { FDataColumn = value; }
    }

    /// <summary>
    /// Gets or sets an expression that contains the barcode data.
    /// </summary>
    [Editor(typeof(ExpressionEditor), typeof(UITypeEditor))]
    [Category("Data")]
    public string Expression
    {
      get { return FExpression; }
      set { FExpression = value; }
    }

    /// <summary>
    /// Gets or sets a value that indicates if the barcode should display a human-readable text.
    /// </summary>
    [DefaultValue(true)]
    [Category("Behavior")]
    public bool ShowText
    {
      get { return FShowText; }
      set { FShowText = value; }
    }

    /// <summary>
    /// Gets or sets the barcode data.
    /// </summary>
    [Category("Data")]
    public string Text
    {
      get { return FText; }
      set { FText = value; }
    }

    /// <summary>
    /// Gets or sets padding within the BarcodeObject.
    /// </summary>
    [Category("Layout")]
    public System.Windows.Forms.Padding Padding
    {
      get { return FPadding; }
      set { FPadding = value; }
    }

    /// <summary>
    /// Gets or sets a zoom of the barcode.
    /// </summary>
    [DefaultValue(1f)]
    [Category("Appearance")]
    public float Zoom
    {
      get { return FZoom; }
      set { FZoom = value; }
    }

    /// <summary>
    /// Gets or sets a value that determines whether it is necessary to hide the object if the
    /// barcode data is empty.
    /// </summary>
    [DefaultValue(true)]
    [Category("Behavior")]
    public bool HideIfNoData
    {
      get { return FHideIfNoData; }
      set { FHideIfNoData = value; }
    }

    /// <summary>
    /// Gets or sets the text that will be displayed if the barcode data is empty.
    /// </summary>
    [Category("Data")]
    public string NoDataText
    {
      get { return FNoDataText; }
      set { FNoDataText = value; }
    }
    #endregion
    
    #region Private Methods
    private bool ShouldSerializePadding()
    {
      return Padding != new System.Windows.Forms.Padding();
    }

    private void SetBarcodeProperties()
    {
      FBarcode.Initialize(Text, ShowText, Angle, Zoom);
    }

    private TextRenderingHint GetTextQuality(TextQuality quality)
    {
      switch (quality)
      {
        case TextQuality.Regular:
          return TextRenderingHint.AntiAliasGridFit;

        case TextQuality.ClearType:
          return TextRenderingHint.ClearTypeGridFit;

        case TextQuality.AntiAlias:
          return TextRenderingHint.AntiAlias;
      }

      return TextRenderingHint.SystemDefault;
    }

    private void DrawBarcode(FRPaintEventArgs e)
    {
      RectangleF displayRect = new RectangleF(
        (AbsLeft + Padding.Left) * e.ScaleX,
        (AbsTop + Padding.Top) * e.ScaleY,
        (Width - Padding.Horizontal) * e.ScaleX,
        (Height - Padding.Vertical) * e.ScaleY);

      Graphics g = e.Graphics;
      GraphicsState state = g.Save();
      try
      {
        Report report = Report;
        if (report != null) 
        {
          if (report.SmoothGraphics)
          {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
          }
          g.TextRenderingHint = GetTextQuality(report.TextQuality);
        }

        FBarcode.DrawBarcode(g, displayRect);
      }
      finally
      {
        g.Restore(state);
      }  
    }

    private void UpdateAutoSize()
    {
      SetBarcodeProperties();
      SizeF size = Barcode.CalcBounds();
      size.Width *= Zoom;
      size.Height *= Zoom;
      if (AutoSize)
      {
        if (Angle == 0 || Angle == 180)
        {
          Width = size.Width + Padding.Horizontal;
          if (size.Height > 0)
            Height = size.Height + Padding.Vertical;
        }
        else if (Angle == 90 || Angle == 270)
        {
          Height = size.Width + Padding.Vertical;
          if (size.Height > 0)
            Width = size.Height + Padding.Horizontal;
        }
      }
    }
    #endregion
    
    #region Public Methods
    /// <inheritdoc/>
    public override void Assign(Base source)
    {
      base.Assign(source);
      
      BarcodeObject src = source as BarcodeObject;
      Barcode = src.Barcode.Clone();
      Angle = src.Angle;
      AutoSize = src.AutoSize;
      DataColumn = src.DataColumn;
      Expression = src.Expression;
      Text = src.Text;
      ShowText = src.ShowText;
      Padding = src.Padding;
      Zoom = src.Zoom;
      HideIfNoData = src.HideIfNoData;
      NoDataText = src.NoDataText;
    }

    /// <inheritdoc/>
    public override void Draw(FRPaintEventArgs e)
    {
      bool error = false;
      string errorText = "";
      
      if (String.IsNullOrEmpty(Text))
      {
        error = true;
        errorText = NoDataText;
      }  
      else
      {
        try
        {
          UpdateAutoSize();
        }
        catch (Exception ex)
        {
          error = true;
          errorText = ex.Message;
        }
      }
      
      base.Draw(e);
      if (!error)
        DrawBarcode(e);
      else
      {
        e.Graphics.DrawString(errorText, DrawUtils.DefaultReportFont, Brushes.Red, 
          new RectangleF(AbsLeft * e.ScaleX, AbsTop * e.ScaleY, Width * e.ScaleX, Height * e.ScaleY));
      }  
      DrawMarkers(e);
      Border.Draw(e, new RectangleF(AbsLeft, AbsTop, Width, Height));
    }

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
      BarcodeObject c = writer.DiffObject as BarcodeObject;
      base.Serialize(writer);

      if (Angle != c.Angle)
        writer.WriteInt("Angle", Angle);
      if (AutoSize != c.AutoSize)
        writer.WriteBool("AutoSize", AutoSize);
      if (DataColumn != c.DataColumn)
        writer.WriteStr("DataColumn", DataColumn);
      if (Expression != c.Expression)
        writer.WriteStr("Expression", Expression);
      if (Text != c.Text)
        writer.WriteStr("Text", Text);
      if (ShowText != c.ShowText)
        writer.WriteBool("ShowText", ShowText);
      if (Padding != c.Padding)
        writer.WriteValue("Padding", Padding);
      if (Zoom != c.Zoom)
        writer.WriteFloat("Zoom", Zoom);
      if (HideIfNoData != c.HideIfNoData)
        writer.WriteBool("HideIfNoData", HideIfNoData);
      if (NoDataText != c.NoDataText)
        writer.WriteStr("NoDataText", NoDataText);
      Barcode.Serialize(writer, "Barcode.", c.Barcode);  
    }

    /// <inheritdoc/>
    public override SmartTagBase GetSmartTag()
    {
      return new BarcodeSmartTag(this);
    }

    /// <inheritdoc/>
    public override ContextMenuBar GetContextMenu()
    {
      return new BarcodeObjectMenu(Report.Designer);
    }

    /// <inheritdoc/>
    public override SizeF GetPreferredSize()
    {
      if ((Page as ReportPage).IsImperialUnitsUsed)
        return new SizeF(Units.Inches * 1, Units.Inches * 1);
      return new SizeF(Units.Centimeters * 2.5f, Units.Centimeters * 2.5f);  
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
        Text = value == null ? "" : value.ToString();
      }
      else if (!String.IsNullOrEmpty(Expression))
      {
        object value = Report.Calc(Expression);
        Text = value == null ? "" : value.ToString();
      }
      
      if (Visible)
        Visible = !String.IsNullOrEmpty(Text) || !HideIfNoData;
      if (Visible)
      {
        try
        {
          UpdateAutoSize();
        }
        catch
        {
        }
      }
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="BarcodeObject"/> class with default settings.
    /// </summary>
    public BarcodeObject()
    {
      Barcode = new Barcode39();
      AutoSize = true;
      DataColumn = "";
      Expression = "";
      Text = "12345678";
      ShowText = true;
      Padding = new System.Windows.Forms.Padding();
      Zoom = 1;
      HideIfNoData = true;
      NoDataText = "";
      SetFlags(Flags.HasSmartTag, true);
    }
  }


  internal static class Barcodes
  {
    private struct BarcodeItem
    {
      public Type ObjType;
      public string BarcodeName;

      public BarcodeItem(Type objType, string barcodeName)
      {
        ObjType = objType;
        BarcodeName = barcodeName;
      }
    }

    private static BarcodeItem[] Items = {
        new BarcodeItem(typeof(Barcode2of5Interleaved), "2/5 Interleaved"),
        new BarcodeItem(typeof(Barcode2of5Industrial), "2/5 Industrial"),
        new BarcodeItem(typeof(Barcode2of5Matrix), "2/5 Matrix"),
        new BarcodeItem(typeof(BarcodeCodabar), "Codabar"),
        new BarcodeItem(typeof(Barcode128), "Code128"),
        new BarcodeItem(typeof(Barcode39), "Code39"),
        new BarcodeItem(typeof(Barcode39Extended), "Code39 Extended"),
        new BarcodeItem(typeof(Barcode93), "Code93"),
        new BarcodeItem(typeof(Barcode93Extended), "Code93 Extended"),
        new BarcodeItem(typeof(BarcodeEAN8), "EAN8"),
        new BarcodeItem(typeof(BarcodeEAN13), "EAN13"),
        new BarcodeItem(typeof(BarcodeMSI), "MSI"),
        new BarcodeItem(typeof(BarcodePostNet), "PostNet"),
        new BarcodeItem(typeof(BarcodeUPC_A), "UPC-A"),
        new BarcodeItem(typeof(BarcodeUPC_E0), "UPC-E0"),
        new BarcodeItem(typeof(BarcodeUPC_E1), "UPC-E1"),
        new BarcodeItem(typeof(BarcodeSupplement2), "Supplement 2"),
        new BarcodeItem(typeof(BarcodeSupplement5), "Supplement 5"),
        new BarcodeItem(typeof(BarcodePDF417), "PDF417"),
        new BarcodeItem(typeof(BarcodeDatamatrix), "Datamatrix")
      };

    public static string GetName(Type type)
    {
      foreach (BarcodeItem item in Items)
      {
        if (item.ObjType == type)
          return item.BarcodeName;
      }
      return "";
    }
    
    public static Type GetType(string name)
    {
      foreach (BarcodeItem item in Items)
      {
        if (item.BarcodeName == name)
          return item.ObjType;
      }
      return null;
    }
    
    public static string[] GetItems()
    {
      List<string> result = new List<string>();
      foreach (BarcodeItem item in Items)
      {
        result.Add(item.BarcodeName);
      }
      
      return result.ToArray();
    }
  }
}
