using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using FastReport.Utils;
using FastReport.TypeConverters;

namespace FastReport.Forms
{
  internal partial class PreviewPageSetupForm : BaseDialogForm
  {
    private PrinterSettings FPrinterSettings;
    private ReportPage FPage;
    private bool FUpdating;

    public ReportPage Page
    {
      get { return FPage; }
      set
      {
        FPage = value;
        UpdateControls();
      }
    }
    
    public bool ApplyToAll
    {
      get { return cbApply.Checked; }
    }

    private bool Equal(float a, float b)
    {
      return Math.Abs(a - b) < 2;
    }

    private bool PaperSizeEqual(PaperSize ps, float width, float height)
    {
      float psWidth = ps.Width / 100f * 25.4f;
      float psHeight = ps.Height / 100f * 25.4f;
      return (Equal(psWidth, width) && Equal(psHeight, height)) ||
        (Equal(psWidth, height) && Equal(psHeight, width));
    }

    private void Swap(ref float a, ref float b)
    {
      float c = a;
      a = b;
      b = c;
    }

    private void UpdateControls()
    {
      FUpdating = true;
      MyRes res = new MyRes("Forms,PageSetup");

      string savePrinterName = FPrinterSettings.PrinterName;
      FPrinterSettings.PrinterName = Page.Report.PrintSettings.Printer;
      if (!FPrinterSettings.IsValid)
        FPrinterSettings.PrinterName = savePrinterName;

      // paper
      cbxPaper.Items.Add(res.Get("Custom"));
      foreach (PaperSize ps in FPrinterSettings.PaperSizes)
      {
        cbxPaper.Items.Add(ps.PaperName);
        if (cbxPaper.SelectedIndex == -1 && PaperSizeEqual(ps, Page.PaperWidth, Page.PaperHeight))
          cbxPaper.SelectedIndex = cbxPaper.Items.Count - 1;
      }
      if (cbxPaper.SelectedIndex == -1)
        cbxPaper.SelectedIndex = 0;
      tbWidth.Text = Converter.ToString(Page.PaperWidth, typeof(PaperConverter));
      tbHeight.Text = Converter.ToString(Page.PaperHeight, typeof(PaperConverter));

      // orientation
      rbPortrait.Checked = !Page.Landscape;
      rbLandscape.Checked = Page.Landscape;

      // margins
      tbLeft.Text = Converter.ToString(Page.LeftMargin, typeof(PaperConverter));
      tbRight.Text = Converter.ToString(Page.RightMargin, typeof(PaperConverter));
      tbTop.Text = Converter.ToString(Page.TopMargin, typeof(PaperConverter));
      tbBottom.Text = Converter.ToString(Page.BottomMargin, typeof(PaperConverter));

      FUpdating = false;
    }

    private void UpdatePage()
    {
      Page.Landscape = rbLandscape.Checked;
      Page.PaperWidth = (float)Converter.FromString(tbWidth.Text, typeof(PaperConverter));
      Page.PaperHeight = (float)Converter.FromString(tbHeight.Text, typeof(PaperConverter));
      Page.LeftMargin = (float)Converter.FromString(tbLeft.Text, typeof(PaperConverter));
      Page.RightMargin = (float)Converter.FromString(tbRight.Text, typeof(PaperConverter));
      Page.TopMargin = (float)Converter.FromString(tbTop.Text, typeof(PaperConverter));
      Page.BottomMargin = (float)Converter.FromString(tbBottom.Text, typeof(PaperConverter));
    }

    private void cbxPaper_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (FUpdating)
        return;
      foreach (PaperSize ps in FPrinterSettings.PaperSizes)
      {
        if (ps.PaperName == (string)cbxPaper.SelectedItem)
        {
          float psWidth = ps.Width / 100f * 25.4f;
          float psHeight = ps.Height / 100f * 25.4f;
          if (ps.Kind == PaperKind.A4)
          {
            psWidth = 210;
            psHeight = 297;
          }
          if (rbLandscape.Checked)
            Swap(ref psWidth, ref psHeight);

          FUpdating = true;
          tbWidth.Text = Converter.ToString(psWidth, typeof(PaperConverter));
          tbHeight.Text = Converter.ToString(psHeight, typeof(PaperConverter));
          FUpdating = false;
          break;
        }
      }
    }

    private void tbWidth_TextChanged(object sender, EventArgs e)
    {
      if (FUpdating)
        return;
      cbxPaper.SelectedIndex = 0;
    }

    private void rbPortrait_CheckedChanged(object sender, EventArgs e)
    {
      if (FUpdating || !(sender as RadioButton).Checked)
        return;
      string m1 = tbLeft.Text;   //     m3
      string m2 = tbRight.Text;  //  m1    m2
      string m3 = tbTop.Text;    //     m4
      string m4 = tbBottom.Text; //
      string w = tbWidth.Text;
      string h = tbHeight.Text;

      // avoid reset papersize to custom
      FUpdating = true;
      tbWidth.Text = h;
      tbHeight.Text = w;

      if (rbLandscape.Checked)
      {
        tbLeft.Text = m3;       // rotate counter-clockwise
        tbRight.Text = m4;
        tbTop.Text = m2;
        tbBottom.Text = m1;
      }
      else
      {
        tbLeft.Text = m4;       // rotate clockwise
        tbRight.Text = m3;
        tbTop.Text = m1;
        tbBottom.Text = m2;
      }

      FUpdating = false;
      pnOrientation.Refresh();
    }

    private void pnOrientation_Paint(object sender, PaintEventArgs e)
    {
      using (Bitmap bmp = ResourceLoader.GetBitmap(rbPortrait.Checked ? "portrait.png" : "landscape.png"))
      {
        bmp.MakeTransparent();
        e.Graphics.DrawImage(bmp, (pnOrientation.Width - bmp.Width) / 2, (pnOrientation.Height - bmp.Height) / 2);
      }
    }

    private void PreviewPageSetupForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      if (DialogResult == DialogResult.OK)
        UpdatePage();
    }

    public override void Localize()
    {
      base.Localize();
      MyRes res = new MyRes("Forms,PageSetup");
      Text = res.Get("");

      gbPaper.Text = res.Get("Paper");
      lblWidth.Text = res.Get("Width");
      lblHeight.Text = res.Get("Height");
      gbOrientation.Text = res.Get("Orientation");
      rbPortrait.Text = res.Get("Portrait");
      rbLandscape.Text = res.Get("Landscape");
      gbMargins.Text = res.Get("Margins");
      lblLeft.Text = res.Get("Left");
      lblRight.Text = res.Get("Right");
      lblTop.Text = res.Get("Top");
      lblBottom.Text = res.Get("Bottom");
      cbApply.Text = Res.Get("Forms,PreviewPageSetup,Apply");
    }

    public PreviewPageSetupForm()
    {
      InitializeComponent();
      Localize();
      FPrinterSettings = new PrinterSettings();
    }
  }
}

