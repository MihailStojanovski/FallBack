using System;
using System.Collections.Generic;
using System.Text;
using FastReport.Forms;

namespace FastReport.Design
{
  /// <summary>
  /// Base class for all import plugins.
  /// </summary>
  public class ImportPlugin : IDesignerPlugin
  {
    #region Fields
    private string FFilter;
    private Designer FDesigner;
    private string FName;
    #endregion
    
    #region Properties
    /// <summary>
    /// Gets or sets the filter string used in the "Open File" dialog.
    /// </summary>
    public string Filter
    {
      get { return FFilter; }
      set { FFilter = value; }
    }
    
    /// <summary>
    /// Gets a reference to the designer.
    /// </summary>
    public Designer Designer
    {
      get { return FDesigner; }
    }

    /// <summary>
    /// Gets or sets the name of plugin.
    /// </summary>
    public string Name
    {
      get { return FName; }
      set { FName = value; }
    }
    
    /// <inheritdoc/>
    public string PluginName
    {
      get { return FName; }
    }
    #endregion
    
    #region IDesignerPlugin Members
    /// <inheritdoc/>
    public void SaveState()
    {
    }

    /// <inheritdoc/>
    public void RestoreState()
    {
    }

    /// <inheritdoc/>
    public void SelectionChanged()
    {
    }

    /// <inheritdoc/>
    public void UpdateContent()
    {
    }

    /// <inheritdoc/>
    public void Lock()
    {
    }

    /// <inheritdoc/>
    public void Unlock()
    {
    }

    /// <inheritdoc/>
    public virtual void Localize()
    {
    }

    /// <inheritdoc/>
    public DesignerOptionsPage GetOptionsPage()
    {
      return null;
    }

    /// <inheritdoc/>
    public virtual void UpdateUIStyle()
    {
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// Loads the specified file into specified report.
    /// </summary>
    /// <param name="report">Report object.</param>
    /// <param name="fileName">File name.</param>
    public virtual void LoadReport(Report report, string fileName)
    {
      report.Clear();
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportPlugin"/> class with default settings.
    /// </summary>
    /// <param name="designer">The report designer.</param>
    public ImportPlugin(Designer designer)
    {
      FDesigner = designer;
      FFilter = "";
    }
  }
}
