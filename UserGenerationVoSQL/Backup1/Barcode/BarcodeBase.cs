using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using FastReport.Utils;
using FastReport.TypeConverters;
using FastReport.TypeEditors;

namespace FastReport.Barcode
{
  /// <summary>
  /// The base class for all barcodes.
  /// </summary>
  [TypeConverter(typeof(BarcodeConverter))]
  public abstract class BarcodeBase
  {
    #region Fields
    internal string Text;
    internal int Angle;
    internal bool ShowText;
    internal float Zoom;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the name of barcode.
    /// </summary>
    [Browsable(false)]
    public string Name
    {
      get { return Barcodes.GetName(GetType()); }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Creates the exact copy of this barcode.
    /// </summary>
    /// <returns>The copy of this barcode.</returns>
    public BarcodeBase Clone()
    {
      BarcodeBase result = Activator.CreateInstance(GetType()) as BarcodeBase;
      result.Assign(this);
      return result;
    }
    
    /// <summary>
    /// Assigns properties from other, similar barcode.
    /// </summary>
    /// <param name="source">Barcode object to assign properties from.</param>
    public virtual void Assign(BarcodeBase source)
    {
    }

    internal virtual void Serialize(FRWriter writer, string prefix, BarcodeBase diff)
    {
      if (diff.GetType() != GetType())
        writer.WriteStr("Barcode", Name);
    }

    internal virtual void Initialize(string text, bool showText, int angle, float zoom)
    {
      Text = text;
      ShowText = showText;
      Angle = (angle / 90 * 90) % 360;
      Zoom = zoom;
    }
    
    internal virtual SizeF CalcBounds()
    {
      return SizeF.Empty;
    }

    internal virtual void DrawBarcode(Graphics g, RectangleF displayRect)
    {
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="BarcodeBase"/> class with default settings.
    /// </summary>
    public BarcodeBase() 
    { 
      Text = "";
    }
  }
}
