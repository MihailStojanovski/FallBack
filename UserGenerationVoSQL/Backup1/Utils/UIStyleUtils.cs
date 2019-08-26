using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Rendering;
using DevComponents.AdvTree;

namespace FastReport.Utils
{
  /// <summary>
  /// The style of FastReport user interface.
  /// </summary>
  public enum UIStyle
  {
    /// <summary>
    /// Specifies the Microsoft Visual Studio 2005 style.
    /// </summary>
    VisualStudio2005,

    /// <summary>
    /// Specifies the Microsoft Office 2003 style (blue).
    /// </summary>
    Office2003,

    /// <summary>
    /// Specifies the Microsoft Office 2007 style (blue).
    /// </summary>
    Office2007Blue,

    /// <summary>
    /// Specifies the Microsoft Office 2007 style (silver).
    /// </summary>
    Office2007Silver,

    /// <summary>
    /// Specifies the Microsoft Office 2007 style (black).
    /// </summary>
    Office2007Black,

    /// <summary>
    /// Specifies the Microsoft Vista style (black).
    /// </summary>
    VistaGlass
  }

  /// <summary>
  /// Contains conversion methods between FastReport's UIStyle to various enums.
  /// </summary>
  public static class UIStyleUtils
  {
    internal static bool IsOffice2007Scheme(UIStyle style)
    {
      return style == UIStyle.Office2007Blue ||
        style == UIStyle.Office2007Silver ||
        style == UIStyle.Office2007Black ||
        style == UIStyle.VistaGlass;
    }

    /// <summary>
    /// Converts FastReport's UIStyle to eDotNetBarStyle.
    /// </summary>
    /// <param name="style">Style to convert.</param>
    /// <returns>Value of eDotNetBarStyle type.</returns>
    public static eDotNetBarStyle GetDotNetBarStyle(UIStyle style)
    {
      switch (style)
      {
        case UIStyle.VisualStudio2005:
          return eDotNetBarStyle.VS2005;
          
        case UIStyle.Office2003:
          return eDotNetBarStyle.Office2003;
          
        case UIStyle.Office2007Blue:
          ((Office2007Renderer)GlobalManager.Renderer).ColorTable = new Office2007ColorTable(eOffice2007ColorScheme.Blue);
          return eDotNetBarStyle.Office2007;
          
        case UIStyle.Office2007Silver:
          ((Office2007Renderer)GlobalManager.Renderer).ColorTable = new Office2007ColorTable(eOffice2007ColorScheme.Silver);
          return eDotNetBarStyle.Office2007;

        case UIStyle.Office2007Black:
          ((Office2007Renderer)GlobalManager.Renderer).ColorTable = new Office2007ColorTable(eOffice2007ColorScheme.Black);
          return eDotNetBarStyle.Office2007;

        case UIStyle.VistaGlass:
          ((Office2007Renderer)GlobalManager.Renderer).ColorTable = new Office2007ColorTable(eOffice2007ColorScheme.VistaGlass);
          return eDotNetBarStyle.Office2007;
      }
      
      return eDotNetBarStyle.Office2007;
    }

    /// <summary>
    /// Converts FastReport's UIStyle to eTabStripStyle.
    /// </summary>
    /// <param name="style">Style to convert.</param>
    /// <returns>Value of eTabStripStyle type.</returns>
    public static eTabStripStyle GetTabStripStyle(UIStyle style)
    {
      switch (style)
      {
        case UIStyle.VisualStudio2005:
          return eTabStripStyle.VS2005Document;

        case UIStyle.Office2003:
          return eTabStripStyle.Office2003;

        case UIStyle.Office2007Blue:
        case UIStyle.Office2007Silver:
        case UIStyle.Office2007Black:
        case UIStyle.VistaGlass:
          return eTabStripStyle.Office2007Document;
      }

      return eTabStripStyle.Office2007Document;
    }

    /// <summary>
    /// Converts FastReport's UIStyle to eTabStripStyle.
    /// </summary>
    /// <param name="style">Style to convert.</param>
    /// <returns>Value of eTabStripStyle type.</returns>
    public static eTabStripStyle GetTabStripStyle1(UIStyle style)
    {
      switch (style)
      {
        case UIStyle.VisualStudio2005:
          return eTabStripStyle.VS2005;

        case UIStyle.Office2003:
          return eTabStripStyle.Office2003;

        case UIStyle.Office2007Blue:
        case UIStyle.Office2007Silver:
        case UIStyle.Office2007Black:
        case UIStyle.VistaGlass:
          return eTabStripStyle.Office2007Dock;
      }

      return eTabStripStyle.Office2007Dock;
    }

    /// <summary>
    /// Converts FastReport's UIStyle to eOffice2007ColorScheme.
    /// </summary>
    /// <param name="style">Style to convert.</param>
    /// <returns>Value of eOffice2007ColorScheme type.</returns>
    public static eOffice2007ColorScheme GetOffice2007ColorScheme(UIStyle style)
    {
      switch (style)
      {
        case UIStyle.Office2007Blue:
          return eOffice2007ColorScheme.Blue;
          
        case UIStyle.Office2007Silver:
          return eOffice2007ColorScheme.Silver;
          
        case UIStyle.Office2007Black:
          return eOffice2007ColorScheme.Black;
          
        case UIStyle.VistaGlass:
          return eOffice2007ColorScheme.VistaGlass;  
      }
      
      return eOffice2007ColorScheme.Blue;
    }

    /// <summary>
    /// Converts FastReport's UIStyle to eColorSchemeStyle.
    /// </summary>
    /// <param name="style">Style to convert.</param>
    /// <returns>Value of eColorSchemeStyle type.</returns>
    public static eColorSchemeStyle GetColorSchemeStyle(UIStyle style)
    {
      switch (style)
      {
        case UIStyle.Office2003:
          return eColorSchemeStyle.Office2003;

        case UIStyle.VisualStudio2005:
          return eColorSchemeStyle.VS2005;
      }

      return eColorSchemeStyle.Office2007;
    }

    /// <summary>
    /// Returns app workspace color for the given style.
    /// </summary>
    /// <param name="style">UI style.</param>
    /// <returns>The color.</returns>
    public static Color GetAppWorkspaceColor(UIStyle style)
    {
      switch (style)
      {
        case UIStyle.Office2003:
          return Color.FromArgb(144, 153, 174);

        case UIStyle.Office2007Blue:
          return Color.FromArgb(101, 145, 205);

        case UIStyle.Office2007Silver:
          return Color.FromArgb(156, 160, 167);

        case UIStyle.Office2007Black:
          return Color.FromArgb(90, 90, 90);

        case UIStyle.VistaGlass:
          return Color.FromArgb(90, 90, 90);
      }
      
      return SystemColors.AppWorkspace;
    }

    /// <summary>
    /// Returns control color for the given style.
    /// </summary>
    /// <param name="style">UI style.</param>
    /// <returns>The color.</returns>
    public static Color GetControlColor(UIStyle style)
    {
      switch (style)
      {
        case UIStyle.Office2003:
          return Color.FromArgb(182, 208, 248);

        case UIStyle.Office2007Blue:
          return Color.FromArgb(180, 212, 255);

        case UIStyle.Office2007Silver:
          return Color.FromArgb(200, 201, 202);

        case UIStyle.Office2007Black:
          return Color.FromArgb(204, 207, 212);

        case UIStyle.VistaGlass:
          return Color.FromArgb(218, 224, 239);
      }
      
      return SystemColors.Control;
    }
  }
}
