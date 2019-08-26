using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace FastReport.Utils
{
  internal static class ImageHelper
  {
    public static Bitmap CloneBitmap(Image source)
    {
      if (source == null)
        return null;

      int width = source.Width;
      int height = source.Height;
      Bitmap image = new Bitmap(width, height);
      image.SetResolution(source.HorizontalResolution, source.VerticalResolution);

      using (Graphics g = Graphics.FromImage(image))
      {
        g.DrawImageUnscaled(source, 0, 0);
      }
      return image;
    }
    
    public static void Save(Image image, Stream stream)
    {
      Save(image, stream, ImageFormat.Png);
    }

    public static void Save(Image image, string fileName, ImageFormat format)
    {
      using (FileStream stream = new FileStream(fileName, FileMode.Create))
      {
        Save(image, stream, format);
      }
    }
    
    public static void Save(Image image, Stream stream, ImageFormat format)
    {
      if (image == null)
        return;
      if (image is Bitmap)
        image.Save(stream, format);
      else if (image is Metafile)
      {
        Metafile emf = null;
        using (Bitmap bmp = new Bitmap(1, 1))
        using (Graphics g = Graphics.FromImage(bmp))
        {
          IntPtr hdc = g.GetHdc();
          emf = new Metafile(stream, hdc);
          g.ReleaseHdc(hdc);
        }
        using (Graphics g = Graphics.FromImage(emf))
        {
          g.DrawImage(image, 0, 0);
        }
      }  
    }
    
    public static Image Load(string fileName)
    {
      if (!String.IsNullOrEmpty(fileName))
      {
        using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
          return Load(stream);
        }
      }
      return null;
    }

    public static Image Load(Stream stream)
    {
      if (stream != null && stream.Length > 0)
      {
        // try wmf header
        byte[] wmfHeader = new byte[4];
        long pos = stream.Position;
        stream.Read(wmfHeader, 0, 4);
        stream.Position = pos;
        if (wmfHeader[0] == 0xD7 && wmfHeader[1] == 0xCD && wmfHeader[2] == 0xC6 && wmfHeader[3] == 0x9A)
          return new Metafile(stream);
        
        // try emf header
        byte[] emfHeader = new byte[44];
        pos = stream.Position;
        stream.Read(emfHeader, 0, 44);
        stream.Position = pos;
        if (emfHeader[40] == 0x20 && emfHeader[41] == 0x45 && emfHeader[42] == 0x4D && emfHeader[43] == 0x46)
          return new Metafile(stream);
        
        // it's a bitmap
        try
        {
          using (Bitmap bmp = new Bitmap(stream))
          {
            return CloneBitmap(bmp);
          }
        }
        catch
        {
          Bitmap errorBmp = new Bitmap(10, 10);
          using (Graphics g = Graphics.FromImage(errorBmp))
          {
            g.DrawLine(Pens.Red, 0, 0, 10, 10);
            g.DrawLine(Pens.Red, 0, 10, 10, 0);
          }
          return errorBmp;
        }
      }  
      return null;
    }

    public static Image LoadURL(string url)
    {
      if (!String.IsNullOrEmpty(url))
      {
        using (WebClient web = new WebClient())
        {
          byte[] bytes = web.DownloadData(url);
          using (MemoryStream stream = new MemoryStream(bytes))
          {
            return Load(stream);
          }
        }  
      }
      return null;
    }

    public static Bitmap GetTransparentBitmap(Image source, float transparency)
    {
      if (source == null)
        return null;

      ColorMatrix colorMatrix = new ColorMatrix();
      colorMatrix.Matrix33 = 1 - transparency;
      ImageAttributes imageAttributes = new ImageAttributes();
      imageAttributes.SetColorMatrix(
         colorMatrix,
         ColorMatrixFlag.Default,
         ColorAdjustType.Bitmap);

      int width = source.Width;
      int height = source.Height;
      Bitmap image = new Bitmap(width, height);
      image.SetResolution(source.HorizontalResolution, source.VerticalResolution);

      using (Graphics g = Graphics.FromImage(image))
      {
        g.Clear(Color.Transparent);
        g.DrawImage(
          source,
          new Rectangle(0, 0, width, height),
          0, 0, width, height,
          GraphicsUnit.Pixel,
          imageAttributes);
      }
      return image;
    }
  }
}
