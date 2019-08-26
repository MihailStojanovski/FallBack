using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using FastReport.Utils;
using FastReport.Forms;
using FastReport.Export;
using System.Globalization;


namespace FastReport.Export.Odf
{
    /// <summary>
    /// Open Document Spreadsheet export (Open Office Calc)
    /// </summary>
    public class ODSExport : ODFExport
    {
        /// <inheritdoc/>
        protected override string GetFileFilter()
        {
            return new MyRes("FileFilters").Get("OdsFile");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODSExport"/> class.
        /// </summary>
        public ODSExport()
        {
            ExportType = 0;            
        }
    }

    /// <summary>
    /// Open Document Text export (Open Office Writer)
    /// </summary>
    public class ODTExport : ODFExport
    {
        /// <inheritdoc/>
        protected override string GetFileFilter()
        {
            return new MyRes("FileFilters").Get("OdtFile");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODTExport"/> class.
        /// </summary>
        public ODTExport()
        {
            ExportType = 1;
        }
    }

    /// <summary>
    /// Base class for other OASIS exports
    /// </summary>
    public class ODFExport : ExportBase
    {
        #region Constants
        const float odfDivider = 37.82f;
        const float odfPageDiv = 10f;
        const float odfMargDiv = 10f;
        #endregion

        #region Private fields
        private bool FPageBreaks;
        private ExportMatrix FMatrix;
        private bool FWysiwyg;
        private string FCreator;
        private int FExportType;

        private float FPageLeft;
        private float FPageTop;
        private float FPageBottom;
        private float FPageRight;
        private bool FPageLandscape;
        private float FPageWidth;
        private float FPageHeight;

        private bool FFirstPage;

        #endregion

        #region Properties

        internal int ExportType
        {
            get { return FExportType; }
            set { FExportType = value; }
        }

        /// <summary>
        /// Switch of page breaks
        /// </summary>
        public bool PageBreaks
        {
            get { return FPageBreaks; }
            set { FPageBreaks = value; }
        }
                
        /// <summary>
        /// Wysiwyg mode, set for better results
        /// </summary>
        public bool Wysiwyg
        {
            get { return FWysiwyg; }
            set { FWysiwyg = value; }
        }
        
        /// <summary>
        /// Creator of the document
        /// </summary>
        public string Creator
        {
            get { return FCreator; }
            set { FCreator = value; }
        }

        #endregion

        #region Private Methods

        private void OdfCreateMeta(string FileName, string Creator)
        {
            using(FileStream file = new FileStream(FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                Out.WriteLine("<office:document-meta xmlns:office=\"urn:oasis:names:tc:opendocument:xmlns:office:1.0\" " +
                    "xmlns:xlink=\"http://www.w3.org/1999/xlink\" " +
                    "xmlns:dc=\"http://purl.org/dc/elements/1.1/\" " +
                    "xmlns:meta=\"urn:oasis:names:tc:opendocument:xmlns:meta:1.0\">");
                Out.WriteLine("  <office:meta>");
                Out.WriteLine("    <meta:generator>fast-report.com/Fast Report.NET/build:" + Config.Version + "</meta:generator>");
                Out.WriteLine("    <meta:initial-creator>" + ExportUtils.XmlString(Creator, false) + "</meta:initial-creator>");
                Out.WriteLine("    <meta:creation-date>" + DateTime.Now.Date.ToShortDateString() + "T" + 
                    DateTime.Now.TimeOfDay.ToString() + "</meta:creation-date>");
                Out.WriteLine("  </office:meta>");
                Out.WriteLine("</office:document-meta>");
            }
        }

        private void OdfCreateMime(string FileName, string MValue)
        {
            using (FileStream file = new FileStream(FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
                Out.WriteLine("application/vnd.oasis.opendocument." + MValue);
        }

        private void OdfCreateManifest(string FileName, int PicCount, string MValue)
        {
            
            using(FileStream file = new FileStream(FileName, FileMode.Create))
            using(StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                Out.WriteLine("<manifest:manifest xmlns=\"urn:oasis:names:tc:opendocument:xmlns:manifest:1.0\">");
                Out.WriteLine("  <manifest:file-entry manifest:media-type=\"application/vnd.oasis.opendocument." + MValue + "\" manifest:full-path=\"/\"/>");
                Out.WriteLine("  <manifest:file-entry manifest:media-type=\"text/xml\" manifest:full-path=\"content.xml\"/>");                
                Out.WriteLine("  <manifest:file-entry manifest:media-type=\"text/xml\" manifest:full-path=\"content.xml\"/>");
                Out.WriteLine("  <manifest:file-entry manifest:media-type=\"text/xml\" manifest:full-path=\"styles.xml\"/>");
                Out.WriteLine("  <manifest:file-entry manifest:media-type=\"text/xml\" manifest:full-path=\"meta.xml\"/>");
                string s = Config.FullTrust ? ".emf" : ".png";
                for (int i = 1; i <= PicCount; i++)
                    Out.WriteLine("  <manifest:file-entry  manifest:media-type=\"image\" manifest:full-path=\"Pictures/Pic" + i.ToString() + s + "\"/>");
                Out.WriteLine("</manifest:manifest>");
            }
        }

        private string OdfGetFrameName(LineStyle Style)
        {
            if (Style == LineStyle.Double)
                return "double";
            else
                return "solid";
        }

        private string OdfMakeXmlHeader()
        {
            return " xmlns:office=\"urn:oasis:names:tc:opendocument:xmlns:office:1.0\"" +
                " xmlns:style=\"urn:oasis:names:tc:opendocument:xmlns:style:1.0\"" +
                " xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"" +
                " xmlns:table=\"urn:oasis:names:tc:opendocument:xmlns:table:1.0\"" +
                " xmlns:draw=\"urn:oasis:names:tc:opendocument:xmlns:drawing:1.0\"" +
                " xmlns:fo=\"urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0\"" +
                " xmlns:xlink=\"http://www.w3.org/1999/xlink\"" +
                " xmlns:dc=\"http://purl.org/dc/elements/1.1/\"" +
                " xmlns:meta=\"urn:oasis:names:tc:opendocument:xmlns:meta:1.0\"" +
                " xmlns:number=\"urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0\"" +
                " xmlns:svg=\"urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0\"" +
                " xmlns:chart=\"urn:oasis:names:tc:opendocument:xmlns:chart:1.0\"" +
                " xmlns:dr3d=\"urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0\"" +
                " xmlns:math=\"http://www.w3.org/1998/Math/MathML\"" +
                " xmlns:form=\"urn:oasis:names:tc:opendocument:xmlns:form:1.0\"" +
                " xmlns:script=\"urn:oasis:names:tc:opendocument:xmlns:script:1.0\"" +
                " xmlns:dom=\"http://www.w3.org/2001/xml-events\"" +
                " xmlns:xforms=\"http://www.w3.org/2002/xforms\"" +
                " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"" +
                " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";
        }

        private void OdfMakeDocStyles(string FileName)
        {
            using (FileStream file = new FileStream(FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                Out.Write("<office:document-styles ");
                Out.Write(OdfMakeXmlHeader());
                Out.WriteLine(">");
                Out.WriteLine("<office:automatic-styles>");
                // rework!
                Out.WriteLine("<style:page-layout style:name=\"pm1\">");
                Out.WriteLine("<style:page-layout-properties " +
                    "fo:page-width=\"" + ExportUtils.FloatToString(FPageWidth / odfPageDiv) + "cm\" " +
                    "fo:page-height=\"" + ExportUtils.FloatToString(FPageHeight / odfPageDiv) + "cm\" " +
                    "fo:margin-top=\"" + ExportUtils.FloatToString(FPageTop / odfMargDiv) + "cm\" " +
                    "fo:margin-bottom=\"" + ExportUtils.FloatToString(FPageBottom / odfMargDiv) + "cm\" " +
                    "fo:margin-left=\"" + ExportUtils.FloatToString(FPageLeft / odfMargDiv) + "cm\" " +
                    "fo:margin-right=\"" + ExportUtils.FloatToString(FPageRight / odfMargDiv) + "cm\"/>");
                Out.WriteLine("</style:page-layout>");
                Out.WriteLine("</office:automatic-styles>");
                Out.WriteLine("<office:master-styles>");
                Out.WriteLine("<style:master-page style:name=\"PageDef\" style:page-layout-name=\"pm1\">");
                Out.WriteLine("<style:header style:display=\"false\"/>");
                Out.WriteLine("<style:footer style:display=\"false\"/>");
                Out.WriteLine("</style:master-page>");
                Out.WriteLine("</office:master-styles>");
                Out.WriteLine("</office:document-styles>");
            }
        }

        private void OdfTableCellStyles(ExportIEMStyle Style, StreamWriter Out)
        {
            Out.Write("<style:table-cell-properties fo:background-color=\"" +
                ExportUtils.HTMLColor(Style.FillColor) + "\" " +
                "style:repeat-content=\"false\" fo:wrap-option=\"wrap\" ");
            if (Style.Angle > 0)
            {
                Out.Write("style:rotation-angle=\"" + (360 - Style.Angle).ToString() + "\" " +
                    "style:rotation-align=\"none\" ");
            }
            if (Style.VAlign == VertAlign.Center)
                Out.Write("style:vertical-align=\"middle\" ");
            if (Style.VAlign == VertAlign.Top)
                Out.Write("style:vertical-align=\"top\" ");
            if (Style.VAlign == VertAlign.Bottom)
                Out.Write("style:vertical-align=\"bottom\" ");
            if ((Style.Border.Lines & BorderLines.Left) > 0)
                Out.Write("fo:border-left=\"" +
                    ExportUtils.FloatToString(Style.Border.Width / odfDivider) + "cm " +
                    OdfGetFrameName(Style.Border.Style) + " " +
                    ExportUtils.HTMLColor(Style.Border.Color) + "\" ");
            if ((Style.Border.Lines & BorderLines.Right) > 0)
                Out.Write("fo:border-right=\"" +
                    ExportUtils.FloatToString(Style.Border.Width / odfDivider) + "cm " +
                    OdfGetFrameName(Style.Border.Style) + " " +
                    ExportUtils.HTMLColor(Style.Border.Color) + "\" ");
            if ((Style.Border.Lines & BorderLines.Top) > 0)
                Out.Write("fo:border-top=\"" +
                    ExportUtils.FloatToString(Style.Border.Width / odfDivider) + "cm " +
                    OdfGetFrameName(Style.Border.Style) + " " +
                    ExportUtils.HTMLColor(Style.Border.Color) + "\" ");
            if ((Style.Border.Lines & BorderLines.Bottom) > 0)
                Out.Write("fo:border-bottom=\"" +
                    ExportUtils.FloatToString(Style.Border.Width / odfDivider) + "cm " +
                    OdfGetFrameName(Style.Border.Style) + " " +
                    ExportUtils.HTMLColor(Style.Border.Color) + "\" ");
            Out.WriteLine("/>");
            Out.WriteLine("</style:style>");
        }

        private void OdfFontFaceDecals(StreamWriter Out)
        {
            List<string> FList = new List<string>();
            ExportIEMStyle Style;
            for (int i = 0; i < FMatrix.StylesCount; i++)
            {
                Style = FMatrix.StyleById(i);
                if ((Style.Font != null) && (FList.IndexOf(Style.Font.Name) == -1))
                    FList.Add(Style.Font.Name);
            }
            Out.WriteLine("<office:font-face-decls>");
            FList.Sort();
            for (int i = 0; i < FList.Count; i++)
                Out.WriteLine("<style:font-face style:name=\"" + FList[i] +
                    "\" svg:font-family=\"&apos;" + FList[i] + "&apos;\" " +
                    "style:font-pitch=\"variable\"/>");
            Out.WriteLine("</office:font-face-decls>");
        }

        private void OdfColumnStyles(StreamWriter Out)
        {
            List<string> FList = new List<string>();
            string s;
            for (int i = 1; i < FMatrix.Width; i++)
            {
                s = ExportUtils.FloatToString((FMatrix.XPosById(i) - FMatrix.XPosById(i - 1)) / odfDivider);
                if (FList.IndexOf(s) == -1)
                    FList.Add(s);
            }
            FList.Sort();
            for (int i = 0; i < FList.Count; i++)
            {
                Out.WriteLine("<style:style style:name=\"co" + FList[i] + "\" " +
                    "style:family=\"table-column\">");
                Out.WriteLine("<style:table-column-properties fo:break-before=\"auto\" " +
                    "style:column-width=\"" + FList[i] + "cm\"/>");
                Out.WriteLine("</style:style>");
            }
        }

        private void OdfRowStyles(StreamWriter Out)
        {
            List<string> FList = new List<string>();
            string s;
            for (int i = 0; i < FMatrix.Height - 1; i++)
            {
                s = ExportUtils.FloatToString((FMatrix.YPosById(i + 1) - FMatrix.YPosById(i)) / odfDivider);
                if (FList.IndexOf(s) == -1)
                    FList.Add(s);
            }
            FList.Sort();
            for (int i = 0; i < FList.Count; i++)
            {
                Out.WriteLine("<style:style style:name=\"ro" + FList[i] + "\" " +
                    "style:family=\"table-row\">");
                Out.WriteLine("<style:table-row-properties fo:break-before=\"auto\" " +
                    "style:row-height=\"" + FList[i] + "cm\"/>");
                Out.WriteLine("</style:style>");
            }
            Out.WriteLine("<style:style style:name=\"ro_breaked\" " +
                "style:family=\"table-row\">");
            Out.WriteLine("<style:table-row-properties fo:break-before=\"page\" " +
                "style:row-height=\"0.001cm\"/>");
            Out.WriteLine("</style:style>");
            Out.WriteLine("<style:style style:name=\"ta1\" style:family=\"table\" style:master-page-name=\"PageDef\">");
            Out.WriteLine("<style:table-properties table:display=\"true\" style:writing-mode=\"lr-tb\"/>");
            Out.WriteLine("</style:style>");
            Out.WriteLine("<style:style style:name=\"ceb\" style:family=\"table-cell\" style:display=\"false\"/>");
        }

        private void ExportODF(Stream stream)
        {
            string s;
            int fx, fy, dx, dy, Page;

            string FTempFolder = Path.GetTempPath() + Path.GetRandomFileName(); 
            Directory.CreateDirectory(FTempFolder);
            Directory.CreateDirectory(FTempFolder + "\\Pictures");
            Directory.CreateDirectory(FTempFolder + "\\Thumbnails");
            Directory.CreateDirectory(FTempFolder + "\\META-INF");

            int PicCount = 0;
            Page = 0;
            OdfMakeDocStyles(FTempFolder + "\\styles.xml");

            #region Content.xml

            using (FileStream file = new FileStream(FTempFolder + "\\content.xml", FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                Out.Write("<office:document-content ");
                Out.Write(OdfMakeXmlHeader());
                Out.WriteLine(">");
                Out.WriteLine("<office:scripts/>");
                ExportIEMStyle Style;
                OdfFontFaceDecals(Out);
                Out.WriteLine("<office:automatic-styles>");
                OdfColumnStyles(Out);                
                OdfRowStyles(Out);

                for (int i = 0; i < FMatrix.StylesCount; i++)
                {
                    Style = FMatrix.StyleById(i);
                    Out.WriteLine("<style:style style:name=\"ce" + i.ToString() + "\" " +
                        "style:family=\"table-cell\" style:parent-style-name=\"Default\">");
                    if (FExportType == 0)
                    {
                        Out.Write("<style:text-properties style:font-name=\"" + Style.Font.Name + "\" " +
                            "fo:font-size=\"" + ExportUtils.FloatToString(Style.Font.Size) + "pt\" ");
                        if ((Style.Font.Style & FontStyle.Underline) > 0)
                        {
                            Out.Write("style:text-underline-style=\"solid\" " +
                                "style:text-underline-width=\"auto\" " +
                                "style:text-underline-color=\"font-color\" ");
                        }
                        if ((Style.Font.Style & FontStyle.Italic) > 0)
                            Out.Write("fo:font-style=\"italic\" ");
                        if ((Style.Font.Style & FontStyle.Bold) > 0)
                            Out.Write("fo:font-weight=\"bold\" ");
                        Out.Write("fo:color=\"" + ExportUtils.HTMLColor(Style.TextColor) + "\"");
                        Out.WriteLine("/>");

                        Out.Write("<style:paragraph-properties ");
                        if (Style.HAlign == HorzAlign.Left)
                            Out.Write("fo:text-align=\"start\" ");
                        if (Style.HAlign == HorzAlign.Center)
                            Out.Write("fo:text-align=\"center\" ");
                        if (Style.HAlign == HorzAlign.Right)
                            Out.Write("fo:text-align=\"end\" ");
                        if (Style.Padding.Left > 0)
                            Out.Write("fo:margin-left=\"" +
                                ExportUtils.FloatToString(Style.Padding.Left / odfDivider) + "cm\" ");
                        if (Style.Padding.Right > 0)
                            Out.Write("fo:margin-right=\"" +
                                ExportUtils.FloatToString(Style.Padding.Right / odfDivider) + "cm\" ");
                        if (Style.Padding.Top > 0)
                            Out.Write("fo:margin-top=\"" +
                                ExportUtils.FloatToString(Style.Padding.Top / odfDivider) + "cm\" ");
                        if (Style.Padding.Bottom > 0)
                            Out.Write("fo:margin-bottom=\"" +
                                ExportUtils.FloatToString(Style.Padding.Bottom / odfDivider) + "cm\" ");
                        Out.WriteLine("/>");
                    }
                    OdfTableCellStyles(Style, Out);
                }

                if (FExportType == 1)
                {
                    Out.WriteLine("<style:style style:name=\"pb\" " +
                        "style:family=\"paragraph\" style:display=\"false\"/>");                 
                    for (int i = 0; i < FMatrix.StylesCount; i++)
                    {
                        Style = FMatrix.StyleById(i);
                        Out.WriteLine("<style:style style:name=\"p" + i.ToString() + "\" " + 
                            "style:family=\"paragraph\" style:parent-style-name=\"Default\">");

                        Out.Write("<style:text-properties style:font-name=\"" + 
                            Style.Font.Name + "\" fo:font-size=\"" +
                            ExportUtils.FloatToString(Style.Font.Size) + "pt\" ");
                        if ((Style.Font.Style & FontStyle.Underline) > 0)
                            Out.Write(" style:text-underline-style=\"solid\" " +
                                "style:text-underline-width=\"auto\" " + 
                                "style:text-underline-color=\"font-color\" ");
                        if ((Style.Font.Style & FontStyle.Italic) > 0)
                            Out.Write(" style:font-style=\"italic\" ");
                        if ((Style.Font.Style & FontStyle.Bold) > 0)
                            Out.Write(" style:font-weight=\"bold\" ");
                        Out.WriteLine(" fo:color=\"" + 
                            ExportUtils.HTMLColor(Style.TextColor) + "\"/>");

                        Out.Write("<style:paragraph-properties ");
                        if (Style.HAlign == HorzAlign.Left)
                            Out.Write("fo:text-align=\"start\" ");
                        if (Style.HAlign == HorzAlign.Center)
                            Out.Write("fo:text-align=\"center\" ");                        
                        if (Style.HAlign == HorzAlign.Right)
                            Out.Write("fo:text-align=\"end\" ");
                        if (Style.Padding.Left > 0)
                            Out.Write("fo:margin-left=\"" +
                                ExportUtils.FloatToString(Style.Padding.Left / odfDivider) + "cm\" ");
                        if (Style.Padding.Right > 0)
                            Out.Write("fo:margin-right=\"" +
                                ExportUtils.FloatToString(Style.Padding.Right / odfDivider) + "cm\" ");
                        if (Style.Padding.Top > 0)
                            Out.Write("fo:margin-top=\"" +
                                ExportUtils.FloatToString(Style.Padding.Top / odfDivider) + "cm\" ");
                        if (Style.Padding.Bottom > 0)
                            Out.Write("fo:margin-bottom=\"" +
                                ExportUtils.FloatToString(Style.Padding.Bottom / odfDivider) + "cm\" ");
                        Out.WriteLine("/>");
                        Out.WriteLine("</style:style>");
                    }
                }

                Out.WriteLine("<style:style style:name=\"gr1\" style:family=\"graphic\">");
                Out.WriteLine("<style:graphic-properties draw:stroke=\"none\" " +
                    "draw:fill=\"none\" draw:textarea-horizontal-align=\"left\" " +
                    "draw:textarea-vertical-align=\"top\" draw:color-mode=\"standard\" " +
                    "draw:luminance=\"0%\" draw:contrast=\"0%\" draw:gamma=\"100%\" " +
                    "draw:red=\"0%\" draw:green=\"0%\" draw:blue=\"0%\" " +
                    "fo:clip=\"rect(0cm 0cm 0cm 0cm)\" draw:image-opacity=\"100%\" " +
                    "style:mirror=\"none\"/>");
                Out.WriteLine("</style:style>");

                Out.WriteLine("</office:automatic-styles>");

                // body
                Out.WriteLine("<office:body>");
                Out.WriteLine("<office:spreadsheet>");
                Out.WriteLine("<table:table table:name=\"Table\" table:style-name=\"ta1\" table:print=\"false\">");

                for (int x = 1; x < FMatrix.Width; x++)
                    Out.WriteLine("<table:table-column table:style-name=\"co" + 
                        ExportUtils.FloatToString((FMatrix.XPosById(x) - 
                        FMatrix.XPosById(x - 1)) / odfDivider) + "\"/>");

                for (int y = 0; y < FMatrix.Height - 1; y++)
                {
                    Out.WriteLine("<table:table-row table:style-name=\"ro" + 
                        ExportUtils.FloatToString((FMatrix.YPosById(y + 1) -
                        FMatrix.YPosById(y)) / odfDivider) + "\">");
                    if (FMatrix.YPosById(y) >= FMatrix.PageBreak(Page))
                    {
                        Page++;
                        if (FPageBreaks)
                            Out.WriteLine("<table:table-row table:style-name=\"ro_breaked\"/>");
                    }
                    for (int x = 0; x < FMatrix.Width; x++)
                    {
                        int i = FMatrix.Cell(x, y);
                        if (i != -1)
                        {
                            ExportIEMObject Obj = FMatrix.ObjectById(i);
                            if (Obj.Counter == 0)
                            {
                                Obj.Counter = 1;
                                FMatrix.ObjectPos(i, out fx, out fy, out dx, out dy);

                                Out.Write("<table:table-cell table:style-name=\"ce" + 
                                    Obj.StyleIndex.ToString() + "\" ");
                                if (dx > 1)
                                    Out.Write("table:number-columns-spanned=\"" + dx.ToString() + "\" ");
                                if (dy > 1)
                                    Out.Write("table:number-rows-spanned=\"" + dy.ToString() + "\" ");
                                Out.WriteLine(">");
                                if (Obj.IsText)
                                {
                                    s = ExportUtils.XmlString(Obj.Text, Obj.HtmlTags);
                                    Out.Write("<text:p");
                                    if (FExportType == 1)
                                        Out.Write(" text:style-name=\"p" + Obj.StyleIndex.ToString() + "\"");
                                    Out.WriteLine(">" + s + "</text:p>");                                    
                                }
                                else
                                {
                                    if (Obj.Width > 0)
                                    {
                                        PicCount++;
                                        if (Config.FullTrust)
                                            using(FileStream emfFile = new FileStream(FTempFolder + "\\Pictures\\pic" + PicCount.ToString() + ".emf", FileMode.Create))
                                                emfFile.Write(Obj.PictureStream.ToArray(), 0, (int)Obj.PictureStream.Length);
                                        else
                                            using (FileStream pngFile = new FileStream(FTempFolder + "\\Pictures\\pic" + PicCount.ToString() + ".png", FileMode.Create))
                                                pngFile.Write(Obj.PictureStream.ToArray(), 0, (int)Obj.PictureStream.Length);
                                        if (FExportType == 1)
                                            Out.WriteLine("<text:p>");
                                        // need for fix of vertical position
                                        Out.WriteLine("<draw:frame draw:z-index=\"" + (PicCount - 1).ToString() + "\" " +
                                            "draw:name=\"Pictures" + PicCount.ToString() + "\" " +
                                            "draw:style-name=\"gr1\" " +
                                            "draw:text-style-name=\"P1\" " +
                                            "svg:width=\"" + ExportUtils.FloatToString(Obj.Width / odfDivider) + "cm\" " +
                                            "svg:height=\"" + ExportUtils.FloatToString(Obj.Height / odfDivider) + "cm\" " +
                                            "svg:x=\"0cm\" svg:y=\"0cm\">");
                                        s = Config.FullTrust ? ".emf" : ".png";
                                        Out.WriteLine("<draw:image " +
                                            "xlink:href=\"Pictures/pic" + PicCount.ToString() + s + "\" " +
                                            "text:anchor-type=\"frame\" xlink:type=\"simple\" xlink:show=\"embed\" xlink:actuate=\"onLoad\"/>");
                                        Out.WriteLine("</draw:frame>");
                                        if (FExportType == 1)
                                            Out.WriteLine("</text:p>");
                                    }
                                }
                                Out.WriteLine("</table:table-cell>");
                               }
                            else
                            {
                                Out.Write("<table:covered-table-cell table:style-name=\"ceb\"");
                                if (FExportType == 1)
                                    Out.WriteLine("><text:p text:style-name=\"pb\"/></table:covered-table-cell>");
                                else
                                    Out.WriteLine("/>");
                            }
                        }
                        else
                        {
                            Out.Write("<table:table-cell");
                            if (FExportType == 1)
                                Out.WriteLine("><text:p text:style-name=\"pb\"/></table:table-cell>");
                            else
                                Out.WriteLine("/>");
                        }
                    }
                    Out.WriteLine("</table:table-row>");
                }
                Out.WriteLine("</table:table>");
                Out.WriteLine("</office:spreadsheet>");
                Out.WriteLine("</office:body>");
                Out.WriteLine("</office:document-content>");
            }
            #endregion

            string ExportMime = FExportType == 0 ? "spreadsheet" : "text";
            OdfCreateManifest(FTempFolder + "\\META-INF\\manifest.xml", PicCount, ExportMime);
            OdfCreateMime(FTempFolder + "\\mimetype", ExportMime);
            OdfCreateMeta(FTempFolder + "\\meta.xml", Creator);

            ZipArchive zip = new ZipArchive();
            zip.AddDir(FTempFolder);
            zip.SaveToStream(Stream);
            Directory.Delete(FTempFolder, true);
        }

        private string GetCellPos(int x, int y)
        {
            return (char)(x + (byte)'A') + (y + 1).ToString();
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc/>
        public override bool ShowDialog()
        {
            using (ODFExportForm form = new ODFExportForm())
            {
                form.Init(this);
                return form.ShowDialog() == DialogResult.OK;
            }
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            FMatrix = new ExportMatrix();
            if (FWysiwyg)
                FMatrix.Inaccuracy = 0.5f;
            else
                FMatrix.Inaccuracy = 10;
            FMatrix.RotatedAsImage = true;// false;
            FMatrix.PlainRich = true;
            FMatrix.AreaFill = true;
            //FMatrix.CropAreaFill = true;
            FMatrix.Report = Report;
            FMatrix.MaxCellHeight = 400;            
            FMatrix.Images = true;
            FMatrix.ShowProgress = ShowProgress;
            FFirstPage = true;
        }

        /// <inheritdoc/>
        protected override void ExportPage(int pageNo)
        {
            using (ReportPage page = GetPage(pageNo))
            {
                FMatrix.AddPage(page);
                
                // rework!
                if (FFirstPage)
                {
                    FPageBottom = page.BottomMargin;
                    FPageLeft = page.LeftMargin;
                    FPageRight = page.RightMargin;
                    FPageTop = page.TopMargin;
                    FPageWidth = page.PaperWidth;
                    FPageHeight = page.PaperHeight;
                    FPageLandscape = page.Landscape;
                    FFirstPage = false;
                }
            }
        }

        /// <inheritdoc/>
        protected override void Finish()
        {
            FMatrix.Prepare();
            ExportODF(Stream);
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ODFExport"/> class.
        /// </summary>
        public ODFExport()
        {
            FExportType = 0;
            FPageBreaks = true;
            FWysiwyg = true;
            FCreator = "FastReport";
        }
    }
}
