using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;
using FastReport.Utils;

namespace FastReport.Forms
{
  internal partial class PrinterSetupForm : BaseDialogForm
  {
    private Report FReport;
    private bool FPrintDialog;
    private bool FCollapsed;
    private PrinterSettings FPrinterSettings;
    
    private bool Collapsed
    {
      get { return FCollapsed; }
      set
      {
        FCollapsed = value;
        if (value)
        {
          gbOther.Visible = false;
          gbPrintMode.Visible = false;
          btnMoreOptions.Text = Res.Get("Buttons,MoreOptions");
          btnOk.Top = gbPageRange.Bottom + 15;
          btnCancel.Top = btnOk.Top;
          btnMoreOptions.Top = btnOk.Top;
        }
        else
        {
          gbOther.Visible = true;
          gbPrintMode.Visible = true;
          btnMoreOptions.Text = Res.Get("Buttons,LessOptions");
          btnOk.Top = gbOther.Bottom + 15;
          btnCancel.Top = btnOk.Top;
          btnMoreOptions.Top = btnOk.Top;
        }
        ClientSize = new Size(ClientSize.Width, btnOk.Bottom + 10);
      }
    }
    
    public Report Report
    {
      get { return FReport; }
      set
      {
        FReport = value;
        UpdateControls();
      }
    }
    
    public bool PrintDialog
    {
      get { return FPrintDialog; }
      set 
      { 
        FPrintDialog = value;
        if (value)
        {
          MyRes res = new MyRes("Forms,PrinterSetup");
          Text = res.Get("");
          cbSavePrinter.Visible = false;
          rbCurrent.Enabled = true;
          btnOk.Text = res.Get("Print");
        }
      }
    }
    
    public PrinterSettings PrinterSettings
    {
      get { return FPrinterSettings; }
      set { FPrinterSettings = value; }
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

    private void UpdateControls()
    {
      MyRes res = new MyRes("Forms,PrinterSetup");
      
      // Printer
      string savePrinter = FPrinterSettings.PrinterName;
      FPrinterSettings.PrinterName = Report.PrintSettings.Printer;
      if (!FPrinterSettings.IsValid)
        FPrinterSettings.PrinterName = savePrinter;
      foreach (string printer in PrinterSettings.InstalledPrinters)
      {
        cbxPrinter.Items.Add(printer);
      }
      cbxPrinter.SelectedItem = FPrinterSettings.PrinterName;
      cbSavePrinter.Checked = Report.PrintSettings.SavePrinterWithReport;
      cbPrintToFile.Checked = Report.PrintSettings.PrintToFile;
      
      // Page range
      rbAll.Checked = Report.PrintSettings.PageRange == PageRange.All;
      rbCurrent.Checked = Report.PrintSettings.PageRange == PageRange.Current;
      rbNumbers.Checked = Report.PrintSettings.PageRange == PageRange.PageNumbers;
      tbNumbers.Text = Report.PrintSettings.PageNumbers;
      
      // Copies
      udCount.Value = Report.PrintSettings.Copies;
      cbCollate.Checked = Report.PrintSettings.Collate;
      
      // Other
      cbxOddEven.Items.Add(res.Get("AllPages"));
      cbxOddEven.Items.Add(res.Get("OddPages"));
      cbxOddEven.Items.Add(res.Get("EvenPages"));
      cbxOddEven.SelectedIndex = (int)Report.PrintSettings.PrintPages;
      
      cbxOrder.Items.Add(res.Get("OrderDirect"));
      cbxOrder.Items.Add(res.Get("OrderReverse"));
      cbxOrder.SelectedIndex = Report.PrintSettings.Reverse ? 1 : 0;
      
      cbxDuplex.Items.Add(Res.Get("Forms,PageSetup,DupDefault"));
      cbxDuplex.Items.Add(Res.Get("Forms,PageSetup,DupSimplex"));
      cbxDuplex.Items.Add(Res.Get("Forms,PageSetup,DupVertical"));
      cbxDuplex.Items.Add(Res.Get("Forms,PageSetup,DupHorizontal"));
      cbxDuplex.SelectedIndex = Report.PrintSettings.Duplex == Duplex.Default ? 0 : (int)Report.PrintSettings.Duplex;

      // Print mode
      cbxPrintMode.Items.Add(res.Get("PrintModeDefault"));
      cbxPrintMode.Items.Add(res.Get("PrintModeSplit"));
      cbxPrintMode.Items.Add(res.Get("PrintModeScale"));
      cbxPrintMode.SelectedIndex = (int)Report.PrintSettings.PrintMode;
      
      cbxPagesOnSheet.Items.Add(res.Get("PagesOnSheet1"));
      cbxPagesOnSheet.Items.Add(res.Get("PagesOnSheet2"));
      cbxPagesOnSheet.Items.Add(res.Get("PagesOnSheet4"));
      cbxPagesOnSheet.Items.Add(res.Get("PagesOnSheet8"));
      cbxPagesOnSheet.SelectedIndex = (int)Report.PrintSettings.PagesOnSheet;
    }
    
    private void UpdatePrintSettings()
    {
      PrintSettings ps = Report.PrintSettings;
      
      // Printer
      ps.Printer = (string)cbxPrinter.SelectedItem;
      if (cbSavePrinter.Visible)
        ps.SavePrinterWithReport = cbSavePrinter.Checked;
      ps.PrintToFile = cbPrintToFile.Checked;
      if (PrintDialog && ps.PrintToFile)
      {
        using (SaveFileDialog dialog = new SaveFileDialog())
        {
          dialog.Filter = Res.Get("FileFilters,PrnFile");
          dialog.DefaultExt = "prn";
          if (dialog.ShowDialog() == DialogResult.OK)
            ps.PrintToFileName = dialog.FileName;
          else
            DialogResult = DialogResult.Cancel;  
        }
      }  
      
      // Page range
      if (rbAll.Checked)
        ps.PageRange = PageRange.All;
      else if (rbCurrent.Checked)
        ps.PageRange = PageRange.Current;
      else
        ps.PageRange = PageRange.PageNumbers;
      ps.PageNumbers = tbNumbers.Text;
      
      // Copies
      ps.Copies = (int)udCount.Value;
      ps.Collate = cbCollate.Checked;
      
      // Other
      ps.PrintPages = (PrintPages)cbxOddEven.SelectedIndex;
      ps.Reverse = cbxOrder.SelectedIndex == 1;
      ps.Duplex = (Duplex)(cbxDuplex.SelectedIndex == 0 ? -1 : cbxDuplex.SelectedIndex);
      if (cbxSource.SelectedIndex != -1)
        ps.PaperSource = FPrinterSettings.PaperSources[cbxSource.SelectedIndex].RawKind;
      
      // Print mode
      ps.PrintMode = (PrintMode)cbxPrintMode.SelectedIndex;
      if (ps.PrintMode != PrintMode.Default)
      {
        PaperSize sz = FPrinterSettings.PaperSizes[cbxPrintOnSheet.SelectedIndex];
        ps.PrintOnSheetWidth = sz.Width / 100f * 25.4f;
        ps.PrintOnSheetHeight = sz.Height / 100f * 25.4f;
        ps.PagesOnSheet = ps.PrintMode == PrintMode.Scale ? 
          (PagesOnSheet)cbxPagesOnSheet.SelectedIndex : PagesOnSheet.One;
      }
      else
      {
        ps.PrintOnSheetWidth = 210;
        ps.PrintOnSheetHeight = 297;
        ps.PagesOnSheet = PagesOnSheet.One;
      }
    }
    
    private void cbxPrinter_DrawItem(object sender, DrawItemEventArgs e)
    {
      if (e.Index >= 0)
      {
        e.DrawBackground();
        Graphics g = e.Graphics;
        g.DrawImage(Res.GetImage(88), e.Bounds.X + 4, e.Bounds.Y);
        TextRenderer.DrawText(g, (string)cbxPrinter.Items[e.Index], e.Font, 
          new Point(e.Bounds.X + 30, e.Bounds.Y), e.ForeColor);
      }
    }

    private void btnMoreOptions_Click(object sender, EventArgs e)
    {
      Collapsed = !Collapsed;
    }

    private void btnSettings_Click(object sender, EventArgs e)
    {
      PrinterUtils.ShowPropertiesDialog(FPrinterSettings);
    }

    private void pnCollate_Paint(object sender, PaintEventArgs e)
    {
      string resName = cbCollate.Checked ? "collate1.png" : "collate2.png";
      using (Bitmap bmp = ResourceLoader.GetBitmap(resName))
      {
        bmp.MakeTransparent(Color.Silver);
        e.Graphics.DrawImage(bmp, 0, 0);
      }
    }

    private void cbCollate_CheckedChanged(object sender, EventArgs e)
    {
      pnCollate.Refresh();
    }

    private void cbxPrintMode_DrawItem(object sender, DrawItemEventArgs e)
    {
      if (e.Index >= 0)
      {
        e.DrawBackground();
        Graphics g = e.Graphics;
        string[] resNames = new string[] { "defaultmode.png", "splitmode.png", "scalemode.png" };
        using (Bitmap bmp = ResourceLoader.GetBitmap(resNames[e.Index]))
        {
          bmp.MakeTransparent(Color.Silver);
          g.DrawImage(bmp, e.Bounds.X + 3, e.Bounds.Y + 3);
        }
        SizeF sz = DrawUtils.MeasureString("Wg");
        g.DrawString((string)cbxPrintMode.Items[e.Index], cbxPrintMode.Font, SystemBrushes.WindowText, 
          e.Bounds.X + 70, e.Bounds.Y + (e.Bounds.Height - sz.Height) / 2);
      }
    }

    private void cbxPrinter_SelectedIndexChanged(object sender, EventArgs e)
    {
      FPrinterSettings.PrinterName = (string)cbxPrinter.SelectedItem;
      int indexOfDefaultPaper = -1;
      PaperSize defaultPaper = FPrinterSettings.DefaultPageSettings.PaperSize;

      cbxPrintOnSheet.Items.Clear();
      foreach (PaperSize ps in FPrinterSettings.PaperSizes)
      {
        cbxPrintOnSheet.Items.Add(ps.PaperName);
        if (ps == defaultPaper)
          indexOfDefaultPaper = cbxPrintOnSheet.Items.Count - 1;
        if (cbxPrintOnSheet.SelectedIndex == -1 &&
          PaperSizeEqual(ps, Report.PrintSettings.PrintOnSheetWidth, Report.PrintSettings.PrintOnSheetHeight))
          cbxPrintOnSheet.SelectedIndex = cbxPrintOnSheet.Items.Count - 1;
      }

      cbxSource.Items.Clear();
      int indexOfAutofeed = -1;
      foreach (PaperSource ps in FPrinterSettings.PaperSources)
      {
        cbxSource.Items.Add(ps.SourceName);
        if (ps.Kind == PaperSourceKind.AutomaticFeed)
          indexOfAutofeed = cbxSource.Items.Count - 1;
        if (Report.PrintSettings.PaperSource == ps.RawKind)
          cbxSource.SelectedIndex = cbxSource.Items.Count - 1;
      }
      if (cbxSource.SelectedIndex == -1 && indexOfAutofeed < cbxSource.Items.Count)
        cbxSource.SelectedIndex = indexOfAutofeed;
      
      if (cbxPrintOnSheet.SelectedIndex == -1)
        cbxPrintOnSheet.SelectedIndex = indexOfDefaultPaper;
    }

    private void cbxPrintMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool enabled = cbxPrintMode.SelectedIndex != 0;
      bool enabled1 = cbxPrintMode.SelectedIndex == 2;
      lblPrintOnSheet.Enabled = enabled;
      cbxPrintOnSheet.Enabled = enabled;
      lblPagesOnSheet.Enabled = enabled1;
      cbxPagesOnSheet.Enabled = enabled1;  
    }

    private void tbNumbers_KeyPress(object sender, KeyPressEventArgs e)
    {
      rbNumbers.Checked = true;
    }

    private void rbCurrent_CheckedChanged(object sender, EventArgs e)
    {
      if ((sender as RadioButton).Checked)
        tbNumbers.Text = "";
    }

    private void PrinterSetupForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      if (DialogResult == DialogResult.OK)
        UpdatePrintSettings();
    }

    private void PrinterSetupForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (DialogResult == DialogResult.OK)
      {
        string s = tbNumbers.Text;
        foreach (char c in s)
        {
          if (!(c == ' ' || c == ',' || c == '-' || (c >= '0' && c <= '9')))
          {
            FRMessageBox.Error(Res.Get("Forms,PrinterSetup,Error") + "\r\n" + 
              Res.Get("Forms,PrinterSetup,Hint"));
            tbNumbers.Focus();
            e.Cancel = true;
            break;
          }
        }
      }
    }

    private void PrinterSetupForm_Shown(object sender, EventArgs e)
    {
      // needed for 120dpi mode
      lblHint.Width = tbNumbers.Right - lblHint.Left;
    }

    public override void Localize()
    {
      base.Localize();
      MyRes res = new MyRes("Forms,PrinterSetup");
      Text = res.Get("PrinterSetup");
      gbPrinter.Text = res.Get("Printer");
      cbSavePrinter.Text = res.Get("SavePrinter");
      btnSettings.Text = res.Get("Settings");
      cbPrintToFile.Text = res.Get("PrintToFile");
      gbPageRange.Text = res.Get("PageRange");
      rbAll.Text = res.Get("All");
      rbCurrent.Text = res.Get("Current");
      rbNumbers.Text = res.Get("Numbers");
      lblHint.Text = res.Get("Hint");
      gbCopies.Text = res.Get("Copies");
      lblCount.Text = res.Get("Count");
      cbCollate.Text = res.Get("Collate");
      gbOther.Text = res.Get("Other");
      lblOddEven.Text = res.Get("OddEven");
      lblOrder.Text = res.Get("Order");
      lblDuplex.Text = res.Get("Duplex");
      lblSource.Text = res.Get("Source");
      gbPrintMode.Text = res.Get("PrintMode");
      lblPrintOnSheet.Text = res.Get("PrintOnSheet");
      lblPagesOnSheet.Text = res.Get("PagesOnSheet");
    }

    public PrinterSetupForm()
    {
      InitializeComponent();
      Localize();
      Collapsed = true;
      FPrinterSettings = new PrinterSettings();
    }
  }
}

