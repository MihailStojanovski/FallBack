using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using FastReport.Utils;
using FastReport.Forms;
using FastReport.Export;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Collections;

namespace FastReport.Export.Html
{
    /// <summary>
    /// Specifies the image format in HTML export.
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>
        /// Specifies the .bmp format.
        /// </summary>
        Bmp,

        /// <summary>
        /// Specifies the .png format.
        /// </summary>
        Png,

        /// <summary>
        /// Specifies the .jpg format.
        /// </summary>
        Jpeg,

        /// <summary>
        /// Specifies the .gif format.
        /// </summary>
        Gif
    }

    /// <summary>
    /// Specifies the units of HTML sizes.
    /// </summary>
    public enum HtmlSizeUnits
    {
        /// <summary>
        /// Specifies the pixel units.
        /// </summary>
        Pixel,
        /// <summary>
        /// Specifies the percent units.
        /// </summary>
        Percent
    }

    internal class HTMLPageData
    {
        private string FCSSText;
        private string FPageText;
        private List<Stream> FPictures;
        private List<string> FGuids;
        private ManualResetEvent FPageEvent;
        private int FPageNumber;
        private float FWidth;
        private float FHeight;

        public float Width
        {
            get { return FWidth; }
            set { FWidth = value; }
        }

        public float Height
        {
            get { return FHeight; }
            set { FHeight = value; }
        }

        public string CSSText
        {
            get { return FCSSText; }
            set { FCSSText = value; }
        }

        public string PageText
        {
            get { return FPageText; }
            set { FPageText = value; }
        }

        public List<Stream> Pictures
        {
            get { return FPictures; }
        }

        public List<string> Guids
        {
            get { return FGuids; }
        }

        public ManualResetEvent PageEvent
        {
            get { return FPageEvent; }
        }

        public int PageNumber
        {
            get { return FPageNumber; }
            set { FPageNumber = value; }
        }

        public HTMLPageData()
        {
            FPictures = new List<Stream>();
            FGuids = new List<string>();
            FPageEvent = new ManualResetEvent(false);
        }
    }

    /// <summary>
    /// Represents the HTML export format enum
    /// </summary>
    public enum HTMLExportFormat
    {
        /// <summary>
        /// Represents the message-HTML type
        /// </summary>
        MessageHTML,
        /// <summary>
        /// Represents the HTML type
        /// </summary>
        HTML
    }

    /// <summary>
    /// Represents the HTML export filter.
    /// </summary>
    public class HTMLExport : ExportBase
    {

        #region Private fields

        private struct HTMLThreadData
        {
            public int ReportPage;
            public int PageNumber;
            public int CurrentPage;
            public Stream PagesStream; 
        }

        private struct PicsArchiveItem
        {
            public string FileName;
            public MemoryStream Stream;
        }

        private bool FWysiwyg;
        private MyRes Res;
        private HtmlTemplates FTemplates;
        private string FTargetPath;
        private string FTargetIndexPath;
        private string FTargetFileName;
        private string FFileName;
        private string FNavFileName;
        private string FOutlineFileName;
        private int FPagesCount;
        private string FDocumentTitle;
        private ImageFormat FImageFormat;
        private ManualResetEvent FFirstPageEvent;
        private bool FSubFolder;
        private bool FNavigator;
        private bool FSinglePage;
        private bool FPictures;
        private bool FWebMode;
        private List<HTMLPageData> FPages;
        private int FCount;
        private string FWebImagePrefix;
        private string FStylePrefix;
        private bool FThreaded;
        private string FPrevWatermarkName;
        private long FPrevWatermarkSize;
        private HtmlSizeUnits FWidthUnits;
        private HtmlSizeUnits FHeightUnits;
        private string FSinglePageFileName;
        private string FSubFolderPath;
        private HTMLExportFormat FFormat;
        private MemoryStream FMimeStream;
        private String FBoundary;
        private List<PicsArchiveItem> FPicsArchive;

        #endregion

        #region Public properties

        internal string StylePrefix
        {
            get { return FStylePrefix; }
            set { FStylePrefix = value; }
        }

        internal string WebImagePrefix
        {
            get { return FWebImagePrefix; }
            set { FWebImagePrefix = value; }
        }

        internal int Count
        {
            get { return FCount; }
        }

        internal List<HTMLPageData> PreparedPages
        {
            get { return FPages; }            
        }

        /// <summary>
        /// Specifies the output format
        /// </summary>
        public HTMLExportFormat Format
        {
            get { return FFormat; }
            set { FFormat = value; }                
        }

        /// <summary>
        /// Specifies the width units in HTML export
        /// </summary>
        public HtmlSizeUnits WidthUnits
        {
            get { return FWidthUnits; }
            set { FWidthUnits = value; }
        }

        /// <summary>
        /// Specifies the height units in HTML export
        /// </summary>
        public HtmlSizeUnits HeightUnits
        {
            get { return FHeightUnits; }
            set { FHeightUnits = value; }
        }

        /// <summary>
        /// Enable or disable the pictures in HTML export
        /// </summary>
        public bool Pictures
        {
            get { return FPictures; }
            set { FPictures = value; }
        }

        /// <summary>
        /// Enable or disable the WEB mode in HTML export
        /// </summary>
        internal bool WebMode
        {
            get { return FWebMode; }
            set { FWebMode = value; }
        }

        /// <summary>
        /// Enable or disable the single HTML page creation 
        /// </summary>
        public bool SinglePage
        {
            get { return FSinglePage; }
            set { FSinglePage = value; }
        }

        /// <summary>
        /// Enable or disable the page navigator in html export
        /// </summary>
        public bool Navigator
        {
            get { return FNavigator; }
            set { FNavigator = value; }
        }

        /// <summary>
        /// Enable or disable the sub-folder for files of export
        /// </summary>
        public bool SubFolder
        {
            get { return FSubFolder;  }
            set { FSubFolder = value; }
        }

        /// <summary>
        ///  Gets or sets the Wysiwyg quality of export
        /// </summary>
        public bool Wysiwyg
        {
            get { return FWysiwyg; }
            set { FWysiwyg = value; }
        }

        /// <summary>
        /// Gets or sets the image format.
        /// </summary>
        public ImageFormat ImageFormat
        {
            get { return FImageFormat; }
            set { FImageFormat = value; }
        }

        #endregion


        #region Private methods

        private string SizeValue(double value, double maxvalue, HtmlSizeUnits units)
        {
            StringBuilder sb = new StringBuilder(6);
            if (units == HtmlSizeUnits.Pixel)
                sb.Append(((int)Math.Round(value)).ToString()).Append("px");
            else if (units == HtmlSizeUnits.Percent)
                sb.Append(((int)Math.Round((value * 100 / maxvalue))).ToString()).Append("%");
            else
                sb.Append(value.ToString());
            return sb.ToString();
        }

        private string HTMLFontStyle(Font font)
        {
            StringBuilder FFontDesc = new StringBuilder(128);
            FFontDesc.Append((((font.Style & FontStyle.Bold) > 0) ? " font-weight: bold;" : String.Empty) +
                (((font.Style & FontStyle.Italic) > 0) ? " font-style: italic;" : " font-style: normal;"));
            if ((font.Style & FontStyle.Underline) > 0 && (font.Style & FontStyle.Strikeout) > 0)
                FFontDesc.Append(" text-decoration: underline | line-through;");
            else if ((font.Style & FontStyle.Underline) > 0)
                FFontDesc.Append(" text-decoration: underline;");
            else if ((font.Style & FontStyle.Strikeout) > 0)
                FFontDesc.Append(" text-decoration: line-through;");
            FFontDesc.Append(" font-family: ").Append(font.Name).Append(";");
            FFontDesc.Append(" font-size: ").Append(Convert.ToString(Math.Round(font.Size * 96 / 72))).Append("px;");          
            return FFontDesc.ToString();
        }

        private string HTMLPadding(Padding padding)
        {
            StringBuilder PaddingDesc = new StringBuilder(26);
            if (padding.Left != 0)
                PaddingDesc.Append(" padding-left: ").Append(padding.Left).Append("px;");
            if (padding.Right != 0)
                PaddingDesc.Append(" padding-right: ").Append(padding.Right).Append("px;");
            if (padding.Top != 0)
                PaddingDesc.Append(" padding-top: ").Append(padding.Top).Append("px;");
            if (padding.Bottom != 0)
                PaddingDesc.Append(" padding-bottom: ").Append(padding.Bottom).Append("px;");
            return PaddingDesc.ToString();
        }

        private string HTMLBorderStyle(BorderLine line)
        {            
            switch (line.Style)
            {
                case LineStyle.Dash:
                case LineStyle.DashDot:
                case LineStyle.DashDotDot:
                    return "dashed";
                case LineStyle.Dot:
                    return "dotted";
                case LineStyle.Double:
                    return "double";
                default:
                    return "solid";
            }
        }

        private string HTMLBorderWidth(BorderLine line)
        {
            if (line.Style == LineStyle.Double)
                return (line.Width * 3).ToString();
            else
                return line.Width.ToString();
        }

        private string HTMLBorder(Border border)
        {
            if (border.Lines > 0)
            {
                StringBuilder BorderDesc = new StringBuilder(192);
                // bottom
                if ((border.Lines & BorderLines.Bottom) > 0)
                    BorderDesc.Append(" border-bottom-width: ").
                        Append(HTMLBorderWidth(border.BottomLine)).
                        Append("px; border-bottom-color: ").
                        Append(ExportUtils.HTMLColor(border.BottomLine.Color)).Append(";").
                        Append(" border-bottom-style: ").Append(HTMLBorderStyle(border.BottomLine)).AppendLine(";");
                else
                    BorderDesc.Append(" border-bottom-width: 0px;");
                // top
                if ((border.Lines & BorderLines.Top) > 0)
                    BorderDesc.Append(" border-top-width: ").
                        Append(HTMLBorderWidth(border.TopLine)).
                        Append("px; border-top-color: ").
                        Append(ExportUtils.HTMLColor(border.TopLine.Color)).Append(";").
                        Append(" border-top-style: ").Append(HTMLBorderStyle(border.TopLine)).AppendLine(";");
                else
                    BorderDesc.Append(" border-top-width: 0px;");
                // left
                if ((border.Lines & BorderLines.Left) > 0)
                    BorderDesc.Append(" border-left-width: ").
                        Append(HTMLBorderWidth(border.LeftLine)).
                        Append("px; border-left-color: ").
                        Append(ExportUtils.HTMLColor(border.LeftLine.Color)).Append(";").
                        Append(" border-left-style: ").Append(HTMLBorderStyle(border.LeftLine)).AppendLine(";");
                else
                    BorderDesc.Append(" border-left-width: 0px;");
                // right
                if ((border.Lines & BorderLines.Right) > 0)
                    BorderDesc.Append(" border-right-width: ").
                        Append(HTMLBorderWidth(border.RightLine)).
                        Append("px; border-right-color: ").
                        Append(ExportUtils.HTMLColor(border.RightLine.Color)).Append(";").
                        Append(" border-right-style: ").Append(HTMLBorderStyle(border.RightLine)).AppendLine(";");
                else
                    BorderDesc.Append(" border-right-width: 0px;");
                return BorderDesc.ToString();
            }
            else
                return String.Empty;
        }

        private string HTMLAlign(HorzAlign horzAlign, VertAlign vertAlign)
        {
            StringBuilder sb = new StringBuilder(64);
            sb.Append(" text-align: ");
            if (horzAlign == HorzAlign.Left)
                sb.Append("Left");
            else if (horzAlign == HorzAlign.Right)
                sb.Append("Right");
            else if (horzAlign == HorzAlign.Center)
                sb.Append("Center");
            else if (horzAlign == HorzAlign.Justify)
                sb.Append("Justify");
            sb.Append("; vertical-align: ");
            if (vertAlign == VertAlign.Top)
                sb.Append("Top");
            else if (vertAlign == VertAlign.Bottom)
                sb.Append("Bottom");
            else if (vertAlign == VertAlign.Center)
                sb.Append("Middle");
            sb.Append(";");
            return  sb.ToString();
        }

        private string HTMLGetImage(ExportIEMObject obj, int PageNumber, int CurrentPage, int ImageNumber)
        {
            if (FPictures)
            {
                System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Bmp;
                if (FImageFormat == ImageFormat.Png)
                    format = System.Drawing.Imaging.ImageFormat.Png;
                else if (FImageFormat == ImageFormat.Jpeg)
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                else if (FImageFormat == ImageFormat.Gif)
                    format = System.Drawing.Imaging.ImageFormat.Gif;
                string ImageFileName = Path.GetFileName(FTargetFileName)+ "." + obj.Hash + "." + format.ToString().ToLower();

                if (!FWebMode)
                {
                    if (obj.Base)
                    {
                        if (obj.Metafile != null)
                            using (FileStream ImageFileStream =
                                new FileStream(FTargetPath + ImageFileName, FileMode.Create))
                                obj.Metafile.Save(ImageFileStream, format);
                        else if (obj.PictureStream != null)
                        {
                            if (FFormat == HTMLExportFormat.HTML)
                            {
                                using (FileStream ImageFileStream =
                                new FileStream(FTargetPath + ImageFileName, FileMode.Create))
                                    obj.PictureStream.WriteTo(ImageFileStream);
                            }
                            else
                            {
                                PicsArchiveItem item;
                                item.FileName = ImageFileName;
                                item.Stream = obj.PictureStream;
                                FPicsArchive.Add(item);
                            }
                        }
                        GeneratedFiles.Add(FTargetPath + ImageFileName);
                    }
                    if (FSubFolder && FSinglePage && !FNavigator)
                        return ExportUtils.HtmlURL(FSubFolderPath + ImageFileName);
                    else
                        return ExportUtils.HtmlURL(ImageFileName);
                }
                else
                {
                    if (obj.Base)
                    {
                        FPages[CurrentPage].Pictures.Add(obj.PictureStream);
                        FPages[CurrentPage].Guids.Add(obj.Hash);
                    }
                    return FWebImagePrefix + "=" + obj.Hash;
                }
            }
            else
                return String.Empty;
        }


        private string HTMLSaveImage(ExportIEMObject obj, int PageNumber, int CurrentPage, int ImageNumber)
        {
            if (FPictures)
            {
                return "<img src=\"" + HTMLGetImage(obj, PageNumber, CurrentPage, ImageNumber) + "\" alt=\"\"/>";
            }
            else
                return String.Empty;
        }

        private string HTMLRtl(bool rtl)
        {
            if (rtl)
                return " direction: rtl; ";
            else
                return " direction: ltr; ";
        }

        private void ExportHTMLPage(object data)
        {
            ExportIEMStyle EStyle;
            int x, y, i, drow, fx, fy, dx, dy;
            ExportIEMObject obj;

            HTMLThreadData d = (HTMLThreadData)data;
            int PageNumber = d.PageNumber;
            int CurrentPage = d.CurrentPage;
            int pageNo = d.ReportPage;
            int ImagesCount = 0;

            using (ReportPage page = GetPage(pageNo))
            {
                ExportMatrix FMatrix = new ExportMatrix();
                if (FWysiwyg)
                    FMatrix.Inaccuracy = 0.5f;
                else
                    FMatrix.Inaccuracy = 10;

                if (FWebMode)
                {
                    FSinglePage = false;
                    FNavigator = false;
                }

                FMatrix.Watermarks = true;
                FMatrix.HTMLMode = true;
                FMatrix.FillAsBitmap = true;
                FMatrix.Zoom = Zoom;
                FMatrix.RotatedAsImage = true;
                FMatrix.PlainRich = true;
                FMatrix.CropAreaFill = false;
                FMatrix.AreaFill = true;
                FMatrix.Report = Report;
                FMatrix.FramesOptimization = true;
                FMatrix.ShowProgress = false;
                FMatrix.Threaded = FThreaded;
                FMatrix.FullTrust = false;
                FMatrix.AddPage(page);
                FMatrix.Prepare();
                
                #region Styles
                StringBuilder Page = new StringBuilder(4096);

                Page.AppendLine("<style type=\"text/css\"><!-- ");
                Page.AppendLine(".page_break {page-break-before: always;}");
                for (x = 0; x < FMatrix.StylesCount; x++)
                {
                    EStyle = FMatrix.StyleById(x);
                    Page.Append(".").Append(FStylePrefix).Append("s").Append(x.ToString()).Append("p").Append(pageNo.ToString()).AppendLine(" {");
                    Page.AppendLine(HTMLFontStyle(EStyle.Font));
                    Page.Append(" color: ").Append(ExportUtils.HTMLColor(EStyle.TextColor)).AppendLine(";");
                    Page.Append(" background-color: ");
                    Page.Append(EStyle.FillColor == Color.Transparent ?
                        "transparent;" : ExportUtils.HTMLColor(EStyle.FillColor) + ";");
                    Page.AppendLine(HTMLAlign(EStyle.HAlign, EStyle.VAlign));
                    Page.Append(HTMLBorder(EStyle.Border));
                    Page.Append(HTMLPadding(EStyle.Padding));
                    Page.AppendLine(HTMLRtl(EStyle.RTL));
                    Page.AppendLine("}");
                }
                
                Page.AppendLine("--></style>");
                if (FWebMode)
                {
                    FPages[CurrentPage].CSSText = Page.ToString();
                    FPages[CurrentPage].PageNumber = PageNumber;
                    Page = new StringBuilder(4096);
                }
                #endregion
                if (!FWebMode)
                {
                    Page.AppendLine("</head>");
                    Page.AppendLine("<body bgcolor=\"#FFFFFF\" text=\"#000000\">");
                }
                #region page table
                Page.Append("<a name=\"PageN").Append(PageNumber.ToString()).AppendLine("\"></a>");
                Page.Append("<table width=\"").
                    Append(SizeValue(Math.Round(FMatrix.MaxWidth), Math.Round(FMatrix.MaxWidth), FWidthUnits)).
                    Append("\" align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" style=\"table-layout: fixed;");
                // add watermark
                if (FMatrix.Pages[0].WatermarkPictureStream != null)
                {
                    string wName;
                    if (FPrevWatermarkSize != FMatrix.Pages[0].WatermarkPictureStream.Length)
                    {
                        ExportIEMObject watermark = new ExportIEMObject();
                        watermark.Width = (FMatrix.Pages[0].Width - FMatrix.Pages[0].LeftMargin - FMatrix.Pages[0].RightMargin) * Units.Millimeters;
                        watermark.Height = (FMatrix.Pages[0].Height - FMatrix.Pages[0].TopMargin - FMatrix.Pages[0].BottomMargin) * Units.Millimeters;
                        watermark.PictureStream = FMatrix.Pages[0].WatermarkPictureStream;
                        FPrevWatermarkSize = FMatrix.Pages[0].WatermarkPictureStream.Length;
                        FPrevWatermarkName = HTMLGetImage(watermark, PageNumber, CurrentPage, ImagesCount++);
                    }
                    wName = FPrevWatermarkName;
                    Page.Append(" background: url(").Append(wName).Append(") no-repeat;");
                }
                Page.AppendLine("\" >");
                #region Column sizes
                Page.Append("<tr style=\"height: 1px\">");
                for (x = 0; x < FMatrix.Width - 1; x++)
                    Page.Append("<td width=\"").
                        Append(SizeValue(Math.Round(FMatrix.XPosById(x + 1) - FMatrix.XPosById(x)), FMatrix.MaxWidth, FWidthUnits)).
                        Append("\"/>");
                if (FMatrix.Width < 2)
                    Page.Append("<td/>");
                Page.AppendLine("</tr>");
                #endregion
                for (y = 0; y < FMatrix.Height - 1; y++)
                {
                    drow = (int)Math.Round((FMatrix.YPosById(y + 1) - FMatrix.YPosById(y)));
                    if (drow == 0)
                        drow = 1;
                    Page.Append("<tr style=\"height:").Append(SizeValue(drow, FMatrix.MaxHeight, FHeightUnits)).AppendLine("\">");
                    for (x = 0; x < FMatrix.Width - 1; x++)
                    {
                        i = FMatrix.Cell(x, y);
                        if (i != -1)
                        {
                            obj = FMatrix.ObjectById(i);
                            if (obj.Counter == 0)
                            {
                                FMatrix.ObjectPos(i, out fx, out fy, out dx, out dy);
                                obj.Counter = 1;
                                Page.Append("<td").
                                    Append((dx > 1 ? " colspan=\"" + dx.ToString() + "\"" : String.Empty)).
                                    Append((dy > 1 ? " rowspan=\"" + dy.ToString() + "\"" : String.Empty)).
                                    Append(" class=\"").Append(FStylePrefix).Append("s").Append(obj.StyleIndex.ToString()).
                                    Append("p").Append(pageNo.ToString()).Append("\"");
                                StringBuilder style = new StringBuilder(256);
                                if (obj.Text.Length == 0)
                                    style.Append("font-size:1px;");
                                if (obj.PictureStream != null && obj.IsText)
                                    style.Append("background-image: url(").
                                        Append(HTMLGetImage(obj, PageNumber, CurrentPage, ImagesCount++)).
                                        Append(");");
                                if (style.Length > 0)
                                    Page.Append(" style=\"").Append(style).Append("\"");
                                Page.Append(">");
                                // TEXT
                                if (obj.IsText)
                                    if (obj.Text.Length > 0)
                                        Page.Append(ExportUtils.HtmlString(obj.Text, obj.HtmlTags));
                                    else
                                        Page.Append("&nbsp;");
                                else
                                    Page.Append(HTMLSaveImage(obj, PageNumber, CurrentPage, ImagesCount++));
                                Page.AppendLine("</td>");
                            }
                        }
                        else
                            Page.AppendLine("</td>");                        
                    }
                    Page.AppendLine("</tr>");
                }
                Page.AppendLine("</table>");
                #endregion
                if (!FWebMode)
                {
                    Page.AppendLine("</body>");
                    if (d.PagesStream == null)
                    {
                        string FPageFileName = FTargetIndexPath + FTargetFileName + PageNumber.ToString() + ".html";
                        GeneratedFiles.Add(FPageFileName);
                        using (FileStream OutStream = new FileStream(FPageFileName, FileMode.Create))
                        using (StreamWriter Out = new StreamWriter(OutStream))
                            Out.Write(String.Format(FTemplates.PageTemplate, new object[] {                     
                        FDocumentTitle,
                        Page.ToString() }));
                    }
                    else
                    {
                        ExportUtils.Write(d.PagesStream, String.Format(FTemplates.PageTemplate, new object[] { FDocumentTitle, Page.ToString() }));
                    }
                }
                else
                {
                    FPages[CurrentPage].Width = FMatrix.MaxWidth / Zoom;
                    FPages[CurrentPage].Height = FMatrix.MaxHeight / Zoom;
                    FPages[CurrentPage].PageText = Page.ToString();
                    FPages[CurrentPage].PageEvent.Set();
                }                

                if (!FSinglePage && FThreaded)
                    if (PageNumber == 1)
                        FFirstPageEvent.Set();
            }
        }

        private void ExportHTMLOutline(Stream OutStream)
        {
            if (!FWebMode)
            {
                // under construction            
            }
            else
            {
                // under construction            
            }
        }

        private void ExportHTMLIndex(Stream Stream)
        {
            using (StreamWriter Out = new StreamWriter(Stream))
                Out.Write(String.Format(FTemplates.IndexTemplate,
                    new object[] { FDocumentTitle, ExportUtils.HtmlURL(FNavFileName), 
                        ExportUtils.HtmlURL(FTargetFileName + 
                        (FSinglePage ? ".main" : "1") + ".html") }));
        }

        private void ExportHTMLNavigator(FileStream OutStream)
        {
            using (StreamWriter Out = new StreamWriter(OutStream))
            {
                //  {0} - pages count {1} - name of report {2} multipage document {3} prefix of pages
                //  {4} first caption {5} previous caption {6} next caption {7} last caption
                //  {8} total caption
                Out.Write(String.Format(FTemplates.NavigatorTemplate, 
                    new object[] { FPagesCount.ToString(), 
                        FDocumentTitle, (FSinglePage ? "0" : "1"), 
                        ExportUtils.HtmlURL(FFileName), Res.Get("First"), Res.Get("Prev"), 
                        Res.Get("Next"), Res.Get("Last"), Res.Get("Total") }));
            }
        }

        #endregion


        #region Protected methods


        /// <inheritdoc/>
        public override bool ShowDialog()
        {
            if (!FWebMode)
                using (HTMLExportForm form = new HTMLExportForm())
                {
                    form.Init(this);
                    return form.ShowDialog() == DialogResult.OK;
                }
            else
                return true;
        }

        /// <inheritdoc/>
        protected override string GetFileFilter()
        {
            if (Format == HTMLExportFormat.HTML)
                return new MyRes("FileFilters").Get("HtmlFile");
            else
                return new MyRes("FileFilters").Get("MhtFile");
        }

        /// <inheritdoc/>
        protected override void Start()
        {           
            FCount = Report.PreparedPages.Count;
            FPagesCount = 0;
            FPrevWatermarkName = String.Empty;
            FPrevWatermarkSize = 0;
            if (!FWebMode)
            {
                if (FFormat == HTMLExportFormat.MessageHTML)
                {
                    FSubFolder = false;
                    FSinglePage = true;
                    FNavigator = false;
                    FMimeStream = new MemoryStream();
                    FBoundary = ExportUtils.GetID();
                }

                if (!FNavigator)
                    FSinglePage = true;
                if (FileName == "" && Stream != null)
                {
                    FSinglePage = true;  
                    FNavigator = false;
                    if (FFormat == HTMLExportFormat.HTML)
                        FPictures = false;                    
                }
                else
                {
                    FTargetFileName = Path.GetFileNameWithoutExtension(FileName);
                    FFileName = FTargetFileName;
                    FTargetIndexPath = Path.GetDirectoryName(FileName);
                }
                if (!String.IsNullOrEmpty(FTargetIndexPath))
                    FTargetIndexPath += "\\";
                if (FSubFolder)
                {
                    FSubFolderPath = FTargetFileName + ".files\\";
                    FTargetPath = FTargetIndexPath + FSubFolderPath;
                    FTargetFileName = FSubFolderPath + FTargetFileName;
                    if (!Directory.Exists(FTargetPath))
                        Directory.CreateDirectory(FTargetPath);
                }
                else
                    FTargetPath = FTargetIndexPath;
                FNavFileName = FTargetFileName + ".nav.html";
                FOutlineFileName = FTargetFileName + ".outline.html";                
                FDocumentTitle = Report.ReportInfo.Name.Length != 0 ?
                    Report.ReportInfo.Name : Path.GetFileNameWithoutExtension(FileName);
                if (FSinglePage)
                {
                    if (FNavigator)
                        FSinglePageFileName = FTargetIndexPath + FTargetFileName + ".main.html";
                    else
                        FSinglePageFileName = FileName;
                }
                if (FSinglePage && FNavigator)
                    File.Delete(FSinglePageFileName);
            }
            else
            {
                FPages.Clear();
                for (int i = 0; i < FCount; i++)
                    FPages.Add(new HTMLPageData());                    
            }
            
            if (!FSinglePage && FThreaded)
                FFirstPageEvent = new ManualResetEvent(false);
        }

        /// <inheritdoc/>
        protected override void ExportPage(int pageNo)
        {
            FPagesCount++;
            if (FSinglePage)
            {
                HTMLThreadData d = new HTMLThreadData();
                d.ReportPage = pageNo;
                d.PageNumber = FPagesCount;
                if (FNavigator)
                {
                    GeneratedFiles.Add(FSinglePageFileName);
                    using (d.PagesStream = new FileStream(FSinglePageFileName,
                        FileMode.Append))
                    {
                        ExportHTMLPage(d);
                    }
                }
                else
                {
                    if (FFormat == HTMLExportFormat.HTML)
                        d.PagesStream = Stream;
                    else
                        d.PagesStream = FMimeStream;
                    ExportHTMLPage(d);
                }
            }
            else if (!FWebMode)
                ProcessPage(FPagesCount - 1, pageNo);
        }

        /// <summary>
        /// Process Page with number p and real page ReportPage
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ReportPage"></param>
        public void ProcessPage(int p, int ReportPage)
        {
            HTMLThreadData d = new HTMLThreadData();
            d.ReportPage = ReportPage;
            d.PageNumber = FPagesCount;
            d.PagesStream = null;
            d.CurrentPage = p;
            if (!FWebMode && FThreaded)
                ThreadPool.QueueUserWorkItem(ExportHTMLPage, d);
            else
                ExportHTMLPage(d);
        }

        /// <inheritdoc/>
        protected override void Finish()
        {
            if (!FSinglePage && !FWebMode && FThreaded)
                while (!FFirstPageEvent.WaitOne(10, true))
                    Application.DoEvents();
                
            if (!FWebMode)
            {
                if (FNavigator)
                {
                    ExportHTMLIndex(Stream);
                    GeneratedFiles.Add(FTargetIndexPath + FNavFileName);
                    using (FileStream OutStream = new FileStream(FTargetIndexPath + FNavFileName, FileMode.Create))
                        ExportHTMLNavigator(OutStream);
                    GeneratedFiles.Add(FTargetIndexPath + FOutlineFileName);
                    using (FileStream OutStream = new FileStream(FTargetIndexPath + FOutlineFileName, FileMode.Create))
                        ExportHTMLOutline(OutStream);
                } 
                else if (FFormat == HTMLExportFormat.MessageHTML)
                {
                    WriteMHTHeader();
                    WriteMimePart(FMimeStream, "text/html", "utf-8", "index.html");

                    for(int i = 0; i < FPicsArchive.Count; i++)
                    {
                        string imagename = FPicsArchive[i].FileName;
                        WriteMimePart(FPicsArchive[i].Stream, "image/" + imagename.Substring(imagename.LastIndexOf('.') + 1), "utf-8", imagename);
                    }

                    string last = "--" + FBoundary + "--";
                    Stream.Write(Encoding.ASCII.GetBytes(last), 0, last.Length);            
                }
            }
        }

        private void WriteMimePart(Stream stream, string mimetype, string charset, string filename)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--").AppendLine(FBoundary);
            sb.Append("Content-Type: ").Append(mimetype).Append(";");
            if (charset != String.Empty)
                sb.Append(" charset=\"").Append(charset).AppendLine("\"");
            else
                sb.AppendLine();
            string body;
            byte[] buff = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(buff, 0, buff.Length);
            if (mimetype == "text/html")
            {
                sb.AppendLine("Content-Transfer-Encoding: quoted-printable");
                body = ExportUtils.QuotedPrintable(buff);
            }
            else
            {
                sb.AppendLine("Content-Transfer-Encoding: base64");
                body = System.Convert.ToBase64String(buff, Base64FormattingOptions.InsertLineBreaks);
            }
            sb.Append("Content-Location: ").AppendLine(ExportUtils.HtmlURL(filename));
            sb.AppendLine();
            sb.AppendLine(body);            
            sb.AppendLine();
            Stream.Write(Encoding.ASCII.GetBytes(sb.ToString()), 0, sb.Length);
        }

        private void WriteMHTHeader()
        {
            StringBuilder sb = new StringBuilder(256);
            string s = "=?utf-8?B?" + System.Convert.ToBase64String(Encoding.UTF8.GetBytes(FileName)) + "?=";
            sb.Append("From: ").AppendLine(s);
            sb.Append("Subject: ").AppendLine(s);
            sb.Append("Date: ").AppendLine(ExportUtils.GetRFCDate(DateTime.Now));
            sb.AppendLine("MIME-Version: 1.0");
            sb.Append("Content-Type: multipart/related; type=\"text/html\"; boundary=\"").Append(FBoundary).AppendLine("\"");
            sb.AppendLine();
            sb.AppendLine("This is a multi-part message in MIME format.");
            sb.AppendLine();
            ExportUtils.Write(Stream, sb.ToString());
        }

        #endregion

        internal void Init_WebMode()       
        {
            FPages = new List<HTMLPageData>();
            FWebMode = true;
            OpenAfterExport = false;            
        }

        internal void Finish_WebMode()
        {
            FPages.Clear();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HTMLExport"/> class.
        /// </summary>
        public HTMLExport()
        {
            FWysiwyg = true;
            FPictures = true;
            FWebMode = false;
            FSubFolder = true;
            FNavigator = true;
            FSinglePage = false;
            FThreaded = false;
            FWidthUnits = HtmlSizeUnits.Pixel;
            FHeightUnits = HtmlSizeUnits.Pixel;
            FImageFormat = ImageFormat.Png;
            FTemplates = new HtmlTemplates();
            FFormat = HTMLExportFormat.HTML;
            FPicsArchive = new List<PicsArchiveItem>();
            Res = new MyRes("Export,Html");
        }
    }

    class HtmlTemplates
    {
        #region private fields
        private string FPageTemplate;
        private string FNavigatorTemplate;
        private string FOutlineTemplate;
        private string FIndexTemplate;        
        private StringBuilder FCapacitor;
        #endregion

        #region private methods
        private void NewCapacitor()
        {
            FCapacitor = new StringBuilder(512);
        }
        private void Part(string str)
        {
            FCapacitor.AppendLine(str);
        }
        private string Capacitor()
        {
            return FCapacitor.ToString();
        }
        #endregion

        #region public properties
        public string PageTemplate
        {
            get { return FPageTemplate; }
            set { FPageTemplate = value; }
        }
        public string NavigatorTemplate
        {
            get { return FNavigatorTemplate; }
            set { FNavigatorTemplate = value; }
        }
        public string OutlineTemplate
        {
            get { return FOutlineTemplate; }
            set { FOutlineTemplate = value; }
        }
        public string IndexTemplate
        {
            get { return FIndexTemplate; }
            set { FOutlineTemplate = value; }
        }
        #endregion

        public HtmlTemplates()
        {
            #region fill page template
            // {0} - title  {1} - main body
            NewCapacitor();
            Part("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">");
            Part("<html><head>");
            Part("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            Part("<meta name=Generator content=\"FastReport http://www.fast-report.com\">");
            Part("<title>{0}</title>");
            Part("{1}");
            Part("</html>");            
            FPageTemplate = Capacitor();
            #endregion

            #region fill navigator template
            //  {0} - pages count {1} - name of report {2} multipage document {3} prefix of pages
            //  {4} first caption {5} previous caption {6} next caption {7} last caption
            //  {8} total caption
            NewCapacitor();
            Part("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">");
            Part("<html><head>");
            Part("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            Part("<meta name=Generator content=\"FastReport http://www.fast-report.com\">");
            Part("<title></title><style type=\"text/css\"><!--");
            Part("body,input,select {{ font-family:\"Lucida Grande\",Calibri,Arial,sans-serif; font-size: 8px; font-weight: bold; font-style: normal; text-align: center; vertical-align: middle; }}");
            Part("input {{text-align: center}}");
            Part(".nav {{ font-size : 9pt; color : #283e66; font-weight : bold; text-decoration : none;}}");
            Part("--></style><script language=\"javascript\" type=\"text/javascript\"><!--");
            Part("  var frPgCnt = {0}; var frRepName = \"{1}\"; var frMultipage = {2}; var frPrefix=\"{3}\";");
            Part("  function DoPage(PgN) {{");
            Part("    if ((PgN > 0) && (PgN <= frPgCnt) && (PgN != parent.frCurPage)) {{");
            Part("      if (frMultipage > 0)  parent.mainFrame.location = frPrefix + PgN + \".html\";");
            Part("      else parent.mainFrame.location = frPrefix + \".main.html#PageN\" + PgN;");
            Part("      UpdateNav(PgN); }} else document.PgForm.PgEdit.value = parent.frCurPage; }}");
            Part("  function UpdateNav(PgN) {{");
            Part("    parent.frCurPage = PgN; document.PgForm.PgEdit.value = PgN;");
            Part("    if (PgN == 1) {{ document.PgForm.bFirst.disabled = 1; document.PgForm.bPrev.disabled = 1; }}");
            Part("    else {{ document.PgForm.bFirst.disabled = 0; document.PgForm.bPrev.disabled = 0; }}");
            Part("    if (PgN == frPgCnt) {{ document.PgForm.bNext.disabled = 1; document.PgForm.bLast.disabled = 1; }}");
            Part("    else {{ document.PgForm.bNext.disabled = 0; document.PgForm.bLast.disabled = 0; }} }}");
            Part("--></script></head>");
            Part("<body bgcolor=\"#DDDDDD\" text=\"#000000\" leftmargin=\"0\" topmargin=\"4\" onload=\"UpdateNav(parent.frCurPage)\">");
            Part("<form name=\"PgForm\" onsubmit=\"DoPage(document.forms[0].PgEdit.value); return false;\" action=\"\">");
            Part("<table cellspacing=\"0\" align=\"left\" cellpadding=\"0\" border=\"0\" width=\"100%\">");
            Part("<tr valign=\"middle\">");
            Part("<td width=\"60\" align=\"center\"><button name=\"bFirst\" class=\"nav\" type=\"button\" onclick=\"DoPage(1); return false;\"><b>{4}</b></button></td>");
            Part("<td width=\"60\" align=\"center\"><button name=\"bPrev\" class=\"nav\" type=\"button\" onclick=\"DoPage(Math.max(parent.frCurPage - 1, 1)); return false;\"><b>{5}</b></button></td>");
            Part("<td width=\"100\" align=\"center\"><input type=\"text\" class=\"nav\" name=\"PgEdit\" value=\"parent.frCurPage\" size=\"4\"></td>");
            Part("<td width=\"60\" align=\"center\"><button name=\"bNext\" class=\"nav\" type=\"button\" onclick=\"DoPage(parent.frCurPage + 1); return false;\"><b>{6}</b></button></td>");
            Part("<td width=\"60\" align=\"center\"><button name=\"bLast\" class=\"nav\" type=\"button\" onclick=\"DoPage(frPgCnt); return false;\"><b>{7}</b></button></td>");
            Part("<td width=\"20\">&nbsp;</td>\r\n");
            Part("<td align=\"right\">{8}: <script language=\"javascript\" type=\"text/javascript\"> document.write(frPgCnt);</script></td>");
            Part("<td width=\"10\">&nbsp;</td>");
            Part("</tr></table></form></body></html>");
            FNavigatorTemplate = Capacitor();
            #endregion

            #region fill outline template
            // under construction
            FOutlineTemplate = String.Empty;
            #endregion

            #region fill index template
            // {0} - title, {1} - navigator frame, {2} - main frame
            NewCapacitor();
            Part("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Frameset//EN\" \"http://www.w3.org/TR/html4/frameset.dtd\"");
            Part("<html><head>");
            Part("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            Part("<meta name=Generator content=\"FastReport http://www.fast-report.com\">");
            Part("<title>{0}</title>");
            Part("<script language=\"javascript\" type=\"text/javascript\"> var frCurPage = 1;</script></head>");
            Part("<frameset rows=\"36,*\" cols=\"*\">");
            Part("<frame name=\"topFrame\" src=\"{1}\" noresize frameborder=\"0\" scrolling=\"no\">");
            Part("<frame name=\"mainFrame\" src=\"{2}\" frameborder=\"0\">");
            Part("</frameset>");
            Part("</html>");
            FIndexTemplate = Capacitor();
            #endregion
        }
    }

}
