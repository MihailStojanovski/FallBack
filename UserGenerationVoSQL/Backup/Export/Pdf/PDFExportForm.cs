using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Export;
using FastReport.Export.Pdf;
using FastReport.Utils;


namespace FastReport.Forms
{
    internal partial class PDFExportForm : BaseExportForm
    {
        public override void Init(ExportBase export)
        {
            base.Init(export);
            PDFExport pdfExport = Export as PDFExport;
            cbCompressed.Checked = pdfExport.Compressed;
            cbEmbedded.Checked = pdfExport.EmbeddingFonts;
            cbBackground.Checked = pdfExport.Background;
            cbPrintOptimized.Checked = pdfExport.PrintOptimized;
            tbTitle.Text = pdfExport.Title;
            tbAuthor.Text = pdfExport.Author;
            tbSubject.Text = pdfExport.Subject;
            tbKeywords.Text = pdfExport.Keywords;
            tbCreator.Text = pdfExport.Creator;
            tbProducer.Text = pdfExport.Producer;

            tbOwnerPassword.Text = pdfExport.OwnerPassword;
            tbUserPassword.Text = pdfExport.UserPassword;
            cbPrintTheDocument.Checked = pdfExport.AllowPrint;
            cbModifyTheDocument.Checked = pdfExport.AllowModify;
            cbCopyOfTextAndGraphics.Checked = pdfExport.AllowCopy;
            cbAnnotations.Checked = pdfExport.AllowAnnotate;
            
            cbHideToolbar.Checked = pdfExport.HideToolbar;
            cbHideMenubar.Checked = pdfExport.HideMenubar;
            cbHideUI.Checked = pdfExport.HideWindowUI;
            cbFitWindow.Checked = pdfExport.FitWindow;
            cbCenterWindow.Checked = pdfExport.CenterWindow;
            cbPrintScaling.Checked = pdfExport.PrintScaling;
            cbPrintAfterOpen.Checked = pdfExport.PrintAfterOpen;
            cbOutline.Checked = pdfExport.Outline;
            
            // not implemented yet
            //pageControlSecurity.Parent = null;
            cbPrintAfterOpen.Visible = false;
            cbOutline.Visible = false;
        }
        
        protected override void Done()
        {
            base.Done();
            PDFExport pdfExport = Export as PDFExport;
            pdfExport.Compressed = cbCompressed.Checked;
            pdfExport.EmbeddingFonts = cbEmbedded.Checked;
            pdfExport.Background = cbBackground.Checked;
            pdfExport.PrintOptimized = cbPrintOptimized.Checked;
            pdfExport.Title = tbTitle.Text;
            pdfExport.Author = tbAuthor.Text;
            pdfExport.Subject = tbSubject.Text;
            pdfExport.Keywords = tbKeywords.Text;
            pdfExport.Creator = tbCreator.Text;
            pdfExport.Producer = tbProducer.Text;

            pdfExport.OwnerPassword = tbOwnerPassword.Text;
            pdfExport.UserPassword = tbUserPassword.Text;
            pdfExport.AllowPrint = cbPrintTheDocument.Checked;
            pdfExport.AllowModify = cbModifyTheDocument.Checked;
            pdfExport.AllowCopy = cbCopyOfTextAndGraphics.Checked;
            pdfExport.AllowAnnotate = cbAnnotations.Checked;

            pdfExport.HideToolbar = cbHideToolbar.Checked;
            pdfExport.HideMenubar = cbHideMenubar.Checked;
            pdfExport.HideWindowUI = cbHideUI.Checked;
            pdfExport.FitWindow = cbFitWindow.Checked;
            pdfExport.CenterWindow = cbCenterWindow.Checked;
            pdfExport.PrintScaling = cbPrintScaling.Checked;
            pdfExport.PrintAfterOpen = cbPrintAfterOpen.Checked;
            pdfExport.Outline = cbOutline.Checked;
        }
        
        public override void Localize()
        {
            base.Localize();
            MyRes res = new MyRes("Export,Misc");
            gbOptions.Text = res.Get("Options");
            res = new MyRes("Export,Pdf");
            Text = res.Get("");
            panPages.Text = res.Get("Export");
            cbCompressed.Text = res.Get("Compressed");
            cbEmbedded.Text = res.Get("EmbeddedFonts");
            cbBackground.Text = res.Get("Background");
            cbPrintOptimized.Text = res.Get("PrintOptimized");
            pageControlInformation.Text = res.Get("Information");
            gbDocumentInfo.Text = res.Get("DocumentInformation");
            lbTitle.Text = res.Get("Title");
            lbAuthor.Text = res.Get("Author");
            lbSubject.Text = res.Get("Subject");
            lbKeywords.Text = res.Get("Keywords");
            lbCreator.Text = res.Get("Creator");
            lbProducer.Text = res.Get("Producer");
            pageControlSecurity.Text = res.Get("Security");
            gbAuth.Text = res.Get("Authentification");
            lbOwnerPassword.Text = res.Get("OwnerPassword");
            lbUserPassword.Text = res.Get("UserPassword");
            gbPermissions.Text = res.Get("Permissions");
            cbPrintTheDocument.Text = res.Get("PrintTheDocument");
            cbModifyTheDocument.Text = res.Get("ModifyTheDocument");
            cbCopyOfTextAndGraphics.Text = res.Get("CopyOfTextAndGraphics");
            cbAnnotations.Text = res.Get("AddOrModifyTextAnnotations");
            pageControlViewer.Text = res.Get("Viewer");
            gbViewerPrfs.Text = res.Get("ViewerPreferences");
            cbHideToolbar.Text = res.Get("HideToolbar");
            cbHideMenubar.Text = res.Get("HideMenubar");
            cbHideUI.Text = res.Get("HideWindowUserInterface");
            cbFitWindow.Text = res.Get("FitWindow");
            cbCenterWindow.Text = res.Get("CenterWindow");
            cbPrintScaling.Text = res.Get("PrintScaling");
            cbPrintAfterOpen.Text = res.Get("PrintAfterOpenOfDocument");
            cbOutline.Text = res.Get("Outline");
        }        
        
        public PDFExportForm()
        {
            InitializeComponent();
        }

    }
}

