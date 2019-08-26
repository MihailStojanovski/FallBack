using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using FastReport.TypeEditors;
using FastReport.Utils;
using FastReport.TypeConverters;
using FastReport.Format;
using System.Drawing.Imaging;

namespace FastReport
{
  /// <summary>
  /// Specifies the watermark image size mode.
  /// </summary>
  public enum WatermarkImageSize 
  { 
    /// <summary>
    /// Specifies the normal (original) size.
    /// </summary>
    Normal,

    /// <summary>
    /// Specifies the centered image.
    /// </summary>
    Center,
    
    /// <summary>
    /// Specifies the stretched image.
    /// </summary>
    Stretch, 
    
    /// <summary>
    /// Specifies the stretched image that keeps its aspect ratio.
    /// </summary>
    Zoom, 
    
    /// <summary>
    /// Specifies the tiled image.
    /// </summary>
    Tile
  }
  
  /// <summary>
  /// Specifies the watermark text rotation.
  /// </summary>
  public enum WatermarkTextRotation 
  { 
    /// <summary>
    /// Specifies a horizontal text.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Specifies a vertical text.
    /// </summary>
    Vertical,

    /// <summary>
    /// Specifies a diagonal text.
    /// </summary>
    ForwardDiagonal,

    /// <summary>
    /// Specifies a backward diagonal text.
    /// </summary>
    BackwardDiagonal
  }
  
  /// <summary>
  /// Represents the report page watermark.
  /// </summary>
  /// <remarks>
  /// Watermark can draw text and/or image behind the page objects on in front of them. To enable
  /// watermark, set its <b>Enabled</b> property to <b>true</b>.
  /// </remarks>
  [TypeConverter(typeof(FRExpandableObjectConverter))]
  [Editor(typeof(WatermarkEditor), typeof(UITypeEditor))]
  public class Watermark : IDisposable
  {
    #region Fields
    private bool FEnabled;
    private PictureObject FPictureObject;
    private TextObject FTextObject;
    private WatermarkTextRotation FTextRotation;
    private WatermarkImageSize FImageSize;
    private bool FShowImageOnTop;
    private bool FShowTextOnTop;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets avalue indicating that watermark is enabled.
    /// </summary>
    [DefaultValue(false)]
    public bool Enabled
    {
      get { return FEnabled; }
      set { FEnabled = value; }
    }
    
    /// <summary>
    /// Gets or sets the watermark image.
    /// </summary>
    public Image Image
    {
      get { return FPictureObject.Image; }
      set { FPictureObject.Image = value; }
    }

    /// <summary>
    /// Gets or sets the watermark image size mode.
    /// </summary>
    [DefaultValue(WatermarkImageSize.Zoom)]
    public WatermarkImageSize ImageSize
    {
      get { return FImageSize; }
      set { FImageSize = value; }
    }
    
    /// <summary>
    /// Gets or sets an image transparency.
    /// </summary>
    /// <remarks>
    /// Valid values are 0..1. 1 means totally transparent image.
    /// </remarks>
    [DefaultValue(0f)]
    public float ImageTransparency
    {
      get { return FPictureObject.Transparency; }
      set { FPictureObject.Transparency = value; }
    }

    /// <summary>
    /// Gets or sets the watermark text.
    /// </summary>
    public string Text
    {
      get { return FTextObject.Text; }
      set { FTextObject.Text = value; }
    }

    /// <summary>
    /// Gets or sets a font of the watermark text.
    /// </summary>
    public Font Font
    {
      get { return FTextObject.Font; }
      set { FTextObject.Font = value; }
    }

    /// <summary>
    /// Gets or sets a text fill.
    /// </summary>
    [Editor(typeof(FillEditor), typeof(UITypeEditor))]
    public FillBase TextFill
    {
      get { return FTextObject.TextFill; }
      set { FTextObject.TextFill = value; }
    }
    
    /// <summary>
    /// Gets or sets a text rotation.
    /// </summary>
    [DefaultValue(WatermarkTextRotation.ForwardDiagonal)]
    public WatermarkTextRotation TextRotation
    {
      get { return FTextRotation; }
      set { FTextRotation = value; }
    }

    /// <summary>
    /// Gets or sets a value indicates that the text should be displayed on top of all page objects.
    /// </summary>
    [DefaultValue(true)]
    public bool ShowTextOnTop
    {
      get { return FShowTextOnTop; }
      set { FShowTextOnTop = value; }
    }

    /// <summary>
    /// Gets or sets a value indicates that the image should be displayed on top of all page objects.
    /// </summary>
    [DefaultValue(false)]
    public bool ShowImageOnTop
    {
      get { return FShowImageOnTop; }
      set { FShowImageOnTop = value; }
    }
    #endregion
    
    #region Private Methods
    private bool ShouldSerializeFont()
    {
      return Font.Name != "Arial" || Font.Size != 60 || Font.Style != FontStyle.Regular;
    }

    private bool ShouldSerializeTextFill()
    {
      return !(TextFill is SolidFill) || (TextFill as SolidFill).Color != Color.LightGray;
    }

    private bool ShouldSerializeImage()
    {
      return Image != null;
    }
    #endregion

    #region Public Methods
    internal void DrawImage(FRPaintEventArgs e, RectangleF displayRect, Report report, bool isPrinting)
    {
      FPictureObject.SetReport(report);
      FPictureObject.Bounds = displayRect;
      PictureBoxSizeMode sizeMode = PictureBoxSizeMode.Normal;
      if (ImageSize == WatermarkImageSize.Stretch)
        sizeMode = PictureBoxSizeMode.StretchImage;
      else if (ImageSize == WatermarkImageSize.Zoom)
        sizeMode = PictureBoxSizeMode.Zoom;
      else if (ImageSize == WatermarkImageSize.Center)
        sizeMode = PictureBoxSizeMode.CenterImage;
      FPictureObject.SizeMode = sizeMode;
      FPictureObject.Tile = ImageSize == WatermarkImageSize.Tile;
      FPictureObject.SetPrinting(isPrinting);
      FPictureObject.DrawImage(e);
    }

    internal void DrawText(FRPaintEventArgs e, RectangleF displayRect, Report report, bool isPrinting)
    {
      FTextObject.SetReport(report);
      FTextObject.Bounds = displayRect;
      int angle = 0;
      switch (TextRotation)
      {
        case WatermarkTextRotation.Horizontal:
          angle = 0;
          break;
        case WatermarkTextRotation.Vertical:
          angle = 270;
          break;
        case WatermarkTextRotation.ForwardDiagonal:
          angle = 360 - (int)(Math.Atan(displayRect.Height / displayRect.Width) * (180 / Math.PI));
          break;
        case WatermarkTextRotation.BackwardDiagonal:
          angle = (int)(Math.Atan(displayRect.Height / displayRect.Width) * (180 / Math.PI));
          break;
      }    
      FTextObject.Angle = angle;  
      FTextObject.SetPrinting(isPrinting);
      FTextObject.DrawText(e);
    }

    /// <summary>
    /// Serializes the watermark.
    /// </summary>
    /// <param name="writer">Writer object.</param>
    /// <param name="prefix">The watermark property name.</param>
    /// <param name="c">Another Watermark object to compare with.</param>
    /// <remarks>
    /// This method is for internal use only.
    /// </remarks>
    public void Serialize(FRWriter writer, string prefix, Watermark c)
    {
      if (Enabled != c.Enabled)
        writer.WriteBool(prefix + ".Enabled", Enabled);
      if (!writer.AreEqual(Image, c.Image))
        writer.WriteValue(prefix + ".Image", Image);
      if (ImageSize != c.ImageSize)
        writer.WriteValue(prefix + ".ImageSize", ImageSize);
      if (ImageTransparency != c.ImageTransparency)
        writer.WriteFloat(prefix + ".ImageTransparency", ImageTransparency);
      if (Text != c.Text)
        writer.WriteStr(prefix + ".Text", Text);
      if (!writer.AreEqual(Font, c.Font))
        writer.WriteValue(prefix + ".Font", Font);
      TextFill.Serialize(writer, prefix + ".TextFill", c.TextFill);
      if (TextRotation != c.TextRotation)
        writer.WriteValue(prefix + ".TextRotation", TextRotation);
      if (ShowTextOnTop != c.ShowTextOnTop)
        writer.WriteBool(prefix + ".ShowTextOnTop", ShowTextOnTop);
      if (ShowImageOnTop != c.ShowImageOnTop)
        writer.WriteBool(prefix + ".ShowImageOnTop", ShowImageOnTop);
    }
    
    /// <summary>
    /// Disposes resources used by the watermark.
    /// </summary>
    public void Dispose()
    {
      FPictureObject.Dispose();
      FTextObject.Dispose();
    }

    /// <summary>
    /// Assigns values from another source.
    /// </summary>
    /// <param name="source">Source to assign from.</param>
    public void Assign(Watermark source)
    {
      Enabled = source.Enabled;
      Image = source.Image == null ? null : source.Image.Clone() as Image;
      ImageSize = source.ImageSize;
      ImageTransparency = source.ImageTransparency;
      Text = source.Text;
      Font = source.Font;
      TextFill = source.TextFill.Clone();
      TextRotation = source.TextRotation;
      ShowTextOnTop = source.ShowTextOnTop;
      ShowImageOnTop = source.ShowImageOnTop;
    }

    /// <summary>
    /// Creates exact copy of this <b>Watermark</b>.
    /// </summary>
    /// <returns>Copy of this watermark.</returns>
    public Watermark Clone()
    {
      Watermark result = new Watermark();
      result.Assign(this);
      return result;
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Watermark"/> class with default settings.
    /// </summary>
    public Watermark()
    {
      FPictureObject = new PictureObject();
      FTextObject = new TextObject();

      FPictureObject.ShowErrorImage = false;
      FTextObject.HorzAlign = HorzAlign.Center;
      FTextObject.VertAlign = VertAlign.Center;
      ImageSize = WatermarkImageSize.Zoom;
      Font = new Font("Arial", 60);
      TextFill = new SolidFill(Color.FromArgb(40, Color.Gray));
      TextRotation = WatermarkTextRotation.ForwardDiagonal;
      ShowTextOnTop = true;
    }
  }
}
