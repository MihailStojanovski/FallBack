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
using FastReport.Table;

namespace FastReport.Export.Pdf
{
    /// <summary>
    /// PDF export (Adobe Acrobat)
    /// </summary>
    public class PDFExport : ExportBase
    {        
        #region Constants
        const string PDF_VER = "1.5"; // minimum Acrobat Reader version 6.0
        const float PDF_DIVIDER = 0.75f;
        const float PDF_PAGE_DIVIDER = 2.8357f;
        const int PDF_PRINTOPT = 3;
        byte[] PDF_PK = { 
            0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 
            0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 
            0x01, 0x08, 0x2E, 0x2E, 0x00, 0xB6, 0xD0, 
            0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 
            0x64, 0x53, 0x69, 0x7A };
        float PDF_TTF_DIVIDER = 1 / (750 * 96f / DrawUtils.ScreenDpi);
        const float KAPPA1 = 1.5522847498f;
        const float KAPPA2 = 2 - KAPPA1;

        #endregion

        #region Private fields
        private string FTitle;
        private string FAuthor;
        private string FSubject;
        private string FKeywords;
        private string FCreator;
        private string FProducer;

        private bool FOutline;
        private bool FDisplayDocTitle;
        private bool FHideToolbar;
        private bool FHideMenubar;
        private bool FHideWindowUI;
        private bool FFitWindow;
        private bool FCenterWindow;
        private bool FPrintScaling;
        private string FUserPassword;
        private string FOwnerPassword;
        private bool FAllowPrint;
        private bool FAllowModify;
        private bool FAllowCopy;
        private bool FAllowAnnotate;

        private bool FEncrypted;
        private long FEncBits;
        private byte[] FEncKey;
        private byte[] FOPass;
        private byte[] FUPass;


        private bool FEmbeddingFonts;
        private bool FCompressed;
        private bool FBackground;
        private bool FPrintOptimized;

        private bool FPrintAfterOpen;

        private string FFileID;

        private long FRootNumber;
        private long FPagesNumber;
        private long FInfoNumber;
        private long FStartXRef;
        
        private List<long> FXRef;
        private List<long> FPagesRef;
        private List<PDFFont> FFonts;
        private List<PDFFont> FPageFonts;

        private List<string> FTrasparentStroke;
        private List<string> FTrasparentFill;

        private float FMarginLeft;
        private float FMarginWoBottom;

        private float FDpiFX;

        private Stream pdf;

        private bool FBuffered;

        #endregion

        #region Properties

        /// <summary>
        /// Enable compression in PDF document
        /// </summary>
        public bool Compressed
        {
            get { return FCompressed; }
            set { FCompressed = value; }
        }

        /// <summary>
        /// Enable embedding of TrueType fonts
        /// </summary>
        public bool EmbeddingFonts
        {
            get { return FEmbeddingFonts; }
            set { FEmbeddingFonts = value; }
        }

        /// <summary>
        /// Enable background export
        /// </summary>
        public bool Background
        {
            get { return FBackground; }
            set { FBackground = value; }
        }

        /// <summary>
        /// Enable background export
        /// </summary>
        public bool PrintOptimized
        {
            get { return FPrintOptimized; }
            set { FPrintOptimized = value; }
        }

        /// <summary>
        /// Enable print after open the exported document
        /// </summary>
        public bool PrintAfterOpen
        {
            get { return FPrintAfterOpen; }
            set { FPrintAfterOpen = value; }
        }

        /// <summary>
        /// Title of the document
        /// </summary>
        public string Title
        {
            get { return FTitle; }
            set { FTitle = value; }
        }

        /// <summary>
        /// Author of the document
        /// </summary>
        public string Author
        {
            get { return FAuthor; }
            set { FAuthor = value; }
        }

        /// <summary>
        /// Subject of the document
        /// </summary>
        public string Subject
        {
            get { return FSubject; }
            set { FSubject = value; }
        }

        /// <summary>
        /// Keywords of the document
        /// </summary>
        public string Keywords
        {
            get { return FKeywords; }
            set { FKeywords = value; }
        }

        /// <summary>
        /// Creator of the document
        /// </summary>
        public string Creator
        {
            get { return FCreator; }
            set { FCreator = value; }
        }

        /// <summary>
        /// Producer of the document
        /// </summary>
        public string Producer
        {
            get { return FProducer; }
            set { FProducer = value; }
        }

        /// <summary>
        /// Enable or disable document Outline
        /// </summary>
        public bool Outline
        {
            get { return FOutline; }
            set { FOutline = value; }
        }
        /// <summary>
        /// Enable or disable display document title
        /// </summary>
        public bool DisplayDocTitle
        {
            get { return FDisplayDocTitle; }
            set { FDisplayDocTitle = value; }
        }
        /// <summary>
        /// Enable or disable hide of toolbar
        /// </summary>
        public bool HideToolbar
        {
            get { return FHideToolbar; }
            set { FHideToolbar = value; }
        }
        /// <summary>
        /// Enable or disable hide of menu bar
        /// </summary>
        public bool HideMenubar
        {
            get { return FHideMenubar; }
            set { FHideMenubar = value; }
        }
        /// <summary>
        /// Enable or disable hide of Windows UI
        /// </summary>
        public bool HideWindowUI
        {
            get { return FHideWindowUI; }
            set { FHideWindowUI = value; }
        }
        /// <summary>
        /// Enable or disable fit window
        /// </summary>
        public bool FitWindow
        {
            get { return FFitWindow; }
            set { FFitWindow = value; }
        }
        /// <summary>
        /// Enable or disable centering window
        /// </summary>
        public bool CenterWindow
        {
            get { return FCenterWindow; }
            set { FCenterWindow = value; }
        }
        /// <summary>
        /// Enable or disable of print scaling
        /// </summary>
        public bool PrintScaling
        {
            get { return FPrintScaling; }
            set { FPrintScaling = value; }
        }
        /// <summary>
        /// Sets the user password
        /// </summary>
        public string UserPassword
        {
            get { return FUserPassword; }
            set { FUserPassword = value; }
        }
        /// <summary>
        /// Sets the owner password
        /// </summary>
        public string OwnerPassword
        {
            get { return FOwnerPassword; }
            set { FOwnerPassword = value; }
        }
        /// <summary>
        /// Enable or disable printing in protected document
        /// </summary>
        public bool AllowPrint
        {
            get { return FAllowPrint; }
            set { FAllowPrint = value; }
        }
        /// <summary>
        /// Enable or disable modifying in protected document
        /// </summary>
        public bool AllowModify
        {
            get { return FAllowModify; }
            set { FAllowModify = value; }
        }
        /// <summary>
        /// Enable or disable copying in protected document
        /// </summary>
        public bool AllowCopy
        {
            get { return FAllowCopy; }
            set { FAllowCopy = value; }
        }
        /// <summary>
        /// Enable or disable annotating in protected document
        /// </summary>
        public bool AllowAnnotate
        {
            get { return FAllowAnnotate; }
            set { FAllowAnnotate = value; }
        }
        #endregion

        #region Private Methods

        private string RC4CryptString(string source, byte[] key, long id)
        {
            byte[] k = new byte[21];
            Array.Copy(key, 0, k, 0, 16);
            k[16] = (byte)id;
            k[17] = (byte)(id >> 8);
            k[18] = (byte)(id >> 16);
            byte[] s = new byte[16];
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            Array.Copy(md5.ComputeHash(k), s, 16);
            RC4 rc4 = new RC4();
            rc4.Start(s);
            byte[] src = ExportUtils.StringToByteArray(source);
            byte[] target = rc4.Crypt(src);
            return ExportUtils.StringFromByteArray(target);
        }

        private void RC4CryptStream(Stream source, Stream target, byte[] key, long id)
        {
            byte[] k = new byte[21];
            Array.Copy(key, 0, k, 0, 16);
            k[16] = (byte)id;
            k[17] = (byte)(id >> 8);
            k[18] = (byte)(id >> 16);

            byte[] s = new byte[16];
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            Array.Copy(md5.ComputeHash(k), s, 16);

            byte[] buffSource = new byte[source.Length];
            source.Position = 0;
            source.Read(buffSource, 0, (int)source.Length);

            RC4 rc4 = new RC4();
            rc4.Start(s);
            byte[] buffTarget = rc4.Crypt(buffSource);
            target.Write(buffTarget, 0, buffTarget.Length);
        }

        private string PrepareString(string text, byte[] key, bool encode, long id)
        {
            StringBuilder result = new StringBuilder(text.Length * 2);
            if (encode)
            {
                result.Append("(").Append(EscapeSpecialChar(RC4CryptString(text, key, id))).Append(")");
            }
            else
                result.Append("<").Append(StrToUTF16(text)).Append(">");
            return result.ToString();
        }

        private byte[] PadPassword(string password)
        {
            byte[] p = ExportUtils.StringToByteArray(password);
            byte[] result = new byte[32];
            int l = p.Length < 32 ? p.Length : 32;
            for (int i = 0; i < l; i++)
                result[i] = p[i];
            if (l < 32)
                for (int i = l; i < 32; i++)
                    result[i] = PDF_PK[i - l];
            return result;
        }
    
        private void PrepareKeys()
        {
            FEncBits = -64; // 0xFFFFFFC0;
            if (FAllowPrint)
                FEncBits += 4;
            if (FAllowModify)
                FEncBits += 8;
            if (FAllowCopy)
                FEncBits += 16;
            if (FAllowAnnotate)
                FEncBits += 32;

            // OWNER KEY            
            if (String.IsNullOrEmpty(FOwnerPassword))
                FOwnerPassword = FUserPassword;

            byte[] p = PadPassword(FOwnerPassword);
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            md5.Initialize();
            byte[] s = new byte[16];
            md5.TransformBlock(p, 0, 32, p, 0);
            md5.TransformFinalBlock(p, 0, 0);
            Array.Copy(md5.Hash, s, 16);
            for (byte i = 1; i <= 50; i++)
            {
                md5.Initialize();
                Array.Copy(md5.ComputeHash(s), 0, s, 0, 16);
            }

            RC4 rc4 = new RC4();
            p = PadPassword(FUserPassword);
            rc4.Start(s);
            byte[] s1 = rc4.Crypt(p);
            byte[] p1 = new byte[16];
            for (byte i = 1; i <= 19; i++)
            {
                for (byte j = 1; j <= 16; j++)
                    p1[j - 1] = (byte)(s[j - 1] ^ i);
                rc4.Start(p1);
                s1 = rc4.Crypt(s1);
            }
            FOPass = new byte[32];
            Array.Copy(s1, FOPass, 32);                

            // ENCRYPTION KEY
            p = PadPassword(FUserPassword);

            md5.Initialize();
            md5.TransformBlock(p, 0, 32, p, 0);
            md5.TransformBlock(FOPass, 0, 32, FOPass, 0);

            byte[] ext = new byte[4];
            ext[0] = (byte)FEncBits;
            ext[1] = (byte)(FEncBits >> 8);
            ext[2] = (byte)(FEncBits >> 16);
            ext[3] = (byte)(FEncBits >> 24);
            md5.TransformBlock(ext, 0, 4, ext, 0);

            byte[] fid = new byte[16];
            for (byte i = 1; i <= 16; i++)
                fid[i - 1] = Convert.ToByte(String.Concat(FFileID[i * 2 - 2], FFileID[i * 2 - 1]), 16);
            md5.TransformBlock(fid, 0, 16, fid, 0);
            md5.TransformFinalBlock(ext, 0, 0);
            Array.Copy(md5.Hash, 0, s, 0, 16);

            for (byte i = 1; i <= 50; i++)
            {
                md5.Initialize();
                Array.Copy(md5.ComputeHash(s), 0, s, 0, 16);
            }
            FEncKey = new byte[16];
            Array.Copy(s, 0, FEncKey, 0, 16);

            // USER KEY
            md5.Initialize();
            md5.TransformBlock(PDF_PK, 0, 32, PDF_PK, 0);
            md5.TransformBlock(fid, 0, 16, fid, 0);
            md5.TransformFinalBlock(fid, 0, 0);
            Array.Copy(md5.Hash, s, 16);

            s1 = new byte[16];
            Array.Copy(FEncKey, s1, 16);
  
            rc4.Start(s1);
            s = rc4.Crypt(s);            

            p1 = new byte[16];
            for (byte i = 1; i <= 19; i++)
            {
                for (byte j = 1; j <= 16; j++)
                    p1[j - 1] = (byte)(s1[j - 1] ^ i);
                rc4.Start(p1);
                s = rc4.Crypt(s);
            }
            FUPass = new byte[32];
            Array.Copy(s, 0, FUPass, 0, 16);
        }

        private void Write(Stream stream, string value)
        {
            stream.Write(ExportUtils.StringToByteArray(value), 0, value.Length);
        }

        private void WriteLn(Stream stream, string value)
        {
            stream.Write(ExportUtils.StringToByteArray(value), 0, value.Length);
            stream.WriteByte(0x0d);
            stream.WriteByte(0x0a);
        }

        private string StrToUTF16(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                StringBuilder sb = new StringBuilder(str.Length * 4 + 4);
                sb.Append("FEFF");
                foreach (char c in str)
                    sb.Append(((int)c).ToString("X4"));
                return sb.ToString();
            }
            else
                return str;
        }

        private string EscapeSpecialChar(string input)
        {
            StringBuilder sb = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '(':
                        sb.Append(@"\(");
                        break;
                    case ')':
                        sb.Append(@"\)");
                        break;
                    case '\\':
                        sb.Append(@"\\");
                        break;
                    case '\r':
                        sb.Append(@"\r");
                        break;
                    case '\n':
                        sb.Append(@"\n");
                        break;
                    default:
                        sb.Append(input[i]);
                        break;
                }
            }
            return sb.ToString();
        }

        private void AddPDFHeader()
        {
            FXRef.Clear();
            FPagesRef.Clear();
            FFonts.Clear();
            WriteLn(pdf, "%PDF-" + PDF_VER);
            UpdateXRef();
        }

        private void AddPage(ReportPage page)
        {            
            FPageFonts.Clear();
            FTrasparentStroke.Clear();
            FTrasparentFill.Clear();

            FMarginWoBottom = (page.PaperHeight - page.TopMargin) * PDF_PAGE_DIVIDER;
            FMarginLeft = page.LeftMargin * PDF_PAGE_DIVIDER;

            long FContentsPos = 0;
            using (MemoryStream FContentBuilder = new MemoryStream(65536))
            {
                // bitmap watermark on bottom
                if (page.Watermark.Enabled && !page.Watermark.ShowImageOnTop && FBackground)
                    AddBitmapWatermark(FContentBuilder, page);

                // text watermark on bottom
                if (page.Watermark.Enabled && !page.Watermark.ShowTextOnTop)
                    AddTextWatermark(FContentBuilder, page);

                foreach (Base c in page.AllObjects)
                {
                    if (c is ReportComponentBase)
                    {
                        ReportComponentBase obj = c as ReportComponentBase;
                        if (obj is CellularTextObject)
                            obj = (obj as CellularTextObject).GetTable();
                        if (obj is TableCell)
                            continue;
                        else
                            if (obj is TableBase)
                            {
                                TableBase table = obj as TableBase;
                                string tableBorder;
                                using (TextObject tableback = new TextObject())
                                {
                                    tableback.Border = table.Border;
                                    tableback.Fill = table.Fill;
                                    tableback.FillColor = table.FillColor;
                                    tableback.Left = table.AbsLeft;
                                    tableback.Top = table.AbsTop;
                                    float tableWidth = 0;
                                    for (int i = 0; i < table.ColumnCount; i++)
                                        tableWidth += table[i, 0].Width;
                                    tableback.Width = (tableWidth < table.Width) ? tableWidth : table.Width;
                                    tableback.Height = table.Height;
                                    AddTextObject(FContentBuilder, tableback, false);
                                    tableBorder = GetPDFBorder(tableback).ToString();
                                }
                                // draw cells
                                AddTable(FContentBuilder, table, true);
                                // draw cells border
                                AddTable(FContentBuilder, table, false);                                
                                // draw table border
                                Write(FContentBuilder, tableBorder);
                            }
                            else if (IsMemo(obj))
                                AddTextObject(FContentBuilder, obj as TextObject, true);
                            else if (obj is BandBase)
                                AddBandObject(FContentBuilder, obj as BandBase);
                            else if (obj is LineObject)
                                AddLine(FContentBuilder, obj as LineObject);
                            else if (obj is ShapeObject)
                                AddShape(FContentBuilder, obj as ShapeObject);
                            else
                                AddPictureObject(FContentBuilder, obj as ReportComponentBase, true);
                    }
                }

                // bitmap watermark on top
                if (page.Watermark.Enabled && page.Watermark.ShowImageOnTop && FBackground)
                    AddBitmapWatermark(FContentBuilder, page);

                // text watermark on top
                if (page.Watermark.Enabled && page.Watermark.ShowTextOnTop)
                    AddTextWatermark(FContentBuilder, page);

                FContentsPos = UpdateXRef();
                WriteLn(pdf, ObjNumber(FContentsPos));
                if (FCompressed)
                {
                    using (MemoryStream memstream = new MemoryStream())
                    {
                        ExportUtils.ZLibDeflate(FContentBuilder, memstream);
                        StringBuilder sb1 = new StringBuilder(80);
                        sb1.Append("<< /Filter /FlateDecode /Length ").Append((memstream.Length).ToString());
                        sb1.Append(" /Length1 ").Append(FContentBuilder.Length.ToString()).AppendLine(" >>");
                        sb1.AppendLine("stream");                        
                        Write(pdf, sb1.ToString());
                        if (FEncrypted)
                            RC4CryptStream(memstream, pdf, FEncKey, FContentsPos);
                        else
                            memstream.WriteTo(pdf);
                    }
                }
                else
                {
                    StringBuilder sb1 = new StringBuilder(80);
                    sb1.Append("<< /Length ").Append(FContentBuilder.Length.ToString()).AppendLine(" >>");
                    sb1.AppendLine("stream");
                    Write(pdf, sb1.ToString());
                    if (FEncrypted)
                        RC4CryptStream(FContentBuilder, pdf, FEncKey, FContentsPos);
                    else
                        FContentBuilder.WriteTo(pdf);                    
                }
                WriteLn(pdf, String.Empty);
                WriteLn(pdf, "endstream");
                WriteLn(pdf, "endobj");
            }
            if (FPageFonts.Count > 0)
                for (int i = 0; i < FPageFonts.Count; i++)
                    if (!FPageFonts[i].Saved)
                    {
                        FPageFonts[i].Reference = UpdateXRef();
                        FPageFonts[i].Saved = true;
                    }                        
            long PageNumber = UpdateXRef();
            FPagesRef.Add(PageNumber);
            WriteLn(pdf, ObjNumber(PageNumber));
            StringBuilder sb = new StringBuilder(512);
            sb.AppendLine("<<").AppendLine("/Type /Page");
            sb.Append("/MediaBox [0 0 ").Append(ExportUtils.FloatToString(page.PaperWidth * PDF_PAGE_DIVIDER)).Append(" ");
            sb.Append(ExportUtils.FloatToString(page.PaperHeight * PDF_PAGE_DIVIDER)).AppendLine(" ]");
            sb.AppendLine("/Parent 1 0 R").AppendLine("/Resources << ");
            if (FPageFonts.Count > 0)
            {
                sb.Append("/Font << ");
                foreach (PDFFont font in FPageFonts)
                    sb.Append(font.Name).Append(" ").Append(ObjNumberRef(font.Reference)).Append(" ");
                sb.AppendLine(" >>");
            }
            sb.AppendLine("/ExtGState <<");
            for (int i = 0; i < FTrasparentStroke.Count; i++)
                sb.Append("/GS").Append(i.ToString()).Append("S << /Type /ExtGState /ca ").Append(FTrasparentStroke[i]).AppendLine(" >>");
            for (int i = 0; i < FTrasparentFill.Count; i++)
                sb.Append("/GS").Append(i.ToString()).Append("F << /Type /ExtGState /CA ").Append(FTrasparentFill[i]).AppendLine(" >>");
            sb.AppendLine(">>");
            sb.Append("/XObject << ").AppendLine(" >>");
            sb.AppendLine("/ProcSet [/PDF /Text /ImageC ]");
            sb.AppendLine(">>");
            sb.Append("/Contents ").AppendLine(ObjNumberRef(FContentsPos));
            sb.AppendLine(">>");
            sb.AppendLine("endobj");
            Write(pdf, sb.ToString());
        }

        private void AddBitmapWatermark(Stream outstream, ReportPage page)
        {
            using (PictureObject pictureWatermark = new PictureObject())
            {
                pictureWatermark.Left = -FMarginLeft;
                pictureWatermark.Top = -page.TopMargin * PDF_PAGE_DIVIDER;
                pictureWatermark.Width = page.PaperWidth * PDF_PAGE_DIVIDER / PDF_DIVIDER;
                pictureWatermark.Height = page.PaperHeight * PDF_PAGE_DIVIDER / PDF_DIVIDER;

                pictureWatermark.SizeMode = PictureBoxSizeMode.Normal;
                pictureWatermark.Image = new Bitmap((int)pictureWatermark.Width, (int)pictureWatermark.Height);
                using (Graphics g = Graphics.FromImage(pictureWatermark.Image))
                {
                    g.Clear(Color.Transparent);
                    page.Watermark.DrawImage(new FRPaintEventArgs(g, 1, 1, Report.GraphicCache), 
                        new RectangleF(0, 0, pictureWatermark.Width, pictureWatermark.Height), Report, true);
                }
                pictureWatermark.Transparency = page.Watermark.ImageTransparency;                                 
                AddPictureObject(outstream, pictureWatermark, false);
            }
        }

        private void AddTextWatermark(Stream outstream, ReportPage page)
        {
            using (TextObject textWatermark = new TextObject())
            {
                textWatermark.HorzAlign = HorzAlign.Center;
                textWatermark.VertAlign = VertAlign.Center;
                textWatermark.Left = -FMarginLeft;
                textWatermark.Top = -page.TopMargin * PDF_PAGE_DIVIDER;
                textWatermark.Width = page.PaperWidth * PDF_PAGE_DIVIDER / PDF_DIVIDER;
                textWatermark.Height = page.PaperHeight * PDF_PAGE_DIVIDER / PDF_DIVIDER;
                textWatermark.Text = page.Watermark.Text;
                textWatermark.TextFill = page.Watermark.TextFill;
                if (page.Watermark.TextRotation == WatermarkTextRotation.Vertical)
                    textWatermark.Angle = 270;
                else if (page.Watermark.TextRotation == WatermarkTextRotation.ForwardDiagonal)
                    textWatermark.Angle = 360 - (int)(Math.Atan(textWatermark.Height / textWatermark.Width) * (180 / Math.PI));
                else if (page.Watermark.TextRotation == WatermarkTextRotation.BackwardDiagonal)
                    textWatermark.Angle = (int)(Math.Atan(textWatermark.Height / textWatermark.Width) * (180 / Math.PI));
                textWatermark.Font = page.Watermark.Font;
                if (page.Watermark.TextFill is SolidFill)
                    textWatermark.TextColor = (page.Watermark.TextFill as SolidFill).Color;
                AddTextObject(outstream, textWatermark, false);
            }
        }

        private void AddTable(Stream outstream, TableBase table, bool drawCells)
        {
            float y = 0;
            for (int i = 0; i < table.RowCount; i++)
            {
                float x = 0;
                for (int j = 0; j < table.ColumnCount; j++)
                {
                    if (!table.IsInsideSpan(table[j, i]))
                    {
                        TableCell textcell = table[j, i];
                        textcell.Left = x;
                        textcell.Top = y;
                        if (drawCells)
                        {
                            Border oldBorder = textcell.Border.Clone();
                            textcell.Border.Lines = BorderLines.None;
                            if (IsMemo(textcell as TextObject))
                                AddTextObject(outstream, textcell as TextObject, false);
                            else
                                AddPictureObject(outstream, textcell as ReportComponentBase, false);
                            textcell.Border = oldBorder;
                        }
                        else
                            Write(outstream, GetPDFBorder(textcell).ToString());
                    }
                    x += (table.Columns[j]).Width;
                }
                y += (table.Rows[i]).Height;
            }
        }

        private void AddShape(Stream outstream, ShapeObject shapeObject)
        {
            if ((shapeObject.Shape == ShapeKind.Rectangle || shapeObject.Shape == ShapeKind.RoundRectangle) && shapeObject.Fill is SolidFill)
            {
                Write(outstream, DrawPDFFillRect(
                    GetLeft(shapeObject.AbsLeft), GetTop(shapeObject.AbsTop),
                    shapeObject.Width * PDF_DIVIDER, shapeObject.Height * PDF_DIVIDER, 
                    shapeObject.Fill));
                Write(outstream, DrawPDFRect(
                    GetLeft(shapeObject.AbsLeft),
                    GetTop(shapeObject.AbsTop),
                    GetLeft(shapeObject.AbsLeft + shapeObject.Width),
                    GetTop(shapeObject.AbsTop + shapeObject.Height),
                    shapeObject.Border.Color, shapeObject.Border.Width * PDF_DIVIDER, shapeObject.Border.Style));
            }
            else if (shapeObject.Shape == ShapeKind.Triangle && shapeObject.Fill is SolidFill)
                Write(outstream, DrawPDFTriangle(GetLeft(shapeObject.AbsLeft), GetTop(shapeObject.AbsTop),
                    shapeObject.Width * PDF_DIVIDER, shapeObject.Height * PDF_DIVIDER, 
                    shapeObject.FillColor, shapeObject.Border.Color, shapeObject.Border.Width * PDF_DIVIDER, shapeObject.Border.Style));
            else if (shapeObject.Shape == ShapeKind.Diamond && shapeObject.Fill is SolidFill)
                Write(outstream, DrawPDFDiamond(GetLeft(shapeObject.AbsLeft), GetTop(shapeObject.AbsTop),
                    shapeObject.Width * PDF_DIVIDER, shapeObject.Height * PDF_DIVIDER,
                    shapeObject.FillColor, shapeObject.Border.Color, shapeObject.Border.Width * PDF_DIVIDER, shapeObject.Border.Style));
            else if (shapeObject.Shape == ShapeKind.Ellipse && shapeObject.Fill is SolidFill)
                Write(outstream, DrawPDFEllipse(GetLeft(shapeObject.AbsLeft), GetTop(shapeObject.AbsTop),
                    shapeObject.Width * PDF_DIVIDER, shapeObject.Height * PDF_DIVIDER,
                    shapeObject.FillColor, shapeObject.Border.Color, shapeObject.Border.Width * PDF_DIVIDER, shapeObject.Border.Style));
            else
                AddPictureObject(outstream, shapeObject, true);
        }

        private void AddLine(Stream outstream, LineObject l)
        {
            Write(outstream, DrawPDFLine(GetLeft(l.AbsLeft),
                GetTop(l.AbsTop), GetLeft(l.AbsLeft + l.Width), GetTop(l.AbsTop + l.Height), 
                l.Border.Color, l.Border.Width * PDF_DIVIDER, l.Border.Style, l.StartCap, l.EndCap));
        }

        private void WriteFont(PDFFont pdfFont)
        {
            long fontFileId = 0;
            string fontName = pdfFont.GetFontName();
            // embedded font 
            if (FEmbeddingFonts)
            {
                fontFileId = UpdateXRef();
                WriteLn(pdf, ObjNumber(fontFileId));
                byte[] fontfile = pdfFont.GetFontData();
                if (FCompressed)
                {
                    using (MemoryStream memstream = new MemoryStream())
                    using (MemoryStream fontstream = new MemoryStream())
                    {
                        fontstream.Write(fontfile, 0, fontfile.Length);
                        ExportUtils.ZLibDeflate(fontstream, memstream);
                        WriteLn(pdf, "<< /Length " + memstream.Length.ToString() + "  /Filter /FlateDecode /Length1 " + fontfile.Length.ToString() + " >>");
                        WriteLn(pdf, "stream");
                        if (FEncrypted)
                            RC4CryptStream(memstream, pdf, FEncKey, fontFileId);
                        else
                            memstream.WriteTo(pdf);
                    }
                }
                else
                {
                    WriteLn(pdf, "<< /Length " + fontfile.Length.ToString() + " /Length1 " + fontfile.Length.ToString() + " >>");
                    WriteLn(pdf, "stream");
                    if (FEncrypted)
                    {
                        using (MemoryStream fontstream = new MemoryStream())
                        {
                            fontstream.Write(fontfile, 0, fontfile.Length);
                            RC4CryptStream(fontstream, pdf, FEncKey, fontFileId);
                        }
                    }
                    else
                        pdf.Write(fontfile, 0, fontfile.Length);
                }
                WriteLn(pdf, String.Empty);
                WriteLn(pdf, "endstream");
                WriteLn(pdf, "endobj");
            }

            // descriptor
            long descriptorId = UpdateXRef();
            WriteLn(pdf, ObjNumber(descriptorId));
            WriteLn(pdf, "<<");
            WriteLn(pdf, "/Type /FontDescriptor");
            WriteLn(pdf, "/FontName /" + fontName);
            //WriteLn(pdf, "/FontFamily /" + fontName);
            WriteLn(pdf, "/Flags 32");
            WriteLn(pdf, "/FontBBox [" + pdfFont.TextMetric.otmrcFontBox.left.ToString() + " " +
                pdfFont.TextMetric.otmrcFontBox.bottom.ToString() + " " +
                pdfFont.TextMetric.otmrcFontBox.right.ToString() + " " +
                pdfFont.TextMetric.otmrcFontBox.top.ToString() + " ]");
            //WriteLn(pdf, "/Style << /Panose <" + pdfFont.GetPANOSE() + "> >>"); 
            WriteLn(pdf, "/ItalicAngle " + pdfFont.TextMetric.otmItalicAngle.ToString());
            WriteLn(pdf, "/Ascent " + pdfFont.TextMetric.otmAscent.ToString());
            WriteLn(pdf, "/Descent " + pdfFont.TextMetric.otmDescent.ToString());
            WriteLn(pdf, "/Leading " + pdfFont.TextMetric.otmTextMetrics.tmInternalLeading.ToString());
            WriteLn(pdf, "/CapHeight " + pdfFont.TextMetric.otmTextMetrics.tmHeight.ToString());
            WriteLn(pdf, "/StemV " + (50 + Math.Round(Math.Sqrt(pdfFont.TextMetric.otmTextMetrics.tmWeight / 65))).ToString());
            WriteLn(pdf, "/AvgWidth " + pdfFont.TextMetric.otmTextMetrics.tmAveCharWidth.ToString());
            WriteLn(pdf, "/MxWidth " + pdfFont.TextMetric.otmTextMetrics.tmMaxCharWidth.ToString());
            WriteLn(pdf, "/MissingWidth " + pdfFont.TextMetric.otmTextMetrics.tmAveCharWidth.ToString());
            if (FEmbeddingFonts)
                WriteLn(pdf, "/FontFile2 " + ObjNumberRef(fontFileId));
            WriteLn(pdf, ">>");
            WriteLn(pdf, "endobj");

            // ToUnicode
            long toUnicodeId = UpdateXRef();
            WriteLn(pdf, ObjNumber(toUnicodeId));
            StringBuilder toUnicode = new StringBuilder(2048);
            toUnicode.AppendLine("/CIDInit /ProcSet findresource begin");
            toUnicode.AppendLine("12 dict begin");
            toUnicode.AppendLine("begincmap");
            toUnicode.AppendLine("/CIDSystemInfo");
            toUnicode.AppendLine("<< /Registry (Adobe)");
            toUnicode.AppendLine("/Ordering (UCS)");
            toUnicode.AppendLine("/Ordering (Identity)");
            toUnicode.AppendLine("/Supplement 0");
            toUnicode.AppendLine(">> def");
            toUnicode.Append("/CMapName /").Append(pdfFont.GetFontName().Replace(',', '+')).AppendLine(" def");
            toUnicode.AppendLine("/CMapType 2 def");
            toUnicode.AppendLine("1 begincodespacerange");
            toUnicode.AppendLine("<0000> <FFFF>");
            toUnicode.AppendLine("endcodespacerange");
            toUnicode.Append(pdfFont.UsedAlphabet.Count.ToString()).AppendLine(" beginbfchar");
            for (int i = 0; i < pdfFont.UsedAlphabet.Count; i++)
                toUnicode.Append("<").Append(pdfFont.UsedAlphabet[i].ToString("X4")).Append("> <").Append(pdfFont.UsedAlphabetUnicode[i].ToString("X4")).AppendLine(">");
            toUnicode.AppendLine("endbfchar");
            toUnicode.AppendLine("endcmap");
            toUnicode.AppendLine("CMapName currentdict /CMap defineresource pop");
            toUnicode.AppendLine("end");
            toUnicode.AppendLine("end");
            if (FCompressed)
            {
                using (MemoryStream memstream = new MemoryStream())
                using (MemoryStream tounicodestream = new MemoryStream())
                {
                    Write(tounicodestream, toUnicode.ToString());
                    ExportUtils.ZLibDeflate(tounicodestream, memstream);
                    WriteLn(pdf, "<< /Length " + memstream.Length.ToString() + "  /Filter /FlateDecode /Length1 " + tounicodestream.Length.ToString() + " >>");
                    WriteLn(pdf, "stream");
                    if (FEncrypted)
                    {
                        RC4CryptStream(memstream, pdf, FEncKey, toUnicodeId);
                        WriteLn(pdf, String.Empty);
                    }
                    else
                        memstream.WriteTo(pdf);
                }
            }
            else
            {
                WriteLn(pdf, "<< /Length " + toUnicode.Length.ToString() + " >>");
                WriteLn(pdf, "stream");
                if (FEncrypted)
                {
                    using (MemoryStream memstream = new MemoryStream())
                    {
                        Write(memstream, toUnicode.ToString());
                        RC4CryptStream(memstream, pdf, FEncKey, toUnicodeId);
                        WriteLn(pdf, String.Empty);
                    }
                }
                else
                    Write(pdf, toUnicode.ToString());
            }
            WriteLn(pdf, "endstream");
            WriteLn(pdf, "endobj");

            //CIDSystemInfo
            long cIDSystemInfoId = UpdateXRef();
            WriteLn(pdf, ObjNumber(cIDSystemInfoId));
            WriteLn(pdf, "<<");
            WriteLn(pdf, "/Registry (Adobe) /Ordering (Identity) /Supplement 0");
            WriteLn(pdf, ">>");
            WriteLn(pdf, "endobj");

            //DescendantFonts
            long descendantFontId = UpdateXRef();
            WriteLn(pdf, ObjNumber(descendantFontId));
            WriteLn(pdf, "<<");
            WriteLn(pdf, "/Type /Font");
            WriteLn(pdf, "/Subtype /CIDFontType2");
            WriteLn(pdf, "/BaseFont /" + fontName);
            WriteLn(pdf, "/CIDSystemInfo " + ObjNumberRef(cIDSystemInfoId));
            WriteLn(pdf, "/FontDescriptor " + ObjNumberRef(descriptorId));
            Write(pdf, "/W [ ");
            for (int i = 0; i < pdfFont.UsedAlphabet.Count; i++)
                Write(pdf, pdfFont.UsedAlphabet[i].ToString() + " [" + pdfFont.Widths[i].ToString() + "] ");
            WriteLn(pdf, "]");
            WriteLn(pdf, ">>");
            WriteLn(pdf, "endobj");
           
            // main
            FXRef[(int)(pdfFont.Reference - 1)] = pdf.Position;
            WriteLn(pdf, ObjNumber(pdfFont.Reference));
            WriteLn(pdf, "<<");
            WriteLn(pdf, "/Type /Font");
            WriteLn(pdf, "/Subtype /Type0");
            WriteLn(pdf, "/BaseFont /" + fontName);
            WriteLn(pdf, "/Encoding /Identity-H");
            WriteLn(pdf, "/DescendantFonts [" + ObjNumberRef(descendantFontId) + "]"); 
            WriteLn(pdf, "/ToUnicode " + ObjNumberRef(toUnicodeId));
            WriteLn(pdf, ">>");
            WriteLn(pdf, "endobj");
        }

        private bool IsMemo(ReportComponentBase Obj)
        {
            return (Obj is TextObject);
        }

        private void AddPictureObject(Stream outstream, ReportComponentBase obj, bool drawBorder)
        {
            if (obj.Width == 0 || obj.Height == 0)
                return;

            float printZoom = FPrintOptimized ? 4 : 2;
            Border oldBorder = obj.Border.Clone();
            obj.Border.Lines = BorderLines.None;            
            using (System.Drawing.Image image = new System.Drawing.Bitmap((int)Math.Round(obj.Width * printZoom), (int)Math.Round(obj.Height * printZoom)))
            using (Graphics g = Graphics.FromImage(image))
            using (GraphicCache cache = new GraphicCache())
            {
                g.TranslateTransform(-obj.AbsLeft * printZoom, -obj.AbsTop * printZoom);
                g.Clear(Color.White);
                obj.Draw(new FRPaintEventArgs(g, printZoom, printZoom, cache));

                float bWidth = obj.Width == 0 ? 1 : obj.Width * PDF_DIVIDER;
                float bHeight = obj.Height == 0 ? 1 : obj.Height * PDF_DIVIDER;

                StringBuilder sb = new StringBuilder(256);
                sb.AppendLine("q");

                if (obj is PictureObject)
                    sb.Append(GetPDFFillTransparent(
                        Color.FromArgb((byte)((1 - (obj as PictureObject).Transparency) * 255f), Color.Black)));

                sb.Append(ExportUtils.FloatToString(bWidth)).Append(" 0 0 ");
                sb.Append(ExportUtils.FloatToString(bHeight)).Append(" ");
                sb.Append(ExportUtils.FloatToString(GetLeft(obj.AbsLeft))).Append(" ");
                sb.Append(ExportUtils.FloatToString(GetTop(obj.AbsTop + obj.Height)));
                sb.AppendLine(" cm");

                sb.AppendLine("BI");
                sb.Append("/W ").AppendLine(image.Width.ToString()).Append("/H ");
                sb.AppendLine(image.Height.ToString()).AppendLine("/CS /RGB");
                sb.AppendLine("/BPC 8").AppendLine("/I true").AppendLine("/F [/DCT]").AppendLine("ID");
                Write(outstream, sb.ToString());
                using (MemoryStream buff = new MemoryStream())
                {
                    image.Save(buff, ImageFormat.Jpeg);
                    outstream.Write(buff.ToArray(), 0, (int)buff.Length);
                }
            }
            WriteLn(outstream, "\r\nEI");
            WriteLn(outstream, "Q");
            obj.Border = oldBorder;
            if (drawBorder)
                Write(outstream, GetPDFBorder(obj).ToString());
        }

        private void AddBandObject(Stream outstream, BandBase band)
        {
            using (TextObject newObj = new TextObject())
            {
                newObj.Left = band.AbsLeft;
                newObj.Top = band.AbsTop;
                newObj.Width = band.Width;
                newObj.Height = band.Height;
                newObj.Fill = band.Fill;
                newObj.Border = band.Border;
                AddTextObject(outstream, newObj, true);
            }
        }

        private void AddTextObject(Stream outstream, TextObject obj, bool drawBorder)
        {
            string Left = ExportUtils.FloatToString(GetLeft(obj.AbsLeft));
            string Top = ExportUtils.FloatToString(GetTop(obj.AbsTop));
            string Right = ExportUtils.FloatToString(GetLeft(obj.AbsLeft + obj.Width));
            string Bottom = ExportUtils.FloatToString(GetTop(obj.AbsTop + obj.Height));
            string Width = ExportUtils.FloatToString(obj.Width * PDF_DIVIDER);
            string Height = ExportUtils.FloatToString(obj.Height * PDF_DIVIDER);

            StringBuilder Result = new StringBuilder(256);

            Result.AppendLine("q");
            Result.Append(ExportUtils.FloatToString(GetLeft(obj.AbsLeft))).Append(" ");
            Result.Append(ExportUtils.FloatToString(GetTop(obj.AbsTop + obj.Height))).Append(" ");
            Result.Append(ExportUtils.FloatToString((obj.Width) * PDF_DIVIDER)).Append(" ");
            Result.Append(ExportUtils.FloatToString((obj.Height) * PDF_DIVIDER)).AppendLine(" re");
            Result.AppendLine("W").AppendLine("n");

            // draw background
            if (obj.Fill is SolidFill || (obj.Fill is GlassFill && !(obj.Fill as GlassFill).Hatch))
                Result.Append(DrawPDFFillRect(GetLeft(obj.AbsLeft), GetTop(obj.AbsTop),
                    obj.Width * PDF_DIVIDER, obj.Height * PDF_DIVIDER, obj.Fill));
            else if (obj.Width > 0 && obj.Height > 0)
            {
                using (PictureObject backgroundPicture = new PictureObject())
                {
                    backgroundPicture.Left = obj.AbsLeft;
                    backgroundPicture.Top = obj.AbsTop;
                    backgroundPicture.Width = obj.Width;
                    backgroundPicture.Height = obj.Height;
                    backgroundPicture.Image = new Bitmap((int)backgroundPicture.Width, (int)backgroundPicture.Height);
                    using (Graphics g = Graphics.FromImage(backgroundPicture.Image))
                    {
                        g.Clear(Color.Transparent);
                        g.TranslateTransform(-obj.AbsLeft, -obj.AbsTop);
                        BorderLines oldLines = obj.Border.Lines;
                        obj.Border.Lines = BorderLines.None;
                        string oldText = obj.Text;
                        obj.Text = String.Empty;
                        obj.Draw(new FRPaintEventArgs(g, 1, 1, Report.GraphicCache));
                        obj.Text = oldText;
                        obj.Border.Lines = oldLines;
                    }
                    AddPictureObject(outstream, backgroundPicture, false);
                }
            }

            if (obj.Underlines)
                AppendUnderlines(Result, obj);

            if (!String.IsNullOrEmpty(obj.Text))
            {
                int ObjectFontNumber = GetObjFontNumber(obj.Font);
                // obj with HtmlTags uses own font/color for each word/run
                if (!obj.HtmlTags)
                    AppendFont(Result, ObjectFontNumber, obj.Font.Size, obj.TextColor);
                
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                using (Font f = new Font(obj.Font.Name, obj.Font.Size * FDpiFX, obj.Font.Style))
                using (GraphicCache cache = new GraphicCache())
                {
                    RectangleF textRect = new RectangleF(
                      obj.AbsLeft + obj.Padding.Left,
                      obj.AbsTop + obj.Padding.Top,
                      obj.Width - obj.Padding.Horizontal,
                      obj.Height - obj.Padding.Vertical);

                    bool transformNeeded = obj.Angle != 0 || obj.FontWidthRatio != 1;

                    // transform, rotate and scale pdf coordinates if needed
                    if (transformNeeded)
                    {
                        textRect.X = -textRect.Width / 2;
                        textRect.Y = -textRect.Height / 2;

                        float angle = (float)((360 - obj.Angle) * Math.PI / 180);
                        float sin = (float)Math.Sin(angle);
                        float cos = (float)Math.Cos(angle);
                        float x = GetLeft(obj.AbsLeft + obj.Width / 2);
                        float y = GetTop(obj.AbsTop + obj.Height / 2);
                        // offset the origin to the middle of bounding rectangle, then rotate
                        Result.Append(ExportUtils.FloatToString(cos)).Append(" ").
                            Append(ExportUtils.FloatToString(sin)).Append(" ").
                            Append(ExportUtils.FloatToString(-sin)).Append(" ").
                            Append(ExportUtils.FloatToString(cos)).Append(" ").
                            Append(ExportUtils.FloatToString(x)).Append(" ").
                            Append(ExportUtils.FloatToString(y)).AppendLine(" cm");

                        // apply additional matrix to scale x coordinate
                        if (obj.FontWidthRatio != 1)
                            Result.Append(ExportUtils.FloatToString(obj.FontWidthRatio)).AppendLine(" 0 0 1 0 0 cm");
                    }

                    // break the text to paragraphs, lines, words and runs
                    StringFormat format = obj.GetStringFormat(cache, 0);
                    Brush textBrush = cache.GetBrush(obj.TextColor);
                    AdvancedTextRenderer renderer = new AdvancedTextRenderer(obj.Text, g, f, textBrush,
                        textRect, format, obj.HorzAlign, obj.VertAlign, obj.LineHeight, obj.Angle, obj.FontWidthRatio,
                        obj.ForceJustify, obj.Wysiwyg, obj.HtmlTags, true);
                    float w = f.Height * 0.1f; // to match .net char X offset
                    // invert offset in case of rtl
                    if (obj.RightToLeft)
                      w = -w;
                    // we don't need this offset if text is centered
                    if (obj.HorzAlign == HorzAlign.Center)
                      w = 0;  

                    // render
                    foreach (AdvancedTextRenderer.Paragraph paragraph in renderer.Paragraphs)
                        foreach (AdvancedTextRenderer.Line line in paragraph.Lines)
                        {                            
                            foreach (RectangleF rect in line.Underlines)
                              Result.Append(DrawPDFUnderline(ObjectFontNumber, f, rect.Left, rect.Top, rect.Width, w, obj.TextColor, transformNeeded));
                            foreach (RectangleF rect in line.Strikeouts)
                              Result.Append(DrawPDFStrikeout(ObjectFontNumber, f, rect.Left, rect.Top, rect.Width, w, obj.TextColor, transformNeeded));
                            
                            foreach (AdvancedTextRenderer.Word word in line.Words)
                                if (renderer.HtmlTags)
                                    foreach (AdvancedTextRenderer.Run run in word.Runs)
                                        using (Font fnt = run.GetFont())
                                        {
                                            ObjectFontNumber = GetObjFontNumber(fnt);
                                            AppendFont(Result, ObjectFontNumber, fnt.Size / FDpiFX, run.Style.Color);
                                            AppendText(Result, ObjectFontNumber, fnt, run.Left, run.Top, w, run.Text, obj.RightToLeft, transformNeeded);
                                        }
                                else
                                    AppendText(Result, ObjectFontNumber, f, word.Left, word.Top, w, word.Text, obj.RightToLeft, transformNeeded);
                        }
                }
            }
            Result.AppendLine("Q");
            if (drawBorder)
                Result.Append(GetPDFBorder(obj).ToString());
            Write(outstream, Result.ToString());
        }

        private void AppendUnderlines(StringBuilder Result, TextObject obj)
        {
            float lineHeight = obj.LineHeight == 0 ? obj.Font.GetHeight() : obj.LineHeight;
            lineHeight *= FDpiFX * PDF_DIVIDER;
            float curY = GetTop(obj.AbsTop) - lineHeight;
            float bottom = GetTop(obj.AbsBottom);
            float left = GetLeft(obj.AbsLeft);
            float right = GetLeft(obj.AbsRight);
            float width = obj.Border.Width * PDF_DIVIDER;
            while (curY > bottom)
            {
                Result.Append(DrawPDFLine(left, curY, right, curY, obj.Border.Color, width, LineStyle.Solid, null, null));
                curY -= lineHeight;
            }
        }

        private float GetBaseline(Font f)
        {
            float baselineOffset = f.SizeInPoints / f.FontFamily.GetEmHeight(f.Style) * f.FontFamily.GetCellAscent(f.Style);
            return DrawUtils.ScreenDpi / 72f * baselineOffset;
        }

        private string DrawPDFUnderline(int fontNumber, Font font, float x, float y, float width, float offsX, Color color, bool transformNeeded)
        {
            PDFFont pdfFont = FPageFonts[fontNumber];
            x = (transformNeeded ? x * PDF_DIVIDER : GetLeft(x)) + offsX;
            y = transformNeeded ? -y * PDF_DIVIDER : GetTop(y);
            float factor = PDF_TTF_DIVIDER * font.Size * FDpiFX * PDF_DIVIDER;
            float uh = GetBaseline(font) * PDF_DIVIDER - pdfFont.TextMetric.otmsUnderscorePosition * factor;
            return DrawPDFLine(x, y - uh, x + width * PDF_DIVIDER, y - uh, color, pdfFont.TextMetric.otmsUnderscoreSize * factor, LineStyle.Solid, null, null);
        }

        private string DrawPDFStrikeout(int fontNumber, Font font, float x, float y, float width, float offsX, Color color, bool transformNeeded)
        {
            PDFFont pdfFont = FPageFonts[fontNumber];
            x = (transformNeeded ? x * PDF_DIVIDER : GetLeft(x)) + offsX;
            y = transformNeeded ? -y * PDF_DIVIDER : GetTop(y);
            float factor = PDF_TTF_DIVIDER * font.Size * FDpiFX * PDF_DIVIDER;
            float uh = GetBaseline(font) * PDF_DIVIDER - pdfFont.TextMetric.otmsStrikeoutPosition * factor;
            return DrawPDFLine(x, y - uh, x + width * PDF_DIVIDER, y - uh, color, pdfFont.TextMetric.otmsStrikeoutSize * factor, LineStyle.Solid, null, null);
        }

        private string DrawPDFRect(float left, float top, float right, float bottom, Color color, float borderWidth, LineStyle lineStyle)
        {
            StringBuilder result = new StringBuilder(64);
            result.Append(GetPDFStrokeColor(color));
            result.Append(ExportUtils.FloatToString(borderWidth * PDF_DIVIDER)).AppendLine(" w").AppendLine("2 J");
            result.AppendLine(DrawPDFDash(lineStyle, borderWidth));
            result.Append(ExportUtils.FloatToString(left)).Append(" ").
                AppendLine(ExportUtils.FloatToString(top)).
                Append(ExportUtils.FloatToString(right - left)).Append(" ").
                Append(ExportUtils.FloatToString(bottom - top)).AppendLine(" re").AppendLine("S");
            return result.ToString();
        }

        private string DrawPDFFillRect(float Left, float Top, float Width, float Height, FillBase fill)
        {
            StringBuilder Result = new StringBuilder(128);
            if (fill is SolidFill && (fill as SolidFill).Color != Color.Transparent)
            {
                Result.Append(GetPDFFillColor((fill as SolidFill).Color));
                Result.Append(ExportUtils.FloatToString(Left)).Append(" ").
                    Append(ExportUtils.FloatToString(Top - Height)).Append(" ").
                    Append(ExportUtils.FloatToString(Width)).Append(" ").
                    Append(ExportUtils.FloatToString(Height)).AppendLine(" re");
                Result.AppendLine("f");
            }
            else if (fill is GlassFill)
            {
                Result.Append(GetPDFFillColor((fill as GlassFill).Color));
                Result.Append(ExportUtils.FloatToString(Left)).Append(" ").
                    Append(ExportUtils.FloatToString(Top - Height)).Append(" ").
                    Append(ExportUtils.FloatToString(Width)).Append(" ").
                    Append(ExportUtils.FloatToString(Height / 2)).AppendLine(" re");
                Result.AppendLine("f");
                Color c = (fill as GlassFill).Color;
                c = Color.FromArgb(255, (int)Math.Round(c.R + (255 - c.R) * (fill as GlassFill).Blend),
                    (int)Math.Round(c.G + (255 - c.G) * (fill as GlassFill).Blend),
                    (int)Math.Round(c.B + (255 - c.B) * (fill as GlassFill).Blend));
                Result.Append(GetPDFFillColor(c));
                Result.Append(ExportUtils.FloatToString(Left)).Append(" ").
                    Append(ExportUtils.FloatToString(Top - Height / 2)).Append(" ").
                    Append(ExportUtils.FloatToString(Width)).Append(" ").
                    Append(ExportUtils.FloatToString(Height / 2)).AppendLine(" re");
                Result.AppendLine("f");
            }
            return Result.ToString();
        }

        private string DrawPDFTriangle(float left, float top, float width, float height, Color fillColor, Color borderColor, float borderWidth, LineStyle lineStyle)
        {
            StringBuilder Result = new StringBuilder(128);
            if (fillColor != Color.Transparent)
                Result.Append(GetPDFFillColor(fillColor));            
            if (borderColor != Color.Transparent)
                Result.Append(GetPDFStrokeColor(borderColor));
            Result.Append(ExportUtils.FloatToString(borderWidth * PDF_DIVIDER)).AppendLine(" w").AppendLine("1 J");
            Result.AppendLine(DrawPDFDash(lineStyle, borderWidth));
            Result.Append(ExportUtils.FloatToString(left + width / 2)).Append(" ").Append(ExportUtils.FloatToString(top)).Append(" m ").
                Append(ExportUtils.FloatToString(left + width)).Append(" ").Append(ExportUtils.FloatToString(top - height)).Append(" l ").
                Append(ExportUtils.FloatToString(left)).Append(" ").Append(ExportUtils.FloatToString(top - height)).Append(" l ").
                Append(ExportUtils.FloatToString(left + width / 2)).Append(" ").Append(ExportUtils.FloatToString(top)).AppendLine(" l");
            if (fillColor == Color.Transparent)
                Result.AppendLine("S");
            else
                Result.AppendLine("B");
            return Result.ToString();
        }

        private string DrawPDFDiamond(float left, float top, float width, float height, Color fillColor, Color borderColor, float borderWidth, LineStyle lineStyle)
        {
            StringBuilder Result = new StringBuilder(128);
            if (fillColor != Color.Transparent)
                Result.Append(GetPDFFillColor(fillColor));
            if (borderColor != Color.Transparent)
                Result.Append(GetPDFStrokeColor(borderColor));
            Result.Append(ExportUtils.FloatToString(borderWidth * PDF_DIVIDER)).AppendLine(" w").AppendLine("1 J");
            Result.AppendLine(DrawPDFDash(lineStyle, borderWidth));
            Result.Append(ExportUtils.FloatToString(left + width / 2)).Append(" ").Append(ExportUtils.FloatToString(top)).Append(" m ").
                Append(ExportUtils.FloatToString(left + width)).Append(" ").Append(ExportUtils.FloatToString(top - height / 2)).Append(" l ").
                Append(ExportUtils.FloatToString(left + width / 2)).Append(" ").Append(ExportUtils.FloatToString(top - height)).Append(" l ").
                Append(ExportUtils.FloatToString(left)).Append(" ").Append(ExportUtils.FloatToString(top - height / 2)).Append(" l ").
                Append(ExportUtils.FloatToString(left + width / 2)).Append(" ").Append(ExportUtils.FloatToString(top)).AppendLine(" l");
            if (fillColor == Color.Transparent)
                Result.AppendLine("S");
            else
                Result.AppendLine("B");
            return Result.ToString();
        }

        private string DrawPDFEllipse(float left, float top, float width, float height, Color fillColor, Color borderColor, float borderWidth, LineStyle lineStyle)
        {
            StringBuilder Result = new StringBuilder(128);
            if (fillColor != Color.Transparent)
                Result.Append(GetPDFFillColor(fillColor));
            if (borderColor != Color.Transparent)
                Result.Append(GetPDFStrokeColor(borderColor));
            Result.Append(ExportUtils.FloatToString(borderWidth * PDF_DIVIDER)).AppendLine(" w");
            Result.AppendLine(DrawPDFDash(lineStyle, borderWidth));
            float rx = width / 2;
            float ry = height / 2;
            Result.Append(ExportUtils.FloatToString(left + width)).Append(" ").Append(ExportUtils.FloatToString(top - ry)).AppendLine(" m");
            Result.Append(ExportUtils.FloatToString(left + width)).Append(" ").Append(ExportUtils.FloatToString(top - ry * KAPPA1)).Append(" ").
                Append(ExportUtils.FloatToString(left + rx * KAPPA1)).Append(" ").Append(ExportUtils.FloatToString(top - height)).Append(" ").
                Append(ExportUtils.FloatToString(left + rx)).Append(" ").Append(ExportUtils.FloatToString(top - height)).AppendLine(" c");
            Result.Append(ExportUtils.FloatToString(left + rx * KAPPA2)).Append(" ").Append(ExportUtils.FloatToString(top - height)).Append(" ").
                Append(ExportUtils.FloatToString(left)).Append(" ").Append(ExportUtils.FloatToString(top - ry * KAPPA1)).Append(" ").
                Append(ExportUtils.FloatToString(left)).Append(" ").Append(ExportUtils.FloatToString(top - ry)).AppendLine(" c");
            Result.Append(ExportUtils.FloatToString(left)).Append(" ").Append(ExportUtils.FloatToString(top - ry * KAPPA2)).Append(" ").
                Append(ExportUtils.FloatToString(left + rx * KAPPA2)).Append(" ").Append(ExportUtils.FloatToString(top)).Append(" ").
                Append(ExportUtils.FloatToString(left + rx)).Append(" ").Append(ExportUtils.FloatToString(top)).AppendLine(" c");
            Result.Append(ExportUtils.FloatToString(left + rx * KAPPA1)).Append(" ").Append(ExportUtils.FloatToString(top)).Append(" ").
                Append(ExportUtils.FloatToString(left + width)).Append(" ").Append(ExportUtils.FloatToString(top - ry * KAPPA2)).Append(" ").
                Append(ExportUtils.FloatToString(left + width)).Append(" ").Append(ExportUtils.FloatToString(top - ry)).AppendLine(" c");
            if (fillColor == Color.Transparent)
                Result.AppendLine("S");
            else
                Result.AppendLine("B");
            return Result.ToString();
        }

        private StringBuilder GetPDFBorder(ReportComponentBase obj)
        {
            StringBuilder Result = new StringBuilder(256);
            if (obj.Border.Shadow)
            {
                Result.Append(DrawPDFFillRect(GetLeft(obj.AbsLeft + obj.Width),
                    GetTop(obj.AbsTop + obj.Border.ShadowWidth),
                    obj.Border.ShadowWidth * PDF_DIVIDER, 
                    obj.Height * PDF_DIVIDER,
                    new SolidFill(obj.Border.ShadowColor)));
                Result.Append(DrawPDFFillRect(GetLeft(obj.AbsLeft + obj.Border.ShadowWidth),
                    GetTop(obj.AbsTop + obj.Height),
                    (obj.Width - obj.Border.ShadowWidth) * PDF_DIVIDER,
                    obj.Border.ShadowWidth * PDF_DIVIDER,
                    new SolidFill(obj.Border.ShadowColor)));
            }
            if (obj.Border.Lines != BorderLines.None)
            {
                if (obj.Border.Lines == BorderLines.All && 
                    obj.Border.LeftLine.Equals(obj.Border.RightLine) &&
                    obj.Border.TopLine.Equals(obj.Border.BottomLine) &&
                    obj.Border.LeftLine.Equals(obj.Border.TopLine))

                    Result.Append(DrawPDFRect(GetLeft(obj.AbsLeft), GetTop(obj.AbsTop),
                        GetLeft(obj.AbsLeft + obj.Width), GetTop(obj.AbsTop + obj.Height),
                        obj.Border.Color, obj.Border.Width * PDF_DIVIDER, 
                        obj.Border.Style));
                else
                {
                    float Left = GetLeft(obj.AbsLeft);
                    float Top = GetTop(obj.AbsTop);
                    float Right = GetLeft(obj.AbsLeft + obj.Width);
                    float Bottom = GetTop(obj.AbsTop + obj.Height);
                    if ((obj.Border.Lines & BorderLines.Left) > 0)
                        Result.Append(DrawPDFLine(Left, Top, Left, Bottom, obj.Border.LeftLine.Color,
                            obj.Border.LeftLine.Width * PDF_DIVIDER, obj.Border.LeftLine.Style, null, null));
                    if ((obj.Border.Lines & BorderLines.Right) > 0)
                        Result.Append(DrawPDFLine(Right, Top, Right, Bottom, obj.Border.RightLine.Color,
                            obj.Border.RightLine.Width * PDF_DIVIDER, obj.Border.RightLine.Style, null, null));
                    if ((obj.Border.Lines & BorderLines.Top) > 0)
                        Result.Append(DrawPDFLine(Left, Top, Right, Top, obj.Border.TopLine.Color,
                            obj.Border.TopLine.Width * PDF_DIVIDER, obj.Border.TopLine.Style, null, null));
                    if ((obj.Border.Lines & BorderLines.Bottom) > 0)
                        Result.Append(DrawPDFLine(Left, Bottom, Right, Bottom, obj.Border.BottomLine.Color,
                            obj.Border.BottomLine.Width * PDF_DIVIDER, obj.Border.BottomLine.Style, null, null));
                }
            }
            return Result;
        }

        private string DrawPDFLine(float left, float top, float right, float bottom, Color color, float width, 
            LineStyle lineStyle, CapSettings startCap, CapSettings endCap)
        {
            StringBuilder Result = new StringBuilder(64);
            Result.Append(GetPDFStrokeColor(color));
            Result.Append(ExportUtils.FloatToString(width)).AppendLine(" w").AppendLine("2 J");
            Result.AppendLine(DrawPDFDash(lineStyle, width));
            Result.Append(ExportUtils.FloatToString(left)).Append(" ").
                Append(ExportUtils.FloatToString(top)).AppendLine(" m").
                Append(ExportUtils.FloatToString(right)).Append(" ").
                Append(ExportUtils.FloatToString(bottom)).AppendLine(" l").
                AppendLine("S");
            if (startCap != null && startCap.Style == CapStyle.Arrow)
                Result.Append(DrawArrow(startCap, width, right, bottom, left, top));
            if (endCap != null && endCap.Style == CapStyle.Arrow)
                Result.Append(DrawArrow(endCap, width, left, top, right, bottom));            
            return Result.ToString();
        }
        
        private string DrawArrow(CapSettings Arrow, float lineWidth, float x1, float y1, float x2, float y2)
        {
            float k1, a, b, c, d;
            float xp, yp, x3, y3, x4, y4;
            float wd = Arrow.Width * lineWidth * PDF_DIVIDER;
            float ld = Arrow.Height * lineWidth * PDF_DIVIDER;
            if (Math.Abs(x2 - x1) > 0)
            {
                k1 = (y2 - y1) / (x2 - x1);
                a = (float)Math.Pow(k1, 2) + 1;
                b = 2 * (k1 * ((x2 * y1 - x1 * y2) / (x2 - x1) - y2) - x2);
                c = (float)Math.Pow(x2, 2) + (float)Math.Pow(y2, 2) - (float)Math.Pow(ld, 2) +
                    (float)Math.Pow((x2 * y1 - x1 * y2) / (x2 - x1), 2) -
                    2 * y2 * (x2 * y1 - x1 * y2) / (x2 - x1);
                d = (float)Math.Pow(b, 2) - 4 * a * c;
                xp = (-b + (float)Math.Sqrt(d)) / (2 * a);
                if ((xp > x1) && (xp > x2) || (xp < x1) && (xp < x2))
                    xp = (-b - (float)Math.Sqrt(d)) / (2 * a);
                yp = xp * k1 + (x2 * y1 - x1 * y2) / (x2 - x1);
                if (y2 != y1)
                {
                    x3 = xp + wd * (float)Math.Sin(Math.Atan(k1));
                    y3 = yp - wd * (float)Math.Cos(Math.Atan(k1));
                    x4 = xp - wd * (float)Math.Sin(Math.Atan(k1));
                    y4 = yp + wd * (float)Math.Cos(Math.Atan(k1));
                }
                else
                {
                    x3 = xp; y3 = yp - wd;
                    x4 = xp; y4 = yp + wd;
                }
            }
            else
            {
                xp = x2; yp = y2 - ld;
                if ((yp > y1) && (yp > y2) || (yp < y1) && (yp < y2))
                    yp = y2 + ld;
                x3 = xp - wd; y3 = yp;
                x4 = xp + wd; y4 = yp;
            }
            StringBuilder result = new StringBuilder(64);
            result.AppendLine("2 J").AppendLine("[] 0 d").Append(ExportUtils.FloatToString(x3)).Append(" ").Append(ExportUtils.FloatToString(y3)).AppendLine(" m").
                Append(ExportUtils.FloatToString(x2)).Append(" ").Append(ExportUtils.FloatToString(y2)).AppendLine(" l").
                Append(ExportUtils.FloatToString(x4)).Append(" ").Append(ExportUtils.FloatToString(y4)).AppendLine(" l").AppendLine("S");
            return result.ToString();
        }

        private string DrawPDFDash(LineStyle lineStyle, float lineWidth)
        {
            if (lineStyle == LineStyle.Solid)
                return "[] 0 d";
            else
            {
                string dash = ExportUtils.FloatToString(lineWidth * 2.0f) + " ";
                string dot = ExportUtils.FloatToString(lineWidth * 0.05f) + " ";
                StringBuilder result = new StringBuilder(64);
                if (lineStyle == LineStyle.Dash)
                    result.Append("[").Append(dash).Append("] 0 d");
                else if (lineStyle == LineStyle.DashDot)
                    result.Append("[").Append(dash).Append(dash).Append(dot).Append(dash).Append("] 0 d");
                else if (lineStyle == LineStyle.DashDotDot)
                    result.Append("[").Append(dash).Append(dash).Append(dot).Append(dash).Append(dot).Append(dash).Append("] 0 d");
                else if (lineStyle == LineStyle.Dot)
                    result.Append("[").Append(dot).Append(dash).Append("] 0 d");
                else
                    result.Append("[] 0 d");
                return result.ToString();
            }
        }

        private void AppendText(StringBuilder Result, int fontNumber, Font font, float x, float y, float offsX, string text, bool rtl, bool transformNeeded)
        {
            PDFFont pdffont = FPageFonts[fontNumber];
            x = (transformNeeded ? x * PDF_DIVIDER : GetLeft(x)) + offsX;
            y = transformNeeded ? -y * PDF_DIVIDER : GetTop(y);
            y -= GetBaseline(font) * PDF_DIVIDER;
          
            string s = pdffont.RemapString(text, rtl);
            Result.AppendLine("BT");
            Result.Append(ExportUtils.FloatToString(x)).Append(" ").Append(ExportUtils.FloatToString(y)).AppendLine(" Td");
            Result.Append("<").Append(ExportUtils.StrToHex2(s)).AppendLine("> Tj");
            Result.AppendLine("ET");
        }
        
        private void AppendFont(StringBuilder Result, int fontNumber, float fontSize, Color fontColor)
        {
          PDFFont pdffont = FPageFonts[fontNumber];                
          Result.Append(pdffont.Name).Append(" ").Append(ExportUtils.FloatToString(fontSize)).AppendLine(" Tf");
          Result.Append(GetPDFFillColor(fontColor));
        }

        private string GetPDFFillColor(Color color)
        {
            StringBuilder result = new StringBuilder();
            result.Append(GetPDFFillTransparent(color));
            result.Append(GetPDFColor(color)).AppendLine(" rg");
            return result.ToString();
        }

        private string GetPDFFillTransparent(Color color)
        {
            StringBuilder result = new StringBuilder();
            string value = ExportUtils.FloatToString((float)color.A / 255f);
            int i = FTrasparentStroke.IndexOf(value);
            if (i == -1)
            {
                FTrasparentStroke.Add(value);
                i = FTrasparentStroke.Count - 1;
            }
            result.Append("/GS").Append(i.ToString()).AppendLine("S gs");
            return result.ToString();
        }

        private string GetPDFStrokeColor(Color color)
        {
            StringBuilder result = new StringBuilder();
            result.Append(GetPDFStrokeTransparent(color));
            result.Append(GetPDFColor(color)).AppendLine(" RG");
            return result.ToString();
        }

        private string GetPDFStrokeTransparent(Color color)
        {
            StringBuilder result = new StringBuilder();
            string value = ExportUtils.FloatToString((float)color.A / 255f);
            int i = FTrasparentFill.IndexOf(value);
            if (i == -1)
            {
                FTrasparentFill.Add(value);
                i = FTrasparentFill.Count - 1;
            }
            result.Append("/GS").Append(i.ToString()).AppendLine("F gs");
            return result.ToString();
        }

        private string GetPDFColor(Color color)
        {
            if (color == Color.Black)
                return "0 0 0";
            else if (color == Color.White)
                return "1 1 1";
            else
            {
                StringBuilder sb = new StringBuilder(32);
                sb.Append(ExportUtils.FloatToString((float)color.R / 255f)).Append(" ");
                sb.Append(ExportUtils.FloatToString((float)color.G / 255f)).Append(" ");
                sb.Append(ExportUtils.FloatToString((float)color.B / 255f));
                return sb.ToString();
            }
        }

        private float GetTop(float p)
        {
            return FMarginWoBottom - p * PDF_DIVIDER;
        }

        private float GetLeft(float p)
        {
            return FMarginLeft + p * PDF_DIVIDER;
        }

        private int GetObjFontNumber(Font font)
        {
            int i;
            for (i = 0; i < FPageFonts.Count; i++)
                if (FontEquals(font, FPageFonts[i].SourceFont))
                    break;
            if (i < FPageFonts.Count)
                return i;
            else
            {                
                FPageFonts.Add(GetGlobalFont(font));
                return FPageFonts.Count - 1;
            }
        }

        private PDFFont GetGlobalFont(Font font)
        {
            int i;
            for (i = 0; i < FFonts.Count; i++)
                if (FontEquals(font, FFonts[i].SourceFont))
                    break;
            if (i < FFonts.Count)
                return FFonts[i];
            else
            {
                PDFFont fontitem = new PDFFont(font);
                fontitem.FillOutlineTextMetrix();
                FFonts.Add(fontitem);
                fontitem.Name = "/F" + (FFonts.Count - 1).ToString();
                return fontitem;
            }
        }

        private bool FontEquals(Font font1, Font font2)
        {
            return (font1.Name == font2.Name) && font1.Style.Equals(font2.Style);
        }

        private void AddPDFFooter()
        {
            foreach (PDFFont font in FFonts)
                WriteFont(font);
            FPagesNumber = 1;
            FXRef[0] = pdf.Position;
            WriteLn(pdf, ObjNumber(FPagesNumber));
            WriteLn(pdf, "<<");
            WriteLn(pdf, "/Type /Pages");
            Write(pdf, "/Kids [");
            foreach (long page in FPagesRef)
                Write(pdf, ObjNumberRef(page) + " ");
            WriteLn(pdf, "]");
            WriteLn(pdf, "/Count " + FPagesRef.Count.ToString());
            WriteLn(pdf, ">>");
            WriteLn(pdf, "endobj");
            FInfoNumber = UpdateXRef();
            WriteLn(pdf, ObjNumber(FInfoNumber));
            WriteLn(pdf, "<<");
            WriteLn(pdf, "/Title " + PrepareString(FTitle, FEncKey, FEncrypted, FInfoNumber));
            WriteLn(pdf, "/Author " + PrepareString(FAuthor, FEncKey, FEncrypted, FInfoNumber));
            WriteLn(pdf, "/Subject " + PrepareString(FSubject, FEncKey, FEncrypted, FInfoNumber));
            WriteLn(pdf, "/Keywords " + PrepareString(FKeywords, FEncKey, FEncrypted, FInfoNumber));
            WriteLn(pdf, "/Creator " + PrepareString(FCreator, FEncKey, FEncrypted, FInfoNumber));
            WriteLn(pdf, "/Producer " + PrepareString(FProducer, FEncKey, FEncrypted, FInfoNumber));
            string s = "D:" + DateTime.Now.ToString("yyyyMMddHHmmss");
            if (FEncrypted)
            {
                WriteLn(pdf, "/CreationDate " + PrepareString(s, FEncKey, FEncrypted, FInfoNumber));
                WriteLn(pdf, "/ModDate " + PrepareString(s, FEncKey, FEncrypted, FInfoNumber));
            }
            else
            {
                WriteLn(pdf, "/CreationDate (" + s + ")");
                WriteLn(pdf, "/ModDate (" + s + ")");
            }
            WriteLn(pdf, ">>");
            WriteLn(pdf, "endobj");
            FRootNumber = UpdateXRef();
            WriteLn(pdf, ObjNumber(FRootNumber));
            WriteLn(pdf, "<<");
            WriteLn(pdf, "/Type /Catalog");
            WriteLn(pdf, "/Pages " + ObjNumberRef(FPagesNumber));
            WriteLn(pdf, "/PageMode /UseNone");
            WriteLn(pdf, "/ViewerPreferences <<");

            if (FDisplayDocTitle && !String.IsNullOrEmpty(FTitle))
                WriteLn(pdf, "/DisplayDocTitle true");
            if (FHideToolbar)
                WriteLn(pdf, "/HideToolbar true");
            if (FHideMenubar)
                WriteLn(pdf, "/HideMenubar true");
            if (FHideWindowUI)
                WriteLn(pdf, "/HideWindowUI true");
            if (FFitWindow)
                WriteLn(pdf, "/FitWindow true");
            if (FCenterWindow)
                WriteLn(pdf, "/CenterWindow true");
            if (!FPrintScaling)
                WriteLn(pdf, "/PrintScaling /None");

            WriteLn(pdf, ">>");
            WriteLn(pdf, ">>");
            WriteLn(pdf, "endobj");            
            FStartXRef = pdf.Position;
            WriteLn(pdf, "xref");
            WriteLn(pdf, "0 " + (FXRef.Count + 1).ToString());
            WriteLn(pdf, "0000000000 65535 f");
            foreach (long xref in FXRef)
                WriteLn(pdf, PrepXRefPos(xref) + " 00000 n");
            WriteLn(pdf, "trailer");
            WriteLn(pdf, "<<");
            WriteLn(pdf, "/Size " + (FXRef.Count + 1).ToString());
            WriteLn(pdf, "/Root " + ObjNumberRef(FRootNumber));
            WriteLn(pdf, "/Info " + ObjNumberRef(FInfoNumber));
            WriteLn(pdf, "/ID [<" + FFileID + "><" + FFileID + ">]");
            if (FEncrypted)
            {
                WriteLn(pdf, "/Encrypt <<");
                WriteLn(pdf, "/Filter /Standard");
                WriteLn(pdf, "/V 2");
                WriteLn(pdf, "/R 3");
                WriteLn(pdf, "/Length 128");
                WriteLn(pdf, "/P " + FEncBits.ToString());
                WriteLn(pdf, "/O (" + EscapeSpecialChar(ExportUtils.StringFromByteArray(FOPass)) + ")");
                WriteLn(pdf, "/U (" + EscapeSpecialChar(ExportUtils.StringFromByteArray(FUPass)) + ")");
                WriteLn(pdf, ">>");
            }
            WriteLn(pdf, ">>");
            WriteLn(pdf, "startxref");
            WriteLn(pdf, FStartXRef.ToString());
            WriteLn(pdf, "%%EOF");
        }

        private string PrepXRefPos(long p)
        {
            string pos = p.ToString();
            return new string('0', 10 - pos.Length) + pos;            
        }

        private string ObjNumber(long FNumber)
        {
            StringBuilder sb = new StringBuilder(10);
            sb.Append(FNumber.ToString()).Append(" 0 obj");
            return  sb.ToString();
        }

        private string ObjNumberRef(long FNumber)
        {
            StringBuilder sb = new StringBuilder(8);
            sb.Append(FNumber.ToString()).Append(" 0 R");
            return sb.ToString();
        }

        private long UpdateXRef()
        {
            FXRef.Add(pdf.Position);
            return FXRef.Count;
        }

        #endregion

        #region Protected Methods
        /// <inheritdoc/>
        protected override string GetFileFilter()
        {
            return new MyRes("FileFilters").Get("PdfFile");
        }

        /// <inheritdoc/>
        public override bool ShowDialog()
        {
            using (PDFExportForm form = new PDFExportForm())
            {
                form.Init(this);
                return form.ShowDialog() == DialogResult.OK;
            }
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            FFileID = ExportUtils.MD5(ExportUtils.GetID());
            if (!String.IsNullOrEmpty(FOwnerPassword) || !String.IsNullOrEmpty(FUserPassword))
            {
                FEncrypted = true;
                FEmbeddingFonts = true;
                PrepareKeys();
            }
            if (FBuffered)
                pdf = new MemoryStream();
            else
                pdf = Stream;
            AddPDFHeader();
        }

        /// <inheritdoc/>
        protected override void ExportPage(int pageNo)
        {
            using (ReportPage page = GetPage(pageNo))
                AddPage(page);
        }

        /// <inheritdoc/>
        protected override void Finish()
        {
            AddPDFFooter();
            foreach (PDFFont fnt in FFonts)
                fnt.Cleanup();
            if (FBuffered)
                ((MemoryStream)pdf).WriteTo(Stream);            
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PDFExport"/> class.
        /// </summary>
        public PDFExport()
        {
            FTitle = String.Empty;
            FAuthor = String.Empty;
            FSubject = String.Empty;
            FKeywords = String.Empty;
            FCreator = "FastReport";
            FProducer = "FastReport.NET";
            FOutline = false;
            FDisplayDocTitle = true;
            FHideToolbar = false;
            FHideMenubar = false;
            FHideWindowUI = false;
            FFitWindow = false;
            FCenterWindow = true;
            FPrintScaling = false;
            FUserPassword = String.Empty;
            FOwnerPassword = String.Empty;
            FAllowPrint = true;
            FAllowModify = true;
            FAllowCopy = true;
            FAllowAnnotate = true;
            FXRef = new List<long>();
            FPagesRef = new List<long>();
            FFonts = new List<PDFFont>();
            FPageFonts = new List<PDFFont>();
            FTrasparentStroke = new List<string>();
            FTrasparentFill = new List<string>();
            FDpiFX = 96f / DrawUtils.ScreenDpi;
            FEmbeddingFonts = false;
            FCompressed = true;
            FBuffered = false;
            FBackground = true;
            FPrintOptimized = true;
            FPrintAfterOpen = false;
            FEncrypted = false;
        }
    }
}
