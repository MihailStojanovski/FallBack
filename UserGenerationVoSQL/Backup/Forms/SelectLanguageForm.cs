using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Utils;
using Microsoft.Win32;
using System.IO;

namespace FastReport.Forms
{
  internal partial class SelectLanguageForm : BaseDialogForm
  {
    private void Init()
    {
      tbFolder.Text = Res.LocaleFolder;
      PopulateLocalizations(tbFolder.Text);
    }

    private void Done()
    {
      Res.DefaultLocaleName = lbxLanguages.SelectedIndex <= 0 ? "" : (string)lbxLanguages.SelectedItem;
      Res.LocaleFolder = tbFolder.Text;
      Res.LoadDefaultLocale();
    }

    private void PopulateLocalizations(string folder)
    {
      lbxLanguages.Items.Clear();
      lbxLanguages.Items.Add(Res.Get("Forms,SelectLanguage,Auto"));
      lbxLanguages.SelectedIndex = 0;
      
      List<string> files = new List<string>();
      if (Directory.Exists(folder))
      {
        foreach (string file in Directory.GetFiles(folder, "*.frl"))
        {
          files.Add(Path.GetFileNameWithoutExtension(file));
        }
      }
      files.Add("English");
      files.Sort();
      
      foreach (string file in files)
      {
        lbxLanguages.Items.Add(file);
        if (String.Compare(file, Res.DefaultLocaleName, true) == 0)
          lbxLanguages.SelectedIndex = lbxLanguages.Items.Count - 1;
      }
    }

    private void tbFolder_ButtonClick(object sender, EventArgs e)
    {
      using (FolderBrowserDialog dialog = new FolderBrowserDialog())
      {
        dialog.SelectedPath = tbFolder.Text;
        dialog.ShowNewFolderButton = false;
        dialog.Description = Res.Get("Forms,SelectLanguage,SelectFolder");
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          tbFolder.Text = dialog.SelectedPath;
          PopulateLocalizations(tbFolder.Text);
        }
      }
    }

    private void lbxLanguages_DoubleClick(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void SelectLanguageForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      if (DialogResult == DialogResult.OK)
        Done();
    }

    public override void Localize()
    {
      base.Localize();

      MyRes res = new MyRes("Forms,SelectLanguage");
      Text = res.Get("");
      lblSelect.Text = res.Get("SelectLanguage");
      lblFolder.Text = res.Get("Folder");
      tbFolder.Image = Res.GetImage(1);
    }

    public SelectLanguageForm()
    {
      InitializeComponent();
      Localize();
      Init();
    }

    private void SelectLanguageForm_Shown(object sender, EventArgs e)
    {
      lbxLanguages.Height = lblFolder.Top - lbxLanguages.Top - 12;
    }
  }
}

