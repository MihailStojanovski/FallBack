using FastReport;
using FastReport.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserGeneration
{
    class ReportGen
    {
        public static void GenerateMultipleReports(string filename,StringBuilder builder)
        {

            // Creating the report and adding a page to it
            Report report = new Report();

            ReportPage page = new ReportPage();
            page.CreateUniqueName();
            page.TopMargin = 10.0f;
            page.LeftMargin = 10.0f;
            page.RightMargin = 10.0f;
            page.BottomMargin = 10.0f;
            // Title Creation
            page.ReportTitle = new ReportTitleBand();
            page.ReportTitle.CreateUniqueName();
            page.ReportTitle.Height = 4.0f * Units.Centimeters;
            // Populationg title with text
            TextObject titleText = new TextObject();
            titleText.CreateUniqueName();
            titleText.Left = 1.0f * Units.Centimeters;
            titleText.Top = 1.0f * Units.Centimeters;
            titleText.Width = 17.0f * Units.Centimeters;
            titleText.Height = 2.0f * Units.Centimeters;
            titleText.HorzAlign = HorzAlign.Center;
            titleText.VertAlign = VertAlign.Center;
            titleText.Font = new Font("Arial", 14.0f, FontStyle.Bold);
            titleText.TextColor = Color.Black;
            titleText.Text = "User and Password";
            page.ReportTitle.Objects.Add(titleText);
            
            // Creating the databand where the users info will be
            DataBand band = new DataBand();
            page.Bands.Add(band);
            band.CreateUniqueName();
            band.Height = 2f * Units.Centimeters;

            TextObject bandText = new TextObject();
            bandText.CreateUniqueName();
            bandText.Bounds = new RectangleF(0.0f * Units.Centimeters, 0.0f,
            20.0f * Units.Centimeters, 2f * Units.Centimeters);
            bandText.HorzAlign = HorzAlign.Left;
            bandText.VertAlign = VertAlign.Center;
            bandText.Text = builder.ToString();
            band.AddChild(bandText);
            builder.Clear();

            report.Pages.Add(page);
            report.Prepare();

            // Export of a single user pdf file
            FastReport.Export.Pdf.PDFExport rtfexport = new FastReport.Export.Pdf.PDFExport();

            using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
            {
                report.Export(rtfexport, mStream);
                report.Export(rtfexport, filename);

            }
            
            
        }
    }
}
