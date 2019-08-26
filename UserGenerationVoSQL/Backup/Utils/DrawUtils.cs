using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace FastReport.Utils
{
  internal static class DrawUtils
  {
    private static Font FDefaultFont;
    private static Font FDefault96Font;
    private static Font FDefaultReportFont;
    private static Font FFixedFont;
    private static int FScreenDpi;
    
    public static int ScreenDpi
    {
      get 
      {
        if (FScreenDpi == 0)
          FScreenDpi = GetDpi();
        return FScreenDpi;  
      }
    }
    
    private static int GetDpi()
    {
      using (Bitmap bmp = new Bitmap(1, 1))
      using (Graphics g = Graphics.FromImage(bmp))
      {
        return (int)g.DpiX;
      }  
    }

    public static Font DefaultFont
    {
      get
      {
        if (FDefaultFont == null)
          FDefaultFont = new Font("Tahoma", 8);
        return FDefaultFont;
      }
    }
    
    public static Font DefaultReportFont
    {
      get
      {
        if (FDefaultReportFont == null)
          FDefaultReportFont = new Font("Arial", 10);
        return FDefaultReportFont;
      }
    }

    public static Font FixedFont
    {
      get
      {
        if (FFixedFont == null)
          FFixedFont = new Font("Courier New", 10);
        return FFixedFont;
      }
    }

    public static Font Default96Font
    {
      get
      {
        if (FDefault96Font == null)
        {
          float sz = 96f / ScreenDpi;
          FDefault96Font = new Font("Tahoma", 8 * sz);
        }  
        return FDefault96Font;
      }
    }

    public static int DefaultItemHeight
    {
      get
      {
        using (Bitmap bmp = new Bitmap(1, 1))
        using (Graphics g = Graphics.FromImage(bmp))
        {
          return (int)Math.Round(MeasureString("Wg", DefaultFont).Height);
        }
      }
    }

    public static SizeF MeasureString(string text)
    {
      return MeasureString(text, DefaultFont);
    }

    public static SizeF MeasureString(string text, Font font)
    {
      using (Bitmap bmp = new Bitmap(1, 1))
      using (StringFormat sf = new StringFormat())
      {
        Graphics g = Graphics.FromImage(bmp);
        return MeasureString(g, text, font, sf);
      }
    }

    public static SizeF MeasureString(Graphics g, string text, Font font, StringFormat format)
    {
      return MeasureString(g, text, font, new RectangleF(0, 0, 10000, 10000), format);
    }

    public static SizeF MeasureString(Graphics g, string text, Font font, RectangleF layoutRect, StringFormat format)
    {
      if (String.IsNullOrEmpty(text))
        return new SizeF(0, 0);
      CharacterRange[] characterRanges = { new CharacterRange(0, text.Length) };
      StringFormatFlags saveFlags = format.FormatFlags;
      format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
      format.SetMeasurableCharacterRanges(characterRanges);
      Region[] regions = g.MeasureCharacterRanges(text, font, layoutRect, format);
      format.FormatFlags = saveFlags;
      RectangleF rect = regions[0].GetBounds(g);
      regions[0].Dispose();
      return rect.Size;
    }

    public static void FloodFill(Bitmap bmp, int x, int y, Color color, Color replacementColor)
    {
      if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height || bmp.GetPixel(x, y) != color)
        return;
      bmp.SetPixel(x, y, replacementColor);
      FloodFill(bmp, x - 1, y, color, replacementColor);
      FloodFill(bmp, x + 1, y, color, replacementColor);
      FloodFill(bmp, x, y - 1, color, replacementColor);
      FloodFill(bmp, x, y + 1, color, replacementColor);
    }

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, DrawingOptions drawingOptions);
    const int WM_PRINT = 0x317;

    [Flags]
    enum DrawingOptions
    {
      PRF_CHECKVISIBLE = 0x01,
      PRF_NONCLIENT = 0x02,
      PRF_CLIENT = 0x04,
      PRF_ERASEBKGND = 0x08,
      PRF_CHILDREN = 0x10,
      PRF_OWNED = 0x20
    }

    public static Bitmap DrawToBitmap(Control control, bool children)
    {
      Bitmap bitmap = new Bitmap(control.Width, control.Height);
      using (Graphics gr = Graphics.FromImage(bitmap))
      {
        IntPtr hdc = gr.GetHdc();
        DrawingOptions options = DrawingOptions.PRF_ERASEBKGND |
          DrawingOptions.PRF_CLIENT | DrawingOptions.PRF_NONCLIENT;
        if (children)
          options |= DrawingOptions.PRF_CHILDREN;  
        SendMessage(control.Handle, WM_PRINT, hdc, options);
        gr.ReleaseHdc(hdc);
      }
      return bitmap;
    }    
  }
}
