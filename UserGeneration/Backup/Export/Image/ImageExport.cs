using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using FastReport.Utils;
using FastReport.Forms;
using System.Runtime.InteropServices;

namespace FastReport.Export.Image
{
  /// <summary>
  /// Specifies the image export format.
  /// </summary>
  public enum ImageExportFormat
  {
    /// <summary>
    /// Specifies the .bmp format.
    /// </summary>
    Bmp,

    /// <summary>
    /// Specifies the .png format.
    /// </summary>
    Png,

    /// <summary>
    /// Specifies the .jpg format.
    /// </summary>
    Jpeg,

    /// <summary>
    /// Specifies the .gif format.
    /// </summary>
    Gif,

    /// <summary>
    /// Specifies the .tif format.
    /// </summary>
    Tiff,

    /// <summary>
    /// Specifies the .emf format.
    /// </summary>
    Metafile
  }

  /// <summary>
  /// Represents the image export filter.
  /// </summary>
  public class ImageExport : ExportBase
  {
    private ImageExportFormat FImageFormat;
    private bool FSeparateFiles;
    private int FResolutionX;
    private int FResolutionY;
    private int FJpegQuality;
    private bool FMultiFrameTiff;
    private bool FMonochromeTiff;
    private EncoderValue FMonochromeTiffCompression;
    private System.Drawing.Image FMasterTiffImage;
    private System.Drawing.Image FBigImage;
    private float FCurOriginY;
    private bool FFirstPage;

    #region Properties
    /// <summary>
    /// Gets or sets the image format.
    /// </summary>
    public ImageExportFormat ImageFormat
    {
      get { return FImageFormat; }
      set { FImageFormat = value; }
    }
    
    /// <summary>
    /// Gets or sets a value that determines whether to generate separate image file 
    /// for each exported page.
    /// </summary>
    /// <remarks>
    /// If this property is set to <b>true</b>, the export filter will produce one big image
    /// containing all exported pages. Be careful using this property with a big report
    /// because it may produce out of memory error.
    /// </remarks>
    public bool SeparateFiles
    {
      get { return FSeparateFiles; }
      set { FSeparateFiles = value; }
    }
    
    /// <summary>
    /// Gets or sets image resolution, in dpi.
    /// </summary>
    /// <remarks>
    /// By default this property is set to 96 dpi. Use bigger values (300-600 dpi)
    /// if you going to print the exported images.
    /// </remarks>
    public int Resolution
    {
      get { return FResolutionX; }
      set 
      { 
        FResolutionX = value;
        FResolutionY = value;
      }
    }

    /// <summary>
    /// Gets or sets horizontal image resolution, in dpi.
    /// </summary>
    /// <remarks>
    /// Separate horizontal and vertical resolution is used when exporting to TIFF. In other
    /// cases, use the <see cref="Resolution"/> property instead.
    /// </remarks>
    public int ResolutionX
    {
      get { return FResolutionX; }
      set { FResolutionX = value; }
    }

    /// <summary>
    /// Gets or sets vertical image resolution, in dpi.
    /// </summary>
    /// <remarks>
    /// Separate horizontal and vertical resolution is used when exporting to TIFF. In other
    /// cases, use the <see cref="Resolution"/> property instead.
    /// </remarks>
    public int ResolutionY
    {
      get { return FResolutionY; }
      set { FResolutionY = value; }
    }

    /// <summary>
    /// Gets or sets the jpg image quality.
    /// </summary>
    /// <remarks>
    /// This property is used if <see cref="ImageFormat"/> is set to <b>Jpeg</b>. By default
    /// it is set to 100. Use lesser value to decrease the jpg file size.
    /// </remarks>
    public int JpegQuality
    {
      get { return FJpegQuality; }
      set { FJpegQuality = value; }
    }

    /// <summary>
    /// Gets or sets the value determines whether to produce multi-frame tiff file.
    /// </summary>
    public bool MultiFrameTiff
    {
      get { return FMultiFrameTiff; }
      set { FMultiFrameTiff = value; }
    }

    /// <summary>
    /// Gets or sets a value that determines whether the Tiff export must produce monochrome image.
    /// </summary>
    /// <remarks>
    /// Monochrome tiff image is compressed using the compression method specified in the 
    /// <see cref="MonochromeTiffCompression"/> property.
    /// </remarks>
    public bool MonochromeTiff
    {
      get { return FMonochromeTiff; }
      set { FMonochromeTiff = value; }
    }

    /// <summary>
    /// Gets or sets the compression method for a monochrome TIFF image.
    /// </summary>
    /// <remarks>
    /// This property is used only when exporting to TIFF image, and the <see cref="MonochromeTiff"/> property
    /// is set to <b>true</b>. 
    /// <para/>The valid values for this property are: <b>EncoderValue.CompressionNone</b>, 
    /// <b>EncoderValue.CompressionLZW</b>, <b>EncoderValue.CompressionRle</b>, 
    /// <b>EncoderValue.CompressionCCITT3</b>, <b>EncoderValue.CompressionCCITT4</b>. 
    /// The default compression method is CCITT4.
    /// </remarks>
    public EncoderValue MonochromeTiffCompression
    {
      get { return FMonochromeTiffCompression; }
      set { FMonochromeTiffCompression = value; }
    }

    private bool IsMultiFrameTiff
    {
      get { return ImageFormat == ImageExportFormat.Tiff && MultiFrameTiff; }
    }
    #endregion

    #region Private Methods
    private System.Drawing.Image CreateImage(int width, int height, string suffix)
    {
      if (ImageFormat == ImageExportFormat.Metafile)
        return CreateMetafile(suffix);
      return new Bitmap(width, height);
    }

    private System.Drawing.Image CreateMetafile(string suffix)
    {
      string extension = Path.GetExtension(FileName);
      string fileName = Path.ChangeExtension(FileName, suffix + extension);
      
      System.Drawing.Image image;
      using (Bitmap bmp = new Bitmap(1, 1))
      using (Graphics g = Graphics.FromImage(bmp))
      {
        IntPtr hdc = g.GetHdc();
        if (suffix == "")
          image = new Metafile(Stream, hdc);
        else
        {
          image = new Metafile(fileName, hdc);
          if (!GeneratedFiles.Contains(fileName))
            GeneratedFiles.Add(fileName);
        }  
        g.ReleaseHdc(hdc);
      }
      return image;
    }

    private ImageCodecInfo GetCodec(string codec)
    {
      foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
      {
        if (ice.MimeType == codec)
          return ice;
      }
      return null;
    }

    private Bitmap ConvertToBitonal(Bitmap original)
    {
      Bitmap source = null;

      // If original bitmap is not already in 32 BPP, ARGB format, then convert
      if (original.PixelFormat != PixelFormat.Format32bppArgb)
      {
        source = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
        source.SetResolution(original.HorizontalResolution, original.VerticalResolution);
        using (Graphics g = Graphics.FromImage(source))
        {
          g.DrawImageUnscaled(original, 0, 0);
        }
      }
      else
      {
        source = original;
      }

      // Lock source bitmap in memory
      BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

      // Copy image data to binary array
      int imageSize = sourceData.Stride * sourceData.Height;
      byte[] sourceBuffer = new byte[imageSize];
      Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, imageSize);

      // Unlock source bitmap
      source.UnlockBits(sourceData);

      // Create destination bitmap
      Bitmap destination = new Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed);

      // Lock destination bitmap in memory
      BitmapData destinationData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

      // Create destination buffer
      imageSize = destinationData.Stride * destinationData.Height;
      byte[] destinationBuffer = new byte[imageSize];

      int sourceIndex = 0;
      int destinationIndex = 0;
      int pixelTotal = 0;
      byte destinationValue = 0;
      int pixelValue = 128;
      int height = source.Height;
      int width = source.Width;
      int threshold = 500;

      // Iterate lines
      for (int y = 0; y < height; y++)
      {
        sourceIndex = y * sourceData.Stride;
        destinationIndex = y * destinationData.Stride;
        destinationValue = 0;
        pixelValue = 128;

        // Iterate pixels
        for (int x = 0; x < width; x++)
        {
          // Compute pixel brightness (i.e. total of Red, Green, and Blue values)
          pixelTotal = sourceBuffer[sourceIndex + 1] + sourceBuffer[sourceIndex + 2] + sourceBuffer[sourceIndex + 3];
          if (pixelTotal > threshold)
          {
            destinationValue += (byte)pixelValue;
          }
          if (pixelValue == 1)
          {
            destinationBuffer[destinationIndex] = destinationValue;
            destinationIndex++;
            destinationValue = 0;
            pixelValue = 128;
          }
          else
          {
            pixelValue >>= 1;
          }
          sourceIndex += 4;
        }
        if (pixelValue != 128)
        {
          destinationBuffer[destinationIndex] = destinationValue;
        }
      }

      // Copy binary image data to destination bitmap
      Marshal.Copy(destinationBuffer, 0, destinationData.Scan0, imageSize);

      // Unlock destination bitmap
      destination.UnlockBits(destinationData);

      // Dispose of source if not originally supplied bitmap
      if (source != original)
      {
        source.Dispose();
      }

      // Return
      destination.SetResolution(ResolutionX, ResolutionY);
      return destination;
    }
    
    private void SaveImage(System.Drawing.Image image, string suffix)
    {
      // store the resolution in output file.
      // Call this method after actual draw because it may affect drawing the text
      if (image is Bitmap)
        (image as Bitmap).SetResolution(ResolutionX, ResolutionY);
      if (IsMultiFrameTiff)
      {
        // select the image encoder
        ImageCodecInfo info = GetCodec("image/tiff");
        EncoderParameters ep = new EncoderParameters(2);
        ep.Param[0] = new EncoderParameter(Encoder.Compression, MonochromeTiff ?
          (long)MonochromeTiffCompression : (long)EncoderValue.CompressionLZW);

        if (image == FMasterTiffImage)
        {
          // save the master bitmap
          if (MonochromeTiff)
            FMasterTiffImage = ConvertToBitonal(image as Bitmap);
          ep.Param[1] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
          FMasterTiffImage.Save(Stream, info, ep);
        }
        else
        {
          // save the frame
          if (MonochromeTiff)
          {
            System.Drawing.Image oldImage = image;
            image = ConvertToBitonal(image as Bitmap);
            oldImage.Dispose();
          }
          ep.Param[1] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
          FMasterTiffImage.SaveAdd(image, ep);
        }
      }
      else if (ImageFormat != ImageExportFormat.Metafile)
      {
        string extension = Path.GetExtension(FileName);
        string fileName = Path.ChangeExtension(FileName, suffix + extension);
        // empty suffix means that we should use the Stream that was created in the ExportBase
        Stream stream = suffix == "" ? Stream : new FileStream(fileName, FileMode.Create);
        if (suffix != "")
          GeneratedFiles.Add(fileName);

        if (ImageFormat == ImageExportFormat.Jpeg)
        {
          // handle jpeg separately to set the JpegQuality
          ImageCodecInfo info = GetCodec("image/jpeg");
          EncoderParameters ep = new EncoderParameters();
          ep.Param[0] = new EncoderParameter(Encoder.Quality, JpegQuality);
          image.Save(stream, info, ep);
        }
        else if (ImageFormat == ImageExportFormat.Tiff && MonochromeTiff)
        {
          // handle monochrome tiff separately
          ImageCodecInfo info = GetCodec("image/tiff");
          EncoderParameters ep = new EncoderParameters();
          ep.Param[0] = new EncoderParameter(Encoder.Compression, (long)MonochromeTiffCompression);

          using (Bitmap bwImage = ConvertToBitonal(image as Bitmap))
          {
            bwImage.Save(stream, info, ep);
          }
        }
        else
        {
          ImageFormat format = System.Drawing.Imaging.ImageFormat.Bmp;
          switch (ImageFormat)
          {
            case ImageExportFormat.Gif:
              format = System.Drawing.Imaging.ImageFormat.Gif;
              break;

            case ImageExportFormat.Png:
              format = System.Drawing.Imaging.ImageFormat.Png;
              break;

            case ImageExportFormat.Tiff:
              format = System.Drawing.Imaging.ImageFormat.Tiff;
              break;
          }
          image.Save(stream, format);
        }
        
        if (suffix != "")
          stream.Dispose();
      }

      if (image != FMasterTiffImage)
        image.Dispose();  
    }
    #endregion

    #region Protected Methods
    /// <inheritdoc/>
    protected override string GetFileFilter()
    {
      string filter = ImageFormat.ToString();
      return Res.Get("FileFilters," + filter + "File");
    }

    /// <inheritdoc/>
    protected override void Start()
    {
      FCurOriginY = 0;
      FFirstPage = true;

      if (!SeparateFiles && !IsMultiFrameTiff)
      {
        // create one big image. To do this, calculate max width and sum of pages height
        float width = 0;
        float height = 0;
        
        foreach (int pageNo in Pages)
        {
          SizeF size = Report.PreparedPages.GetPageSize(pageNo);
          if (size.Width > width)
            width = size.Width;
          height += size.Height;
        }

        FBigImage = CreateImage((int)(width * ResolutionX / 96f), (int)(height * ResolutionY / 96f), "");
      }
    }

    /// <inheritdoc/>
    protected override void ExportPage(int pageNo)
    {
      ReportPage page = GetPage(pageNo);
      float zoomX = ResolutionX / 96f;
      float zoomY = ResolutionY / 96f;
      int width = (int)(page.PaperWidth * Units.Millimeters * zoomX);
      int height = (int)(page.PaperHeight * Units.Millimeters * zoomY);
      
      int suffixDigits = Pages[Pages.Length - 1].ToString().Length;
      string fileSuffix = FFirstPage ? "" : (pageNo + 1).ToString("".PadLeft(suffixDigits, '0'));

      System.Drawing.Image image = null;
      if (SeparateFiles || IsMultiFrameTiff)
      {
        image = CreateImage(width, height, fileSuffix);
        if (IsMultiFrameTiff && FMasterTiffImage == null)
          FMasterTiffImage = image;
      }
      else
        image = FBigImage;

      using (Graphics g = Graphics.FromImage(image))
      {
        if (image == FBigImage)
          g.TranslateTransform(0, FCurOriginY);
        g.ScaleTransform(1, zoomY / zoomX);
        page.Draw(new FRPaintEventArgs(g, zoomX, zoomX, Report.GraphicCache));
      }

      if (SeparateFiles || IsMultiFrameTiff)
        SaveImage(image, fileSuffix);
      
      page.Dispose();
      FCurOriginY += height;
      FFirstPage = false;
    }

    /// <inheritdoc/>
    protected override void Finish()
    {
      if (IsMultiFrameTiff)
      {
        // close the file.
        EncoderParameters ep = new EncoderParameters(1);
        ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
        FMasterTiffImage.SaveAdd(ep);
      }
      else if (!SeparateFiles)
        SaveImage(FBigImage, "");
    }
    #endregion

    #region Public Methods
    /// <inheritdoc/>
    public override bool ShowDialog()
    {
      using (ImageExportForm form = new ImageExportForm())
      {
        form.Init(this);
        return form.ShowDialog() == DialogResult.OK;
      }
    }

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
      base.Serialize(writer);
      writer.WriteValue("ImageFormat", ImageFormat);
      writer.WriteBool("SeparateFiles", SeparateFiles);
      writer.WriteInt("ResolutionX", ResolutionX);
      writer.WriteInt("ResolutionY", ResolutionY);
      writer.WriteInt("JpegQuality", JpegQuality);
      writer.WriteBool("MultiFrameTiff", MultiFrameTiff);
      writer.WriteBool("MonochromeTiff", MonochromeTiff);
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageExport"/> class.
    /// </summary>
    public ImageExport()
    {
      FImageFormat = ImageExportFormat.Jpeg;
      FSeparateFiles = true;
      Resolution = 96;
      FJpegQuality = 100;
      FMonochromeTiffCompression = EncoderValue.CompressionCCITT4;
    }
  }
}
