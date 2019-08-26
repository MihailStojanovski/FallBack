using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Export;
using FastReport.Export.Csv;
using FastReport.Utils;
using System.Globalization;


namespace FastReport.Forms
{
    internal partial class CsvExportForm : BaseExportForm
    {
        public override void Init(ExportBase export)
        {
            base.Init(export);
            CSVExport csvExport = Export as CSVExport;
            tbSeparator.Text = csvExport.Separator;
            if (csvExport.Encoding == Encoding.Default)
                cbbCodepage.SelectedIndex = 0;
            else if (csvExport.Encoding == Encoding.UTF8)
                cbbCodepage.SelectedIndex = 1;
            else if (csvExport.Encoding == Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage))
                cbbCodepage.SelectedIndex = 2;
            cbDataOnly.Checked = csvExport.DataOnly;
        }
        
        protected override void Done()
        {
            base.Done();
            CSVExport csvExport = Export as CSVExport;
            csvExport.Separator = tbSeparator.Text;
            if (cbbCodepage.SelectedIndex == 0)
                csvExport.Encoding = Encoding.Default;
            else if (cbbCodepage.SelectedIndex == 1)
                csvExport.Encoding = Encoding.UTF8;
            else if (cbbCodepage.SelectedIndex == 2)
                csvExport.Encoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            csvExport.DataOnly = cbDataOnly.Checked;
        }
        
        public override void Localize()
        {
            base.Localize();
            MyRes res = new MyRes("Export,Csv");
            Text = res.Get("");
            lblSeparator.Text = res.Get("Separator");
            lblCodepage.Text = res.Get("Codepage");                        
            cbbCodepage.Items[0] = res.Get("Default");
            cbbCodepage.Items[1] = res.Get("Unicode");
            cbbCodepage.Items[2] = res.Get("OEM");
            cbDataOnly.Text = res.Get("DataOnly");
            res = new MyRes("Export,Misc");            
            gbOptions.Text = res.Get("Options");
        }        
        
        public CsvExportForm()
        {
            InitializeComponent();
        }

        private void gbPageRange_Enter(object sender, EventArgs e)
        {

        }

    }
}

