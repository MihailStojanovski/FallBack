using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Forms;
using FastReport.Utils;
using System.Drawing.Printing;

namespace FastReport.Export.Text
{

    internal partial class TextExportPrintForm : BaseDialogForm
    {
        private PrinterSettings FPrinterSettings;
        private TextExport export;
        private string FCurrentPage;

        public string CurrentPage
        {
            get { return FCurrentPage; }
            set { FCurrentPage = value; }
        }

        private void tbNumbers_TextChanged(object sender, EventArgs e)
        {
            rbNumbers.Checked = true;
        }

        private void rbAll_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                tbNumbers.Text = "";
        }

        private void TextExportPrintForm_FormClosing(object sender, FormClosingEventArgs e)
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

        /// <inheritdoc/>
        public override void Localize()
        {
            base.Localize();
            MyRes res = new MyRes("Forms,PrinterSetup");
            Text = res.Get("");
            gbPageRange.Text = res.Get("PageRange");
            rbAll.Text = res.Get("All");
            rbCurrent.Text = res.Get("Current");
            rbNumbers.Text = res.Get("Numbers");
            lblHint.Text = res.Get("Hint");
            btnOk.Text = res.Get("Print");
            gbPrinter.Text = res.Get("Printer");
            gbCopies.Text = res.Get("Copies");
            lblCount.Text = res.Get("Count");
            gbOther.Text = res.Get("Other");
            res = new MyRes("Export,Text");
            lblPrinterType.Text = res.Get("PrinterType");
            lblCommands.Text = res.Get("Commands");

        }

        private void cbxPrinterTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbcbbCommands.Items.Clear();
            if (cbxPrinterTypes.SelectedIndex >= 0 && cbxPrinterTypes.SelectedIndex < export.PrinterTypes.Count)
                foreach (TextExportPrinterCommand command in export.PrinterTypes[cbxPrinterTypes.SelectedIndex].Commands)
                    cbcbbCommands.Items.Add(command.Name, command.Active);
        }

        private void cbcbbCommands_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            export.PrinterTypes[cbxPrinterTypes.SelectedIndex].Commands[e.Index].Active = !export.PrinterTypes[cbxPrinterTypes.SelectedIndex].Commands[e.Index].Active;
        }

        public TextExportPrintForm(TextExport TextExport)
        {
            FPrinterSettings = new PrinterSettings();
            InitializeComponent();
            Localize();
            export = TextExport;
        }

        private void TextExportPrintForm_Shown(object sender, EventArgs e)
        {
            FPrinterSettings.PrinterName = export.PrinterName;
            string savePrinter = FPrinterSettings.PrinterName;
            if (!FPrinterSettings.IsValid)
                FPrinterSettings.PrinterName = savePrinter;
            foreach (string printer in PrinterSettings.InstalledPrinters)
                cbxPrinter.Items.Add(printer);
            if (cbxPrinter.Items.Count > 0)
                cbxPrinter.SelectedItem = FPrinterSettings.PrinterName;
            foreach (TextExportPrinterType printerType in export.PrinterTypes)
                cbxPrinterTypes.Items.Add(printerType.Name);
            if (export.PrinterType >= 0 && export.PrinterType < export.PrinterTypes.Count)
                cbxPrinterTypes.SelectedIndex = export.PrinterType;
            udCount.Value = (decimal)export.Copies;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            export.PrinterName = cbxPrinter.Text;
            export.Copies = (int)udCount.Value;
            export.PrinterType = cbxPrinterTypes.SelectedIndex;
            if (rbCurrent.Checked)
                export.PageNumbers = FCurrentPage;
            else if (rbNumbers.Checked)
                export.PageNumbers = tbNumbers.Text;
            DialogResult = DialogResult.OK;
        }

        private void cbxPrinter_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                Graphics g = e.Graphics;
                g.DrawImage(Res.GetImage(88), e.Bounds.X + 4, e.Bounds.Y);
                using (Brush b = new SolidBrush(e.ForeColor))
                {
                    g.DrawString((string)cbxPrinter.Items[e.Index], e.Font, b,
                      e.Bounds.X + 30, e.Bounds.Y);
                }
            }
        }

    }
}

