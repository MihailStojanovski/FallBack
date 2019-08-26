using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Export;
using FastReport.Export.OoXML;
using FastReport.Utils;


namespace FastReport.Forms
{
    internal partial class PowerPoint2007ExportForm : BaseExportForm
    {
        public override void Init(ExportBase export)
        {
            base.Init(export);
            PowerPoint2007Export pptExport = Export as PowerPoint2007Export;
            comboBox1.SelectedIndex = (int)pptExport.ImageFormat;
        }
        
        protected override void Done()
        {
            base.Done();
            PowerPoint2007Export pptExport = Export as PowerPoint2007Export;
            pptExport.ImageFormat = (PptImageFormat)comboBox1.SelectedIndex;
        }
        
        public override void Localize()
        {
            base.Localize();
            MyRes res = new MyRes("Export,Pptx");
            Text = res.Get("");
            res = new MyRes("Export,Misc");
            gbOptions.Text = res.Get("Options");
            lblImageFormat.Text = res.Get("Pictures");
        }        
        
        public PowerPoint2007ExportForm()
        {
            InitializeComponent();
        }

    }
}

