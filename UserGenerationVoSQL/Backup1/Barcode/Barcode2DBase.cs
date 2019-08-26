using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FastReport.Barcode
{
  /// <summary>
  /// The base class for 2D-barcodes such as PDF417 and Datamatrix.
  /// </summary>
  public abstract class Barcode2DBase : BarcodeBase
  {
    private static Font FFont = new Font("Arial", 8);
    
    private void DrawBarcode(Graphics g, float width, float height)
    {
      SizeF originalSize = CalcBounds();
      float kx = width / originalSize.Width;
      float ky = height / originalSize.Height;

      Draw2DBarcode(g, kx, ky);

      // draw the text.
      if (ShowText)
      {
        // When we print, .Net automatically scales the font. However, we need to handle this process.
        // Downscale the font to the screen resolution, then scale by required value (ky).
        float fontZoom = 18f / (int)g.MeasureString(Text, FFont).Height * ky;
        using (Font drawFont = new Font(FFont.Name, FFont.Size * fontZoom, FFont.Style))
        {
          g.DrawString(Text, drawFont, Brushes.Black, new RectangleF(0, height - 18 * ky, width, 18 * ky));
        }
      }
    }
    
    internal virtual void Draw2DBarcode(Graphics g, float kx, float ky)
    {
    }
    
    internal override void DrawBarcode(Graphics g, RectangleF displayRect)
    {
      float width = Angle == 90 || Angle == 270 ? displayRect.Height : displayRect.Width;
      float height = Angle == 90 || Angle == 270 ? displayRect.Width : displayRect.Height;

      GraphicsState state = g.Save();
      try
      {
        // rotate
        g.TranslateTransform(displayRect.Left, displayRect.Top);
        g.RotateTransform(Angle);

        switch (Angle)
        {
          case 90:
            g.TranslateTransform(0, -displayRect.Width);
            break;

          case 180:
            g.TranslateTransform(-displayRect.Width, -displayRect.Height);
            break;

          case 270:
            g.TranslateTransform(-displayRect.Height, 0);
            break;
        }

        DrawBarcode(g, width, height);
      }
      finally
      {
        g.Restore(state);
      }
    }
  }
}
