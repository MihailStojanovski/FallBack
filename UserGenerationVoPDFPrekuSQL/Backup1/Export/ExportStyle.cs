using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using FastReport.Format;
using System.Windows.Forms;
using FastReport.Utils;

namespace FastReport.Export
{
    class ExportIEMStyle
    {
        #region Private fields
        private Font FFont;
        private VertAlign FVAlign;
        private HorzAlign FHAlign;
        private FillBase FTextFill;
        private FillBase FFill;
        private FormatBase FFormat;
        private Border FBorder;
        private Padding FPadding;
        private float FFirstTabOffset;
        private bool FUnderlines;
        private int FAngle;
        private bool FRTL;
        #endregion

        #region Public Properties

        public bool RTL
        {
            get { return FRTL; }
            set { FRTL = value; }
        }
        public Font Font
        {
            get { return FFont; }
            set { FFont = value; }
        }
        public VertAlign VAlign
        {
            get { return FVAlign; }
            set { FVAlign = value; }
        }
        public HorzAlign HAlign
        {
            get { return FHAlign; }
            set { FHAlign = value; }
        }
        public FillBase TextFill
        {
            get { return FTextFill; }
            set { FTextFill = value; }
        }
        public Color TextColor
        {
            get { return ExportUtils.GetColorFromFill(TextFill); }
        }
        public Color FillColor
        {
            get { return ExportUtils.GetColorFromFill(Fill); }
        }
        public FillBase Fill
        {
            get { return FFill; }
            set { FFill = value; }
        }
        public FormatBase Format
        {
            get { return FFormat; }
            set { FFormat = value; }
        }
        public Border Border
        {
            get { return FBorder; }
            set { FBorder = value; }
        }
        public Padding Padding
        {
            get { return FPadding; }
            set { FPadding = value; }
        }
        public float FirstTabOffset
        {
            get { return FFirstTabOffset; }
            set { FFirstTabOffset = value; }
        }
        public bool Underlines
        {
            get { return FUnderlines; }
            set { FUnderlines = value; }
        }
        public int Angle
        {
            get { return FAngle; }
            set { FAngle = value; }
        }

        #endregion

        public bool Equals(ExportIEMStyle Style)
        {
            return
                    (Style.HAlign == HAlign) &&
                    (Style.VAlign == VAlign) &&
                    Style.Border.Equals(Border) &&
                    Style.TextFill.Equals(TextFill) &&
                    Style.Fill.Equals(Fill) &&                    
                    Style.Font.Equals(Font) &&
                    Style.RTL == RTL &&
                    (Style.Format.ToString() == Format.ToString()) &&
                    Style.Padding.Equals(Padding) &&
                    (Style.FirstTabOffset == FirstTabOffset) &&
                    (Style.Underlines == Underlines) &&
                    (Style.Angle == Angle);
        }

        public void Assign(ExportIEMStyle Style)
        {
            Font = Style.Font;
            VAlign = Style.VAlign;
            HAlign = Style.HAlign;
            Format = Style.Format.Clone();
            TextFill = Style.TextFill.Clone();
            RTL = Style.RTL;
            Fill = Style.Fill.Clone();
            Border = Style.Border.Clone();
            Padding = Style.Padding;
            FirstTabOffset = Style.FirstTabOffset;
            Underlines = Style.Underlines;
            Angle = Style.Angle;
        }

        public ExportIEMStyle()
        {
            Font = DrawUtils.DefaultFont;
            Format = new GeneralFormat();
            VAlign = VertAlign.Top;
            HAlign = HorzAlign.Left;
            TextFill = new SolidFill();
            Fill = new SolidFill();
            (Fill as SolidFill).Color = Color.Transparent;
            Border = new Border();
            Padding = new Padding();
        }
    }
}
