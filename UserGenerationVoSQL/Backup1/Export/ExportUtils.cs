using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Globalization;
using System.IO;
using FastReport.Utils;
using System.IO.Compression;
using FastReport.Format;


namespace FastReport.Export
{
    internal static class ExportUtils
    {
        public const string XConv = "0123456789ABCDEF";
        private const int BASE = 65521;
        private const int NMAX = 5552;

        private static long[] CrcTable = {
            0000000000, 1996959894, 3993919788, 2567524794,
            0124634137, 1886057615, 3915621685, 2657392035,
            0249268274, 2044508324, 3772115230, 2547177864,
            0162941995, 2125561021, 3887607047, 2428444049,
            0498536548, 1789927666, 4089016648, 2227061214,
            0450548861, 1843258603, 4107580753, 2211677639,
            0325883990, 1684777152, 4251122042, 2321926636,
            0335633487, 1661365465, 4195302755, 2366115317,
            0997073096, 1281953886, 3579855332, 2724688242,
            1006888145, 1258607687, 3524101629, 2768942443,
            0901097722, 1119000684, 3686517206, 2898065728,
            0853044451, 1172266101, 3705015759, 2882616665,
            0651767980, 1373503546, 3369554304, 3218104598,
            0565507253, 1454621731, 3485111705, 3099436303,
            0671266974, 1594198024, 3322730930, 2970347812,
            0795835527, 1483230225, 3244367275, 3060149565,
            1994146192, 0031158534, 2563907772, 4023717930,
            1907459465, 0112637215, 2680153253, 3904427059,
            2013776290, 0251722036, 2517215374, 3775830040,
            2137656763, 0141376813, 2439277719, 3865271297,
            1802195444, 0476864866, 2238001368, 4066508878,
            1812370925, 0453092731, 2181625025, 4111451223,
            1706088902, 0314042704, 2344532202, 4240017532,
            1658658271, 0366619977, 2362670323, 4224994405,
            1303535960, 0984961486, 2747007092, 3569037538,
            1256170817, 1037604311, 2765210733, 3554079995,
            1131014506, 0879679996, 2909243462, 3663771856,
            1141124467, 0855842277, 2852801631, 3708648649,
            1342533948, 0654459306, 3188396048, 3373015174,
            1466479909, 0544179635, 3110523913, 3462522015,
            1591671054, 0702138776, 2966460450, 3352799412,
            1504918807, 0783551873, 3082640443, 3233442989,
            3988292384, 2596254646, 0062317068, 1957810842,
            3939845945, 2647816111, 0081470997, 1943803523,
            3814918930, 2489596804, 0225274430, 2053790376,
            3826175755, 2466906013, 0167816743, 2097651377,
            4027552580, 2265490386, 0503444072, 1762050814,
            4150417245, 2154129355, 0426522225, 1852507879,
            4275313526, 2312317920, 0282753626, 1742555852,
            4189708143, 2394877945, 0397917763, 1622183637,
            3604390888, 2714866558, 0953729732, 1340076626,
            3518719985, 2797360999, 1068828381, 1219638859,
            3624741850, 2936675148, 0906185462, 1090812512,
            3747672003, 2825379669, 0829329135, 1181335161,
            3412177804, 3160834842, 0628085408, 1382605366,
            3423369109, 3138078467, 0570562233, 1426400815,
            3317316542, 2998733608, 0733239954, 1555261956,
            3268935591, 3050360625, 0752459403, 1541320221,
            2607071920, 3965973030, 1969922972, 0040735498,
            2617837225, 3943577151, 1913087877, 0083908371,
            2512341634, 3803740692, 2075208622, 0213261112,
            2463272603, 3855990285, 2094854071, 0198958881,
            2262029012, 4057260610, 1759359992, 0534414190,
            2176718541, 4139329115, 1873836001, 0414664567,
            2282248934, 4279200368, 1711684554, 0285281116,
            2405801727, 4167216745, 1634467795, 0376229701,
            2685067896, 3608007406, 1308918612, 0956543938,
            2808555105, 3495958263, 1231636301, 1047427035,
            2932959818, 3654703836, 1088359270, 0936918000,
            2847714899, 3736837829, 1202900863, 0817233897,
            3183342108, 3401237130, 1404277552, 0615818150,
            3134207493, 3453421203, 1423857449, 0601450431,
            3009837614, 3294710456, 1567103746, 0711928724,
            3020668471, 3272380065, 1510334235, 0755167117
        };

        public static string FloatToString(float value)
        {
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberGroupSeparator = String.Empty;            
            provider.NumberDecimalSeparator = ".";
            return Convert.ToString(Math.Round(value, 2), provider);
        }

        public static bool ParseTextToDecimal(string text, FormatBase format, out decimal value)
        {
            value = 0;
            if (format is NumberFormat || format is CurrencyFormat)
                return decimal.TryParse(text, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out value);
            return false;
        }

        public static string HTMLColor(Color color)
        {
            StringBuilder sb = new StringBuilder(7);
            return sb.Append("#").Append(ByteToHex(color.R)).Append(ByteToHex(color.G)).Append(ByteToHex(color.B)).ToString();           
        }

        public static string ByteToHex(byte Byte)
        {
            StringBuilder sb = new StringBuilder(2);
            return sb.Append(XConv[(Byte >> 4)].ToString()).Append(XConv[(Byte & 0xF)].ToString()).ToString();
        }

        public static string UInt16Tohex(UInt16 word)
        {
            StringBuilder sb = new StringBuilder(4);
            return sb.Append(ByteToHex((byte)((word >> 8) & 0xFF))).Append(ByteToHex((byte)(word & 0xFF))).ToString();
        }

        public static string TruncReturns(string Str)
        {
            int l;
            l = Str.Length;
            if ((l > 1) && (Str.Substring(l - 2, 2) == "\r\n"))
                return Str.Substring(0, l - 2);
            else
                return Str;
        }

        private static string HtmlString(string text, bool htmlTags, bool xmlCRLF)
        {
          StringBuilder Result = new StringBuilder();
          if (text.Length == 1 && text[0] == ' ' && !xmlCRLF)
            Result.Append("&nbsp;");
          else
          {
            for (int i = 0; i < text.Length; i++)
            {
              string tag = String.Empty;
              bool match = false;
              if (htmlTags && text[i] == '<')
              {
                // <b>, <i>, <u>
                if (i + 3 <= text.Length)
                {
                  tag = text.Substring(i, 3).ToLower();
                  match = tag == "<b>" || tag == "<i>" || tag == "<u>";
                  if (match)
                    i += 3;
                }

                // </b>, </i>, </u>
                if (!match && i + 4 <= text.Length && text[i + 1] == '/')
                {
                  tag = text.Substring(i, 4).ToLower();
                  match = tag == "</b>" || tag == "</i>" || tag == "</u>";
                  if (match)
                    i += 4;
                }

                // <sub>, <sup>
                if (!match && i + 5 <= text.Length)
                {
                  tag = text.Substring(i, 5).ToLower();
                  match = tag == "<sub>" || tag == "<sup>";
                  if (match)
                    i += 5;
                }

                // </sub>, </sup>
                if (!match && i + 6 <= text.Length && text[i + 1] == '/')
                {
                  match = true;
                  tag = text.Substring(i, 6).ToLower();
                  match = tag == "</sub>" || tag == "</sup>";
                  if (match)
                    i += 6;
                }

                // <strike>
                if (!match && i + 8 <= text.Length)
                {
                  tag = text.Substring(i, 8).ToLower();
                  match = tag == "<strike>";
                  if (match)
                    i += 8;
                }

                // </strike>
                if (!match && i + 9 <= text.Length)
                {
                  tag = text.Substring(i, 9).ToLower();
                  match = tag == "</strike>";
                  if (match)
                    i += 9;
                }

                // <font color
                if (!match && i + 12 < text.Length && text.Substring(i, 12).ToLower() == "<font color=")
                {
                  int start = i + 12;
                  int end = start;
                  for (; end < text.Length && text[end] != '>'; end++)
                  {
                  }

                  if (end < text.Length)
                  {
                    tag = text.Substring(i, 12) + "\"" + text.Substring(start, end - start) + "\">";
                    i = end + 1;
                    match = true;
                  }
                }

                // </font>
                if (!match && i + 7 <= text.Length)
                {
                  tag = text.Substring(i, 7).ToLower();
                  match = tag == "</font>";
                  if (match)
                    i += 7;
                }
              }

              if (match)
              {
                tag = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(tag);
                Result.Append(tag);
                i--;
              }
              else
              {
                if (text[i] == '&')
                  Result.Append("&amp;");
                else if (i < text.Length - 1 && text[i] == '\r' && text[i + 1] == '\n')
                {
                  if (xmlCRLF)
                    Result.Append("&#10;");
                  else
                    Result.Append("<br>");
                  i++;
                }
                else if (text[i] == '"')
                  Result.Append("&quot;");
                else if (text[i] == '<')
                  Result.Append("&lt;");
                else if (text[i] == '>')
                  Result.Append("&gt;");
                else
                  Result.Append(text[i]);
              }
            }
          }
          return Result.ToString();
        }

        public static string QuotedPrintable(byte[] Values)
        {
            StringBuilder sb = new StringBuilder((int)(Values.Length * 1.3));            
            int length = 0;
            foreach (byte c in Values)
            {
                if (length > 73)
                {
                    length = 0;
                    sb.Append("=").AppendLine();
                }
                if (c < 9 || c == 61 || c > 126)
                {
                    sb.Append("=").Append(XConv[(c >> 4)].ToString()).Append(XConv[(c & 0xF)].ToString());
                    length += 3;
                }
                else
                {
                    sb.Append((char)c);
                    length++;
                }
            }
            return sb.ToString();
        }        

        public static string HtmlString(string text, bool htmlTags)
        {
          return HtmlString(text, htmlTags, false);
        }

        public static string XmlString(string Str, bool htmlTags)
        {
          return HtmlString(Str, htmlTags, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static string HtmlURL(string Value)
        {
            StringBuilder Result = new StringBuilder();
            for (int i = 0; i < Value.Length; i++)
            {
                switch (Value[i])
                {
                  case '\\':
                        Result.Append("/");
                        break;
                    case '&':
                    case '<':
                    case '>':
                    case '{':
                    case '}':
                    case ';':
                    case '?':
                    case ' ':
                        Result.Append("%" + ExportUtils.ByteToHex((byte)Value[i]));
                        break;
                    default:
                        Result.Append(Value[i]);
                        break;
                }
            }
            return Result.ToString();
        }

        public static string MD5(string input)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            md5.Initialize();
            byte[] inputBytes = StringToByteArray(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));
            return sb.ToString();            
        }

        public static byte[] MD5buf(byte[] buf, int length)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            md5.Initialize();
            return md5.ComputeHash(buf, 0, length);
        }

        public static string MD5Stream(Stream stream)
        {
            long oldpos = stream.Position;
            stream.Position = 0;
            byte[] buff = new byte[stream.Length];
            stream.Read(buff, 0, (int)stream.Length);
            stream.Position = oldpos;
            byte[] hash = MD5buf(buff, buff.Length);
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));
            return sb.ToString();
        }

        public static void Write(Stream stream, string value)
        {
            byte[] buf = Encoding.UTF8.GetBytes(value);
            stream.Write(buf, 0, buf.Length);
        }

        public static void WriteLn(Stream stream, string value)
        {
            byte[] buf = Encoding.UTF8.GetBytes(value);
            stream.Write(buf, 0, buf.Length);
            stream.WriteByte(13);
            stream.WriteByte(10);
        }

        public static void Write(Stream stream, byte value)
        {
            stream.WriteByte(value);
        }

        public static void GZip(Stream src, Stream dst, string filename)
        {
            long crc = Crc32(src);
            long size = src.Length;

            // gzip header
            dst.WriteByte(0x1F);
            dst.WriteByte(0x8B);
            dst.WriteByte(0x8);
            dst.WriteByte(0x8);

            //MTIME - zero
            dst.WriteByte(0x0);
            dst.WriteByte(0x0);
            dst.WriteByte(0x0);
            dst.WriteByte(0x0);

            dst.WriteByte(0x0);
            dst.WriteByte(0x0);

            byte[] fname = Encoding.UTF8.GetBytes(filename);

            // filename
            dst.Write(fname, 0, fname.Length);
            dst.WriteByte(0x0);            

            src.Position = 0;
            
            using (DeflateStream compressor = new DeflateStream(dst, CompressionMode.Compress, true))
            {
                int bufflength = 2048;
                byte[] buff = new byte[bufflength];
                int i;
                while ((i = src.Read(buff, 0, bufflength)) > 0)
                {                    
                    compressor.Write(buff, 0, i);
                }
            }
            // write crc
            dst.WriteByte((byte)(crc & 0xFF));
            dst.WriteByte((byte)(crc >> 8 & 0xFF));
            dst.WriteByte((byte)(crc >> 16 & 0xFF));
            dst.WriteByte((byte)(crc >> 24 & 0xFF));

            // size
            dst.WriteByte((byte)(size & 0xFF));
            dst.WriteByte((byte)(size >> 8 & 0xFF));
            dst.WriteByte((byte)(size >> 16 & 0xFF));
            dst.WriteByte((byte)(size >> 24 & 0xFF));
        }

        private static long Crc32(Stream src)
        {
            long oldPos = src.Position;
            src.Position = 0;
            long c = 0xffffffff;            
            while (src.Position < src.Length)
            {
                byte b = (byte)src.ReadByte();
                c = CrcTable[(c ^ b) & 0xff] ^ (c >> 8);
            }
            src.Position = 0;
            return c ^ 0xffffffff;
        }

        public static void ZLibDeflate(Stream src, Stream dst)
        {
            dst.WriteByte(0x78);
            dst.WriteByte(0xDA);
            src.Position = 0;
            long adler = 1L;
            using (DeflateStream compressor = new DeflateStream(dst, CompressionMode.Compress, true))
            {
                int bufflength = 2048;
                byte[] buff = new byte[bufflength];
                int i;
                while ((i = src.Read(buff, 0, bufflength)) > 0)
                {
                    adler = Adler32(adler, buff, 0, i);
                    compressor.Write(buff, 0, i);
                }
            }
            dst.WriteByte((byte)(adler >> 24 & 0xFF));
            dst.WriteByte((byte)(adler >> 16 & 0xFF));
            dst.WriteByte((byte)(adler >> 8 & 0xFF));
            dst.WriteByte((byte)(adler & 0xFF));
        }

        public static long Adler32(long adler, byte[] buf, int index, int len)
        {
            if (buf == null) { return 1L; }

            long s1 = adler & 0xffff;
            long s2 = (adler >> 16) & 0xffff;
            int k;

            while (len > 0)
            {
                k = len < NMAX ? len : NMAX;
                len -= k;
                while (k >= 16)
                {
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    k -= 16;
                }
                if (k != 0)
                {
                    do
                    {
                        s1 += buf[index++] & 0xff;
                        s2 += s1;
                    }
                    while (--k != 0);
                }
                s1 %= BASE;
                s2 %= BASE;
            }
            return (s2 << 16) | s1;
        }

        public static string ReverseString(string str)
        {
            StringBuilder result = new StringBuilder(str.Length);
            int i, j;
            for (j = 0, i = str.Length - 1; i >= 0; i--, j++)
                result.Append(str[i]);
            return result.ToString();
        }

        public static string StrToHex(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length * 2);
            foreach (char c in s)
                sb.Append(((byte)c).ToString("X2"));
            return sb.ToString();
        }

        public static StringBuilder StrToHex2(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length * 2);
            foreach (char c in s)
                sb.Append(((UInt16)c).ToString("X4"));
            return sb;
        }

        public static string GetID()
        {
            return ExportUtils.MD5(Guid.NewGuid().ToString());
        }

        public static void CopyStream(Stream source, Stream target)
        {           
            source.Position = 0;
            int bufflength = 2048;
            byte[] buff = new byte[bufflength];
            int i;
            while ((i = source.Read(buff, 0, bufflength)) > 0)
                target.Write(buff, 0, i);
        }

        public static byte[] StringToByteArray(string source)
        {
            byte[] result = new byte[source.Length];
            for (int i = 0; i < source.Length; i++)
                result[i] = (byte)source[i];
            return result;
        }

        public static string StringFromByteArray(byte[] array)
        {
            StringBuilder result = new StringBuilder(array.Length);
            foreach (byte b in array)
                result.Append((char)b);
            return result.ToString();
        }

        internal static Color GetColorFromFill(FillBase Fill)
        {
            if (Fill is SolidFill)
                return (Fill as SolidFill).Color;
            else if (Fill is GlassFill)
                return (Fill as GlassFill).Color;
            else if (Fill is HatchFill)
                return (Fill as HatchFill).BackColor;
            else if (Fill is PathGradientFill)
                return (Fill as PathGradientFill).CenterColor;
            else if (Fill is LinearGradientFill)
                return GetMiddleColor((Fill as LinearGradientFill).StartColor, (Fill as LinearGradientFill).EndColor);
            else
                return Color.White;
        }

        private static Color GetMiddleColor(Color color1, Color color2)
        {
            return Color.FromArgb(255, (color1.R + color2.R) / 2, (color1.G + color2.G) / 2, (color1.B + color2.B) / 2);
        }

        public static string GetRFCDate(DateTime datetime)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0:R}", datetime);
            int hours = TimeZone.CurrentTimeZone.GetUtcOffset(datetime).Hours;
            int minutes = TimeZone.CurrentTimeZone.GetUtcOffset(datetime).Minutes;
            if (hours == 0 && minutes == 0)
                return sb.ToString();
            else
            {
                string offset = (hours >= 0 && minutes >= 0 ? "+" : "") + hours.ToString("00") + minutes.ToString("00");
                return sb.ToString().Replace("GMT", offset);
            }
        }
    }

    internal class RC4
    {
        private byte[] fKey;

        public void Start(byte[] key)
        {
            byte[] k = new byte[256];
            int l = key.GetLength(0);
            if (key.Length > 0 && l <= 256)
            {
                for (int i = 0; i < 256; i++)
                {
                    fKey[i] = (byte)i;
                    k[i] = key[i % l];
                }
            }

            byte j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (byte)(j + fKey[i] + k[i]);
                byte tmp = fKey[i];
                fKey[i] = fKey[j];
                fKey[j] = tmp;
            }
        }

        public byte[] Crypt(byte[] source)
        {
            byte i = 0;
            byte j = 0;
            int l = source.GetLength(0);
            byte[] result = new byte[l];
            for (int k = 0; k < l; k++)
            {
                i = (byte)(i + 1);
                j = (byte)(j + fKey[i]);
                byte tmp = fKey[i];
                fKey[i] = fKey[j];
                fKey[j] = tmp;
                result[k] = (byte)(source[k] ^ fKey[(byte)(fKey[i] + fKey[j])]);
            }
            return result;
        }

        public RC4()
        {
            fKey = new byte[256];
        }

    }
}
