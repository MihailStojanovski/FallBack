using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Drawing.Imaging;

namespace FastReport.Utils
{
  /// <summary>
  /// Contains methods that peform string to object and vice versa conversions.
  /// </summary>
  public static class Converter
  {
    /// <summary>
    /// Converts an object to a string.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The string that contains the converted value.</returns>
    public static string ToString(object value)
    {
      if (value == null)
        return "";
      if (value is string)
        return (string)value;
      if (value is float)
        return ((float)value).ToString(CultureInfo.InvariantCulture.NumberFormat);
      if (value is Enum)
        return Enum.Format(value.GetType(), value, "G");
      if (value is Image)
      {
        using (MemoryStream stream = new MemoryStream())
        {
          ImageHelper.Save(value as Image, stream);
          return Convert.ToBase64String(stream.ToArray());
        }
      }
      if (value is Stream)
      {
        Stream stream = value as Stream;
        byte[] bytes = new byte[stream.Length];
        stream.Position = 0;
        stream.Read(bytes, 0, bytes.Length);
        return Convert.ToBase64String(bytes);
      }
      if (value is string[])
      {
        string result = "";
        foreach (string s in (value as string[]))
        {
          result += s + "\r\n";
        }
        if (result.EndsWith("\r\n"))
          result = result.Remove(result.Length - 2);
        return result;
      }
      if (value is Type)
      {
        Type type = (Type)value;
        bool shortTypeName = type.Assembly.FullName.StartsWith("mscorlib") ||
          type.Assembly == typeof(Converter).Assembly;

        if (shortTypeName)
          return type.FullName;
        return type.AssemblyQualifiedName;
      }
      return TypeDescriptor.GetConverter(value).ConvertToInvariantString(value);
    }

    /// <summary>
    /// Converts a value to a string using the specified converter.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <param name="converterType">The type of converter.</param>
    /// <returns>The string that contains the converted value.</returns>
    public static string ToString(object value, Type converterType)
    {
      TypeConverter converter = Activator.CreateInstance(converterType) as TypeConverter;
      return converter.ConvertToInvariantString(value);
    }

    /// <summary>
    /// Converts a string value to the specified data type.
    /// </summary>
    /// <param name="type">The data type to convert to.</param>
    /// <param name="value">The string to convert from.</param>
    /// <returns>The object of type specified in the <b>type</b> parameter that contains 
    /// a converted value.</returns>
    public static object FromString(Type type, string value)
    {
      if (type == typeof(string))
        return value;
      if (value == null || value == "")
        return null;
      if (type == typeof(float))
        return float.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
      if (type == typeof(Enum))
        return Enum.Parse(type, value);
      if (type == typeof(Image) || type == typeof(Bitmap))
      {
        using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(value)))
        {
          return ImageHelper.Load(stream);
        }
      }
      if (type == typeof(Stream))
      {
        return new MemoryStream(Convert.FromBase64String(value));
      }
      if (type == typeof(Type))
        return Type.GetType(value);
      if (type == typeof(string[]))
      {
        value = value.Replace("\r\n", "\r");
        return value.Split(new char[] { '\r' });
      }
      if (type == typeof(Color))
        return new ColorConverter().ConvertFromInvariantString(value);
      return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
    }

    /// <summary>
    /// Converts a string to an object using the specified converter.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="converterType">The type of converter.</param>
    /// <returns>The object that contains the converted value.</returns>
    public static object FromString(string value, Type converterType)
    {
      TypeConverter converter = Activator.CreateInstance(converterType) as TypeConverter;
      return converter.ConvertFromInvariantString(value);
    }

    /// <summary>
    /// Converts a string containing special symbols to the xml-compatible string.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>The result string.</returns>
    /// <remarks>
    /// This method replaces some special symbols like &lt;, &gt; into xml-compatible 
    /// form: &amp;lt;, &amp;gt;. To convert such string back to original form, use the
    /// <see cref="FromXml"/> method.
    /// </remarks>
    public static string ToXml(string s)
    {
      return ToXml(s, true);
    }

    /// <summary>
    /// Converts a string containing special symbols to the xml-compatible string.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <param name="convertCrlf">Determines whether it is necessary to convert cr-lf symbols to xml form.</param>
    /// <returns>The result string.</returns>
    public static string ToXml(string s, bool convertCrlf)
    {
      StringBuilder result = new StringBuilder(s.Length * 2);
      for (int i = 0; i < s.Length; i++)
      {
        switch (s[i])
        {
          case '\n':
          case '\r':
            if (convertCrlf)
              result.Append("&#" + (int)s[i] + ";");
            else
              result.Append(s[i]);
            break;
          case '"':
            result.Append("&quot;");
            break;
          case '&':
            result.Append("&amp;");
            break;
          case '<':
            result.Append("&lt;");
            break;
          case '>':
            result.Append("&gt;");
            break;
          default:
            result.Append(s[i]);
            break;
        }
      }
      return result.ToString();
    }

    /// <summary>
    /// Converts a value to xml-compatible string.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The result string.</returns>
    public static string ToXml(object value)
    {
      return ToXml(ToString(value));
    }
    
    /// <summary>
    /// Convert the xml-compatible string to the regular one.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>The result string.</returns>
    /// <remarks>
    /// This is counterpart to the <see cref="ToXml(string)"/> method.
    /// </remarks>
    public static string FromXml(string s)
    {
      StringBuilder result = new StringBuilder(s.Length);
      int i = 0;
      while (i < s.Length)
      {
        if (s[i] == '&')
        {
          if (i + 3 < s.Length && s[i + 1] == '#')
          {
            int j = i + 3;
            while (s[j] != ';')
              j++;
            result.Append((char)int.Parse(s.Substring(i + 2, j - i - 2)));
            i = j;
          }
          else if (i + 5 < s.Length && s.Substring(i + 1, 5) == "quot;")
          {
            result.Append('"');
            i += 5;
          }
          else if (i + 4 < s.Length && s.Substring(i + 1, 4) == "amp;")
          {
            result.Append('&');
            i += 4;
          }
          else if (i + 3 < s.Length && s.Substring(i + 1, 3) == "lt;")
          {
            result.Append('<');
            i += 3;
          }
          else if (i + 3 < s.Length && s.Substring(i + 1, 3) == "gt;")
          {
            result.Append('>');
            i += 3;
          }
          else
            result.Append(s[i]);
        }
        else
          result.Append(s[i]);
        i++;
      }
      return result.ToString();
    }

    /// <summary>
    /// Decreases the precision of floating-point value.
    /// </summary>
    /// <param name="value">The initial value.</param>
    /// <param name="precision">The number of decimal digits in the fraction.</param>
    /// <returns>The value with lesser precision.</returns>
    public static float DecreasePrecision(float value, int precision)
    {
      return (float)Math.Round(value, precision);
    }
    
    /// <summary>
    /// Converts a string value to the float.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>The float value.</returns>
    /// <remarks>
    /// Both "." or "," decimal separators are allowed.
    /// </remarks>
    public static float StringToFloat(string value)
    {
      return (float)FromString(typeof(float), value.Replace(',', '.'));
    }

    /// <summary>
    /// Converts a string value to the float.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <param name="removeNonDigit">Indicates whether to ignore non-digit symbols.</param>
    /// <returns>The float value.</returns>
    public static float StringToFloat(string value, bool removeNonDigit)
    {
      string result = "";
      for (int i = 0; i < value.Length; i++)
      {
        if (value[i] >= '0' && value[i] <= '9')
          result += value[i];
        if (value[i] == '.' || value[i] == ',')
          result += '.';  
      }
      try
      {
        if (result != "")
          return (float)FromString(typeof(float), result);
      }
      catch
      {
      }  
      return 0;  
    }
    
    /// <summary>
    /// Converts the string containing several text lines to a collection of strings.
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <param name="list">The collection instance.</param>
    public static void StringToIList(string text, IList list)
    {
      list.Clear();
      string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
      foreach (string s in lines)
      {
        list.Add(s);
      }
    }
    
    /// <summary>
    /// Converts a collection of strings to a string.
    /// </summary>
    /// <param name="list">The collection to convert.</param>
    /// <returns>The string that contains all lines from the collection.</returns>
    public static string IListToString(IList list)
    {
      string text = "";
      foreach (object obj in list)
      {
        text += obj.ToString() + "\r\n";
      }
      if (text.EndsWith("\r\n"))
        text = text.Remove(text.Length - 2);
      return text;  
    }

    /// <summary>
    /// Converts <b>null</b> value to 0, false, empty string, depending on <b>type</b>.
    /// </summary>
    /// <param name="type">The data type.</param>
    /// <returns>The value of the <b>type</b> data type.</returns>
    public static object ConvertNull(Type type)
    {
      if (type == typeof(string))
        return "";
      else if (type == typeof(char))
        return ' ';
      else if (type == typeof(DateTime))
        return DateTime.MinValue;
      else if (type == typeof(TimeSpan))
        return TimeSpan.Zero;
      else if (type == typeof(bool))
        return false;
      else if (type == typeof(byte[]))
        return null;
      else if (type.IsClass)
        return null;
      else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        return null;
      return Convert.ChangeType(0, type);
    }

    /// <summary>
    /// Converts <b>string</b> value to <b>byte[]</b>.
    /// </summary>
    /// <param name="Str">The string to convert</param>
    /// <returns>The value of the <b>byte[]</b> data type.</returns>
    public static byte[] StringToByteArray(string Str)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        return encoding.GetBytes(Str);
    }

  }
}