using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Collections;
using FastReport.Utils;
using FastReport.Forms;
using FastReport.Export;
using FastReport.Table;

namespace FastReport.Export.OoXML
{
    /// <summary>
    /// Power point shape
    /// </summary>
    internal class PptShape : OoXMLBase
    {
        const float PPT_DIVIDER = 360000 / 37.8f;

        #region Class overrides
        public override string RelationType 
        { 
            get 
            {
                if (FObject is PictureObject || 
                    FObject is Barcode.BarcodeObject ||
                    FObject is CheckBoxObject ||
                    FObject is ZipCodeObject ||
                    FObject is MSChart.MSChartObject ||
                    FObject is RichObject ||
                    FObject is TextObject ) 
                        return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image";
                throw new Exception(); 
            } 
        }
        public override string ContentType { get { throw new Exception(); } }
        
        public override string FileName 
        { 
            get 
            {
                if (FObject is PictureObject || 
                    FObject is Barcode.BarcodeObject ||
                    FObject is CheckBoxObject ||
                    FObject is ZipCodeObject ||
                    FObject is MSChart.MSChartObject ||
                    FObject is RichObject ||
                    FObject is TextObject) 
                        return Name;
                throw new Exception("Cannot store object as image"); 
            }

        }
        #endregion

        int Id;
        int RelationIdentifier;
        string Name;
        PowerPoint2007Export FPPT_export;

        ulong X;
        ulong Y;
        ulong CX;
        ulong CY;
        bool flipH;
        bool flipV;

        StringBuilder           FTextStrings;
        ReportComponentBase     FObject;

        #region "Private methods"
        private new string Quoted(string p) { return "\"" + p + "\" "; }
        private new string Quoted(long p) { return "\"" + p.ToString() + "\" "; }
        private string QuotedString(ulong a) { return "\"" + a.ToString() + "\" "; }
        private string GetRGBString(Color c)
        {
            return "\"" + /*ExportUtils.ByteToHex(c.A) +*/ ExportUtils.ByteToHex(c.R) + ExportUtils.ByteToHex(c.G) + ExportUtils.ByteToHex(c.B) + "\"";
        }
        private string GetDashType(LineStyle style)
        {
            switch (style)
            {
                case LineStyle.Solid:       return "<a:prstDash val=\"solid\" />";
                case LineStyle.Dot:         return "<a:prstDash val=\"sysDot\" />";
                case LineStyle.Dash:        return "<a:prstDash val=\"sysDash\" />";
                case LineStyle.DashDot:     return "<a:prstDash val=\"sysDashDot\" />";
                case LineStyle.DashDotDot:  return "<a:prstDash val=\"sysDashDotDot\" />";
            }
            throw new Exception("Unsupported dash style");
        }

        private string TranslateText(string text)
        {
            StringBuilder TextStrings = new StringBuilder();
            int start_idx = 0;

            while (true)
            {
                int idx = text.IndexOfAny("&<>".ToCharArray(), start_idx);
                if (idx != -1)
                {
                    TextStrings.Append(text.Substring(start_idx, idx - start_idx));
                    switch (text[idx])
                    {
                        case '&': TextStrings.Append("&amp;"); break;
                        case '<': TextStrings.Append("&lt;"); break;
                        case '>': TextStrings.Append("&gt;"); break;
                    }
                    start_idx = ++idx;
                    continue;
                }
                TextStrings.Append(text.Substring(start_idx));
                break;
            }

            return TextStrings.ToString();
        }

        #endregion

        public int RelationID { get { return RelationIdentifier; } }
        public ReportComponentBase Object { get { return FObject; } }

        #region Constructors

        public PptShape(int Id, int RelationID, string Name, ReportComponentBase obj, PowerPoint2007Export ppt_export)
        {
            this.Id = Id;
            this.RelationIdentifier = RelationID;
            this.Name = Name;
            this.FObject = obj;
            this.FPPT_export = ppt_export;

            this.X = (ulong)(obj.AbsLeft * PPT_DIVIDER + FPPT_export.LeftMargin);
            this.Y = (ulong)(obj.AbsTop * PPT_DIVIDER + FPPT_export.TopMargin); 

            if (obj.Height < 0) 
            { this.CY = (ulong) -(obj.Height * PPT_DIVIDER);  flipV = true; }
            else 
            { this.CY = (ulong)(obj.Height * PPT_DIVIDER); flipV = false; }

            if (obj.Width < 0)
            { this.CX = (ulong)-(obj.Width * PPT_DIVIDER); flipH = true; }
            else
            { this.CX = (ulong)(obj.Width * PPT_DIVIDER); flipH = false; }

            if (CX == 0) CX = 1; // 588;
            if (CY == 0) CY = 1588;

            FTextStrings = new StringBuilder();
        
        }
        #endregion

        private bool Export_Borders(StreamWriter Out, bool rotated)
        {
            const long EMU = 12700;
            Border b = FObject.Border;

            bool same_border = 
                b.Lines == BorderLines.All && 
                (b.BottomLine.Color == b.LeftLine.Color) &&
                (b.BottomLine.Color == b.TopLine.Color) &&
                (b.BottomLine.Color == b.RightLine.Color) &&

                (b.BottomLine.DashStyle == b.LeftLine.DashStyle) &&
                (b.BottomLine.DashStyle == b.TopLine.DashStyle) &&
                (b.BottomLine.DashStyle == b.RightLine.DashStyle) &&

                (b.BottomLine.Width == b.LeftLine.Width) &&
                (b.BottomLine.Width == b.TopLine.Width) &&
                (b.BottomLine.Width == b.RightLine.Width);

            if (FObject is LineObject || (FObject is ShapeObject) /*|| (FObject is TableCell)*/ ||
                ( same_border && ! rotated) )
            {
                ulong bw = (ulong)(EMU * FObject.Border.Width);
                Out.WriteLine("<a:ln w=" + QuotedString(bw) + ">");
                Out.WriteLine("<a:solidFill>");
                Out.WriteLine("<a:srgbClr val=" + GetRGBString(b.Color) + " />");
                Out.WriteLine("</a:solidFill>");
                Out.WriteLine(GetDashType(b.Style));
                if (FObject is LineObject)
                {
                    LineObject line = FObject as LineObject;
                    string StartCap = null;
                    string EndCap = null;

                    switch (line.StartCap.Style)
                    {
                        case CapStyle.Arrow:    StartCap = "arrow";     break;
                        case CapStyle.Circle:   StartCap = "oval";      break;
                        case CapStyle.Diamond:  StartCap = "diamond";   break;
                        case CapStyle.Square:   StartCap = "diamond";   break;
                    }
                    if(StartCap != null) Out.WriteLine("<a:tailEnd type="+Quoted(StartCap)+" />");

                    switch (line.EndCap.Style)
                    {
                        case CapStyle.Arrow:    EndCap = "arrow";   break;
                        case CapStyle.Circle:   EndCap = "oval";    break;
                        case CapStyle.Diamond:  EndCap = "diamond"; break;
                        case CapStyle.Square:   EndCap = "diamond"; break;
                    }
                    if (EndCap != null) Out.WriteLine("<a:headEnd type=" + Quoted(EndCap) + " />");
                }
                Out.WriteLine("</a:ln>");
            }

#if false
            if (FObject.Border.Shadow)
            {
                ulong sw = (ulong)(EMU * FObject.Border.ShadowWidth);
                Out.WriteLine("<a:effectLst>");
                Out.WriteLine("<a:outerShdw blurRad=\"50800\" dist=" + QuotedString(sw) + " dir=\"2700000\" algn=\"tl\" rotWithShape=\"0\">");
                Out.WriteLine("<a:srgbClr val=" + GetRGBString(FObject.Border.ShadowColor) + " >"); 
                Out.WriteLine("<a:alpha val=\"40000\" />");
                Out.WriteLine("</a:srgbClr>");
                Out.WriteLine("</a:outerShdw>");
                Out.WriteLine("</a:effectLst>");
            }
#endif
            return (!same_border || rotated) && (FObject.Border.Lines != BorderLines.None);
        }

        internal void ExportFourBorders(StreamWriter Out)
        {
            Border b = FObject.Border;

            if ((b.Lines & BorderLines.Left) == BorderLines.Left)
            {
                Export_Line(Out, X, Y, 0, CY, b.LeftLine.Color, b.LeftLine.Width, b.LeftLine.Style);
            }
            if ((b.Lines & BorderLines.Bottom) == BorderLines.Bottom)
            {
                Export_Line(Out, X, Y + CY, CX, 0, b.BottomLine.Color, b.BottomLine.Width, b.BottomLine.Style);
            }
            if ((b.Lines & BorderLines.Right) == BorderLines.Right)
            {
                Export_Line(Out, X + CX, Y, 0, CY, b.RightLine.Color, b.RightLine.Width, b.RightLine.Style);
            }
            if ((b.Lines & BorderLines.Top) == BorderLines.Top)
            {
                Export_Line(Out, X, Y, CX, 0, b.TopLine.Color, b.TopLine.Width, b.TopLine.Style);
            }
        }

        private void Export_Fills(StreamWriter Out)
        {
            const long PXA = 60000;
            if (FObject.Fill is LinearGradientFill)
            {
                LinearGradientFill linear = FObject.Fill as LinearGradientFill;
                Out.WriteLine("<a:gradFill flip=\"none\" rotWithShape=\"1\">");
                Out.WriteLine("<a:gsLst>");
                Out.WriteLine("<a:gs pos=\"0\">");
                Out.WriteLine("<a:srgbClr val="+ GetRGBString(linear.StartColor) +" />");
                Out.WriteLine("</a:gs>");
                Out.WriteLine("<a:gs pos=\"100000\">");
                Out.WriteLine("<a:srgbClr val="+ GetRGBString(linear.EndColor) +" />");
                Out.WriteLine("</a:gs>");
                Out.WriteLine("</a:gsLst>");
                Out.WriteLine("<a:lin ang="+Quoted((linear.Angle * PXA).ToString())+" scaled=\"1\" />");
                Out.WriteLine("<a:tileRect />");
                Out.WriteLine("</a:gradFill>");
            }
            else if (FObject.Fill is SolidFill)
            {
                SolidFill fill = FObject.Fill as SolidFill;
                if (fill.Color.A == 0)
                {
                    Out.WriteLine("<a:noFill />");
                }
                else
                {
                    Out.WriteLine("<a:solidFill>");
                    Out.WriteLine("<a:srgbClr val=" + GetRGBString(fill.Color) + " />");
                    Out.WriteLine("</a:solidFill>");
                }
            }
            else if (FObject.Fill is GlassFill)
            { 
                GlassFill fill = FObject.Fill as GlassFill;
                Out.WriteLine("<a:gradFill flip=\"none\" rotWithShape=\"1\">");
                Out.WriteLine("<a:gsLst>");
                Out.WriteLine("<a:gs pos=\"0\">");
                Out.WriteLine("<a:srgbClr val="+ GetRGBString(fill.Color) +">");
                Out.WriteLine("<a:alpha val=\"50000\" />");
                Out.WriteLine("</a:srgbClr>");
                Out.WriteLine("</a:gs>");
                Out.WriteLine("<a:gs pos=\"50000\">");
                Out.WriteLine("<a:srgbClr val="+ GetRGBString(fill.Color) +">");
                Out.WriteLine("<a:alpha val=\"50000\" />");
                Out.WriteLine("</a:srgbClr>");
                Out.WriteLine("</a:gs>");
                Out.WriteLine("<a:gs pos=\"50001\">");
                Out.WriteLine("<a:srgbClr val="+ GetRGBString(fill.Color) +" />");
                Out.WriteLine("</a:gs>");
                Out.WriteLine("<a:gs pos=\"100000\">");
                Out.WriteLine("<a:srgbClr val="+ GetRGBString(fill.Color) +" />");
                Out.WriteLine("</a:gs>");
                Out.WriteLine("</a:gsLst>");
                Out.WriteLine("<a:lin ang=" + Quoted(5400000) + " scaled=\"1\" />");
                Out.WriteLine("<a:tileRect />");
                Out.WriteLine("</a:gradFill>");
            }
            else if(FObject.Fill is HatchFill)
            {
                Out.WriteLine("<a:blipFill dpi=\"0\" rotWithShape=\"1\">");
                Out.WriteLine("<a:blip r:embed=" + Quoted(rId) + " cstate=\"print\" />");
                Out.WriteLine("<a:srcRect />");
                Out.WriteLine("<a:stretch>");
                Out.WriteLine("<a:fillRect />");
                Out.WriteLine("</a:stretch>");
                Out.WriteLine("</a:blipFill>");

            }
            else
            {
                throw new Exception("Unknown fill");
            }

        }

        private string Get_Anchor()
        {
            if(FObject is TextObject) switch ( (FObject as TextObject).VertAlign )
            {
                case VertAlign.Top:     return "\"t\"";
                case VertAlign.Center:  return "\"ctr\"";
                case VertAlign.Bottom:  return "\"b\"";
            }
            if (FObject is TableBase) return "\"ctr\"";
            if (FObject is ShapeObject) return "\"ctr\"";

            throw new Exception("Bad vertical align");
        }

        internal bool Export_XFRM(StreamWriter Out, char Ch)
        {
            bool swap = false;
            bool rotated = false;
            // <a:xfrm>
            if (FObject is LineObject && (FObject as LineObject).Diagonal )
            {
                    if (flipV == true) Y = Y - CY;
                    if (flipH == true) X = X - CX;
                    Out.WriteLine("<" + Ch + ":xfrm flipH=" + Quoted(flipH ? "0" : "1") + " flipV=" + Quoted(flipV ? "0" : "1") + " >");
            }
            else if (FObject is TextObject)
            {
                TextObject obj = FObject as TextObject;
                long dy = ((long)(CY - CX)) / 2;

                switch (obj.Angle)
                { 
                    case 0:
                        Out.WriteLine("<" + Ch + ":xfrm>");
                        break;
                    case 90:
                        Y = (ulong)((long)Y + dy);
                        X = (ulong)((long)X - dy);
                        Out.WriteLine("<" + Ch + ":xfrm rot=\"5400000\">");
                        swap = true;
                        break;
                    case 180:
                        Out.WriteLine("<" + Ch + ":xfrm rot=\"10800000\">");
                        break;
                    case 270:
                        Y = (ulong)((long)Y + dy);
                        X = (ulong)((long)X - dy);
                        Out.WriteLine("<" + Ch + ":xfrm rot=\"16200000\">");
                        swap = true;
                        break;
                    default:
                        Out.WriteLine("<" + Ch + ":xfrm rot=\"" + 60000*obj.Angle + "\">");
                        rotated = true;
                        break;
                }
            }
            else
                Out.WriteLine("<" + Ch + ":xfrm>");

            Out.WriteLine("<a:off x=" + QuotedString(X) + " y=" + QuotedString(Y) + " />");
            if( swap )
                Out.WriteLine("<a:ext cx=" + QuotedString(CY) + " cy=" + QuotedString(CX) + " />");
            else
                Out.WriteLine("<a:ext cx=" + QuotedString(CX) + " cy=" + QuotedString(CY) + " />");

            Out.WriteLine("</"+Ch+":xfrm>");

            return rotated;
        }

        internal bool Export_spPr(StreamWriter Out, string PresetGeometry)
        {
            bool do_borders = false;
            bool rotated;

            if (CX != 0 && CY != 0)
            {
                Out.WriteLine("<p:spPr>");

                rotated = Export_XFRM(Out, 'a');

                if (PresetGeometry != null)
                {
                    Out.WriteLine("<a:prstGeom prst=" + Quoted(PresetGeometry) + ">");
                    Out.WriteLine("<a:avLst />");
                    Out.WriteLine("</a:prstGeom>");
                }

                Export_Fills(Out);

                do_borders = Export_Borders(Out, rotated);

                if (do_borders)
                {
                    float left, right, top, bottom;

                    if ((FObject.Border.Lines & BorderLines.Left) == BorderLines.Left)
                        left = FObject.Border.LeftLine.Width * PPT_DIVIDER;
                    else left = 0;
                    if ((FObject.Border.Lines & BorderLines.Top) == BorderLines.Top)
                        top = FObject.Border.TopLine.Width * PPT_DIVIDER;
                    else top = 0;
                    if ((FObject.Border.Lines & BorderLines.Right) == BorderLines.Right)
                        right = FObject.Border.RightLine.Width * PPT_DIVIDER;
                    else right = 0;
                    if ((FObject.Border.Lines & BorderLines.Bottom) == BorderLines.Bottom)
                        bottom = FObject.Border.BottomLine.Width * PPT_DIVIDER;
                    else bottom = 0;

                    //X += (ulong)left;
                    //Y += (ulong)top;
                    //CX -= (ulong)(left + right);
                    //CY -= (ulong)(top + bottom);
                }

                Out.WriteLine("</p:spPr>");
            }
            else
                Out.WriteLine("<p:spPr />");

            return do_borders;
        }

        #region "Export Non-Visual prroperties"
        internal void Export_nvPicPr(StreamWriter Out, int PicCount)
        {
            Out.WriteLine("<p:nvPicPr>");
            Out.WriteLine("<p:cNvPr id=" + Quoted(Id.ToString()) +
                " name=" + Quoted("Picture" + Id) + " descr=" + Quoted("image" + PicCount.ToString() + ".png") + " />");
            Out.WriteLine("<p:cNvPicPr>");
            Out.WriteLine("<a:picLocks noChangeAspect=\"1\" />");
            Out.WriteLine("</p:cNvPicPr>");
            Out.WriteLine("<p:nvPr />");
            Out.WriteLine("</p:nvPicPr>");
        }
        internal void Export_nvSpPr(StreamWriter Out)
        {
            Out.WriteLine("<p:nvSpPr>");
            Out.WriteLine("<p:cNvPr id=" + Quoted(Id.ToString()) + " name=" + Quoted(Name) + " />");
            Out.WriteLine("<p:cNvSpPr>");
            Out.WriteLine("<a:spLocks noGrp=" + Quoted("1") + " />");  // fix me
            Out.WriteLine("</p:cNvSpPr>");
            Out.WriteLine("<p:nvPr>");

            // Out placeholder
            Out.Write("<p:ph ");
            Out.Write("/>");

            Out.WriteLine("</p:nvPr>");
            Out.WriteLine("</p:nvSpPr>");
        }
        internal void Export_nvGraphicFramePr(StreamWriter Out)
        { 
            Out.Write("<p:nvGraphicFramePr>");
            Out.Write("<p:cNvPr id="+ Quoted(Id.ToString()) +" name="+Quoted(Name)+" />");
            Out.Write("<p:cNvGraphicFramePr>");
            Out.Write("<a:graphicFrameLocks noGrp=\"1\" />");
            Out.Write("</p:cNvGraphicFramePr>");
            Out.Write("<p:nvPr />");
            Out.Write("</p:nvGraphicFramePr>");
        }
        internal void Export_nvCxnSpPr(StreamWriter Out)
        {
            Out.WriteLine("<p:nvCxnSpPr>");
            Out.WriteLine("<p:cNvPr id=" + Quoted(Id.ToString()) + " name=" + Quoted(Name) + " />");
            Out.WriteLine("<p:cNvCxnSpPr />");
            Out.WriteLine("<p:nvPr />");
            Out.WriteLine("</p:nvCxnSpPr>");
        }
        #endregion

        internal void Export_blipFill(StreamWriter Out)
        {
            Out.WriteLine("<p:blipFill>");
            Out.WriteLine("<a:blip r:embed=" + Quoted("rId" + this.RelationID.ToString()) + " cstate=\"print\" />");
            Out.WriteLine("<a:stretch>");
            Out.WriteLine("<a:fillRect />");
            Out.WriteLine("</a:stretch>");
            Out.WriteLine("</p:blipFill>");
        }


        internal void Open_Paragraph()
        {
            string align = "ctr";
            TextObject text_obj = (FObject is TextObject) ? FObject as TextObject : null;

            if (FObject is TextObject) switch (text_obj.HorzAlign)
                {
                    case HorzAlign.Left:    align ="l"; break;
                    case HorzAlign.Right:   align = "r";  break;
                    case HorzAlign.Center:  align ="ctr"; break;
                    case HorzAlign.Justify: align ="just"; break;
                }
            
            FTextStrings.AppendLine("<a:p><a:pPr algn=" + Quoted(align) + " />"); 
        }

        internal void Add_Run(
            Font                Font,
            Color               TextColor,
            string              Text
            )
        {
            long                Size = (long) (Font.Size * 100);
            bool                Italic = Font.Italic;
            bool                Underline = Font.Underline;

            if (Text != null)
            {
                FTextStrings.AppendLine("<a:r>");
                FTextStrings.AppendLine("<a:rPr lang=\"en-US\" sz=" + Quoted(Size) +
                    "b=" + Quoted(Font.Bold ? "1" : "0") +
                    "i=" + Quoted(Font.Italic ? "1" : "0") +
                    (Font.Underline ? ("u=" + Quoted("sng")) : "") +
                    " smtClean=\"0\" >");
                FTextStrings.AppendLine("<a:solidFill><a:srgbClr val=" + GetRGBString(TextColor) + " /></a:solidFill>");
                FTextStrings.AppendLine("<a:latin typeface=" + Quoted(Font.Name) + /*"pitchFamily=" + Quoted( "22" ) +*/ " />");
                FTextStrings.AppendLine("</a:rPr>");

                FTextStrings.AppendLine("<a:t>" + this.TranslateText(Text) + "</a:t>");
                FTextStrings.AppendLine("</a:r>");
            }

            FTextStrings.AppendLine("<a:r><a:rPr lang=\"en-US\" sz=" + Quoted(Size) + "></a:rPr><a:t> </a:t></a:r>");
        }

        internal void Close_Paragraph()
        {
            FTextStrings.AppendLine("</a:p>");
        }

        internal void Export_txBody(StreamWriter Out)
        {
            Out.WriteLine("<p:txBody>");

            Out.WriteLine("<a:bodyPr vert=\"horz\" lIns=\"45720\" tIns=\"22860\" rIns=\"45720\" bIns=\"22860\" rtlCol=\"0\" anchor=" + Get_Anchor() + ">");
            
            Out.WriteLine("<a:normAutofit />");
            Out.WriteLine("</a:bodyPr>");
            Out.WriteLine("<a:lstStyle />");

            Out.WriteLine(FTextStrings.ToString());
            
            Out.WriteLine("</p:txBody>");
        }

        internal void ResetText()
        {
            FTextStrings = null;
            FTextStrings = new StringBuilder();
        }

        internal void MoveObject(ReportComponentBase obj)
        {
            this.FObject = obj;
            this.X = (ulong)(obj.AbsLeft * PPT_DIVIDER + FPPT_export.LeftMargin);
            this.Y = (ulong)(obj.AbsTop * PPT_DIVIDER + FPPT_export.TopMargin);
            this.CX = (ulong)(obj.Width * PPT_DIVIDER);

            if (obj.Height < 0)
            { this.CY = (ulong)-(obj.Height * PPT_DIVIDER); flipV = true; }
            else { this.CY = (ulong)(obj.Height * PPT_DIVIDER); flipV = false; }

            if (obj.Width < 0)
            { this.CX = (ulong)-(obj.Width * PPT_DIVIDER); flipH = true; }
            else
            { this.CX = (ulong)(obj.Width * PPT_DIVIDER); flipH = false; }

            if (CX == 0) CX = 1588;
            if (CY == 0) CY = 1588;
        }

        private void Export_Line(StreamWriter Out, ulong x, ulong y, ulong dx, ulong dy, Color LineColor, float width, LineStyle style )
        {
            Out.WriteLine("<p:cxnSp>");
            Out.WriteLine("<p:nvCxnSpPr><p:cNvPr id=\"62\" name=\"Straight Connector 61\" /><p:cNvCxnSpPr />");
            Out.WriteLine("<p:nvPr />");
            Out.WriteLine("</p:nvCxnSpPr>");
            Out.WriteLine("<p:spPr>");
            
            Out.WriteLine("<a:xfrm>"+
                "<a:off x=" + Quoted(x.ToString()) + " y=" + Quoted(y.ToString()) + " />" + 
                "<a:ext cx=" + Quoted(dx.ToString()) + " cy="+Quoted(dy.ToString())+" />"+
                "</a:xfrm>");

            width *= 12700;

            Out.WriteLine("<a:prstGeom prst=\"line\"><a:avLst /> </a:prstGeom>");
            Out.WriteLine("<a:ln w="+Quoted(width.ToString())+"><a:solidFill>");
            Out.WriteLine("<a:srgbClr val=" + GetRGBString(LineColor) + " />");
            Out.WriteLine("</a:solidFill>" + GetDashType(style) + "</a:ln></p:spPr>");
            Out.WriteLine("<p:style><a:lnRef idx=\"1\"><a:schemeClr val=\"accent1\" /></a:lnRef><a:fillRef idx=\"0\">");
            Out.WriteLine("<a:schemeClr val=\"accent1\" /></a:fillRef><a:effectRef idx=\"0\"><a:schemeClr val=\"accent1\" /></a:effectRef>");
            Out.WriteLine("<a:fontRef idx=\"minor\"><a:schemeClr val=\"tx1\" /></a:fontRef></p:style></p:cxnSp>");
        }
        
        internal void Export_Shadow(StreamWriter Out)
        { 
            ulong x, y, cx, cy;

            x = (ulong)((Object.AbsLeft + Object.Border.ShadowWidth /*- 1*/) * PPT_DIVIDER + this.FPPT_export.LeftMargin);
            y = (ulong)((Object.AbsBottom + Object.Border.ShadowWidth / 2) * PPT_DIVIDER + this.FPPT_export.TopMargin);
            cx = (ulong)(Object.Width * PPT_DIVIDER);
            Export_Line(Out, x, y, cx, 0, Object.Border.ShadowColor, Object.Border.ShadowWidth, LineStyle.Solid );

            x = (ulong)((Object.AbsRight + Object.Border.ShadowWidth / 2) * PPT_DIVIDER + this.FPPT_export.LeftMargin);
            y = (ulong)((Object.AbsTop + Object.Border.ShadowWidth /*- 1*/) * PPT_DIVIDER + this.FPPT_export.TopMargin);
            cy = (ulong)(Object.Height * PPT_DIVIDER);
            Export_Line(Out, x, y, 0, cy, Object.Border.ShadowColor, Object.Border.ShadowWidth, LineStyle.Solid);
        }
    };

    /// <summary>
    /// Power Point Layout Descriptor
    /// </summary>
    internal class PptLayoutDescriptor
    {
        public string Name;
        public string Type;
        public PptShape[] Shapes;

        public PptLayoutDescriptor(string Type, string Name, PptShape[] Shapes) 
        {
            this.Type = Type;
            this.Name = Name;
            this.Shapes = Shapes;
        }
    }

    /// <summary>
    /// Power Point base class for style element
    /// </summary>
    internal class PptStyleBase
    {
        private uint FLevel = 0;
        private long marL=342900;
        private long indent = -342900;
        private char algn = 'l';
        private long defTabSz = 914400;
        private long rtl = 0;
        private long eaLnBrk = 1;
        private long latinLnBrk = 0;
        private long hangingPunct = 1;

        private string Quoted(long v) { return "\"" + v.ToString() + "\" "; }
        private string Quoted(char v) { return "\"" + v + "\" "; }
        private string Quoted(string v) { return "\"" + v + "\" "; }

        internal void Export(StreamWriter Out)
        { 
            Out.WriteLine("<a:lvl" + FLevel.ToString() + "pPr ");
            Out.WriteLine("marL=" + Quoted(marL));
            Out.WriteLine("indent=" + Quoted(indent));
            Out.WriteLine("algn=" + Quoted(algn));
            Out.WriteLine("defTabSz=" + Quoted(defTabSz));
            Out.WriteLine("rtl=" + Quoted(rtl));
            Out.WriteLine("eaLnBrk=" + Quoted(eaLnBrk));
            Out.WriteLine("latinLnBrk=" + Quoted(latinLnBrk));
            Out.WriteLine("hangingPunct=" + Quoted(hangingPunct));
            Out.WriteLine(">");

            Out.WriteLine("<a:spcBef>");
            Out.WriteLine("<a:spcPct val=" + Quoted("0") + "/>");
            Out.WriteLine("</a:spcBef>");
            
            Out.WriteLine("<a:buNone />");

            Out.WriteLine("<a:defRPr sz=" + Quoted("4400") + "kern=" + Quoted("1200") + ">");
            Out.WriteLine("<a:solidFill>");
            Out.WriteLine("<a:schemeClr val=" + Quoted("tx1") + "/>");
            Out.WriteLine("</a:solidFill>");
            Out.WriteLine("<a:latin typeface="  + Quoted("+mj-lt") + "/>");
            Out.WriteLine("<a:ea typeface="     + Quoted("+mj-ea") + "/>");
            Out.WriteLine("<a:cs typeface="     + Quoted("+mj-cs") + "/>");
            Out.WriteLine("</a:defRPr>");

            Out.WriteLine("</a:lvl" + FLevel.ToString() + "pPr>");
        }

        internal PptStyleBase(long MarL)
        {
            this.marL = MarL;
        }
    }

    /// <summary>
    /// Base class for styles group
    /// </summary>
    internal class PptStyleGroupBase
    {
        private ArrayList FStyleGroup = new ArrayList();

        internal void Export(StreamWriter Out)
        {
            foreach (PptStyleBase style in FStyleGroup)
            {
                style.Export(Out);
            }
        }

        internal void AddStyle(PptStyleBase style)
        {
            FStyleGroup.Add(style);
        }
    }

    /// <summary>
    /// Base class for slides, masters, and layouts
    /// </summary>
    internal abstract class OoSlideBase : OoXMLBase
    {
        protected static ulong FStaticSlideMasterId = 2147483648;

        protected ulong FSlideMasterId;
        
        protected void ExportShape(StreamWriter Out, PptShape shape, string ShapeType)
        {
            bool do_borders;
            Out.WriteLine("<p:sp>");
            shape.Export_nvSpPr(Out);
            do_borders = shape.Export_spPr(Out, ShapeType);
            shape.Export_txBody(Out);
            Out.WriteLine("</p:sp>");

            if (shape.Object.Border.Shadow)
            {
                shape.Export_Shadow(Out);
            }

            if (do_borders == true)
            {
                shape.ExportFourBorders(Out);
            }
        }

        protected void ExportColorMapOverride(StreamWriter Out)
        {
            Out.WriteLine("<p:clrMapOvr>");
            Out.WriteLine("<a:masterClrMapping />");
            Out.WriteLine("</p:clrMapOvr>");
        }

        protected void ExportShapeTree(StreamWriter Out, PptShape[] shape_list)
        {
            Out.WriteLine("<p:spTree>");

            Out.WriteLine("<p:nvGrpSpPr>");
            Out.WriteLine("<p:cNvPr id=\"1\" name=\"\" />");
            Out.WriteLine("<p:cNvGrpSpPr />");
            Out.WriteLine("<p:nvPr />");
            Out.WriteLine("</p:nvGrpSpPr>");

            Out.WriteLine("<p:grpSpPr>");
            Out.WriteLine("<a:xfrm>");
            Out.WriteLine("<a:off x=\"0\" y=\"0\" />");
            Out.WriteLine("<a:ext cx=\"0\" cy=\"0\" />");
            Out.WriteLine("<a:chOff x=\"0\" y=\"0\" />");
            Out.WriteLine("<a:chExt cx=\"0\" cy=\"0\" />");
            Out.WriteLine("</a:xfrm>");
            Out.WriteLine("</p:grpSpPr>");

#if false
            for (int i = 0; i < shape_list.Length; i++)
            {
                ExportShape(Out, shape_list[i]);
            }
#endif

            Out.WriteLine("</p:spTree>");
        }

        protected void ExportSlideBackground(StreamWriter Out)
        {
            Out.WriteLine("<p:bg>");
            Out.WriteLine("<p:bgRef idx=\"1001\">");
            Out.WriteLine("<a:schemeClr val=\"bg1\" />");
            Out.WriteLine("</p:bgRef>");
            Out.WriteLine("</p:bg>");
        }

        internal ulong SlideMasterId { get { return FSlideMasterId; } }

        internal OoSlideBase()
        {
            FSlideMasterId = FStaticSlideMasterId;
            FStaticSlideMasterId++;
        }
    }

    /// <summary>
    /// Slide masters object
    /// </summary>
    internal class OoPptSlideMaster : OoSlideBase
    {
        #region Class overrides
        public override string RelationType { get { return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideMaster"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml"; } }
        public override string FileName { get { return "ppt/slideMasters/slideMaster1.xml"; } }
        #endregion

        #region Private fileds
//        private ArrayList FLayoutList = new ArrayList();
        #endregion

        #region Private methods
//        private string Quoted(long v) { return "\"" + v.ToString() + "\" "; }

        private void ExportLayoutIDList( StreamWriter Out, ArrayList LayoutList )
        {
            Out.WriteLine("<p:sldLayoutIdLst>");

            foreach (OoSlideBase layout_item in LayoutList)
            {
                Out.WriteLine("<p:sldLayoutId id=" + Quoted(layout_item.SlideMasterId.ToString()) + "r:id=" + Quoted(layout_item.rId) + "/>");
            }

            Out.WriteLine("</p:sldLayoutIdLst>");
        }

        private void ExportTitleStyles(StreamWriter Out)
        {
            Out.WriteLine("<p:titleStyle>");
#if false
            Out.WriteLine("<a:lvl1pPr algn=\"ctr\" defTabSz=\"914400\" rtl=\"0\" eaLnBrk=\"1\" latinLnBrk=\"0\" hangingPunct=\"1\">");
            Out.WriteLine("<a:spcBef>");
            Out.WriteLine("<a:spcPct val=\"0\" />");
            Out.WriteLine("</a:spcBef>");
            Out.WriteLine("<a:buNone />");
            Out.WriteLine("<a:defRPr sz=\"4400\" kern=\"1200\">");
            Out.WriteLine("<a:solidFill>");
            Out.WriteLine("<a:schemeClr val=\"tx1\" />");
            Out.WriteLine("</a:solidFill>");
            Out.WriteLine("<a:latin typeface=\"+mj-lt\" />");
            Out.WriteLine("<a:ea typeface=\"+mj-ea\" />");
            Out.WriteLine("<a:cs typeface=\"+mj-cs\" />");
            Out.WriteLine("</a:defRPr>");
            Out.WriteLine("</a:lvl1pPr>");
#endif
            Out.WriteLine("</p:titleStyle>");
        }

        private void ExportBodyStyles(StreamWriter Out)
        {
            Out.WriteLine("<p:bodyStyle>");
#if false
            for (int i = 0; i < diffs.Length; i++)
            {
                Out.WriteLine("<a:lvl" + (1 + i) + "pPr marL=" + Quoted(diffs[i].MarL) + "indent=" + Quoted(diffs[i].ident) + "algn=\"l\" defTabSz=\"914400\" rtl=\"0\" eaLnBrk=\"1\" latinLnBrk=\"0\" hangingPunct=\"1\">");
                Out.WriteLine("<a:spcBef>");
                Out.WriteLine("<a:spcPct val=\"20000\" />");
                Out.WriteLine("</a:spcBef>");
                Out.WriteLine("<a:buFont typeface=\"Arial\" pitchFamily=\"34\" charset=\"0\" />");
                Out.WriteLine("<a:buChar char=" + Quoted(diffs[i].buChar) + " />");
                Out.WriteLine("<a:defRPr sz=" + Quoted(diffs[i].sz) + " kern=\"1200\">");
                Out.WriteLine("<a:solidFill>");
                Out.WriteLine("<a:schemeClr val=\"tx1\" />");
                Out.WriteLine("</a:solidFill>");
                Out.WriteLine("<a:latin typeface=\"+mn-lt\" />");
                Out.WriteLine("<a:ea typeface=\"+mn-ea\" />");
                Out.WriteLine("<a:cs typeface=\"+mn-cs\" />");
                Out.WriteLine("</a:defRPr>");
                Out.WriteLine("</a:lvl" + (1 + i) + "pPr>");
            }
#endif
            Out.WriteLine("</p:bodyStyle>");
        }

        private void ExportOtherStyles(StreamWriter Out)
        {
            Out.WriteLine("<p:otherStyle>");
            Out.WriteLine("<a:defPPr>");
            Out.WriteLine("<a:defRPr lang=\"en-US\" />");
            Out.WriteLine("</a:defPPr>");
#if false
            for (int i = 0; i < MarL.Length; i++)
            {
                Out.WriteLine("<a:lvl" + (1 + i) + "pPr marL=" + MarL[i] + " algn=\"l\" defTabSz=\"914400\" rtl=\"0\" eaLnBrk=\"1\" latinLnBrk=\"0\" hangingPunct=\"1\">");
                Out.WriteLine("<a:defRPr sz=\"1800\" kern=\"1200\">");
                Out.WriteLine("<a:solidFill>");
                Out.WriteLine("<a:schemeClr val=\"tx1\" />");
                Out.WriteLine("</a:solidFill>");
                Out.WriteLine("<a:latin typeface=\"+mn-lt\" />");
                Out.WriteLine("<a:ea typeface=\"+mn-ea\" />");
                Out.WriteLine("<a:cs typeface=\"+mn-cs\" />");
                Out.WriteLine("</a:defRPr>");
                Out.WriteLine("</a:lvl" + (1 + i) + "pPr>");
            }
#endif
            Out.WriteLine("</p:otherStyle>");
        }

        #endregion

        #region Internal methods
        internal void Export(PowerPoint2007Export OoXML)
        {
            ExportRelations(OoXML);

            using (FileStream file = new FileStream(OoXML.TempFolder + "/" + FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine("<p:sldMaster xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" xmlns:p=\"http://schemas.openxmlformats.org/presentationml/2006/main\">");
                Out.WriteLine("<p:cSld>");
                ExportSlideBackground(Out);
                ExportShapeTree( Out, null /*master_shapes*/ );
                Out.WriteLine("</p:cSld>");
                Out.WriteLine("<p:clrMap bg1=\"lt1\" tx1=\"dk1\" bg2=\"lt2\" tx2=\"dk2\" accent1=\"accent1\" accent2=\"accent2\" accent3=\"accent3\" accent4=\"accent4\" accent5=\"accent5\" accent6=\"accent6\" hlink=\"hlink\" folHlink=\"folHlink\" />");
                ExportLayoutIDList( Out, OoXML.SlideLayoutList );

                Out.WriteLine("<p:txStyles>");
                ExportTitleStyles(Out);
                ExportBodyStyles(Out);
                ExportOtherStyles(Out);
                Out.WriteLine("</p:txStyles>");

                Out.WriteLine("</p:sldMaster>");
            }
        }
        #endregion

    }

    /// <summary>
    /// Ordinaty slide 
    /// </summary>
    internal class OoPptSlide : OoSlideBase
    {
//        static int FPicCount = 0;
        static int FSlideCount = 0;
        static int FSlideIDCount = 256;
        static int id = 1; // Fix it

        public Dictionary<string, PptShape> CheckboxList = new Dictionary<string, PptShape>();

        const float PPT_DIVIDER = 360000 / 37.8f; 
        const int NO_RELATION = 0;

        #region "Class overrides"
        public override string RelationType { get { return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-officedocument.presentationml.slide+xml"; } }
        public override string FileName { get { return "ppt/slides/slide" + FSlideNumber +".xml"; } }
        #endregion
    
        #region "Private fields"
        private int FSlideNumber;
        private int FSlideID;
        private float printZoom = 1;
        private PowerPoint2007Export FPPT_export;
        #endregion

        #region "Public properties"
        public int SlideID { get { return FSlideID; } }
        public int PictureCount { get { return FPPT_export.PictureCount; } set { FPPT_export.PictureCount = value; } }
        //public OoPptPresentation Presentation { get { return FPresentation; } }
        #endregion

        #region "Private methods"
        private ulong GetTop(float p)
        {
            return (ulong) ( /* FMarginWoBottom - */ p * PPT_DIVIDER );
        }

        private ulong GetLeft(float p)
        {
            return (ulong) ( /* FMarginLeft + */ p * PPT_DIVIDER );
        }
        #endregion


        // Constructor
        internal OoPptSlide(PowerPoint2007Export ppt_export)
        {
            FPPT_export = ppt_export;
            FSlideNumber = ++FSlideCount;
            FSlideID = ++FSlideIDCount;
        }

        internal void Reset()
        {
            FSlideCount = 1;
            FSlideNumber = 1;

            FSlideID = FSlideIDCount = 256;
            FStaticSlideMasterId = 2147483648;
        }

        private void AddBandObject(StreamWriter outstream, BandBase band)
        {
            if (band.HasBorder || band.HasFill ) using (TextObject newObj = new TextObject())
            {
                newObj.Left = band.AbsLeft;
                newObj.Top = band.AbsTop;
                newObj.Width = band.Width;
                newObj.Height = band.Height;
                newObj.Fill = band.Fill;
                newObj.Border = band.Border;
                newObj.Text = "";
                AddTextObject(outstream, 0, newObj);
            }
        }

        private void AddShape(StreamWriter Out, ShapeObject shape_object)
        {
            string shape_name;

            PptShape shape = new PptShape(id + 1, NO_RELATION, shape_object.ToString() + " " + id, shape_object, this.FPPT_export);

            switch (shape_object.Shape)
            {
                case ShapeKind.Diamond: shape_name = "diamond"; break;
                case ShapeKind.Ellipse: shape_name = "ellipse"; break;
                case ShapeKind.Rectangle: shape_name = "rect"; break;
                case ShapeKind.RoundRectangle: shape_name = "roundRect"; break;
                case ShapeKind.Triangle: shape_name = "triangle"; break;
                default: throw new Exception("Unsupported shape kind");
            }

            using (Font f = new Font("system", 8))
            {
                // append epmty space to avoid anoying PPt notification
                shape.Open_Paragraph();
                shape.Add_Run(f, shape_object.FillColor, null);
                shape.Close_Paragraph();
            }

            ExportShape(Out, shape, shape_name);
            id++;
        }

        private void AddTextObject(StreamWriter outstream, int nRelationID, TextObject obj)
        {
            PptShape shape;
            if (obj.Fill is HatchFill)
            {
                shape = SaveImage(obj, nRelationID, "ppt/media/HatchFill", false);
                this.AddRelation(nRelationID, shape);
            }
            else
            {
                shape = new PptShape(id + 1, NO_RELATION, "TextBox " + id, obj, FPPT_export);
            }


            float FDpiFX = 96f / DrawUtils.ScreenDpi;


            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            using (Font f = new Font(obj.Font.Name, obj.Font.Size * FDpiFX, obj.Font.Style))
            using (GraphicCache cache = new GraphicCache())
            {
                RectangleF textRect = new RectangleF(
                  obj.AbsLeft + obj.Padding.Left,
                  obj.AbsTop + obj.Padding.Top,
                  obj.Width - obj.Padding.Horizontal,
                  obj.Height - obj.Padding.Vertical);

                StringFormat format = obj.GetStringFormat(cache, 0);
                Brush textBrush = cache.GetBrush(obj.TextColor);
                AdvancedTextRenderer renderer = new AdvancedTextRenderer(obj.Text, g, f, textBrush,
                    textRect, format, obj.HorzAlign, obj.VertAlign, obj.LineHeight, obj.Angle, obj.FontWidthRatio,
                    obj.ForceJustify, obj.Wysiwyg, obj.HtmlTags, true);

                float w = f.Height * 0.1f; // to match .net char X offset
                // render
                if (renderer.Paragraphs.Count == 0)
                {
                    // append empty space
                    shape.Open_Paragraph();
                    shape.Add_Run(f, obj.TextColor, null);
                    shape.Close_Paragraph();
                }
                else foreach (AdvancedTextRenderer.Paragraph paragraph in renderer.Paragraphs)
                    {
                        shape.Open_Paragraph();
                        foreach (AdvancedTextRenderer.Line line in paragraph.Lines)
                        {
                            foreach (AdvancedTextRenderer.Word word in line.Words)
                                if (renderer.HtmlTags)
                                    foreach (AdvancedTextRenderer.Run run in word.Runs)
                                        using (Font fnt = run.GetFont())
                                        {
                                            shape.Add_Run(fnt, run.Style.Color, run.Text);
                                        }
                                else
                                    shape.Add_Run(f, obj.TextColor, word.Text);
                        }
                        shape.Close_Paragraph();
                    }
            }

            ExportShape(outstream, shape, "rect");
            id++;
        }

        private void AddLine(StreamWriter Out, LineObject obj)
        {
            PptShape shape = new PptShape(id + 1, NO_RELATION, "LineObject " + id, obj, FPPT_export);

            Out.WriteLine("<p:cxnSp>");

            shape.Export_nvCxnSpPr(Out);

            if (obj.Diagonal) 
                shape.Export_spPr(Out, "line"); 
            else 
                shape.Export_spPr(Out, "straightConnector1");

            //Out.WriteLine("<a:ln>");
            //Out.WriteLine("<a:tailEnd type="arrow" />");
            //Out.WriteLine("</a:ln>");
            //Out.WriteLine("</p:spPr>"); <<-- fix it

            Out.WriteLine("<p:style>");
            Out.WriteLine("<a:lnRef idx=\"1\">");
            Out.WriteLine("<a:schemeClr val=\"accent1\" />");
            Out.WriteLine("</a:lnRef>");
            Out.WriteLine("<a:fillRef idx=\"0\">");
            Out.WriteLine("<a:schemeClr val=\"accent1\" />");
            Out.WriteLine("</a:fillRef>");
            Out.WriteLine("<a:effectRef idx=\"0\">");
            Out.WriteLine("<a:schemeClr val=\"accent1\" />");
            Out.WriteLine("</a:effectRef>");
            Out.WriteLine("<a:fontRef idx=\"minor\">");
            Out.WriteLine("<a:schemeClr val=\"tx1\" />");
            Out.WriteLine("</a:fontRef>");
            Out.WriteLine("</p:style>");

            Out.WriteLine("</p:cxnSp>");

            id++;
        }

        // Save any object as image file
        private PptShape SaveImage(ReportComponentBase obj, int rId, string FileName, bool ClearBackground)
        {
            FPPT_export.PictureCount++; // Increase picture counter

            string file_extension = "png";
            System.Drawing.Imaging.ImageFormat image_format = System.Drawing.Imaging.ImageFormat.Png;
            if (this.FPPT_export.ImageFormat == PptImageFormat.Jpeg)
            {
                file_extension = "jpg";
                image_format = System.Drawing.Imaging.ImageFormat.Jpeg;
            }

            string ImageFileName = FPPT_export.TempFolder + "/" + FileName + FPPT_export.PictureCount.ToString() + "." + file_extension;

            using (System.Drawing.Image image = new System.Drawing.Bitmap((int)Math.Round(obj.Width * printZoom), (int)Math.Round(obj.Height * printZoom)))
            using (Graphics g = Graphics.FromImage(image))
            using (GraphicCache cache = new GraphicCache())
            {
                g.TranslateTransform(-obj.AbsLeft * printZoom, -obj.AbsTop * printZoom);
                if (ClearBackground)
                {
                    g.Clear(Color.White);
                }
                obj.Draw(new FRPaintEventArgs(g, printZoom, printZoom, cache));
                image.Save(ImageFileName, image_format);
            }

            return new PptShape(id + 1, rId, FileName + FPPT_export.PictureCount.ToString() + "." + file_extension, obj, FPPT_export);
        }

        private void AddCheckboxObject(StreamWriter Out, int rId, CheckBoxObject checkbox, out int rIdOut)
        {
            PptShape shape;
            string KEY = checkbox.Name + checkbox.Checked.ToString();

            if (!CheckboxList.ContainsKey(KEY))
            {
                rId++;

                shape = SaveImage(checkbox, rId, "ppt/media/Checkbox", false);
                this.AddRelation(rId, shape);
                CheckboxList.Add(KEY, shape);
            }
            else
            {
                shape = CheckboxList[KEY];
                shape.MoveObject(checkbox);
            }

            Out.WriteLine("<p:pic>");
            shape.Export_nvPicPr(Out, id);
            shape.Export_blipFill(Out);
            shape.Export_spPr(Out, "rect");
            Out.WriteLine("</p:pic>");

            rIdOut = rId;
            id++;
        }

        private void AddPictureObject(StreamWriter Out, int rId, ReportComponentBase obj, string FileName)
        {
            PptShape shape = SaveImage(obj, rId, FileName, !(obj is PictureObject));

            Out.WriteLine("<p:pic>");
            shape.Export_nvPicPr(Out, FPPT_export.PictureCount);
            shape.Export_blipFill(Out);
            shape.Export_spPr(Out, "rect");
            Out.WriteLine("</p:pic>");

            this.AddRelation(rId, shape);

            id++;
        }

        private void AddTable(StreamWriter Out, TableBase table)
        {
//            const int ColMultiplier = 20;
            //const long EMU = 10000; // 12700;

#if false // export into Office tables
            PptShape shape = new PptShape(id + 1, "Table " + id, table);

            Out.WriteLine("<p:graphicFrame>");

            shape.Export_nvGraphicFramePr(Out);
            shape.Export_XFRM(Out, 'p');

            Out.WriteLine("<a:graphic>");
            Out.WriteLine("<a:graphicData uri=\"http://schemas.openxmlformats.org/drawingml/2006/table\">");
            Out.WriteLine("<a:tbl>");
            Out.WriteLine("<a:tblPr>");
            Out.WriteLine("<a:tableStyleId>{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}</a:tableStyleId>");
            Out.WriteLine("</a:tblPr>");

            Out.WriteLine("<a:tblGrid>");
            for (int j = 0; j < table.Columns.Count; j++)
            {
                TableColumn column = table.Columns[j];
                long w = (long)(column.Width * EMU);
                Out.WriteLine("<a:gridCol w=" + Quoted(w) + " />");
            }
            Out.WriteLine("</a:tblGrid>");

            for (int i = 0; i < table.RowCount; i++)
            {
                TableRow row = table.Rows[i];
                long h = (long)(row.Height * EMU);
                Out.WriteLine("<a:tr h="+Quoted(h)+">");
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    Out.WriteLine("<a:tc>");
#if true
Out.WriteLine("<a:txBody><a:bodyPr /><a:lstStyle /><a:p><a:endParaRPr lang=\"en-US\" dirty=\"0\" /></a:p></a:txBody>");
#else
                        TableCell textcell = table[j, i];
                        shape.ResetText();
                        shape.AppendText(textcell.Font, 0, 0, 0, textcell.Text, false, false);
                        shape.Export_txBody(Out);
#endif
Out.WriteLine("<a:tcPr marL=\"0\" marR=\"0\" marT=\"0\" marB=\"0\" />");

                    Out.WriteLine("</a:tc>");
                }
                Out.WriteLine("</a:tr>");
            }

            Out.WriteLine("</a:tbl>");
            Out.WriteLine("</a:graphicData>");
            Out.WriteLine("</a:graphic>");
            Out.WriteLine("</p:graphicFrame>");

            id++;
#else
            using (TextObject tableBack = new TextObject())
            {
                tableBack.Left = table.AbsLeft;
                tableBack.Top = table.AbsTop;
                float tableWidth = 0;
                for (int i = 0; i < table.ColumnCount; i++)
                    tableWidth += table[i, 0].Width;
                tableBack.Width = (tableWidth < table.Width) ? tableWidth : table.Width;
                tableBack.Height = table.Height;
                tableBack.Fill = table.Fill;
                tableBack.Text = "";

                // exporting the table fill
                AddTextObject(Out, 0, tableBack);

                // exporting the table cells
                float x = 0;
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    float y = 0;
                    for (int i = 0; i < table.RowCount; i++)
                    {
                        if (!table.IsInsideSpan(table[j, i]))
                        {
                            TableCell textcell = table[j, i];

                            textcell.Left = x;
                            textcell.Top = y;

                            AddTextObject(Out, 0, textcell);
                        }
                        y += (table.Rows[i]).Height;
                    }
                    x += (table.Columns[j]).Width;
                }

                // exporting the table border
                tableBack.Fill = new SolidFill();
                tableBack.Border = table.Border;
                AddTextObject(Out, 0, tableBack);
            }
#endif
        }

        internal void Export(PowerPoint2007Export OoXML, ReportPage page)
        {
            int rId = 0;

            foreach (OoPptSlideLayout layout in OoXML.SlideLayoutList)
            {
                AddRelation( ++rId, OoXML.SlideLayoutList[0] as OoPptSlideLayout);
            }

            // Export slide
            using (FileStream file = new FileStream(OoXML.TempFolder + "/" + FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine("<p:sld xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" xmlns:p=\"http://schemas.openxmlformats.org/presentationml/2006/main\">");
                Out.WriteLine("<p:cSld>");
                Out.WriteLine("<p:spTree>");

                Out.WriteLine("<p:nvGrpSpPr>");
                Out.WriteLine("<p:cNvPr id=\"1\" name=\"\" />");
                Out.WriteLine("<p:cNvGrpSpPr />");
                Out.WriteLine("<p:nvPr />");
                Out.WriteLine("</p:nvGrpSpPr>");

                Out.WriteLine("<p:grpSpPr>");
                Out.WriteLine("<a:xfrm>");
                Out.WriteLine("<a:off x=\"0\" y=\"0\" />");
                Out.WriteLine("<a:ext cx=\"0\" cy=\"0\" />");
                Out.WriteLine("<a:chOff x=\"0\" y=\"0\" />");
                Out.WriteLine("<a:chExt cx=\"0\" cy=\"0\" />");
                Out.WriteLine("</a:xfrm>");
                Out.WriteLine("</p:grpSpPr>");

                using (page) foreach (Base c in page.AllObjects)
                {
                    ReportComponentBase obj = c as ReportComponentBase;
                    if (obj is CellularTextObject)
                        obj = (obj as CellularTextObject).GetTable();
                    if (obj is TableCell)
                        continue;
                    else if (obj is TableBase)              
                        AddTable( Out, obj as TableBase );
                    else if (obj is TextObject)
                        AddTextObject(Out, ++rId, obj as TextObject);
                    else if (obj is BandBase)               
                        AddBandObject(Out, obj as BandBase);
                    else if (obj is LineObject)             
                        AddLine(Out, obj as LineObject);
                    else if (obj is ShapeObject)            
                        AddShape(Out, obj as ShapeObject);
                    else if (obj is PictureObject)          
                        AddPictureObject(Out, ++rId, obj as PictureObject, "ppt/media/image");
                    else if (obj is Barcode.BarcodeObject)  
                        AddPictureObject(Out, ++rId, obj as ReportComponentBase, "ppt/media/BarcodeImage");
                    else if (obj is ZipCodeObject)          
                        AddPictureObject(Out, ++rId, obj as ReportComponentBase, "ppt/media/ZipCodeImage");
                    else if (obj is MSChart.MSChartObject)  
                        AddPictureObject(Out, ++rId, obj as ReportComponentBase, "ppt/media/MSChartImage");
                    else if (obj is RichObject)             
                        AddPictureObject(Out, ++rId, obj as ReportComponentBase, "ppt/media/RichTextImage");
                    else if (obj is CheckBoxObject)         
                        AddCheckboxObject(Out, rId, obj as CheckBoxObject, out rId);
                    else if (obj == null)
                    {
                        ;
                    }
                    else 
                    {
                        AddPictureObject(Out, ++rId, obj as ReportComponentBase, "ppt/media/FixMeImage");
                    }
                }

                Out.WriteLine("</p:spTree>");
                Out.WriteLine("</p:cSld>");
                ExportColorMapOverride(Out);
                Out.WriteLine("</p:sld>");
            }
            ExportRelations(OoXML);
        }
    }

    /// <summary>
    /// Slide layout object
    /// </summary>
    internal class OoPptSlideLayout : OoSlideBase
    {
        #region Class overrides
        public override string RelationType { get { return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideLayout"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml"; } }
        public override string FileName { get { return "ppt/slideLayouts/slideLayout" + FIndex.ToString() + ".xml"; } }
        #endregion

        #region Private fields
        private PptLayoutDescriptor Descriptor;
        private int FIndex;
        #endregion

        internal void Export(PowerPoint2007Export OoXML)
        {
            ExportRelations( OoXML );

            using (FileStream file = new FileStream(OoXML.TempFolder + "/" + FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine("<p:sldLayout ");
                Out.WriteLine("xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" ");
                Out.WriteLine("xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" ");
                Out.WriteLine("xmlns:p=\"http://schemas.openxmlformats.org/presentationml/2006/main\" type=" + Quoted(Descriptor.Type) + " preserve=\"1\">");

                Out.WriteLine("<p:cSld name=" + Quoted(Descriptor.Name) + ">");
                ExportShapeTree(Out, Descriptor.Shapes);
                Out.WriteLine("</p:cSld>");
                ExportColorMapOverride(Out);
                Out.WriteLine("</p:sldLayout>");
            }
        }

        // Constructor
        internal OoPptSlideLayout(int Index, PptLayoutDescriptor descriptor)
        {
            Descriptor = descriptor;
            FIndex = Index;
        }
    }

    /// <summary>
    /// Presentation class
    /// </summary>
    internal class OoPptPresentation : OoXMLBase
    {
        #region Class overrides
        public override string RelationType { get { return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"; } }
        public override string FileName { get { return "ppt/presentation.xml"; } }
        #endregion

        internal void Export(PowerPoint2007Export OoXML)
        {

            ExportRelations(OoXML);
            
            using (FileStream file = new FileStream(OoXML.TempFolder + "\\ppt\\presentation.xml", FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine("<p:presentation xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" xmlns:p=\"http://schemas.openxmlformats.org/presentationml/2006/main\" saveSubsetFonts=\"1\">");
                Out.WriteLine("<p:sldMasterIdLst>");
                Out.WriteLine("<p:sldMasterId id=" + Quoted(OoXML.SlideMaster.SlideMasterId.ToString()) + "r:id=" + Quoted(OoXML.SlideMaster.rId) + "/> ");
                Out.WriteLine("</p:sldMasterIdLst>");

                Out.WriteLine("<p:sldIdLst>");
                foreach (OoXMLBase obj in RelationList) if (obj is OoPptSlide)
                {
                    OoPptSlide slide = obj as OoPptSlide;
                    Out.WriteLine("<p:sldId id=" + Quoted(slide.SlideID) + " r:id=" + Quoted(slide.rId) + " />");
                }
                Out.WriteLine("</p:sldIdLst>");

                ReportPage page = OoXML.FirstPage;

                Out.WriteLine("<p:sldSz cx=" + Quoted(OoXML.PaperWidth) + " cy=" + Quoted(OoXML.PaperHeight) + " type=\"custom\" /> ");

                Out.WriteLine("<p:notesSz cx=\"6858000\" cy=\"9144000\" /> ");
                //
                Out.WriteLine("<p:defaultTextStyle>");
                Out.WriteLine("<a:defPPr>");
                Out.WriteLine("<a:defRPr lang=\"en-US\" />");
                Out.WriteLine("</a:defPPr>");
                Out.WriteLine("</p:defaultTextStyle>");
                //
                Out.WriteLine("</p:presentation>");
            }
        }

    }

    /// <summary>
    /// PPt Application Properties class
    /// </summary>
    internal class OoPptApplicationProperties : OoXMLBase
    {
        #region Class overrides
        public override string RelationType { get { return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-officedocument.extended-properties+xml"; } }
        public override string FileName { get { return "docProps/app.xml"; } }
        #endregion

        public void Export(OOExportBase OoXML)
        {
            using (FileStream file = new FileStream(OoXML.TempFolder + "/" + FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine(
                    "<Properties xmlns=\"http://schemas.openxmlformats.org/officeDocument/2006/extended-properties\" xmlns:vt=\"http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes\">" +
                    "<TotalTime>1</TotalTime>" +
                    "<Words>0</Words>" +
                    "<Application>Microsoft Office PowerPoint</Application>" +
                    "<PresentationFormat>On-screen Show (4:3)</PresentationFormat>" +
                    "<Paragraphs>0</Paragraphs>" +
                    "<Slides>0</Slides>" +
                    "<Notes>0</Notes> " +
                    "<HiddenSlides>0</HiddenSlides>" +
                    "<MMClips>0</MMClips>" +
                    "<ScaleCrop>false</ScaleCrop>" +
                    "<HeadingPairs>" +
                    "<vt:vector size=\"4\" baseType=\"variant\">" +
                    "<vt:variant>" +
                    "<vt:lpstr>Theme</vt:lpstr>" +
                    "</vt:variant>" +
                    "<vt:variant>" +
                    "<vt:i4>1</vt:i4>" +
                    "</vt:variant>" +
                    "<vt:variant>" +
                    "<vt:lpstr>Slide Titles</vt:lpstr>" +
                    "</vt:variant>" +
                    "<vt:variant>" +
                    "<vt:i4>0</vt:i4>" +
                    "</vt:variant>" +
                    "</vt:vector>" +
                    "</HeadingPairs>" +
                    "<TitlesOfParts>" +
                    "<vt:vector size=\"1\" baseType=\"lpstr\">" +
                    "<vt:lpstr>Office Theme</vt:lpstr>" +
                    "</vt:vector>" +
                    "</TitlesOfParts>" +
                    "<LinksUpToDate>false</LinksUpToDate>" +
                    "<SharedDoc>false</SharedDoc>" +
                    "<HyperlinksChanged>false</HyperlinksChanged>" +
                    "<AppVersion>12.0000</AppVersion>" +
                    "</Properties>");
            }
        }
    }

    /// <summary>
    /// Ppt Table styles class
    /// </summary>
    internal class OoPptTableStyles : OoXMLBase
    {
        #region Class overrides
        public override string RelationType { get { return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/tableStyles"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-officedocument.presentationml.tableStyles+xml"; } }
        public override string FileName { get { return "ppt/tableStyles.xml"; } }
        #endregion

        internal void Export(PowerPoint2007Export OoXML)
        {
            using (FileStream file = new FileStream(OoXML.TempFolder + "/" + FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine("<a:tblStyleLst xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" def=\"{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}\"/>");
            }
        }
    }

    /// <summary>
    /// Ppt Presentation properties class
    /// </summary>
    internal class OoPptPresProperties : OoXMLBase
    {
        #region Class overrides
        public override string RelationType { get { return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/presProps"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-officedocument.presentationml.presProps+xml"; } }
        public override string FileName { get { return "ppt/presProps.xml"; } }
        #endregion

        internal void Export(PowerPoint2007Export OoXML)
        {
            using (FileStream file = new FileStream(OoXML.TempFolder + "/" + FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine("<p:presentationPr xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" xmlns:p=\"http://schemas.openxmlformats.org/presentationml/2006/main\" />");
            }
        }
    }

    /// <summary>
    /// Ppt View Properties class
    /// </summary>
    internal class OoPptViewProps : OoXMLBase
    {
        #region Class overrides
        public override string RelationType { get { return "http://schemas.openxmlformats.org/officeDocument/2006/relationships/viewProps"; } }
        public override string ContentType { get { return "application/vnd.openxmlformats-officedocument.presentationml.viewProps+xml"; } }
        public override string FileName { get { return "ppt/viewProps.xml"; } }
        #endregion

        internal void Export(PowerPoint2007Export OoXML)
        {
            using (FileStream file = new FileStream(OoXML.TempFolder + "/" + FileName, FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine("<p:viewPr xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" xmlns:p=\"http://schemas.openxmlformats.org/presentationml/2006/main\" lastView=\"sldThumbnailView\">");
                Out.WriteLine("<p:normalViewPr showOutlineIcons=\"0\">");
                Out.WriteLine("<p:restoredLeft sz=\"15620\" autoAdjust=\"0\" />");
                Out.WriteLine("<p:restoredTop sz=\"94660\" autoAdjust=\"0\" />");
                Out.WriteLine("</p:normalViewPr>");
                Out.WriteLine("<p:slideViewPr>");
                Out.WriteLine("<p:cSldViewPr>");
                Out.WriteLine("<p:cViewPr varScale=\"1\">");
                Out.WriteLine("<p:scale>");
                Out.WriteLine("<a:sx n=\"104\" d=\"100\" />");
                Out.WriteLine("<a:sy n=\"104\" d=\"100\" />");
                Out.WriteLine("</p:scale>");
                Out.WriteLine("<p:origin x=\"-222\" y=\"-90\" />");
                Out.WriteLine("</p:cViewPr>");
                Out.WriteLine("<p:guideLst>");
                Out.WriteLine("<p:guide orient=\"horz\" pos=\"2160\" />");
                Out.WriteLine("<p:guide pos=\"2880\" />");
                Out.WriteLine("</p:guideLst>");
                Out.WriteLine("</p:cSldViewPr>");
                Out.WriteLine("</p:slideViewPr>");
                Out.WriteLine("<p:outlineViewPr>");
                Out.WriteLine("<p:cViewPr>");
                Out.WriteLine("<p:scale>");
                Out.WriteLine("<a:sx n=\"33\" d=\"100\" />");
                Out.WriteLine("<a:sy n=\"33\" d=\"100\" />");
                Out.WriteLine("</p:scale>");
                Out.WriteLine("<p:origin x=\"0\" y=\"0\" />");
                Out.WriteLine("</p:cViewPr>");
                Out.WriteLine("</p:outlineViewPr>");
                Out.WriteLine("<p:notesTextViewPr>");
                Out.WriteLine("<p:cViewPr>");
                Out.WriteLine("<p:scale>");
                Out.WriteLine("<a:sx n=\"100\" d=\"100\" />");
                Out.WriteLine("<a:sy n=\"100\" d=\"100\" />");
                Out.WriteLine("</p:scale>");
                Out.WriteLine("<p:origin x=\"0\" y=\"0\" />");
                Out.WriteLine("</p:cViewPr>");
                Out.WriteLine("</p:notesTextViewPr>");
                Out.WriteLine("<p:gridSpacing cx=\"73736200\" cy=\"73736200\" />");
                Out.WriteLine("</p:viewPr>");
            }
        }
    }

    /// <summary>
    /// Specifies the image format in PowerPoint export.
    /// </summary>
    public enum PptImageFormat
    {
        /// <summary>
        /// Specifies the .png format.
        /// </summary>
        Png,

        /// <summary>
        /// Specifies the .jpg format.
        /// </summary>
        Jpeg
    }

    /// <summary>
    /// Represents the PowerPoint 2007 export.
    /// </summary>
    public class PowerPoint2007Export : OOExportBase
    {
        #region Slide layouts initializer
        PptLayoutDescriptor[] LayoutDescriptors = 
            {
                new PptLayoutDescriptor("blank", "Blank", new PptShape[]
                    {
                        //new PptShape( 2, "Date Placeholder 1", 0, 0, 0, 0, "dt", "half", null, 10),
                        //new PptShape( 3, "Footer Placeholder 2", 0, 0, 0, 0, "ftr", "quarter", null, 11),
                        //new PptShape( 4, "Slide Number Placeholder 3", 0, 0, 0, 0, "sldNum", "quarter", null, 12)
                    }
                )
            };
        #endregion

        #region Private fields
        private OoPptPresentation           FPresentation;
        private OoXMLCoreDocumentProperties FCoreDocProp;
        private OoPptApplicationProperties  FAppProp;
        private OoPptViewProps              FViewProps;
        private OoPptTableStyles            FTableStyles;
        private OoPptPresProperties         FPresentationProperties;
        private OoXMLThemes                 FThemes;

        private long                        FPaperWidth;
        private long                        FPaperHeight;
        private long                        FLeftMargin;
        private long                        FTopMargin;

        private OoPptSlideMaster            FSlideMasters;
        private ArrayList                   FSlideLayouts;
        private ArrayList                   FSlideList;
        private PptImageFormat FImageFormat;
        private int FPictureCount;
        #endregion

        #region Internal properties
        internal int PictureCount
        {
          get { return FPictureCount; }
          set { FPictureCount = value; }
        }
        internal OoPptSlideMaster SlideMaster { get { return FSlideMasters; } }
        internal ArrayList SlideLayoutList { get { return FSlideLayouts; } }
        internal ArrayList SlideList { get { return FSlideList; } }
        internal ReportPage FirstPage { get { return GetPage(0); } }

        internal long PaperWidth { get { return FPaperWidth; } }
        internal long PaperHeight { get { return FPaperHeight; } }
        internal long LeftMargin { get { return FLeftMargin; } }
        internal long TopMargin { get { return FTopMargin; } }
        
        /// <summary>
        /// Gets or sets the image format used when exporting.
        /// </summary>
        public PptImageFormat ImageFormat
        {
            get { return FImageFormat; }
            set { FImageFormat = value; }
        }

        #endregion

        #region Private Methods

        private void CreateRelations()
        {
            using (FileStream file = new FileStream(FTempFolder + "\\_rels\\.rels", FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.WriteLine("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
                Out.WriteLine("<Relationship Id=\"rId3\" Type=" + Quoted(FAppProp.RelationType) + " Target=" + Quoted(FAppProp.FileName) + " />");
                Out.WriteLine("<Relationship Id=\"rId2\" Type=" + Quoted(FCoreDocProp.RelationType) + " Target=" + Quoted(FCoreDocProp.FileName) + " />");
                Out.WriteLine("<Relationship Id=\"rId1\" Type=" + Quoted(FPresentation.RelationType) + " Target="+Quoted(FPresentation.FileName)+" />");
                Out.WriteLine("</Relationships>");
            }
        }

        private void CreateContentTypes()
        {
            using (FileStream file = new FileStream(FTempFolder + "\\[Content_Types].xml", FileMode.Create))
            using (StreamWriter Out = new StreamWriter(file))
            {
                Out.WriteLine(xml_header);
                Out.Write("<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");

                for (int i = 0; i < FSlideLayouts.Count; i++)
                {
                    OoPptSlideLayout layout = FSlideLayouts[i] as OoPptSlideLayout;
                    Out.Write("<Override PartName=" + QuotedRoot(layout.FileName) + " ContentType=" + Quoted(layout.ContentType) + "/>");
                }

                for (int i = 0; i < FSlideList.Count; i++)
                {
                    OoPptSlide slide = FSlideList[i] as OoPptSlide;
                    Out.Write("<Override PartName=" + QuotedRoot(slide.FileName) + " ContentType=" + Quoted(slide.ContentType) + "/>");
                }

                Out.Write("<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\" />");
                Out.Write("<Default Extension=\"xml\" ContentType=\"application/xml\" />");
                Out.Write("<Default Extension=\"png\" ContentType=\"image/png\"/>");
                Out.Write("<Default Extension=\"jpg\" ContentType=\"image/jpeg\"/>");

                Out.Write("<Override PartName=" + QuotedRoot(FPresentation.FileName) + " ContentType=" + Quoted(FPresentation.ContentType) + "/>");
                Out.Write("<Override PartName=" + QuotedRoot(FCoreDocProp.FileName) + " ContentType=" + Quoted(FCoreDocProp.ContentType) + "/>");
                Out.Write("<Override PartName=" + QuotedRoot(FAppProp.FileName) + " ContentType=" + Quoted(FAppProp.ContentType) + "/>");
                Out.Write("<Override PartName=" + QuotedRoot(SlideMaster.FileName) + " ContentType=" + Quoted(SlideMaster.ContentType) + "/>");
                Out.Write("<Override PartName=" + QuotedRoot(FTableStyles.FileName) + " ContentType=" + Quoted(FTableStyles.ContentType) + "/>");
                Out.Write("<Override PartName=" + QuotedRoot(FViewProps.FileName) + " ContentType=" + Quoted(FViewProps.ContentType) + "/>");
                Out.Write("<Override PartName=" + QuotedRoot(FPresentationProperties.FileName) + " ContentType=" + Quoted(FPresentationProperties.ContentType) + "/>");
                Out.Write("<Override PartName=" + QuotedRoot(FThemes.FileName) + " ContentType=" + Quoted(FThemes.ContentType) + "/>");

                Out.Write("</Types>");
            }
        }

        private void ExportOOPPT(Stream Stream)
        {
            CreateContentTypes();

            FPresentationProperties.Export( this );
            FTableStyles.Export(this);
            FViewProps.Export(this);
            FThemes.Export(this, "FastReport.Resources.theme1.xml", "/ppt/theme/theme1.xml" );

            CreateRelations();
          
            FAppProp.Export(this);
            FCoreDocProp.Export(this);
            FPresentation.Export(this);
            FSlideMasters.Export(this);

            foreach (OoPptSlideLayout layout in FSlideLayouts)
            {
                layout.Export(this);
            }

            ZipArchive zip = new ZipArchive();
            zip.AddDir(FTempFolder);
            zip.SaveToStream(Stream);
            Directory.Delete(FTempFolder, true);
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc/>
        public override bool ShowDialog()
        {
            using (PowerPoint2007ExportForm form = new PowerPoint2007ExportForm())
            {
                form.Init(this);
                return form.ShowDialog() == DialogResult.OK;
            }
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            FExportTickCount = Environment.TickCount;

            Directory.CreateDirectory(FTempFolder);
            Directory.CreateDirectory(FTempFolder + "\\_rels");
            Directory.CreateDirectory(FTempFolder + "\\docProps");
            Directory.CreateDirectory(FTempFolder + "\\ppt");
            Directory.CreateDirectory(FTempFolder + "\\ppt\\_rels");
            Directory.CreateDirectory(FTempFolder + "\\ppt\\media");
            Directory.CreateDirectory(FTempFolder + "\\ppt\\theme");
            Directory.CreateDirectory(FTempFolder + "\\ppt\\slides");
            Directory.CreateDirectory(FTempFolder + "\\ppt\\slides\\_rels");
            Directory.CreateDirectory(FTempFolder + "\\ppt\\slideMasters");
            Directory.CreateDirectory(FTempFolder + "\\ppt\\slideLayouts");
        }

        /// <inheritdoc/>
        protected override void ExportPage(int pageNo)
        {
            const long EMUpmm = 36000;

            this.FPaperWidth = (long)(FirstPage.PaperWidth * EMUpmm);
            this.FPaperHeight = (long)(FirstPage.PaperHeight * EMUpmm);
            this.FLeftMargin = (long)(FirstPage.LeftMargin * EMUpmm);
            this.FTopMargin = (long)(FirstPage.TopMargin * EMUpmm);
            
            OoPptSlide slide = new OoPptSlide(this);
            if (pageNo == 0) slide.Reset();
            FSlideList.Add(slide);
            int relatives_count = FPresentation.RelationList.Count;
            FPresentation.AddRelation(relatives_count + 1, slide);
            slide.Export(this, GetPage(pageNo));
        }

        /// <inheritdoc/>
        protected override void Finish()
        {
            ExportOOPPT(Stream);

            FExportTickCount = Environment.TickCount - FExportTickCount;
            if (Config.ReportSettings.ShowPerformance)
                Report.Preview.ShowPerformance(String.Format(Res.Get("Performance"), FExportTickCount ));
        }

        /// <inheritdoc/>
        protected override string GetFileFilter()
        {
            return new MyRes("FileFilters").Get("PptxFile");
        }
        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="PowerPoint2007Export"/> class with the default settings.
        /// </summary>
        public PowerPoint2007Export()
        {
            FTempFolder = Path.GetTempPath() + Path.GetRandomFileName();

            FPresentation = new OoPptPresentation();
            FCoreDocProp = new OoXMLCoreDocumentProperties();
            FAppProp = new OoPptApplicationProperties();

            FSlideMasters = new OoPptSlideMaster();

            FViewProps = new OoPptViewProps();
            FTableStyles = new OoPptTableStyles();
            FPresentationProperties = new OoPptPresProperties();
            FThemes = new OoXMLThemes();

            // Set relations to presentation.xml.rels
            FPresentation.AddRelation(1, FSlideMasters);
            FPresentation.AddRelation(2, FPresentationProperties);
            FPresentation.AddRelation(3, FViewProps);
            FPresentation.AddRelation(4, FThemes);
            FPresentation.AddRelation(5, FTableStyles);

            FSlideLayouts = new ArrayList();
            FSlideList = new ArrayList();
     
            // Set relations between layouts and Slide Master
            for (int i = 0; i < LayoutDescriptors.Length; i++)
            {
                OoPptSlideLayout current_layout = new OoPptSlideLayout(1 + i, LayoutDescriptors[i]);
                current_layout.AddRelation(1, FSlideMasters);
                FSlideMasters.AddRelation(1 + i, current_layout);
                FSlideLayouts.Add(current_layout);
            }

            FSlideMasters.AddRelation(150, FThemes);
        }
    }
}
