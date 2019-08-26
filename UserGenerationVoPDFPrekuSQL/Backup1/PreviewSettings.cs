using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using FastReport.Utils;
using FastReport.TypeConverters;
using FastReport.TypeEditors;

namespace FastReport
{
  /// <summary>
  /// Specifies the set of buttons available in the preview.
  /// </summary>
  [Flags]
  [Editor(typeof(FlagsEditor), typeof(UITypeEditor))]
  public enum PreviewButtons
  {
    /// <summary>
    /// No buttons visible.
    /// </summary>
    None = 0,

    /// <summary>
    /// The "Print" button is visible.
    /// </summary>
    Print = 1,

    /// <summary>
    /// The "Open" button is visible.
    /// </summary>
    Open = 2,

    /// <summary>
    /// The "Save" button is visible.
    /// </summary>
    Save = 4,

    /// <summary>
    /// The "Email" button is visible.
    /// </summary>
    Email = 8,

    /// <summary>
    /// The "Find" button is visible.
    /// </summary>
    Find = 16,

    /// <summary>
    /// The zoom buttons are visible.
    /// </summary>
    Zoom = 32,

    /// <summary>
    /// The "Outline" button is visible.
    /// </summary>
    Outline = 64,

    /// <summary>
    /// The "Page setup" button is visible.
    /// </summary>
    PageSetup = 128,

    /// <summary>
    /// The "Edit" button is visible.
    /// </summary>
    Edit = 256,

    /// <summary>
    /// The "Watermark" button is visible.
    /// </summary>
    Watermark = 512,

    /// <summary>
    /// The page navigator buttons are visible.
    /// </summary>
    Navigator = 1024,
    
    /// <summary>
    /// The "Close" button is visible.
    /// </summary>
    Close = 2048,

    /// <summary>
    /// All buttons are visible.
    /// </summary>
    // if you add something to this enum, don't forget to correct "All" member
    All = Print | Open | Save | Email | Find | Zoom | Outline | PageSetup | Edit | 
     Watermark | Navigator | Close
  }


  /// <summary>
  /// Contains some settings of the preview window.
  /// </summary>
  [TypeConverter(typeof(FRExpandableObjectConverter))]
  public class PreviewSettings
  {
    #region Fields
    private PreviewButtons FButtons;
    private int FPagesInCache;
    private bool FShowInTaskbar;
    private bool FTopMost;
    private Icon FIcon;
    private string FText;
    private bool FFastScrolling;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets a set of buttons that will be visible in the preview's toolbar.
    /// </summary>
    /// <remarks>
    /// Here is an example how you can disable the "Print" and "EMail" buttons:
    /// Config.PreviewSettings.Buttons = PreviewButtons.Open | 
    /// PreviewButtons.Save | 
    /// PreviewButtons.Find | 
    /// PreviewButtons.Zoom | 
    /// PreviewButtons.Outline | 
    /// PreviewButtons.PageSetup | 
    /// PreviewButtons.Edit | 
    /// PreviewButtons.Watermark | 
    /// PreviewButtons.Navigator | 
    /// PreviewButtons.Close; 
    /// </remarks>
    [DefaultValue(PreviewButtons.All)]
    public PreviewButtons Buttons
    {
      get { return FButtons; }
      set { FButtons = value; }
    }

    /// <summary>
    /// Gets or sets the number of prepared pages that can be stored in the memory cache during preview.
    /// </summary>
    /// <remarks>
    /// Decrease this value if your prepared report contains a lot of pictures. This will
    /// save the RAM memory.
    /// </remarks>
    [DefaultValue(50)]
    public int PagesInCache
    {
      get { return FPagesInCache; }
      set { FPagesInCache = value; }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the preview window is displayed in the Windows taskbar. 
    /// </summary>
    [DefaultValue(false)]
    public bool ShowInTaskbar
    {
      get { return FShowInTaskbar; }
      set { FShowInTaskbar = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the preview window should be displayed as a topmost form. 
    /// </summary>
    [DefaultValue(false)]
    public bool TopMost
    {
      get { return FTopMost; }
      set { FTopMost = value; }
    }
    
    /// <summary>
    /// Gets or sets the icon for the preview window.
    /// </summary>
    public Icon Icon
    {
      get 
      { 
        if (FIcon == null)
          FIcon = ResourceLoader.GetIcon("icon16.ico");
        return FIcon;
      }  
      set { FIcon = value; }
    }
    
    /// <summary>
    /// Gets or sets the text for the preview window.
    /// </summary>
    /// <remarks>
    /// If no text is set, the default text "Preview" will be used.
    /// </remarks>
    public string Text
    {
      get { return FText; }
      set { FText = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the fast scrolling method should be used.
    /// </summary>
    /// <remarks>
    /// If you enable this property, the gradient background will be disabled.
    /// </remarks>
    [DefaultValue(false)]
    public bool FastScrolling
    {
      get { return FFastScrolling; }
      set { FFastScrolling = value; }
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <b>PreviewSettings</b> class with default settings. 
    /// </summary>
    public PreviewSettings()
    {
      FButtons = PreviewButtons.All;
      FPagesInCache = 50;
      FText = "";
    }
  }
}