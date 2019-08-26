using System;
using System.Drawing;
using System.ComponentModel;
using FastReport.TypeConverters;
using FastReport.Utils;

namespace FastReport
{
  /// <summary>
  /// This class represents the report information such as name, author, description etc.
  /// </summary>
  [TypeConverter(typeof(FRExpandableObjectConverter))]
  public class ReportInfo
  {
    #region Fields
    private string FName;
    private string FAuthor;
    private string FVersion;
    private string FDescription;
    private Image FPicture;
    private DateTime FCreated;
    private DateTime FModified;
    private bool FSavePreviewPicture;
    private string FCreatorVersion;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the name of a report.
    /// </summary>
    public string Name
    {
      get { return FName; }
      set { FName = value; }
    }

    /// <summary>
    /// Gets or sets the author of a report.
    /// </summary>
    public string Author
    {
      get { return FAuthor; }
      set { FAuthor = value; }
    }

    /// <summary>
    /// Gets or sets the report version.
    /// </summary>
    public string Version
    {
      get { return FVersion; }
      set { FVersion = value; }
    }

    /// <summary>
    /// Gets or sets the report description.
    /// </summary>
    public string Description
    {
      get { return FDescription; }
      set { FDescription = value; }
    }

    /// <summary>
    /// Gets or sets the picture associated with a report.
    /// </summary>
    public Image Picture
    {
      get { return FPicture; }
      set { FPicture = value; }
    }

    /// <summary>
    /// Gets or sets the report creation date and time.
    /// </summary>
    public DateTime Created
    {
      get { return FCreated; }
      set { FCreated = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating that report was modified in the designer.
    /// </summary>
    public DateTime Modified
    {
      get { return FModified; }
      set { FModified = value; }
    }

    /// <summary>
    /// Gets or sets a value that determines whether to fill the <see cref="Picture"/> property
    /// automatically.
    /// </summary>
    [DefaultValue(false)]
    public bool SavePreviewPicture
    {
      get { return FSavePreviewPicture; }
      set { FSavePreviewPicture = value; }
    }
    
    /// <summary>
    /// Gets the version of FastReport that was created this report file.
    /// </summary>
    public string CreatorVersion
    {
      get { return FCreatorVersion; }
      set { FCreatorVersion = value; }
    }
    
    private string CurrentVersion
    {
      get { return GetType().Assembly.GetName().Version.ToString(); }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Resets all properties to its default values.
    /// </summary>
    public void Clear()
    {
      FName = "";
      FAuthor = "";
      FVersion = "";
      FDescription = "";
      if (FPicture != null)
        FPicture.Dispose();
      FPicture = null;
      FCreated = DateTime.Now;
      FModified = DateTime.Now;
      FSavePreviewPicture = false;
      FCreatorVersion = CurrentVersion;
    }
    
    internal void Serialize(FRWriter writer, ReportInfo c)
    {
      if (Name != c.Name)
        writer.WriteStr("ReportInfo.Name", Name);
      if (Author != c.Author)
        writer.WriteStr("ReportInfo.Author", Author);
      if (Version != c.Version)
        writer.WriteStr("ReportInfo.Version", Version);
      if (Description != c.Description)
        writer.WriteStr("ReportInfo.Description", Description);
      if (!writer.AreEqual(Picture, c.Picture))
        writer.WriteValue("ReportInfo.Picture", Picture);
      writer.WriteValue("ReportInfo.Created", Created);
      FModified = DateTime.Now;
      writer.WriteValue("ReportInfo.Modified", Modified);
      if (SavePreviewPicture != c.SavePreviewPicture)
        writer.WriteBool("ReportInfo.SavePreviewPicture", SavePreviewPicture);
      writer.WriteStr("ReportInfo.CreatorVersion", CurrentVersion);
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportInfo"/> class with default settings.
    /// </summary>
    public ReportInfo() 
    {
      Clear();
    }
  }
}