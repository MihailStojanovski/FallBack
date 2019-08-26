using System;
using System.Text;
using System.Drawing;

namespace FastReport.Barcode
{
  /// <summary>
  /// Generates the UPC E0 barcode.
  /// </summary>
  public class BarcodeUPC_E0 : BarcodeEAN
  {
    // UPC E Parity Pattern Table , Number System 0
    internal static string[,] tabelle_UPC_E0 = {
      {"E", "E", "E", "o", "o", "o" },    // 0
      {"E", "E", "o", "E", "o", "o" },    // 1
      {"E", "E", "o", "o", "E", "o" },    // 2
      {"E", "E", "o", "o", "o", "E" },    // 3
      {"E", "o", "E", "E", "o", "o" },    // 4
      {"E", "o", "o", "E", "E", "o" },    // 5
      {"E", "o", "o", "o", "E", "E" },    // 6
      {"E", "o", "E", "o", "E", "o" },    // 7
      {"E", "o", "E", "o", "o", "E" },    // 8
      {"E", "o", "o", "E", "o", "E" }     // 9
    };

    internal override void DrawText(Graphics g, string barData)
    {
      DrawString(g, -8, -2, "0", true);
      
      // parts of pattern: 3 + 24 + 6
      float x1 = GetWidth(FPattern.Substring(0, 3));
      float x2 = GetWidth(FPattern.Substring(0, 3 + 24));
      DrawString(g, x1, x2, barData.Substring(0, 6));

      x1 = GetWidth(FPattern) + 1;
      x2 = x1 + 7;
      DrawString(g, x1, x2, barData.Substring(6, 1), true);
    }

    internal override string GetPattern()
    {
      Text = DoCheckSumming(Text, 7);
      int c = CharToInt(Text[6]);

      // Startcode
      string result = "A0A";

      for (int i = 0; i <= 5; i++)
      {
        if (tabelle_UPC_E0[c, i] == "E")
        {
          for (int j = 0; j <= 3; j++)
            result += tabelle_EAN_C[CharToInt(Text[i])][3 - j];
        }
        else
          result += tabelle_EAN_A[CharToInt(Text[i])];
      }

      // Stopcode
      result += "0A0A0A";
      return result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarcodeUPC_E0"/> class with default settings.
    /// </summary>
    public BarcodeUPC_E0()
    {
      FExtra1 = 8;
      FExtra2 = 8;
    }
  }

  /// <summary>
  /// Generates the UPC E1 barcode.
  /// </summary>
  public class BarcodeUPC_E1 : BarcodeUPC_E0
  {
    internal override string GetPattern()
    {
      Text = DoCheckSumming(Text, 7);
      int c = CharToInt(Text[6]);

      // Startcode
      string result = "A0A";
      for (int i = 0; i <= 5; i++)
      {
        if (tabelle_UPC_E0[c, i] == "E")
          result += tabelle_EAN_A[CharToInt(Text[i])];
        else
        {
          for (int j = 0; j <= 3; j++)
            result += tabelle_EAN_C[CharToInt(Text[i])][3 - j];
        }    
      }

      // Stopcode
      result += "0A0A0A";
      return result;
    }
  }

  /// <summary>
  /// Generates the UPC A barcode.
  /// </summary>
  public class BarcodeUPC_A : BarcodeUPC_E0
  {
    internal override void DrawText(Graphics g, string barData)
    {
      DrawString(g, -8, -2, barData.Substring(0, 1), true);

      // parts of pattern: 7 + 20 + 5 + 20 + 7
      float x1 = GetWidth(FPattern.Substring(0, 7));
      float x2 = GetWidth(FPattern.Substring(0, 7 + 20));
      DrawString(g, x1, x2, barData.Substring(1, 5));

      x1 = GetWidth(FPattern.Substring(0, 7 + 20 + 5));
      x2 = GetWidth(FPattern.Substring(0, 7 + 20 + 5 + 20));
      DrawString(g, x1, x2, barData.Substring(6, 5));

      x1 = GetWidth(FPattern) + 1;
      x2 = x1 + 7;
      DrawString(g, x1, x2, barData.Substring(11, 1), true);
    }

    internal override string GetPattern()
    {
      Text = DoCheckSumming(Text, 12);

      //Startcode
      string result = "A0A";
      for (int i = 0; i <= 5; i++)
      {
        string code = tabelle_EAN_A[CharToInt(Text[i])];
        result += i == 0 ? MakeLong(code) : code;
      }  

      //Trennzeichen
      result += "0A0A0";
      for (int i = 6; i <= 11; i++)
      {
        string code = tabelle_EAN_C[CharToInt(Text[i])];
        result += i == 11 ? MakeLong(code) : code;
      }  

      //Stopcode
      return result + "A0A";
    }
  }

  /// <summary>
  /// Generates the 2-digit supplement barcode.
  /// </summary>
  public class BarcodeSupplement2 : BarcodeUPC_E0
  {
    internal string getSupp(string Nr)
    {
      int sum = 0;
      string tmp = Nr.Substring(0, Nr.Length - 1);

      int fak = tmp.Length;

      for (int i = 0; i < tmp.Length; i++)
      {
        sum += int.Parse(tmp[i].ToString()) * ((fak % 2) == 0 ? 9 : 3);
        fak--;
      }

      sum = ((sum % 10) % 10) % 10;
      return tmp + sum.ToString();
    }

    internal override void DrawText(Graphics g, string barData)
    {
      DrawString(g, 0, FDrawArea.Width, barData);
    }

    internal override string GetPattern()
    {
      Text = SetLen(2);

      int i32 = int.Parse(Text);
      string mS;

      switch (i32 % 4)
      {
        case 0:
          mS = "oo";
          break;
        case 1:
          mS = "oE";
          break;
        case 2:
          mS = "Eo";
          break;
        case 3:
          mS = "EE";
          break;
        default:
          mS = "";
          break;
      }

      string tmp = getSupp(Text + "0");

      string result = "506";   // Startcode
      for (int i = 0; i <= 1; i++)
      {
        if (mS[i] == 'E')
        {
          for (int j = 0; j <= 3; j++)
            result += tabelle_EAN_C[CharToInt(tmp[i])][3 - j];
        }
        else
          result += tabelle_EAN_A[CharToInt(tmp[i])];

        if (i < 1)
          result += "05"; // character delineator 
      }

      return result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarcodeSupplement2"/> class with default settings.
    /// </summary>
    public BarcodeSupplement2()
    {
      FTextUp = true;
      FExtra1 = 0;
      FExtra2 = 0;
    }
  }

  /// <summary>
  /// Generates the 5-digit supplement barcode.
  /// </summary>
  internal class BarcodeSupplement5 : BarcodeSupplement2
  {
    internal override string GetPattern()
    {
      Text = SetLen(5);
      string tmp = getSupp(Text + "0");
      char c = tmp[5];

      string result = "506";   // Startcode

      for (int i = 0; i <= 4; i++)
      {
        if (tabelle_UPC_E0[CharToInt(c), i] == "E")
        {
          for (int j = 0; j <= 3; j++)
            result += tabelle_EAN_C[CharToInt(tmp[i])][3 - j];
        }
        else
          result += tabelle_EAN_A[CharToInt(tmp[i])];

        if (i < 4)
          result += "05"; // character delineator 
      }

      return result;
    }
  }
}
