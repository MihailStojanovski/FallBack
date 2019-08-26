using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Export;
using FastReport.Export.Mht;
using FastReport.Utils;


namespace FastReport.Forms
{
    internal partial class MHTExportForm : BaseExportForm
    {

        public override void Init(ExportBase export)
        {
            base.Init(export);
            MHTExport MHTExport = Export as MHTExport;
            cbWysiwyg.Checked = MHTExport.Wysiwyg;
            cbPictures.Checked = MHTExport.Pictures;
        }
        
        protected override void Done()
        {
            base.Done();
            MHTExport MHTExport = Export as MHTExport;
            MHTExport.Wysiwyg = cbWysiwyg.Checked;
            MHTExport.Pictures = cbPictures.Checked;
        }
        
        public override void Localize()
        {
            base.Localize();
            MyRes res = new MyRes("Export,Mht");
            Text = res.Get("");
            res = new MyRes("Export,Misc");
            gbOptions.Text = res.Get("Options");
            cbWysiwyg.Text = res.Get("Wysiwyg");
            cbPictures.Text = res.Get("Pictures");
        }        
        
        public MHTExportForm()
        {
            InitializeComponent();
        }

        private void gbPageRange_Enter(object sender, EventArgs e)
        {

        }
    }
}

