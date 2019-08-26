using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Export;
using FastReport.Export.Dbf;
using FastReport.Utils;
using System.Globalization;

namespace FastReport.Forms
{
    internal partial class DbfExportForm : BaseExportForm
    {
        #region Fields

        private bool FFileSelected;
        private string FFileName;

        #endregion // Fields

        #region Methods

        public override void Init(ExportBase export)
        {
            base.Init(export);
            DBFExport dbfExport = Export as DBFExport;
            if (dbfExport.Encoding == Encoding.Default)
                cbbCodepage.SelectedIndex = 0;
            else if (dbfExport.Encoding == Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage))
                cbbCodepage.SelectedIndex = 1;
            cbDataOnly.Checked = dbfExport.DataOnly;
            cbLoadFieldNamesFromFile.Checked = dbfExport.LoadFieldNamesFromFile;
            FFileSelected = false;
            FFileName = "";
        }
        
        protected override void Done()
        {
            base.Done();
            DBFExport dbfExport = Export as DBFExport;
            if (cbbCodepage.SelectedIndex == 0)
                dbfExport.Encoding = Encoding.Default;
            else if (cbbCodepage.SelectedIndex == 1)
                dbfExport.Encoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            dbfExport.DataOnly = cbDataOnly.Checked;
            if (cbLoadFieldNamesFromFile.Checked && FFileSelected)
            {
                dbfExport.LoadFieldNamesFromFile = true;
                dbfExport.FieldNamesFileName = FFileName;
            }
        }
        
        public override void Localize()
        {
            base.Localize();
            MyRes res = new MyRes("Export,Dbf");
            Text = res.Get("");
            lblCodepage.Text = res.Get("Codepage");                        
            cbbCodepage.Items[0] = res.Get("Default");
            cbbCodepage.Items[1] = res.Get("OEM");
            cbDataOnly.Text = res.Get("DataOnly");
            cbLoadFieldNamesFromFile.Text = res.Get("Load");
            res = new MyRes("Export,Misc");            
            gbOptions.Text = res.Get("Options");
            tbFieldNamesFile.Image = Res.GetImage(1);
        }        
        
        public DbfExportForm()
        {
            InitializeComponent();
        }

        private void tbFieldNamesFile_ButtonClick(object sender, EventArgs e)
        {
          FFileSelected = false;
          tbFieldNamesFile.Text = "";
          FFileName = "";
          OpenFileDialog dialog = new OpenFileDialog();
          MyRes res = new MyRes("FileFilters");
          dialog.Filter = res.Get("TxtFile");
          dialog.RestoreDirectory = true;
          if (dialog.ShowDialog() == DialogResult.OK)
          {
            tbFieldNamesFile.Text = dialog.FileName;
            FFileName = dialog.FileName;
            FFileSelected = true;
          }
        }

        #endregion // Methods

    }
}