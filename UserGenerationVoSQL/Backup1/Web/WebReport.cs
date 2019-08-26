using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Caching;
using System.IO;
using FastReport;
using FastReport.Export;
using FastReport.Export.Html;
using FastReport.Utils;
using System.Threading;
using System.Drawing;
using System.Web.Configuration;
using System.Web.UI.Design;
using System.Drawing.Design;
using System.Collections;
using System.Data;

namespace FastReport.Web
{
    /// <summary>
    /// Represents the Web Report.
    /// </summary>        
    [Designer(typeof(WebReportComponentDesigner))]
    [ToolboxBitmap(typeof(WebReport), "Resources.Report.bmp")]
    public partial class WebReport : WebControl, INamingContainer
    {
        #region Private fields
      
        private ImageButton btnFirst;
        private ImageButton btnPrev;
        private ImageButton btnNext;
        private ImageButton btnLast;
        private HtmlTable tblNavigator;
        private HtmlGenericControl div;
        private ImageButton btnExport;
        private ImageButton btnPrint;
        private DropDownList cbbExportList;
        private DropDownList cbbZoom;
        private ImageButton btnRefresh;
        private Label lblPages;
        private TextBox tbPage;        

        private const string FPicsPrefix = "frximg";
        private const string FBtnPrefix = "frxbtn";
        private const string FExportPrefix = "frxexp";               

        #endregion

        #region Public properties

        /// <summary>
        /// Get or sets auto width of report
        /// </summary>
        [DefaultValue(false)]
        [Category("Layout")]
        [Browsable(true)]
        public bool AutoWidth
        {
            get
            {
                return (this.ViewState["frxAutoWidth"] != null) ?
                    (bool)this.ViewState["frxAutoWidth"] : false;
            }
            set { this.ViewState["frxAutoWidth"] = value; }
        }

        /// <summary>
        /// Get or sets auto height of report
        /// </summary>
        [DefaultValue(false)]
        [Category("Layout")]
        [Browsable(true)]
        public bool AutoHeight
        {
            get
            {
                return (this.ViewState["frxAutoHeight"] != null) ?
                    (bool)this.ViewState["frxAutoHeight"] : false;
            }
            set { this.ViewState["frxAutoHeight"] = value; }
        }

        /// <summary>
        /// Gets or sets Padding of Report section
        /// </summary>
        [Category("Report")]
        [Browsable(true)]
        public System.Windows.Forms.Padding Padding
        {
            get 
            {       
                if (this.ViewState["frxPadding"] == null)
                    return new System.Windows.Forms.Padding(0, 0, 0, 0);
                else
                    return (System.Windows.Forms.Padding)this.ViewState["frxPadding"];
            }
            set { this.ViewState["frxPadding"] = value; }
        }

        /// <summary>
        /// Delay in cache in minutes
        /// </summary>
        [DefaultValue(15)]
        [Browsable(true)]
        public int CacheDelay
        {
            get
            {
                return (this.ViewState["frxCacheDelay"] != null) ?
              (int)this.ViewState["frxCacheDelay"] : 15;
            }
            set { this.ViewState["frxCacheDelay"] = value; }
        }

        /// <summary>
        /// Report Resource String
        /// </summary>
        [Category("Report")]
        [Browsable(true)]
        public string ReportResourceString
        {
            get
            {
                return (this.ViewState["frxRRS"] != null) ?
                    (string)this.ViewState["frxRRS"] : "";
            }
            set { this.ViewState["frxRRS"] = value; }
        }

        /// <summary>
        /// Gets or sets report data source(s).
        /// </summary>
        /// <remarks>
        /// To pass several datasources, use ';' delimiter, for example: 
        /// "sqlDataSource1;sqlDataSource2"
        /// </remarks>
        [Category("Report")]
        [Browsable(true)]
        public string ReportDataSources
        {
          get
          {
            return (this.ViewState["frxReportDataSources"] != null) ?
                (string)this.ViewState["frxReportDataSources"] : "";
          }
          set { this.ViewState["frxReportDataSources"] = value; }
        }

        /// <summary>
        ///  Switch toolbar visibility
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowToolbar
        {
            get
            {
                return (this.ViewState["frxToolbar"] != null) ?
                    (bool)this.ViewState["frxToolbar"] : true;
            }
            set { this.ViewState["frxToolbar"] = value; }
        }

        /// <summary>
        /// Sets the path to custom buttons on site. 
        /// </summary>
        /// <remarks>
        /// Pictures should be named:
        /// Export.gif, First.gif, First_disabled.gif, Last.gif, Last_disabled.gif, Next.gif, 
        /// Next_disabled.gif, Prev.gif, Prev_disabled.gif, Print.gif, Refresh.gif, Zoom.gif
        /// </remarks>
        [DefaultValue("")]
        [Category("Toolbar")]
        [Browsable(true)]
        public string ButtonsPath
        {
            get
            {
                return (this.ViewState["frxButtonsPath"] != null) ?
                    (string)this.ViewState["frxButtonsPath"] : "";
            }
            set { this.ViewState["frxButtonsPath"] = value; }
        }

        /// <summary>
        ///  Switch visibility of Exports in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowExports
        {
            get
            {
                return (this.ViewState["frxExports"] != null) ?
                    (bool)this.ViewState["frxExports"] : true;
            }
            set { this.ViewState["frxExports"] = value; }
        }

        /// <summary>
        ///  Switch visibility of Print button in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowPrint
        {
            get
            {
                return (this.ViewState["frxPrint"] != null) ?
                    (bool)this.ViewState["frxPrint"] : true;
            }
            set { this.ViewState["frxPrint"] = value; }
        }

        /// <summary>
        ///  Switch visibility of First Button in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowFirstButton
        {
            get
            {
                return (this.ViewState["frxFirstButton"] != null) ?
                    (bool)this.ViewState["frxFirstButton"] : true;
            }
            set { this.ViewState["frxFirstButton"] = value; }
        }

        /// <summary>
        ///  Switch visibility of Previous Button in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowPrevButton
        {
            get
            {
                return (this.ViewState["frxPrevButton"] != null) ?
                    (bool)this.ViewState["frxPrevButton"] : true;
            }
            set { this.ViewState["frxPrevButton"] = value; }
        }

        /// <summary>
        ///  Switch visibility of Next Button in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowNextButton
        {
            get
            {
                return (this.ViewState["frxNextButton"] != null) ?
                    (bool)this.ViewState["frxNextButton"] : true;
            }
            set { this.ViewState["frxNextButton"] = value; }
        }

        /// <summary>
        ///  Switch visibility of Last Button in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowLastButton
        {
            get
            {
                return (this.ViewState["frxLastButton"] != null) ?
                    (bool)this.ViewState["frxLastButton"] : true;
            }
            set { this.ViewState["frxLastButton"] = value; }
        }

        /// <summary>
        ///  Switch visibility of Zoom in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowZoomButton
        {
            get
            {
                return (this.ViewState["frxZoomButton"] != null) ?
                    (bool)this.ViewState["frxZoomButton"] : true;
            }
            set { this.ViewState["frxZoomButton"] = value; }
        }

        /// <summary>
        ///  Switch visibility of Refresh in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowRefreshButton
        {
            get
            {
                return (this.ViewState["frxRefreshButton"] != null) ?
                    (bool)this.ViewState["frxRefreshButton"] : true;
            }
            set { this.ViewState["frxRefreshButton"] = value; }
        }

        /// <summary>
        ///  Switch visibility of Page Number in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowPageNumber
        {
            get
            {
                return (this.ViewState["frxShowPageNumber"] != null) ?
                    (bool)this.ViewState["frxShowPageNumber"] : true;
            }
            set { this.ViewState["frxShowPageNumber"] = value; }
        }

        /// <summary>
        ///  Switch visibility of RTF export in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowRtfExport
        {
            get
            {
                return (this.ViewState["frxShowRtfExport"] != null) ?
                    (bool)this.ViewState["frxShowRtfExport"] : true;
            }
            set { this.ViewState["frxShowRtfExport"] = value; }
        }

        /// <summary>
        ///  Switch visibility of MHT (web-archive) export in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowMhtExport
        {
            get
            {
                return (this.ViewState["frxShowMhtExport"] != null) ?
                    (bool)this.ViewState["frxShowMhtExport"] : true;
            }
            set { this.ViewState["frxShowMhtExport"] = value; }
        }

        /// <summary>
        ///  Switch visibility of ODS export in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowOdsExport
        {
            get
            {
                return (this.ViewState["frxShowOdsExport"] != null) ?
                    (bool)this.ViewState["frxShowOdsExport"] : true;
            }
            set { this.ViewState["frxShowOdsExport"] = value; }
        }

        /// <summary>
        ///  Switch visibility of Excel 2007 export in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowExcel2007Export
        {
            get
            {
                return (this.ViewState["frxShowExcel2007Export"] != null) ?
                    (bool)this.ViewState["frxShowExcel2007Export"] : true;
            }
            set { this.ViewState["frxShowExcel2007Export"] = value; }
        }

        /// <summary>
        ///  Switch visibility of PowerPoint 2007 export in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowPowerPoint2007Export
        {
            get
            {
                return (this.ViewState["frxShowPowerPoint2007Export"] != null) ?
                    (bool)this.ViewState["frxShowPowerPoint2007Export"] : true;
            }
            set { this.ViewState["frxShowPowerPoint2007Export"] = value; }
        }

        /// <summary>
        ///  Switch visibility of XML (Excel) export in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowXmlExcelExport
        {
            get
            {
                return (this.ViewState["frxShowXmlExcelExport"] != null) ?
                    (bool)this.ViewState["frxShowXmlExcelExport"] : true;
            }
            set { this.ViewState["frxShowXmlExcelExport"] = value; }
        }

        /// <summary>
        ///  Switch visibility of PDF (Adobe Acrobat) export in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowPdfExport
        {
            get
            {
                return (this.ViewState["frxShowPdfExport"] != null) ?
                    (bool)this.ViewState["frxShowPdfExport"] : true;
            }
            set { this.ViewState["frxShowPdfExport"] = value; }
        }

        /// <summary>
        ///  Switch visibility of CSV (comma separated values) export in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowCsvExport
        {
            get
            {
                return (this.ViewState["frxShowCsvExport"] != null) ?
                    (bool)this.ViewState["frxShowCsvExport"] : true;
            }
            set { this.ViewState["frxShowCsvExport"] = value; }
        }

        /// <summary>
        ///  Switch visibility of text (plain text) export in toolbar
        /// </summary>
        [DefaultValue(true)]
        [Category("Toolbar")]
        [Browsable(true)]
        public bool ShowTextExport
        {
            get
            {
                return (this.ViewState["frxShowTextExport"] != null) ?
                    (bool)this.ViewState["frxShowTextExport"] : true;
            }
            set { this.ViewState["frxShowTextExport"] = value; }
        }

        /// <summary>
        /// Set the Toolbar color
        /// </summary>
        [DefaultValue(0xECE9D8)]
        [Category("Toolbar")]
        [Browsable(true)]
        public System.Drawing.Color ToolbarColor
        {
            get
            {
                return (this.ViewState["frxToolbarColor"] != null) ?
              (System.Drawing.Color)this.ViewState["frxToolbarColor"] : Color.FromArgb(0xECE9D8);
            }
            set { this.ViewState["frxToolbarColor"] = value; }
        }

        /// <summary>
        /// Switch the pictures visibility in report
        /// </summary>
        [DefaultValue(true)]
        [Category("Report")]
        [Browsable(true)]
        public bool Pictures
        {
            get
            {
                return (this.ViewState["frxPictures"] != null) ?
              (bool)this.ViewState["frxPictures"] : true;
            }
            set { this.ViewState["frxPictures"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of report file.
        /// </summary>
        [Category("Report")]
        [Browsable(true)]
        [Editor(typeof(ReportFileEditor), typeof(UITypeEditor))]
        public string ReportFile
        {
            get
            {
                return (this.ViewState["frxReportFile"] != null) ?
              (string)this.ViewState["frxReportFile"] : "";
            }
            set { this.ViewState["frxReportFile"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of localization file.
        /// </summary>
        [Category("Report")]
        [Browsable(true)]
        [Editor(typeof(LocalizationFileEditor), typeof(UITypeEditor))]
        public string LocalizationFile
        {
          get
          {
            return (this.ViewState["frxLocalizationFile"] != null) ?
              (string)this.ViewState["frxLocalizationFile"] : "";
          }
          set { this.ViewState["frxLocalizationFile"] = value; }
        }

        /// <summary>
        /// Set the zoom factor of previewed page between 0..1
        /// </summary>
        [DefaultValue(1f)]
        [Category("Report")]
        [Browsable(true)]
        public float Zoom
        {
            get
            {
                return (this.ViewState["frxZoom"] != null) ?
              (float)this.ViewState["frxZoom"] : 1f;
            }
            set { this.ViewState["frxZoom"] = value; }
        }

        /// <summary>
        /// Direct access to Report object
        /// </summary>
        [Browsable(false)]
        public Report Report
        {
            get 
            { 
                return (Report)CacheGet("frx" + ReportGuid); 
            }
            set 
            {
                CacheRemove("frx" + ReportGuid);
                CacheAdd("frx" + ReportGuid, value, //null,
                new CacheItemRemovedCallback(this.RemovedCallback), 
                CacheDelay); 
            } 
        }

        /// <summary>
        /// Direct access to HTML export engine
        /// </summary>
        [Browsable(false)]
        public HTMLExport HTMLExport
        {
            get { return (HTMLExport)CacheGet("frxHTML" + ReportGuid); }
            set { CacheAdd("frxHTML" + ReportGuid, value, null, CacheDelay); }
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public int TotalPages
        {
            get { return (HTMLExport != null) ? HTMLExport.Count : 0; }            
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public bool ReportDone
        {
            get
            {
                return (this.ViewState["frxReportDone"] != null) ?
              (bool)this.ViewState["frxReportDone"] : false;
            }
            set { this.ViewState["frxReportDone"] = value; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when report execution is started.
        /// </summary>
        [Browsable(true)]
        public event EventHandler StartReport;
        #endregion

        #region Internal properties

        internal bool HTMLDone
        {
            get
            {
                return (this.ViewState["frxHTMLDone"] != null) ?
              (bool)this.ViewState["frxHTMLDone"] : false;
            }
            set { this.ViewState["frxHTMLDone"] = value; }
        }

        internal int CurrentPage
        {
            get { return (this.ViewState["frxCurrentPage"] != null) ? (int)this.ViewState["frxCurrentPage"] : 0; }
            set { this.ViewState["frxCurrentPage"] = value; }
        }

        internal string ReportGuid
        {
            get
            {
                string s = (string)this.ViewState["frxReportGuid"];
                if (s == null)
                {
                    s = Guid.NewGuid().ToString();
                    this.ViewState["frxReportGuid"] = s;
                }
                return s;
                //return (this.ViewState["frxReportGuid"] != null) ? (string)this.ViewState["frxReportGuid"] :
                //    (string)(this.ViewState["frxReportGuid"] = Guid.NewGuid().ToString());
            }
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnStartReport(EventArgs e)
        {
            if (StartReport != null)
                StartReport(this, e);
        }

        /// <summary>
        /// Force go to next report page
        /// </summary>
        public void NextPage()
        {
            if (CurrentPage < TotalPages - 1)
                CurrentPage++;
        }

        /// <summary>
        /// Force go to previous report page
        /// </summary>
        public void PrevPage()
        {
            if (CurrentPage > 0)
                CurrentPage--;
        }

        /// <summary>
        /// Force go to first report page
        /// </summary>
        public void FirstPage()
        {
            CurrentPage = 0;
        }

        /// <summary>
        /// Force go to last report page
        /// </summary>
        public void LastPage()
        {
            CurrentPage = TotalPages - 1;
        }

        /// <summary>
        /// Force go to "value" report page
        /// </summary>
        public void SetPage(int value)
        {
            if (value >= 0 && value < TotalPages)
                CurrentPage = value;
        }

        /// <summary>
        /// Prepare the report
        /// </summary>
        public void Prepare()
        {
            Refresh();
        }

        /// <summary>
        /// Force refresh of report
        /// </summary>
        public void Refresh()
        {
            CurrentPage = 0;
            ReportDone = false;
            PrepareReport();
        }        

        #region Protected methods

        private void RemovedCallback(String k, Object v, CacheItemRemovedReason r)
        {
            if (v is Report)
            {
                if (((Report)v).PreparedPages != null) 
                    ((Report)v).PreparedPages.Clear();
                ((Report)v).Dispose();
            }
        }

        private Control FindControlRecursive(Control root, string id)
        {
          if (root.ID == id)
            return root;

          foreach (Control ctl in root.Controls)
          {
            Control foundCtl = FindControlRecursive(ctl, id);
            if (foundCtl != null)
              return foundCtl;
          }

          return null;
        }
        
        internal void RegisterData(Report report, IServiceProvider provider)
        {
          string[] dataSources = ReportDataSources.Split(new char[] { ';' });
          foreach (string dataSource in dataSources)
          {
            IDataSource ds = FindControlRecursive(Page, dataSource) as IDataSource;
            if (ds == null)
              continue;
            string dataName = (ds as Control).ID;

            // at design time, use design time data view
            if (provider != null)
            {
              if (ds is AccessDataSource)
              {
                // for MS Access data source, try to convert relative DataFile path.
                // This is needed to avoid the exception when access the database in the design time.
                AccessDataSource accessDs = ds as AccessDataSource;
                string saveDataFile = accessDs.DataFile;
                try
                {
                  accessDs.DataFile = WebUtils.MapPath(provider, accessDs.DataFile);
                  report.RegisterDataAsp(accessDs, accessDs.ID);
                }
                finally
                {
                  accessDs.DataFile = saveDataFile;
                }
              }
              else
              {
                object[] attrs = ds.GetType().GetCustomAttributes(typeof(DesignerAttribute), false);
                if (attrs != null && attrs.Length == 1)
                {
                  DesignerAttribute designerAttr = attrs[0] as DesignerAttribute;
                  DataSourceDesigner dsDesigner = Activator.CreateInstance(Type.GetType(designerAttr.DesignerTypeName)) as DataSourceDesigner;
                  
                  try
                  {
                    dsDesigner.Initialize(ds as IComponent);
                    dsDesigner.RefreshSchema(false);
                    
                    DesignerDataSourceView view = dsDesigner.GetView("");
                    bool isSampleData = false;
                    IEnumerable data = view.GetDesignTimeData(10, out isSampleData);
                    report.RegisterDataAsp(data, dataName);
                  }
                  finally
                  {
                    dsDesigner.Dispose();
                  }
                }
              }
            }
            else
            {
              report.RegisterDataAsp(ds, dataName);
            }
          }
        }

        internal void RegisterData(Report report)
        {
          string[] dataSources = ReportDataSources.Split(new char[] { ';' });
          foreach (string dataSource in dataSources)
          {
            IDataSource ds = FindControlRecursive(Page, dataSource) as IDataSource;
            if (ds == null)
              continue;
            string dataName = (ds as Control).ID;
            report.RegisterDataAsp(ds, dataName);
          }
        }

        private void PrepareReport()
        {
            if (!ReportDone)
            {
                Config.WebMode = true;
                if (Report == null)
                    Report = new Report();
                else
                    Report.Clear();

                if (!String.IsNullOrEmpty(ReportFile))
                {
                    string fileName = ReportFile;
                    if (!WebUtils.IsAbsolutePhysicalPath(fileName))
                        fileName = this.Context.Request.MapPath(fileName, base.AppRelativeTemplateSourceDirectory, true);
                    Report.Load(fileName);
                }
                else if (ReportResourceString != string.Empty)
                    Report.ReportResourceString = ReportResourceString;

                RegisterData(Report);
                OnStartReport(EventArgs.Empty);
                Config.ReportSettings.ShowProgress = false;
                //// usefilecache 
                if (!ReportDone)
                  ReportDone = Report.Prepare(false);
                HTMLDone = false;
            }
            if (!HTMLDone && ReportDone)
            {
                HTMLExport = new HTMLExport();
                HTMLExport.StylePrefix = this.ID;
                HTMLExport.Init_WebMode();
                HTMLExport.Pictures = Pictures;
                HTMLExport.Zoom = Zoom;
                
                if (AutoWidth)
                    HTMLExport.WidthUnits = HtmlSizeUnits.Percent;
                if (AutoHeight)
                    HTMLExport.HeightUnits = HtmlSizeUnits.Percent;

                HTMLExport.WebImagePrefix = this.Page.Request.CurrentExecutionFilePath + "?" + FPicsPrefix;
                HTMLExport.Export(Report, (Stream)null);                
                HTMLDone = true;
            }
        }

        private bool SendPicture()
        {
            string prefix = "";
            if (this.Page.Request.Params[FPicsPrefix] != null)
            {
                prefix = Convert.ToString(this.Page.Request.Params[FPicsPrefix]);
                this.Page.Response.ContentType = "image/png";
            }
            else if (this.Page.Request.Params[FBtnPrefix] != null)
            {
                prefix = Convert.ToString(this.Page.Request.Params[FBtnPrefix]);
                this.Page.Response.ContentType = "image/gif";
            }                

            if (prefix != String.Empty)
            {
                byte[] image= (byte[])CacheGet(prefix);
                if (image != null)    
                    this.Page.Response.BinaryWrite(image);
                this.Page.Response.End();
                return true;
            }
            else
                return false;
        }

        private bool SendExportFile()
        {            
            if (this.Page.Request.Params[FExportPrefix] != null)
            {
                string guid = this.Page.Request.Params[FExportPrefix];
                WebExportItem ExportItem = (WebExportItem)CacheGet("frxExport" + guid);
                if (ExportItem != null)
                {
                    this.Page.Response.ContentType = "application/" + ExportItem.Format;                    
                    this.Page.Response.AppendHeader("Content-Disposition", 
                        "attachment; filename=\"" + ExportItem.FileName + "\"");                    
                    byte[] buff = ExportItem.File.ToArray();
                    this.Page.Response.BinaryWrite(buff);
                    this.Page.Response.End();
                    return true;
                }
            }            
            return false;
        }

        private void SendReportPage()
        {
            if (ShowToolbar)
            {
                string zoomvalue = Math.Round(Zoom * 100).ToString();
                for (int i = 0; i < cbbZoom.Items.Count; i++)
                    if (cbbZoom.Items[i].Value == zoomvalue)
                    {
                        cbbZoom.SelectedIndex = i;
                        break;
                    }
                tbPage.Text = (CurrentPage + 1).ToString();
                lblPages.Text = String.Format(Res.Get("Misc,ofM"), TotalPages.ToString());
            }
            if ((HTMLExport != null) && (HTMLExport.Count > 0))
            {
                //if (SinglePage)
                //{
                //    StringBuilder htmlpage = new StringBuilder();                    
                //    for (int int j = 0; j < HTMLExport.Count; j++)
                //    {
                //        htmlpage.AppendLine(HTMLExport.PreparedPages[j].CSSText);
                //        htmlpage.AppendLine(HTMLExport.PreparedPages[j].PageText);
                //        for (i = 0; i < HTMLExport.PreparedPages[j].Pictures.Count; i++)
                //        {
                //            Stream stream = HTMLExport.PreparedPages[j].Pictures[i];
                //            byte[] image = new byte[stream.Length];
                //            stream.Position = 0;
                //            int n = stream.Read(image, 0, (int)stream.Length);
                //            CacheRemove(HTMLExport.PreparedPages[j].Guids[i]);
                //            CacheAdd(HTMLExport.PreparedPages[j].Guids[i], image, null, 3);
                //        }
                //    }
                //    div.InnerHtml = htmlpage.ToString();                    
                //}
                //else
                //{
                    //while (!HTMLExport.PreparedPages[CurrentPage].PageEvent.WaitOne(10, true))
                    //    System.Windows.Forms.Application.DoEvents();
                if (HTMLExport.PreparedPages[CurrentPage].PageText == null)
                    HTMLExport.ProcessPage(CurrentPage, CurrentPage);
                        
                    if (HTMLExport.PreparedPages[CurrentPage].CSSText != null
                        && HTMLExport.PreparedPages[CurrentPage].PageText != null)
                    {
                        div.InnerHtml = HTMLExport.PreparedPages[CurrentPage].CSSText + HTMLExport.PreparedPages[CurrentPage].PageText;                        
                        for (int i = 0; i < HTMLExport.PreparedPages[CurrentPage].Pictures.Count; i++)
                        {
                            Stream stream = HTMLExport.PreparedPages[CurrentPage].Pictures[i];
                            byte[] image = new byte[stream.Length];
                            stream.Position = 0;
                            int n = stream.Read(image, 0, (int)stream.Length);
                            CacheRemove(HTMLExport.PreparedPages[CurrentPage].Guids[i]);
                            CacheAdd(HTMLExport.PreparedPages[CurrentPage].Guids[i], image, null, 3);
                        }
                    }
                //}                
            }                        
        }

        /// <inheritdoc/>
        protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            bool result = true;            
            if (args is CommandEventArgs)
            {
                CommandEventArgs c = (CommandEventArgs)args;
                if (c.CommandName == btnNext.CommandName)
                    NextPage();
                else if (c.CommandName == btnFirst.CommandName)
                    FirstPage();
                else if (c.CommandName == btnPrev.CommandName)
                    PrevPage();
                else if (c.CommandName == btnLast.CommandName)
                    LastPage();
                else
                    result = false;
            }
            else
                result = false;
            return result;
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)        
        {
            this.EnsureChildControls();
            base.OnLoad(e);
            if (HttpContext.Current != null)
            {
                if (!SendPicture() && !SendExportFile())
                    PrepareReport();
            }
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!String.IsNullOrEmpty(LocalizationFile))
            {
              string fileName = LocalizationFile;
              if (!WebUtils.IsAbsolutePhysicalPath(fileName))
                fileName = this.Context.Request.MapPath(fileName, base.AppRelativeTemplateSourceDirectory, true);
              Res.LoadLocale(fileName);
            }
        }

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            CreateNavigatorControls();
            base.CreateChildControls();
        }

        /// <inheritdoc/>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (HttpContext.Current == null)            
                RenderDesignModeNavigatorControls(writer);
            else
            {
                SetEnableButtons(); 
                SendReportPage();
                base.RenderContents(writer);
            }            
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public WebReport()
        {
            this.Width = Unit.Pixel(550);
            this.Height = Unit.Pixel(500);
            this.ForeColor = Color.Black;
            this.BackColor = Color.White;
        }
    }
}
