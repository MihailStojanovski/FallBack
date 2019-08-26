using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Diagnostics;
using FastReport.Utils;
using FastReport.Forms;
using FastReport.Design;
using FastReport.Data;
using FastReport.Export.Email;
using DevComponents.DotNetBar;

namespace FastReport.Preview
{
  internal class PreviewTab : TabItem
  {
    private PreviewWorkspace FWorkspace;
    private PreviewControl FPreview;
    private PreparedPages FPreparedPages;
    private bool FFake;
    private UIStyle FStyle;

    #region Properties
    public Report Report
    {
      get { return FPreview.Report; }
    }
    
    public SearchInfo SearchInfo
    {
      get { return Workspace.SearchInfo; }
      set { Workspace.SearchInfo = value; }
    }

    private PreviewWorkspace Workspace
    {
      get { return FWorkspace; }
    }
    
    public PreviewControl Preview
    {
      get { return FPreview; }
    }
    
    public PreparedPages PreparedPages
    {
      get { return FPreparedPages; }
    }
    
    public int PageNo
    {
      get { return Workspace.PageNo; }
      set { Workspace.PageNo = value; }
    }
    
    public int PageCount
    {
      get { return Workspace.PageCount; }
    }
    
    public float Zoom
    {
      get { return Workspace.Zoom; }
      set { Workspace.Zoom = value; }
    }

    public bool Disabled
    {
      get { return Workspace.Disabled; }
    }
    
    public bool Fake
    {
      get { return FFake; }
      set { FFake = value; }
    }
    
    public UIStyle Style
    {
      get { return FStyle; }
      set
      {
        FStyle = value;
        Workspace.ColorSchemeStyle = UIStyleUtils.GetDotNetBarStyle(value);
        Workspace.Office2007ColorTable = UIStyleUtils.GetOffice2007ColorScheme(value);
      }
    }
    #endregion
    
    #region Private Methods
    private void form_FormClosed(object sender, FormClosedEventArgs e)
    {
      (sender as PreviewSearchForm).Dispose();
    }
    #endregion

    #region Protected Methods
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (disposing)
        FPreparedPages.Clear();
    }
    #endregion

    #region Public Methods
    public void UpdatePages()
    {
      Workspace.UpdatePages();
    }

    public void PositionTo(int pageNo, PointF point)
    {
      Workspace.PositionTo(pageNo, point);
    }

    public void RefreshReport()
    {
      Workspace.RefreshReport();
    }
    
    public void AddToTabControl(DevComponents.DotNetBar.TabControl tabControl)
    {
      tabControl.Controls.Add(Workspace);
      tabControl.Tabs.Add(this);
    }
    
    public void Focus()
    {
      Workspace.Focus();
    }
    
    public void Refresh()
    {
      Workspace.Refresh();
    }
    
    public void UnlockLayout()
    {
      Workspace.UnlockLayout();
    }
    #endregion

    #region Preview commands
    public bool Print()
    {
      if (Disabled)
        return false;
      return FPreparedPages.Print(PageNo);
    }
    
    public void Save()
    {
      if (Disabled)
        return;

      using (SaveFileDialog dialog = new SaveFileDialog())
      {
        dialog.Filter = Res.Get("FileFilters,PreparedReport");
        dialog.DefaultExt = "*.fpx";
        if (dialog.ShowDialog() == DialogResult.OK)
          Save(dialog.FileName);
      }
    }
    
    public void Save(string fileName)
    {
      if (Disabled)
        return;

      FPreparedPages.Save(fileName);
    }
    
    public void Save(Stream stream)
    {
      if (Disabled)
        return;

      FPreparedPages.Save(stream);
    }
    
    public void Load()
    {
      using (OpenFileDialog dialog = new OpenFileDialog())
      {
        dialog.Filter = Res.Get("FileFilters,PreparedReport");
        if (dialog.ShowDialog() == DialogResult.OK)
          Load(dialog.FileName);
      }
    }
    
    public void Load(string fileName)
    {
      using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        Load(stream);
      }  
    }
    
    public void Load(Stream stream)
    {
      FPreparedPages.Load(stream);
      UpdatePages();
      First();
    }

    public void SendEmail()
    {
      EmailExport export = new EmailExport(Report);
      export.Account = Config.EmailSettings;
      export.Address = Report.EmailSettings.FirstRecipient;
      export.CC = Report.EmailSettings.CCRecipients;
      export.Subject = Report.EmailSettings.Subject;
      export.MessageBody = Report.EmailSettings.Message;

      if (export.ShowDialog() == DialogResult.OK)
      {
        Application.DoEvents();
        export.SendEmail(Report);
      }  
    }

    public void First()
    {
      PageNo = 1;
    }
    
    public void Prior()
    {
      PageNo--;
    }

    public void Next()
    {
      PageNo++;
    }

    public void Last()
    {
      PageNo = PageCount;
    }
    
    public void ZoomIn()
    {
      if (Disabled)
        return;

      Zoom += 0.25f;
    }
    
    public void ZoomOut()
    {
      if (Disabled)
        return;

      Zoom -= 0.25f;
    }
    
    public void ZoomWholePage()
    {
      if (Disabled)
        return;

      SizeF pageSize = FPreparedPages.GetPageSize(PageNo - 1);
      if (pageSize.Width / Workspace.ClientSize.Width > pageSize.Height / Workspace.ClientSize.Height)
        Zoom = (Workspace.ClientSize.Width - 20) / pageSize.Width;
      else
        Zoom = (Workspace.ClientSize.Height - 20) / pageSize.Height;
    }
    
    public void ZoomPageWidth()
    {
      if (Disabled)
        return;

      SizeF pageSize = FPreparedPages.GetPageSize(PageNo - 1);
      Zoom = (Workspace.ClientSize.Width - 20 - SystemInformation.VerticalScrollBarWidth) / pageSize.Width;
    }
    
    public void Find()
    {
      if (Disabled)
        return;

      PreviewSearchForm form = new PreviewSearchForm();
      form.Owner = FPreview.FindForm();
      form.PreviewTab = this;
      form.FormClosed += new FormClosedEventHandler(form_FormClosed);
      form.Show();
    }

    public bool Find(string text, bool matchCase, bool wholeWord)
    {
      if (Disabled)
        return false;

      SearchInfo = new SearchInfo(this);
      return SearchInfo.Find(text, matchCase, wholeWord);
    }
    
    public bool FindNext()
    {
      if (Disabled)
        return false;

      if (SearchInfo != null)
        return SearchInfo.FindNext();
      return false;
    }
    
    public void EditPage()
    {
      if (Disabled)
        return;

      using (Report report = new Report())
      {
        ReportPage page = FPreparedPages.GetPage(PageNo - 1);
        
        OverlayBand overlay = new OverlayBand();
        overlay.Name = "Overlay";
        overlay.Width = (page.PaperWidth - page.LeftMargin - page.RightMargin) * Units.Millimeters;
        overlay.Height = (page.PaperHeight - page.TopMargin - page.BottomMargin) * Units.Millimeters;
        
        // remove bands, convert them to Text objects if necessary
        ObjectCollection allObjects = page.AllObjects;
        foreach (Base c in allObjects)
        {
          if (c is BandBase)
          {
            BandBase band = c as BandBase;
            if (band.HasBorder || band.HasFill)
            {
              TextObject textObj = new TextObject();
              textObj.Bounds = band.Bounds;
              textObj.Border = band.Border.Clone();
              textObj.Fill = band.Fill.Clone();
              overlay.Objects.Add(textObj);
            }
            
            for (int i = 0; i < band.Objects.Count; i++)
            {
              ReportComponentBase obj = band.Objects[i];
              if (!(obj is BandBase))
              {
                obj.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                obj.Dock = DockStyle.None;
                obj.Left = obj.AbsLeft;
                obj.Top = obj.AbsTop;
                overlay.Objects.Add(obj);
                i--;
              }
            }
          }
        }
        
        page.Clear();
        page.Overlay = overlay;
        report.Pages.Add(page);
        page.SetReport(report);
        page.SetRunning(false);
        
        if (report.DesignPreviewPage())
        {
          page = report.Pages[0] as ReportPage;
          FPreparedPages.ModifyPage(PageNo - 1, page);
          Refresh();
        }
      }
    }
    
    public void EditWatermark()
    {
      if (Disabled)
        return;

      ReportPage page = FPreparedPages.GetPage(PageNo - 1);
      using (WatermarkEditorForm editor = new WatermarkEditorForm())
      {
        editor.Watermark = page.Watermark;
        if (editor.ShowDialog() == DialogResult.OK)
        {
          if (editor.ApplyToAll)
            FPreparedPages.ApplyWatermark(editor.Watermark);
          else
          {
            page.Watermark = editor.Watermark;
            FPreparedPages.ModifyPage(PageNo - 1, page);
          }
          Refresh();
        }  
      }
    }
    
    public void PageSetup()
    {
      if (Disabled)
        return;

      using (PreviewPageSetupForm form = new PreviewPageSetupForm())
      {
        ReportPage page = FPreparedPages.GetPage(PageNo - 1);
        form.Page = page;
        if (form.ShowDialog() == DialogResult.OK)
        {
          // avoid weird visual effects
          Refresh();
          
          if (form.ApplyToAll)
          {
            // get original report page
            ReportPage originalPage = page.OriginalComponent.OriginalComponent as ReportPage;
            // no original page - probably we load the existing .fpx file
            if (originalPage == null)
              return;
            // update the report template and refresh a report
            originalPage.Landscape = page.Landscape;
            originalPage.PaperWidth = page.PaperWidth;
            originalPage.PaperHeight = page.PaperHeight;
            originalPage.LeftMargin = page.LeftMargin;
            originalPage.TopMargin = page.TopMargin;
            originalPage.RightMargin = page.RightMargin;
            originalPage.BottomMargin = page.BottomMargin;
            RefreshReport();
          }
          else
          {
            // update current page only
            FPreparedPages.ModifyPage(PageNo - 1, page);
            UpdatePages();
          }
        }
      }
    }
    #endregion

    public PreviewTab(PreviewControl preview, PreparedPages preparedPages, string text)
    {
      FPreview = preview;
      FPreparedPages = preparedPages;

      FWorkspace = new PreviewWorkspace(this);
      FWorkspace.Dock = DockStyle.Fill;
      AttachedControl = FWorkspace;

      Text = text;
      Zoom = preview.Zoom;
      Style = preview.UIStyle;
      First();
    }
  }


  internal class SearchInfo
  {
    private PreviewTab FPreviewTab;
    private string FText;
    private bool FMatchCase;
    private bool FWholeWord;

    public int PageNo;
    public int ObjNo;
    public int RangeNo;
    public CharacterRange[] Ranges;
    public bool Visible;

    public bool Find(string text, bool matchCase, bool wholeWord)
    {
      FText = text;
      FMatchCase = matchCase;
      FWholeWord = wholeWord;
      PageNo = FPreviewTab.PageNo;
      RangeNo = -1;
      return FindNext();
    }

    public bool FindNext()
    {
      Visible = true;
      for (; PageNo <= FPreviewTab.PageCount; PageNo++)
      {
        ReportPage page = FPreviewTab.PreparedPages.GetPage(PageNo - 1);
        ObjectCollection pageObjects = page.AllObjects;
        for (; ObjNo < pageObjects.Count; ObjNo++)
        {
          ISearchable obj = pageObjects[ObjNo] as ISearchable;
          if (obj != null)
          {
            Ranges = obj.SearchText(FText, FMatchCase, FWholeWord);
            if (Ranges != null)
            {
              RangeNo++;
              if (RangeNo < Ranges.Length)
              {
                FPreviewTab.PositionTo(PageNo, (obj as ComponentBase).AbsBounds.Location);
                FPreviewTab.Refresh();
                return true;
              }
            }
          }
          RangeNo = -1;
        }
        ObjNo = 0;
      }
      PageNo = 1;
      Visible = false;
      return false;
    }

    public SearchInfo(PreviewTab tab)
    {
      FPreviewTab = tab;
    }
  }
}
