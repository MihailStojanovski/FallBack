using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Globalization;
using FastReport.Utils;
using System.IO;
using System.Threading;

namespace FastReport.Export.Pdf
{
    internal class PDFFont
    {
        #region DLL import
        [DllImport("Gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("Gdi32.dll")]
        private static extern IntPtr DeleteObject(IntPtr hgdiobj);
        [DllImport("Gdi32.dll")]
        private static extern int GetOutlineTextMetrics(IntPtr hdc, int cbData, ref OutlineTextMetric lpOTM);
        [DllImport("Gdi32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetGlyphIndices(IntPtr hdc, string lpstr, int c, [In, Out] ushort[] pgi, uint fl);
        [DllImport("Gdi32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetFontData(IntPtr hdc, uint dwTable, uint dwOffset, [In, Out] byte[] lpvBuffer, uint cbData);
        [DllImport("Gdi32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetFontData(IntPtr hdc, uint dwTable, uint dwOffset, [In, Out] IntPtr lpvBuffer, uint cbData);
        [DllImport("usp10.dll")]
        private static extern int ScriptFreeCache(ref IntPtr psc);
        [DllImport("usp10.dll")]
        private static extern int ScriptItemize(
            [MarshalAs(UnmanagedType.LPWStr)] string pwcInChars, int cInChars, int cMaxItems,
            ref SCRIPT_CONTROL psControl, ref SCRIPT_STATE psState, [In, Out] SCRIPT_ITEM[] pItems, ref int pcItems);
        [DllImport("usp10.dll")]
        private static extern int ScriptLayout(
            int cRuns,[MarshalAs(UnmanagedType.LPArray)] byte[] pbLevel,
            [MarshalAs(UnmanagedType.LPArray)] int[] piVisualToLogical,
            [MarshalAs(UnmanagedType.LPArray)] int[] piLogicalToVisual);
        [DllImport("usp10.dll")]
        private static extern int ScriptShape(
            IntPtr hdc, ref IntPtr psc, [MarshalAs(UnmanagedType.LPWStr)] string pwcChars,
            int cChars, int cMaxGlyphs, ref SCRIPT_ANALYSIS psa,
            [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwOutGlyphs,
            [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwLogClust,
            [Out, MarshalAs(UnmanagedType.LPArray)] SCRIPT_VISATTR[] psva, ref int pcGlyphs);
        [DllImport("usp10.dll")]
        private static extern int ScriptPlace(
            IntPtr hdc, ref IntPtr psc, [MarshalAs(UnmanagedType.LPArray)] ushort[] pwGlyphs,
            int cGlyphs, [MarshalAs(UnmanagedType.LPArray)] SCRIPT_VISATTR[] psva,
            ref SCRIPT_ANALYSIS psa, [MarshalAs(UnmanagedType.LPArray)] int[] piAdvance,
            [Out, MarshalAs(UnmanagedType.LPArray)] GOFFSET[] pGoffset, ref ABC pABC);
        [DllImport("usp10.dll")]
        private static extern uint ScriptRecordDigitSubstitution(uint lcid, ref SCRIPT_DIGITSUBSTITUTE psds);
        [DllImport("usp10.dll")]
        private static extern int ScriptApplyDigitSubstitution(
            ref SCRIPT_DIGITSUBSTITUTE psds, ref SCRIPT_CONTROL psc, ref SCRIPT_STATE pss);
        #endregion

        #region Font Structures
        [StructLayout(LayoutKind.Sequential)]
        internal struct SCRIPT_STATE
        {
            public short data;

            public int uBidiLevel
            {
                get { return data & 0x001F; }
            }

            public void SetRtl()
            {
                data = 0x801;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SCRIPT_ANALYSIS
        {
            public short data;
            public SCRIPT_STATE state;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_CONTROL
        {
            public int data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_DIGITSUBSTITUTE
        {
            public short NationalDigitLanguage;
            public short TraditionalDigitLanguage;
            public byte DigitSubstitute;
            public int dwReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_ITEM
        {
            public int iCharPos;
            public SCRIPT_ANALYSIS analysis;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPT_VISATTR
        {
            public short data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GOFFSET
        {
            public int du;
            public int dv;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ABC
        {
            public int abcA;
            public int abcB;
            public int abcC;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FontPanose
        {
            public byte bFamilyType;
            public byte bSerifStyle;
            public byte bWeight;
            public byte bProportion;
            public byte bContrast;
            public byte bStrokeVariation;
            public byte ArmStyle;
            public byte bLetterform;
            public byte bMidline;
            public byte bXHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FontRect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FontPoint
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FontTextMetric
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OutlineTextMetric
        {
            public uint otmSize;
            public FontTextMetric otmTextMetrics;
            public byte otmFiller;
            public FontPanose otmPanoseNumber;
            public uint otmfsSelection;
            public uint otmfsType;
            public int otmsCharSlopeRise;
            public int otmsCharSlopeRun;
            public int otmItalicAngle;
            public uint otmEMSquare;
            public int otmAscent;
            public int otmDescent;
            public uint otmLineGap;
            public uint otmsCapEmHeight;
            public uint otmsXHeight;
            public FontRect otmrcFontBox;
            public int otmMacAscent;
            public int otmMacDescent;
            public uint otmMacLineGap;
            public uint otmusMinimumPPEM;
            public FontPoint otmptSubscriptSize;
            public FontPoint otmptSubscriptOffset;
            public FontPoint otmptSuperscriptSize;
            public FontPoint otmptSuperscriptOffset;
            public uint otmsStrikeoutSize;
            public int otmsStrikeoutPosition;
            public int otmsUnderscoreSize;
            public int otmsUnderscorePosition;
            public string otmpFamilyName;
            public string otmpFaceName;
            public string otmpStyleName;
            public string otmpFullName;
        }
        #endregion

        public List<int> Widths;
        public List<ushort> UsedAlphabet;
        public List<ushort> UsedAlphabetUnicode;
        public OutlineTextMetric TextMetric;
        public string Name;
        public Font SourceFont;
        public long Reference;
        public bool Saved;

        private Bitmap tempBitmap;
        private IntPtr FUSCache;
        private SCRIPT_DIGITSUBSTITUTE FDigitSubstitute;
        private float FDpiFX;

        #region Public Methods
        public void FillOutlineTextMetrix()
        {
            if (SourceFont != null)
            {
                using (Graphics g = Graphics.FromImage(tempBitmap))
                {
                    IntPtr hdc = g.GetHdc();
                    IntPtr f = SourceFont.ToHfont();
                    try
                    {
                        SelectObject(hdc, f);
                        GetOutlineTextMetrics(hdc, Marshal.SizeOf(typeof(OutlineTextMetric)), ref TextMetric);
                    }
                    finally
                    {
                        DeleteObject(f);
                        g.ReleaseHdc(hdc);
                    }
                }
            }
        }

        public byte[] GetFontData()
        {
            byte[] result = null;
            if (SourceFont != null)
            {
                using (Graphics g = Graphics.FromImage(tempBitmap))
                {
                    IntPtr hdc = g.GetHdc();
                    IntPtr f = SourceFont.ToHfont();
                    try
                    {
                        SelectObject(hdc, f);
                        uint fontDataSize = GetFontData(hdc, 0, 0, IntPtr.Zero, 0);
                        result = new byte[fontDataSize];
                        GetFontData(hdc, 0, 0, result, fontDataSize);
                    }
                    finally
                    {
                        DeleteObject(f);
                        g.ReleaseHdc(hdc);
                    }
                }
            }
            return result;
        }

        public string RemapString(string str, bool rtl)
        {
            // control chars should not be here...
            str = str.Replace("\r", "").Replace("\n", "");  
            int maxGlyphs = str.Length * 3;
            ushort[] glyphs = new ushort[maxGlyphs];
            int[] widths = new int[maxGlyphs];
            int actualLength = 0;

            using (Graphics g = Graphics.FromImage(tempBitmap))
            {
                IntPtr hdc = g.GetHdc();
                IntPtr f = SourceFont.ToHfont();
                try
                {
                    SelectObject(hdc, f);
                    actualLength = GetGlyphIndices(hdc, str, glyphs, widths, rtl);
                }
                finally
                {
                    DeleteObject(f);                    
                    g.ReleaseHdc(hdc);
                }
            }            
            
            StringBuilder sb = new StringBuilder(actualLength);
            for (int i = 0; i < actualLength; i++)
            {
                ushort c = glyphs[i];
                if (UsedAlphabet.IndexOf(c) == -1)
                {
                    UsedAlphabet.Add(c);
                    Widths.Add(widths[i]);
                    if (actualLength > str.Length) // ligatures found - skip entire string
                        UsedAlphabetUnicode.Add(TextMetric.otmTextMetrics.tmDefaultChar);
                    else
                        UsedAlphabetUnicode.Add(str[(rtl ? actualLength - i - 1 : i)]);
                }
                sb.Append((char)c);
            }
            return sb.ToString();
        }

        public int GetGlyphIndices(IntPtr hdc, string text, ushort[] glyphs, int[] widths, bool rtl)
        {
            if (String.IsNullOrEmpty(text))
                return 0;

            int destIndex = 0;
            int maxGlyphs = text.Length * 3;
            int maxItems = text.Length * 2;

            List<Run> runs = Itemize(text, rtl, maxItems);
            runs = Layout(runs, rtl);

            foreach (Run run in runs)
            {
                ushort[] tempGlyphs = new ushort[maxGlyphs];
                int[] tempWidths = new int[maxGlyphs];
                int length = GetGlyphs(hdc, run, tempGlyphs, tempWidths, maxGlyphs, rtl);
                Array.Copy(tempGlyphs, 0, glyphs, destIndex, length);
                Array.Copy(tempWidths, 0, widths, destIndex, length);
                destIndex += length;                
            }

            return destIndex;
        }

        private int GetGlyphs(IntPtr hdc, Run run, ushort[] glyphs, int[] widths, int maxGlyphs, bool rtl)
        {
            // initialize structures
            SCRIPT_ANALYSIS psa = run.analysis;
            ushort[] pwLogClust = new ushort[maxGlyphs];
            int pcGlyphs = 0;
            SCRIPT_VISATTR[] psva = new SCRIPT_VISATTR[maxGlyphs];
            GOFFSET[] pGoffset = new GOFFSET[maxGlyphs];
            ABC pABC = new ABC();
            // make glyphs
            ScriptShape(hdc, ref FUSCache, run.text, run.text.Length, glyphs.Length, ref psa, glyphs, pwLogClust, psva, ref pcGlyphs);
            // make widths
            ScriptPlace(hdc, ref FUSCache, glyphs, pcGlyphs, psva, ref psa, widths, pGoffset, ref pABC);
            return pcGlyphs;
        }

        private List<Run> Itemize(string s, bool rtl, int maxItems)
        {
            SCRIPT_ITEM[] pItems = new SCRIPT_ITEM[maxItems];
            int pcItems = 0;

            // initialize Control and State
            SCRIPT_CONTROL control = new SCRIPT_CONTROL();
            SCRIPT_STATE state = new SCRIPT_STATE();
            if (rtl)
            {
                // this is needed to start paragraph from right
                state.SetRtl();
                // to substitute arabic digits
                ScriptApplyDigitSubstitution(ref FDigitSubstitute, ref control, ref state);
            }

            // itemize
            ScriptItemize(s, s.Length, pItems.Length, ref control, ref state, pItems, ref pcItems);

            // create Run list. Note that ScriptItemize actually returns pcItems+1 items, 
            // so this can be used to calculate char range easily
            List<Run> list = new List<Run>();
            for (int i = 0; i < pcItems; i++)
            {
                string text = s.Substring(pItems[i].iCharPos, pItems[i + 1].iCharPos - pItems[i].iCharPos);
                list.Add(new Run(text, pItems[i].analysis));
            }

            return list;
        }

        private List<Run> Layout(List<Run> runs, bool rtl)
        {
            byte[] pbLevel = new byte[runs.Count];
            int[] piVisualToLogical = new int[runs.Count];

            // build the pbLevel array
            for (int i = 0; i < runs.Count; i++)
                pbLevel[i] = (byte)runs[i].analysis.state.uBidiLevel;

            // layout runs
            ScriptLayout(runs.Count, pbLevel, piVisualToLogical, null);

            // return runs in their visual order
            List<Run> visualRuns = new List<Run>();
            for (int i = 0; i < piVisualToLogical.Length; i++)
                visualRuns.Add(runs[piVisualToLogical[i]]);

            return visualRuns;
        }

        public string GetFontName()
        {
            // get the english name of a font
            string fontName = SourceFont.FontFamily.GetName(1033);
            StringBuilder Result = new StringBuilder(fontName.Length * 3);
            foreach (char c in fontName)
            {                
                switch (c)
                {
                    case ' ':
                    case '%':
                    case '(':
                    case ')':
                    case '<':
                    case '>':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case '/':
                    case '#':
                        Result.Append("#");
                        Result.Append(((int)c).ToString("X2"));
                        break;
                    default:
                        if ((int)c > 126 || (int)c < 32)
                        {
                            Result.Append('#');
                            Result.Append(((int)c).ToString("X2"));
                        }
                        else
                            Result.Append(c);
                        break;
                }
            }
            StringBuilder Style = new StringBuilder(9);
            if ((SourceFont.Style & FontStyle.Bold) > 0)
                Style.Append("Bold");
            if ((SourceFont.Style & FontStyle.Italic) > 0)
                Style.Append("Italic");            
            if (Style.Length > 0)
                Result.Append(",").Append(Style.ToString());
            return Result.ToString();
        }

        public string GetPANOSE()
        {
            //01 05 02 02 04 00 00 00 00 00 00 00
            StringBuilder panose = new StringBuilder(36);
            panose.Append("01 05 ");
            panose.Append(TextMetric.otmPanoseNumber.bFamilyType.ToString("X")).Append(" ");
            panose.Append(TextMetric.otmPanoseNumber.bSerifStyle.ToString("X")).Append(" ");
            panose.Append(TextMetric.otmPanoseNumber.bWeight.ToString("X")).Append(" ");
            panose.Append(TextMetric.otmPanoseNumber.bProportion.ToString("X")).Append(" ");
            panose.Append(TextMetric.otmPanoseNumber.bContrast.ToString("X")).Append(" ");
            panose.Append(TextMetric.otmPanoseNumber.bStrokeVariation.ToString("X")).Append(" ");
            panose.Append(TextMetric.otmPanoseNumber.ArmStyle.ToString("X")).Append(" ");
            panose.Append(TextMetric.otmPanoseNumber.bLetterform.ToString("X")).Append(" ");
            panose.Append(TextMetric.otmPanoseNumber.bMidline.ToString("X")).Append(" ");
            panose.Append(TextMetric.otmPanoseNumber.bXHeight.ToString("X"));
            return panose.ToString();
        }

        #endregion

        public void Cleanup()
        {
            // free cache
            ScriptFreeCache(ref FUSCache);
            tempBitmap.Dispose();
            SourceFont.Dispose();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PDFFont(Font font)
        {
            FDpiFX = 96f / DrawUtils.ScreenDpi;
            SourceFont = new Font(font.Name, 750 * FDpiFX, font.Style);
            Saved = false;
            TextMetric = new OutlineTextMetric();
            UsedAlphabet = new List<ushort>();
            UsedAlphabetUnicode = new List<ushort>();
            Widths = new List<int>();
            tempBitmap = new Bitmap(1, 1);
            FUSCache = IntPtr.Zero;
            FDigitSubstitute = new SCRIPT_DIGITSUBSTITUTE();
            ScriptRecordDigitSubstitution(0x0400, ref FDigitSubstitute);
        }


        private class Run
        {
            public SCRIPT_ANALYSIS analysis;
            public string text;

            public Run(string text, SCRIPT_ANALYSIS a)
            {
                this.text = text;
                this.analysis = a;
            }
        }
    }
}
