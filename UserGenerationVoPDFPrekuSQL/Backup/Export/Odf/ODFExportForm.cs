using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Export;
using FastReport.Export.Odf;
using FastReport.Utils;


namespace FastReport.Forms
{
    internal partial class ODFExportForm : BaseExportForm
    {
        public override void Init(ExportBase export)
        {
            base.Init(export);
            MyRes res = new MyRes("Export," + ((export is ODSExport) ? "Ods" : "Odt"));
            Text = res.Get("");
            ODFExport odfExport = Export as ODFExport;
            cbWysiwyg.Checked = odfExport.Wysiwyg;
            cbPageBreaks.Checked = odfExport.PageBreaks;
        }
        
        protected override void Done()
        {
            base.Done();
            ODFExport odfExport = Export as ODFExport;
            odfExport.Wysiwyg = cbWysiwyg.Checked;
            odfExport.PageBreaks = cbPageBreaks.Checked;
        }
        
        public override void Localize()
        {
            base.Localize();
            MyRes res = new MyRes("Export,Misc");
            gbOptions.Text = res.Get("Options");
            cbWysiwyg.Text = res.Get("Wysiwyg");
            cbPageBreaks.Text = res.Get("PageBreaks");
        }        
        
        public ODFExportForm()
        {
            InitializeComponent();
        }

        private void gbPageRange_Enter(object sender, EventArgs e)
        {

        }

    }
}

