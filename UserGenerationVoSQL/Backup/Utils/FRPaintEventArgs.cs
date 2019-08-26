using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace FastReport.Utils
{
  /// <summary>
  /// Provides a data for paint event.
  /// </summary>
  public class FRPaintEventArgs
  {
    private Graphics FGraphics;
    private float FScaleX;
    private float FScaleY;
    private GraphicCache FCache;
    
    /// <summary>
    /// Gets a <b>Graphics</b> object to draw on.
    /// </summary>
    public Graphics Graphics
    {
      get { return FGraphics; }
    }
    
    /// <summary>
    /// Gets the X scale factor.
    /// </summary>
    public float ScaleX
    {
      get { return FScaleX; }
    }

    /// <summary>
    /// Gets the Y scale factor.
    /// </summary>
    public float ScaleY
    {
      get { return FScaleY; }
    }
    
    /// <summary>
    /// Gets the cache that contains graphics objects.
    /// </summary>
    public GraphicCache Cache
    {
      get { return FCache; }
    }

    /// <summary>
    /// Initializes a new instance of the <b>FRPaintEventArgs</b> class with specified settings.
    /// </summary>
    /// <param name="g"><b>Graphics</b> object to draw on.</param>
    /// <param name="scaleX">X scale factor.</param>
    /// <param name="scaleY">Y scale factor.</param>
    /// <param name="cache">Cache that contains graphics objects.</param>
    public FRPaintEventArgs(Graphics g, float scaleX, float scaleY, GraphicCache cache)
    {
      FGraphics = g;
      FScaleX = scaleX;
      FScaleY = scaleY;
      FCache = cache;
    }
  }

}
