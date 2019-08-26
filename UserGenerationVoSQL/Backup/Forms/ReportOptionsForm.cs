using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using FastReport.Utils;
using FastReport.Design;
using FastReport.Code;

namespace FastReport.Forms
{
  internal partial class ReportOptionsForm : BaseDialogForm
  {
    private Report FReport;
    
    public Report Report
    {
      get { return FReport; }
      set { FReport = value; }
    }

    private void ReportOptionsForm_Shown(object sender, EventArgs e)
    {
      // needed for 120dpi mode
      tbDescription.Height = btnLoad.Top - tbDescription.Top - 14;
      tbRefAssemblies.Height = btnAdd.Top - tbRefAssemblies.Top - 12;
      lblScriptNote.Width = tbRefAssemblies.Width;
      tbRecipients.Height = lblSubject.Top - tbRecipients.Top - 8;
      tbMessage.Height = pnEmail.Height - tbMessage.Top - 16;
    }

    private void ReportOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (!Done())
        e.Cancel = true;
    }

    private void btnLoad_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog dialog = new OpenFileDialog())
      {
        dialog.Filter = Res.Get("FileFilters,Images");
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          pbPicture.Image = Image.FromFile(dialog.FileName);
        }
      }
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
      pbPicture.Image = null;
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog dialog = new OpenFileDialog())
      {
        dialog.Filter = Res.Get("FileFilters,Assembly");
        if (dialog.ShowDialog() == DialogResult.OK)
          tbRefAssemblies.Text += "\r\n" + Path.GetFileName(dialog.FileName);
      }
    }

    private void tbPasswordLoad_TextChanged(object sender, EventArgs e)
    {
      tbRetypePassword.Text = "";
    }

    private void rbInherit_CheckedChanged(object sender, EventArgs e)
    {
      btnBrowse.Enabled = rbInherit.Checked;
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog dialog = new OpenFileDialog())
      {
        dialog.Filter = Res.Get("FileFilters,Report");
        if (dialog.ShowDialog() == DialogResult.OK)
          lblBaseName.Text = dialog.FileName;
        else
          lblBaseName.Text = "";  
      }
    }

    private void tbDescription_ButtonClick(object sender, EventArgs e)
    {
      using (StringCollectionEditorForm form = new StringCollectionEditorForm())
      {
        form.PlainText = tbDescription.Text;
        if (form.ShowDialog() == DialogResult.OK)
          tbDescription.Text = form.PlainText;
      }
    }

    private void Init()
    {
      cbDoublePass.Checked = Report.DoublePass;
      cbCompress.Checked = Report.Compressed;
      cbUseFileCache.Checked = Report.UseFileCache;
      cbConvertNulls.Checked = Report.ConvertNulls;
      cbxTextQuality.SelectedIndex = (int)Report.TextQuality;
      cbSmoothGraphics.Checked = Report.SmoothGraphics;

      tbName.Text = Report.ReportInfo.Name;
      tbAuthor.Text = Report.ReportInfo.Author;
      tbVersion.Text = Report.ReportInfo.Version;
      tbDescription.Text = Report.ReportInfo.Description;
      pbPicture.Image = Report.ReportInfo.Picture;
      cbSavePreviewPicture.Checked = Report.ReportInfo.SavePreviewPicture;
      lblCreated1.Text = Report.ReportInfo.Created.ToString();
      lblModified1.Text = Report.ReportInfo.Modified.ToString();
      
      rbC.Checked = Report.ScriptLanguage == Language.CSharp;
      rbVB.Checked = Report.ScriptLanguage == Language.Vb;
      tbRefAssemblies.Lines = Report.ReferencedAssemblies;
      
      tbPassword.Text = Report.Password;
      tbRetypePassword.Text = tbPassword.Text;
      
      if (Report.IsAncestor)
      {
        lblInheritance.Text = Res.Get("Forms,ReportOptions,Inherited") + "\r\n" + Report.BaseReport;
      }
      else
      {
        lblInheritance.Text = Res.Get("Forms,ReportOptions,NotInherited");
        rbDetach.Enabled = false;
      }

      tbRecipients.Text = Report.EmailSettings.RecipientsText;
      tbSubject.Text = Report.EmailSettings.Subject;
      tbMessage.Text = Report.EmailSettings.Message;
    }

    private bool Done()
    {
      if (DialogResult == DialogResult.OK)
      {
        if (tbPassword.Text != tbRetypePassword.Text)
        {
          FRMessageBox.Error(Res.Get("Forms,ReportOptions,PasswordError"));
          pcPages.ActivePage = pnSecurity;
          tbRetypePassword.Focus();
          return false;
        }

        Report.DoublePass = cbDoublePass.Checked;
        Report.Compressed = cbCompress.Checked;
        Report.UseFileCache = cbUseFileCache.Checked;
        Report.ConvertNulls = cbConvertNulls.Checked;
        Report.TextQuality = (TextQuality)cbxTextQuality.SelectedIndex;
        Report.SmoothGraphics = cbSmoothGraphics.Checked;

        Report.ReportInfo.Name = tbName.Text;
        Report.ReportInfo.Author = tbAuthor.Text;
        Report.ReportInfo.Version = tbVersion.Text;
        Report.ReportInfo.Description = tbDescription.Text;
        Report.ReportInfo.Picture = pbPicture.Image;
        Report.ReportInfo.SavePreviewPicture = cbSavePreviewPicture.Checked;
        
        Report.ScriptLanguage = rbC.Checked ? Language.CSharp : Language.Vb;
        Report.ReferencedAssemblies = tbRefAssemblies.Lines;
        Report.Password = tbPassword.Text;

        if (rbDetach.Checked)
        {
          Report.BaseReport = "";
        }
        else if (rbInherit.Checked && !String.IsNullOrEmpty(lblBaseName.Text.Trim()))
        {
          Report.BaseReport = lblBaseName.Text;
        }

        Report.EmailSettings.RecipientsText = tbRecipients.Text;
        Report.EmailSettings.Subject = tbSubject.Text;
        Report.EmailSettings.Message = tbMessage.Text;
      }
      return true;
    }
    
    public override void Localize()
    {
      base.Localize();
      MyRes res = new MyRes("Forms,ReportOptions");
      Text = res.Get("");

      pnGeneral.Text = res.Get("General");
      pnDescription.Text = res.Get("Description");
      pnScript.Text = res.Get("Script");
      pnSecurity.Text = res.Get("Security");
      pnInheritance.Text = res.Get("Inheritance");

      cbDoublePass.Text = res.Get("DoublePass");
      cbCompress.Text = res.Get("Compress");
      cbUseFileCache.Text = res.Get("UseFileCache");
      cbConvertNulls.Text = res.Get("ConvertNulls");
      lblTextQuality.Text = res.Get("TextQuality");
      cbxTextQuality.Items.AddRange(new string[] {
        res.Get("QualityDefault"), res.Get("QualityRegular"), 
        res.Get("QualityClearType"), res.Get("QualityAntiAlias") });
      cbSmoothGraphics.Text = res.Get("SmoothGraphics");

      lblName.Text = res.Get("Name");
      lblAuthor.Text = res.Get("Author");
      lblDescription.Text = res.Get("Description1");
      lblVersion.Text = res.Get("Version");
      lblPicture.Text = res.Get("Picture");
      btnLoad.Text = res.Get("Load");
      btnClear.Text = res.Get("Clear");
      cbSavePreviewPicture.Text = res.Get("SavePreviewPicture");
      lblCreated.Text = res.Get("Created");
      lblModified.Text = res.Get("Modified");
      tbDescription.Image = Res.GetImage(68);
      
      lblLanguage.Text = res.Get("Language");
      lblScriptNote.Text = res.Get("Note");
      lblRefAssemblies.Text = res.Get("RefAssemblies");
      btnAdd.Text = res.Get("Add");

      lblPassword.Text = res.Get("Password");
      lblRetypePassword.Text = res.Get("RetypePassword");
      
      lblChooseInheritance.Text = res.Get("Choose");
      rbDontChange.Text = res.Get("DontChange");
      rbDetach.Text = res.Get("Detach");
      rbInherit.Text = res.Get("Inherit");
      btnBrowse.Text = res.Get("Browse");

      lblRecipients.Text = res.Get("Recipients");
      lblSubject.Text = res.Get("Subject");
      lblMessage.Text = res.Get("Message");
    }

    public ReportOptionsForm(Report report)
    {
      FReport = report;
      InitializeComponent();
      Localize();
      
      Init();
    }
  }

}
