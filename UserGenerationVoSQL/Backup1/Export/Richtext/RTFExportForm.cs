using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Export;
using FastReport.Export.RichText;
using FastReport.Utils;


namespace FastReport.Forms
{
    internal partial class RTFExportForm : BaseExportForm
    {

        public override void Init(ExportBase export)
        {
            base.Init(export);
            RTFExport rtfExport = Export as RTFExport;
            cbWysiwyg.Checked = rtfExport.Wysiwyg;
            cbPageBreaks.Checked = rtfExport.PageBreaks;
            if (rtfExport.Pictures)
                cbbPictures.SelectedIndex = rtfExport.ImageFormat == RTFImageFormat.Metafile ? 1 : (rtfExport.ImageFormat == RTFImageFormat.Jpeg ? 2 : 3);
            else 
                cbbPictures.SelectedIndex = 0;
        }
        
        protected override void Done()
        {
            base.Done();
            RTFExport rtfExport = Export as RTFExport;
            rtfExport.Wysiwyg = cbWysiwyg.Checked;
            rtfExport.PageBreaks = cbPageBreaks.Checked;
            rtfExport.Pictures = cbbPictures.SelectedIndex > 0;
            if (cbbPictures.SelectedIndex == 1)
                rtfExport.ImageFormat = RTFImageFormat.Metafile;
            else if (cbbPictures.SelectedIndex == 2)
                rtfExport.ImageFormat = RTFImageFormat.Jpeg;
            else            
                rtfExport.ImageFormat = RTFImageFormat.Png;
        }
        
        public override void Localize()
        {
            base.Localize();
            MyRes res = new MyRes("Export,RichText");
            Text = res.Get("");
            res = new MyRes("Export,Misc");            
            gbOptions.Text = res.Get("Options");
            cbWysiwyg.Text = res.Get("Wysiwyg");
            cbPageBreaks.Text = res.Get("PageBreaks");
            lblPictures.Text = res.Get("Pictures");
            cbbPictures.Items[0] = res.Get("None");
            cbbPictures.Items[1] = res.Get("Metafile");
        }        
        
        public RTFExportForm()
        {
            InitializeComponent();
        }

        private void gbPageRange_Enter(object sender, EventArgs e)
        {

        }

    }
}

