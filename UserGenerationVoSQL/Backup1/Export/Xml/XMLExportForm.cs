using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Export;
using FastReport.Export.Xml;
using FastReport.Utils;


namespace FastReport.Forms
{
    internal partial class XMLExportForm : BaseExportForm
    {

        public override void Init(ExportBase export)
        {
            base.Init(export);
            XMLExport xmlExport = Export as XMLExport;
            cbWysiwyg.Checked = xmlExport.Wysiwyg;
            cbPageBreaks.Checked = xmlExport.PageBreaks;
        }
        
        protected override void Done()
        {
            base.Done();
            XMLExport xmlExport = Export as XMLExport;
            xmlExport.Wysiwyg = cbWysiwyg.Checked;
            xmlExport.PageBreaks = cbPageBreaks.Checked;
        }
        
        public override void Localize()
        {
            base.Localize();
            MyRes res = new MyRes("Export,Xml");
            Text = res.Get("");
            res = new MyRes("Export,Misc");            
            gbOptions.Text = res.Get("Options");
            cbWysiwyg.Text = res.Get("Wysiwyg");
            cbPageBreaks.Text = res.Get("PageBreaks");
        }        
        
        public XMLExportForm()
        {
            InitializeComponent();
        }

        private void gbPageRange_Enter(object sender, EventArgs e)
        {

        }

    }
}

