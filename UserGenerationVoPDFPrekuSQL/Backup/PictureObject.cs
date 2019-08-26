using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Design;
using FastReport.TypeEditors;
using FastReport.Design;
using FastReport.Utils;
using FastReport.Design.PageDesigners.Page;
using FastReport.Forms;
using FastReport.Data;
using DevComponents.DotNetBar;
using FastReport.TypeConverters;

namespace FastReport
{
  /// <summary>
  /// Represents a Picture object that can display pictures.
  /// </summary>
  /// <remarks>
  /// The Picture object can display the following kind of pictures:
  /// <list type="bullet">
  ///   <item>
  ///     <description>picture that is embedded in the report file. Use the <see cref="Image"/>
  ///     property to do this;</description>
  ///   </item>
  ///   <item>
  ///     <description>picture that is stored in the database BLOb field. Use the <see cref="DataColumn"/>
  ///     property to specify the name of data column you want to show;</description>
  ///   </item>
  ///   <item>
  ///     <description>picture that is stored in the local disk file. Use the <see cref="ImageLocation"/>
  ///     property to specify the name of the file;</description>
  ///   </item>
  ///   <item>
  ///     <description>picture that is stored in the Web. Use the <see cref="ImageLocation"/>
  ///     property to specify the picture's URL.</description>
  ///   </item>
  /// </list>
  /// <para/>Use the <see cref="SizeMode"/> property to specify a size mode. The <see cref="MaxWidth"/>
  /// and <see cref="MaxHeight"/> properties can be used to restrict the image size if <b>SizeMode</b>
  /// is set to <b>AutoSize</b>.
  /// <para/>The <see cref="TransparentColor"/> property can be used to display an image with
  /// transparent background. Use the <see cref="Transparency"/> property if you want to display
  /// semi-transparent image.
  /// </remarks>
  public class PictureObject : ReportComponentBase, IHasEditor
  {
    #region Fields
    private Image FImage;
    private PictureBoxSizeMode FSizeMode;
    private System.Windows.Forms.Padding FPadding;
    private int FImageIndex;
    private string FImageLocation;
    private string FDataColumn;
    private float FMaxWidth;
    private float FMaxHeight;
    private Color FTransparentColor;
    private float FTransparency;
    private bool FShowErrorImage;
    private bool FTile;
    private Bitmap FTransparentImage;
    private byte[] FImageData;
    private bool FDragAccept;
    private bool FShouldDisposeImage;
    private int FAngle;
    private PictureBoxSizeMode FSaveSizeMode;
    #endregion
    
    #region Properties
    /// <summary>
    /// Gets or sets the image.
    /// </summary>
    /// <remarks>
    /// By default, image that you assign to this property is never disposed - you should
    /// take care about it. If you want to dispose the image when this <b>PictureObject</b> is disposed,
    /// set the <see cref="ShouldDisposeImage"/> property to <b>true</b> right after you assign an image:
    /// <code>
    /// myPictureObject.Image = new Bitmap("file.bmp");
    /// myPictureObject.ShouldDisposeImage = true;
    /// </code>
    /// </remarks>
    [Category("Data")]
    [Editor(typeof(ImageEditor), typeof(UITypeEditor))]
    public Image Image
    {
      get { return FImage; }
      set 
      { 
        FImage = value; 
        UpdateAutoSize();
        UpdateTransparentImage();
        ResetImageIndex();
        ShouldDisposeImage = false;
      }
    }

    /// <summary>
    /// Gets or sets a value that specifies how an image is positioned within a PictureObject.
    /// </summary>
    [DefaultValue(PictureBoxSizeMode.Zoom)]
    [Category("Behavior")]
    public PictureBoxSizeMode SizeMode
    {
      get { return FSizeMode; }
      set 
      { 
        FSizeMode = value;
        UpdateAutoSize();
      }
    }
    
    /// <summary>
    /// Gets or sets the maximum width of a Picture object, in pixels.
    /// </summary>
    /// <remarks>
    /// Use this property to restrict the object size if the <see cref="SizeMode"/> property 
    /// is set to <b>AutoSize</b>.
    /// </remarks>
    [DefaultValue(0f)]
    [Category("Layout")]
    [TypeConverter(typeof(UnitsConverter))]
    public float MaxWidth
    {
      get { return FMaxWidth; }
      set { FMaxWidth = value; }
    }

    /// <summary>
    /// Gets or sets the maximum height of a Picture object, in pixels.
    /// </summary>
    /// <remarks>
    /// Use this property to restrict the object size if the <see cref="SizeMode"/> property 
    /// is set to <b>AutoSize</b>.
    /// </remarks>
    [DefaultValue(0f)]
    [Category("Layout")]
    [TypeConverter(typeof(UnitsConverter))]
    public float MaxHeight
    {
      get { return FMaxHeight; }
      set { FMaxHeight = value; }
    }

    /// <summary>
    /// Gets or sets padding within the PictureObject.
    /// </summary>
    [Category("Layout")]
    public System.Windows.Forms.Padding Padding
    {
      get { return FPadding; }
      set { FPadding = value; }
    }

    /// <summary>
    /// Gets or sets the path for the image to display in the PictureObject.
    /// </summary>
    /// <remarks>
    /// This property may contain the path to the image file as well as external URL.
    /// </remarks>
    [Category("Data")]
    public string ImageLocation
    {
      get { return FImageLocation; }
      set 
      { 
        FImageLocation = value; 
        LoadImage();
      }
    }

    /// <summary>
    /// Gets or sets the data column name to get the image from.
    /// </summary>
    [Editor(typeof(DataColumnEditor), typeof(UITypeEditor))]
    [Category("Data")]
    public string DataColumn
    {
      get { return FDataColumn; }
      set { FDataColumn = value; }
    }

    /// <summary>
    /// Gets or sets the color of the image that will be treated as transparent.
    /// </summary>
    [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
    [Category("Appearance")]
    public Color TransparentColor
    {
      get { return FTransparentColor; }
      set 
      { 
        FTransparentColor = value; 
        UpdateTransparentImage();
      }
    }
    
    /// <summary>
    /// Gets or sets the transparency of the PictureObject.
    /// </summary>
    /// <remarks>
    /// Valid range of values is 0..1. Default value is 0.
    /// </remarks>
    [DefaultValue(0f)]
    [Category("Appearance")]
    public float Transparency
    {
      get { return FTransparency; }
      set
      {
        if (value < 0)
          value = 0;
        if (value > 1)
          value = 1;
        FTransparency = value;
        UpdateTransparentImage();
      }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the PictureObject should display 
    /// the error indicator if there is no image in it.
    /// </summary>
    [DefaultValue(false)]
    [Category("Behavior")]
    public bool ShowErrorImage
    {
      get { return FShowErrorImage; }
      set { FShowErrorImage = value; }
    }
    
    /// <summary>
    /// Gets or sets a value indicating that the image should be tiled.
    /// </summary>
    [DefaultValue(false)]
    [Category("Appearance")]
    public bool Tile
    {
      get { return FTile; }
      set { FTile = value; }
    }

    /// <summary>
    /// Gets or sets the image rotation angle, in degrees. Possible values are 0, 90, 180, 270.
    /// </summary>
    [DefaultValue(0)]
    [Category("Appearance")]
    public int Angle
    {
      get { return FAngle; }
      set { FAngle = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating that the image stored in the <see cref="Image"/> 
    /// property should be disposed when this object is disposed.
    /// </summary>
    /// <remarks>
    /// By default, image assigned to the <see cref="Image"/> property is never disposed - you should
    /// take care about it. If you want to dispose the image when this <b>PictureObject</b> is disposed,
    /// set this property to <b>true</b> right after you assign an image to the <see cref="Image"/> property.
    /// </remarks>
    [Browsable(false)]
    public bool ShouldDisposeImage
    {
      get { return FShouldDisposeImage; }
      set { FShouldDisposeImage = value; }
    }

    /// <inheritdoc/>
    public override float Width
    {
      get { return base.Width; }
      set
      {
        if (MaxWidth != 0 && value > MaxWidth)
          value = MaxWidth;
        base.Width = value;
      }
    }

    /// <inheritdoc/>
    public override float Height
    {
      get { return base.Height; }
      set
      {
        if (MaxHeight != 0 && value > MaxHeight)
          value = MaxHeight;
        base.Height = value;
      }
    }

    internal bool IsFileLocation
    {
      get 
      {
        if (String.IsNullOrEmpty(ImageLocation))
          return false;
        Uri uri = CalculateUri();
        return uri.IsFile;
      }
    }

    internal bool IsWebLocation
    {
      get
      {
        if (String.IsNullOrEmpty(ImageLocation))
          return false;
        Uri uri = CalculateUri();
        return !uri.IsFile;
      }
    }

    internal bool IsDataColumn
    {
      get { return !String.IsNullOrEmpty(DataColumn); }
    }
    #endregion

    #region Private Methods
    private bool ShouldSerializeImage()
    {
      return Image != null;
    }

    private bool ShouldSerializePadding()
    {
      return Padding != new System.Windows.Forms.Padding();
    }

    private bool ShouldSerializeTransparentColor()
    {
      return TransparentColor != Color.Transparent;
    }

    private void UpdateAutoSize()
    {
      if (SizeMode == PictureBoxSizeMode.AutoSize)
      {
        if (Image == null || Image.Width == 0 || Image.Height == 0)
        {
          if (IsRunning)
          {
            Width = 0;
            Height = 0;
          }
        }
        else
        {
          bool rotate = Angle == 90 || Angle == 270;
          Width = (rotate ? Image.Height : Image.Width) + Padding.Horizontal;
          Height = (rotate ? Image.Width : Image.Height) + Padding.Vertical;

          // if width/height restrictions are set, use zoom mode to keep aspect ratio
          if (IsRunning && (MaxWidth != 0 || MaxHeight != 0))
            SizeMode = PictureBoxSizeMode.Zoom;
        }
      }
    }
    
    private void UpdateTransparentImage()
    {
      if (FTransparentImage != null)
        FTransparentImage.Dispose();
      FTransparentImage = null;
      if (Image is Bitmap)
      {
        if (TransparentColor != Color.Transparent)
        {
          FTransparentImage = new Bitmap(Image);
          FTransparentImage.MakeTransparent(TransparentColor);
        }
        else if (Transparency != 0)
        {
          FTransparentImage = ImageHelper.GetTransparentBitmap(Image, Transparency);
        }
      }
    }
    
    private Uri CalculateUri()
    {
      try
      {
        return new Uri(ImageLocation);
      }
      catch (UriFormatException)
      {
        return new Uri(Path.GetFullPath(ImageLocation));
      }
    }

    private void DrawImageInternal(FRPaintEventArgs e, RectangleF drawRect)
    {
      if (Image == null)
        return;

      RectangleF rect = drawRect;
      bool rotate = Angle == 90 || Angle == 270;
      float imageWidth = rotate ? Image.Height : Image.Width;
      float imageHeight = rotate ? Image.Width : Image.Height;

      switch (SizeMode)
      {
        case PictureBoxSizeMode.Normal:
        case PictureBoxSizeMode.AutoSize:
          rect.Width = imageWidth * e.ScaleX;
          rect.Height = imageHeight * e.ScaleY;
          if (Angle == 90 || Angle == 180)
            rect.X -= rect.Width - drawRect.Width;
          if (Angle == 180)
            rect.Y -= rect.Height - drawRect.Height;
          break;

        case PictureBoxSizeMode.CenterImage:
          rect.Offset((Width - imageWidth) * e.ScaleX / 2, (Height - imageHeight) * e.ScaleY / 2);
          rect.Width = imageWidth * e.ScaleX;
          rect.Height = imageHeight * e.ScaleY;
          break;

        case PictureBoxSizeMode.StretchImage:
          break;

        case PictureBoxSizeMode.Zoom:
          float kx = drawRect.Width / imageWidth;
          float ky = drawRect.Height / imageHeight;
          if (kx < ky)
            rect.Height = imageHeight * kx;
          else
            rect.Width = imageWidth * ky;
          rect.Offset(
            (Width * e.ScaleX - rect.Width) / 2,
            (Height * e.ScaleY - rect.Height) / 2);
          break;
      }

      System.Drawing.Drawing2D.Matrix matrix = e.Graphics.Transform;
      float gridCompensationX = matrix.OffsetX + rect.X;
      gridCompensationX = (int)gridCompensationX - gridCompensationX;
      float gridCompensationY = matrix.OffsetY + rect.Y;
      gridCompensationY = (int)gridCompensationY - gridCompensationY;

      rect.Offset(gridCompensationX, gridCompensationY);

      PointF upperLeft = PointF.Empty;
      PointF upperRight = PointF.Empty;
      PointF lowerLeft = PointF.Empty;

      switch (Angle)
      {
        case 90:
          upperLeft = new PointF(rect.Right, rect.Top);
          upperRight = new PointF(rect.Right, rect.Bottom);
          lowerLeft = new PointF(rect.Left, rect.Top);
          break;

        case 180:
          upperLeft = new PointF(rect.Right, rect.Bottom);
          upperRight = new PointF(rect.Left, rect.Bottom);
          lowerLeft = new PointF(rect.Right, rect.Top);
          break;

        case 270:
          upperLeft = new PointF(rect.Left, rect.Bottom);
          upperRight = new PointF(rect.Left, rect.Top);
          lowerLeft = new PointF(rect.Right, rect.Bottom);
          break;

        default:
          upperLeft = new PointF(rect.Left, rect.Top);
          upperRight = new PointF(rect.Right, rect.Top);
          lowerLeft = new PointF(rect.Left, rect.Bottom);
          break;
      }

      e.Graphics.DrawImage(FTransparentImage != null ? FTransparentImage : Image,
        new PointF[] { upperLeft, upperRight, lowerLeft });
    }

    private void LoadImage()
    {
      if (!String.IsNullOrEmpty(ImageLocation))
      {
        try
        {
          Uri uri = CalculateUri();
          if (uri.IsFile)
            Image = ImageHelper.Load(uri.LocalPath);
          else
            Image = ImageHelper.LoadURL(uri.ToString());
        }
        catch
        {
          Image = null;
        }
        
        ShouldDisposeImage = true;
      }
    }
    #endregion
    
    #region Protected Methods
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (Image != null && ShouldDisposeImage)
          Image.Dispose();
        Image = null;
      }
      base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override SelectionPoint[] GetSelectionPoints()
    {
      if (SizeMode == PictureBoxSizeMode.AutoSize && Image != null && Image.Width != 0 && Image.Height != 0)
        return new SelectionPoint[] { new SelectionPoint(AbsLeft, AbsTop, SizingPoint.LeftTop) };
      return base.GetSelectionPoints();
    }
    #endregion
    
    #region Public Methods
    /// <inheritdoc/>
    public override void Assign(Base source)
    {
      base.Assign(source);
      
      PictureObject src = source as PictureObject;
      SizeMode = src.SizeMode;
      MaxWidth = src.MaxWidth;
      MaxHeight = src.MaxHeight;
      Padding = src.Padding;
      ImageLocation = src.ImageLocation;
      DataColumn = src.DataColumn;
      TransparentColor = src.TransparentColor;
      Transparency = src.Transparency;
      ShowErrorImage = src.ShowErrorImage;
      Tile = src.Tile;
      Angle = src.Angle;
      Image = src.Image == null ? null : src.Image.Clone() as Image;
      if (src.Image == null && src.FImageData != null)
        FImageData = src.FImageData;
      ShouldDisposeImage = true;
    }
    
    /// <summary>
    /// Draws the image.
    /// </summary>
    /// <param name="e">Paint event args.</param>
    public void DrawImage(FRPaintEventArgs e)
    {
      Graphics g = e.Graphics;
      if (Image == null)
      {
        if (IsDesigning)
          g.DrawImage(Res.GetImage(103), (int)(AbsLeft * e.ScaleX) + 3, (int)(AbsTop * e.ScaleY) + 3);
        else if (ShowErrorImage)
          g.DrawImage(Res.GetImage(80), (int)(AbsLeft * e.ScaleX) + 3, (int)(AbsTop * e.ScaleY) + 3);
        return;
      }
      RectangleF drawRect = new RectangleF(
        (AbsLeft + Padding.Left) * e.ScaleX,
        (AbsTop + Padding.Top) * e.ScaleY,
        (Width - Padding.Horizontal) * e.ScaleX,
        (Height - Padding.Vertical) * e.ScaleY);

      GraphicsState state = g.Save();
      try
      {
        g.SetClip(drawRect);
        Report report = Report;
        if (report != null && report.SmoothGraphics)
        {
          g.InterpolationMode = InterpolationMode.HighQualityBicubic;
          g.SmoothingMode = SmoothingMode.AntiAlias;
        }

        if (!Tile)
          DrawImageInternal(e, drawRect);
        else
        {
          float y = drawRect.Top;
          float width = Image.Width * e.ScaleX;
          float height = Image.Height * e.ScaleY;
          while (y < drawRect.Bottom)
          {
            float x = drawRect.Left;
            while (x < drawRect.Right)
            {
              if (FTransparentImage != null)
                g.DrawImage(FTransparentImage, x, y, width, height);
              else
                g.DrawImage(Image, x, y, width, height);
              x += width;
            }
            y += height;
          }
        }
      }
      finally
      {
        g.Restore(state);
      }
    }

    /// <inheritdoc/>
    public override SizeF GetPreferredSize()
    {
      if ((Page as ReportPage).IsImperialUnitsUsed)
        return new SizeF(Units.Inches * 1, Units.Inches * 1);
      return new SizeF(Units.Millimeters * 20, Units.Millimeters * 20);
    }

    /// <inheritdoc/>
    public override void Draw(FRPaintEventArgs e)
    {
      UpdateAutoSize();
      base.Draw(e);
      DrawImage(e);
      DrawMarkers(e);
      Border.Draw(e, new RectangleF(AbsLeft, AbsTop, Width, Height));

      if (FDragAccept)
        DrawDragAcceptFrame(e, Color.Silver);
    }

    /// <inheritdoc/>
    public override void HandleDragOver(FRMouseEventArgs e)
    {
      if (PointInObject(new PointF(e.X, e.Y)) && e.DragSource is PictureObject)
        e.Handled = true;
      FDragAccept = e.Handled;
    }

    /// <inheritdoc/>
    public override void HandleDragDrop(FRMouseEventArgs e)
    {
      DataColumn = (e.DragSource as PictureObject).DataColumn;
      FDragAccept = false;
    }

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
      PictureObject c = writer.DiffObject as PictureObject;
      base.Serialize(writer);

      if (SizeMode != c.SizeMode)
        writer.WriteValue("SizeMode", SizeMode);
      if (FloatDiff(MaxWidth, c.MaxWidth))
        writer.WriteFloat("MaxWidth", MaxWidth);
      if (FloatDiff(MaxHeight, c.MaxHeight))
        writer.WriteFloat("MaxHeight", MaxHeight);
      if (Padding != c.Padding)
        writer.WriteValue("Padding", Padding);
      if (writer.SerializeTo != SerializeTo.Preview && writer.SerializeTo != SerializeTo.SourcePages &&
        ImageLocation != c.ImageLocation)
        writer.WriteStr("ImageLocation", ImageLocation);
      if (DataColumn != c.DataColumn)
        writer.WriteStr("DataColumn", DataColumn);
      if (TransparentColor != c.TransparentColor)
        writer.WriteValue("TransparentColor", TransparentColor);
      if (FloatDiff(Transparency, c.Transparency))
        writer.WriteFloat("Transparency", Transparency);
      if (ShowErrorImage != c.ShowErrorImage)
        writer.WriteBool("ShowErrorImage", ShowErrorImage);
      if (Tile != c.Tile)
        writer.WriteBool("Tile", Tile);
      if (Angle != c.Angle)
        writer.WriteInt("Angle", Angle);
      // store image data
      if (writer.SerializeTo != SerializeTo.SourcePages)
      {
        if (writer.SerializeTo == SerializeTo.Preview || 
          (String.IsNullOrEmpty(ImageLocation) && String.IsNullOrEmpty(DataColumn)))
        {
          if (writer.BlobStore != null)
          {
            // check FImageIndex >= writer.BlobStore.Count is needed when we close the designer
            // and run it again, the BlobStore is empty, but FImageIndex is pointing to
            // previous BlobStore item and is not -1.
            if (FImageIndex == -1 || FImageIndex >= writer.BlobStore.Count)
            {
              MemoryStream stream;
              if (FImageData == null)
              {
                stream = new MemoryStream();
                ImageHelper.Save(Image, stream, ImageFormat.Png);
              }
              else
                stream = new MemoryStream(FImageData);
              FImageIndex = writer.BlobStore.Add(stream);
            }
          }
          else 
          {
            if (Image == null && FImageData != null)
              writer.WriteStr("Image", Convert.ToBase64String(FImageData));
            else if (!writer.AreEqual(Image, c.Image))
              writer.WriteValue("Image", Image);
          }
          
          if (writer.BlobStore != null || writer.SerializeTo == SerializeTo.Undo)
            writer.WriteInt("ImageIndex", FImageIndex);
        }
      }    
    }

    /// <inheritdoc/>
    public override void Deserialize(FRReader reader)
    {
      base.Deserialize(reader);
      if (reader.HasProperty("ImageIndex"))
      {
        FImageIndex = reader.ReadInt("ImageIndex");
        if (reader.BlobStore != null && FImageIndex != -1)
        {
          int saveIndex = FImageIndex;
          Image = ImageHelper.Load(reader.BlobStore.Get(FImageIndex));
          FImageIndex = saveIndex;
          ShouldDisposeImage = true;
        }
      }
    }

    /// <inheritdoc/>
    public override ContextMenuBar GetContextMenu()
    {
      return new PictureObjectMenu(Report.Designer);
    }

    /// <inheritdoc/>
    public override SmartTagBase GetSmartTag()
    {
      return new PictureObjectSmartTag(this);
    }

    /// <summary>
    /// Invokes the object's editor.
    /// </summary>
    /// <returns><b>true</b> if object was edited succesfully.</returns>
    public bool InvokeEditor()
    {
      using (PictureEditorForm form = new PictureEditorForm(this))
      {
        return form.ShowDialog() == DialogResult.OK;
      }
    }
    
    internal void ResetImageIndex()
    {
      FImageIndex = -1;
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
      return expressions.ToArray();
    }

    /// <inheritdoc/>
    public override void InitializeComponent()
    {
      base.InitializeComponent();
      ResetImageIndex();
    }

    /// <inheritdoc/>
    public override void FinalizeComponent()
    {
      base.FinalizeComponent();
      ResetImageIndex();
    }

    /// <inheritdoc/>
    public override void SaveState()
    {
      base.SaveState();
      FSaveSizeMode = SizeMode;
    }

    /// <inheritdoc/>
    public override void RestoreState()
    {
      base.RestoreState();
      // avoid UpdateAutoSize call, use FSizeMode
      FSizeMode = FSaveSizeMode;
    }

    /// <inheritdoc/>
    public override void GetData()
    {
      base.GetData();
      if (!String.IsNullOrEmpty(DataColumn))
      {
        // reset the image
        Image = null;
        FImageData = null;

        object data = Report.GetColumnValueNullable(DataColumn);
        if (data is byte[])
        {
          FImageData = (byte[])data;
          // if autosize is on, load the image.
          if (SizeMode == PictureBoxSizeMode.AutoSize)
          {
            using (MemoryStream stream = new MemoryStream(FImageData))
            {
              Image = ImageHelper.Load(stream);
              ShouldDisposeImage = true;
            }  
          }  
        }
        else if (data is Image)
        {
          Image = data as Image;
        }
      }
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="PictureObject"/> class with default settings.
    /// </summary>
    public PictureObject()
    {
      FSizeMode = PictureBoxSizeMode.Zoom;
      FPadding = new System.Windows.Forms.Padding();
      FImageLocation = "";
      FDataColumn = "";
      FTransparentColor = Color.Transparent;
      SetFlags(Flags.HasSmartTag, true);
      ResetImageIndex();
    }
  }
}
