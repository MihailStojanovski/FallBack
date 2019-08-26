using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data.Common;
using FastReport.TypeConverters;
using FastReport.Forms;
using FastReport.Data;
using FastReport.Utils;

namespace FastReport
{
  /// <summary>
  /// This class contains settings that will be applied to the Report component.
  /// </summary>
  [TypeConverter(typeof(FRExpandableObjectConverter))]
  public class ReportSettings
  {
    private Language FDefaultLanguage;
    private ProgressForm FProgress;
    private bool FShowProgress;
    private bool FShowPerformance;

    /// <summary>
    /// Occurs before displaying a progress window.
    /// </summary>
    public event EventHandler StartProgress;

    /// <summary>
    /// Occurs after closing a progress window.
    /// </summary>
    public event EventHandler FinishProgress;

    /// <summary>
    /// Occurs when progress state is changed.
    /// </summary>
    public event ProgressEventHandler Progress;

    /// <include file='Resources/doc.xml' path='//CodeDoc/Topics/EnvironmentSettings/DatabaseLogin/*'/>
    public event DatabaseLoginEventHandler DatabaseLogin;

    /// <summary>
    /// Occurs after the database connection is established.
    /// </summary>
    public event AfterDatabaseLoginEventHandler AfterDatabaseLogin;

    /// <summary>
    /// Occurs when discovering the business object's structure.
    /// </summary>
    public event FilterPropertiesEventHandler FilterBusinessObjectProperties;

    /// <summary>
    /// Occurs when determining the kind of business object's property.
    /// </summary>
    public event GetPropertyKindEventHandler GetBusinessObjectPropertyKind;

    /// <summary>
    /// Gets or sets the default script language.
    /// </summary>
    [DefaultValue(Language.CSharp)]
    public Language DefaultLanguage
    {
      get { return FDefaultLanguage; }
      set { FDefaultLanguage = value; }
    }

    /// <summary>
    /// Gets or sets a value that determines whether to show the progress window
    /// when perform time-consuming operations such as run, print, export.
    /// </summary>
    [DefaultValue(true)]
    public bool ShowProgress
    {
      get { return FShowProgress; }
      set { FShowProgress = value; }
    }

    /// <summary>
    /// Gets or sets a value that determines whether to show the information about
    /// the report performance (report generation time, memory consumed) in the 
    /// lower right corner of the preview window.
    /// </summary>
    [DefaultValue(false)]
    public bool ShowPerformance
    {
      get { return FShowPerformance; }
      set { FShowPerformance = value; }
    }
    
    internal void OnStartProgress(Report report)
    {
      FProgress = null;
      report.SetAborted(false);

      if (ShowProgress)
      {
        if (StartProgress != null)
          StartProgress(report, EventArgs.Empty);
        else 
        {
          FProgress = new ProgressForm(report);
          FProgress.ShowProgressMessage("");
          FProgress.Show();
          FProgress.Refresh();
        }
      }
    }

    internal void OnFinishProgress(Report report)
    {
      if (ShowProgress)
      {
        if (FinishProgress != null)
          FinishProgress(report, EventArgs.Empty);
        else if (FProgress != null)
        {
          FProgress.Close();
          FProgress.Dispose();
          FProgress = null;
        }
      }  
    }

    internal void OnProgress(Report report, string message)
    {
      OnProgress(report, message, 0, 0);
    }

    internal void OnProgress(Report report, string message, int pageNumber, int totalPages)
    {
      if (ShowProgress)
      {
        if (Progress != null)
          Progress(report, new ProgressEventArgs(message, pageNumber, totalPages));
        else if (FProgress != null)
          FProgress.ShowProgressMessage(message);
      }    
    }
    
    internal void OnDatabaseLogin(DataConnectionBase sender, DatabaseLoginEventArgs e)
    {
      if (Config.DesignerSettings.ApplicationConnection != null && 
        sender.GetType() == Config.DesignerSettings.ApplicationConnectionType)
      {
        e.ConnectionString = Config.DesignerSettings.ApplicationConnection.ConnectionString;
      }
      
      if (DatabaseLogin != null)
        DatabaseLogin(sender, e);
    }

    internal void OnAfterDatabaseLogin(DataConnectionBase sender, AfterDatabaseLoginEventArgs e)
    {
      if (AfterDatabaseLogin != null)
        AfterDatabaseLogin(sender, e);
    }

    internal void OnFilterBusinessObjectProperties(object sender, FilterPropertiesEventArgs e)
    {
      if (FilterBusinessObjectProperties != null)
        FilterBusinessObjectProperties(sender, e);
    }

    internal void OnGetBusinessObjectPropertyKind(object sender, GetPropertyKindEventArgs e)
    {
      if (GetBusinessObjectPropertyKind != null)
        GetBusinessObjectPropertyKind(sender, e);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportSettings"/> class.
    /// </summary>
    public ReportSettings()
    {
      FShowProgress = true;
    }
  }
}
