using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace FastReport.Barcode
{
  /// <summary>
  /// The base class for EAN barcodes.
  /// </summary>
  public abstract class BarcodeEAN : LinearBarcodeBase
  {
    // Pattern for Barcode EAN Charset C
    // S1   L1   S2   L2
    internal static string[] tabelle_EAN_C = {
      "7150",    // 0 
      "6160",    // 1 
      "6061",    // 2 
      "5350",    // 3 
      "5071",    // 4 
      "5170",    // 5 
      "5053",    // 6 
      "5251",    // 7 
      "5152",    // 8 
      "7051"     // 9 
    };

    // Pattern for Barcode EAN Charset A
    // L1   S1   L2   S2
    internal static string[] tabelle_EAN_A = {  
      "2605",    //0 
      "1615",    //1 
      "1516",    //2 
      "0805",    //3 
      "0526",    //4 
      "0625",    //5 
      "0508",    //6 
      "0706",    //7 
      "0607",    //8 
      "2506"     //9 
    };

    // Pattern for Barcode EAN Zeichensatz B}
    //L1   S1   L2   S2
    internal static string[] tabelle_EAN_B = {
      "0517",    // 0 
      "0616",    // 1 
      "1606",    // 2 
      "0535",    // 3 
      "1705",    // 4 
      "0715",    // 5 
      "3505",    // 6 
      "1525",    // 7 
      "2515",    // 8 
      "1507"     // 9 
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="BarcodeEAN"/> class with default settings.
    /// </summary>
    public BarcodeEAN()
    {
      FRatioMin = 2;
      FRatioMax = 3;
    }
  }


  /// <summary>
  /// Generates the EAN8 barcode.
  /// </summary>
  public class BarcodeEAN8 : BarcodeEAN
  {
    internal override void DrawText(Graphics g, string barData)
    {
      // parts of pattern: 3 + 16 + 5 + 16 + 3
      float x1 = GetWidth(FPattern.Substring(0, 3));
      float x2 = GetWidth(FPattern.Substring(0, 3 + 16 + 1));
      DrawString(g, x1, x2, barData.Substring(0, 4));

      x1 = GetWidth(FPattern.Substring(0, 3 + 16 + 5 - 1));
      x2 = GetWidth(FPattern.Substring(0, 3 + 16 + 5 + 16));
      DrawString(g, x1, x2, barData.Substring(4, 4));
    }

    internal override string GetPattern()
    {
      Text = DoCheckSumming(Text, 8);

      string result = "A0A";   // Startcode

      for (int i = 0; i <= 3; i++)
      {
        result += tabelle_EAN_A[CharToInt(Text[i])];
      }
      
      result += "0A0A0";   // Center Guard Pattern

      for (int i = 4; i <= 7; i++)
      {
        result += tabelle_EAN_C[CharToInt(Text[i])];
      }
      
      result += "A0A";   // Stopcode
      return result;
    }
  }

  /// <summary>
  /// Generates the EAN13 barcode.
  /// </summary>
  public class BarcodeEAN13 : BarcodeEAN
  {
    //Zuordung der Paraitaetsfolgen f�r EAN13
    private static string[,] tabelle_ParityEAN13 = {
      {"A", "A", "A", "A", "A", "A"},    // 0 
      {"A", "A", "B", "A", "B", "B"},    // 1 
      {"A", "A", "B", "B", "A", "B"},    // 2 
      {"A", "A", "B", "B", "B", "A"},    // 3 
      {"A", "B", "A", "A", "B", "B"},    // 4 
      {"A", "B", "B", "A", "A", "B"},    // 5 
      {"A", "B", "B", "B", "A", "A"},    // 6 
      {"A", "B", "A", "B", "A", "B"},    // 7 
      {"A", "B", "A", "B", "B", "A"},    // 8 
      {"A", "B", "B", "A", "B", "A"}     // 9 
    };

    internal override void DrawText(Graphics g, string barData)
    {
      DrawString(g, -8, -2, barData.Substring(0, 1));

      // parts of pattern: 3 + 24 + 5 + 24 + 3
      float x1 = GetWidth(FPattern.Substring(0, 3));
      float x2 = GetWidth(FPattern.Substring(0, 3 + 24 + 1));
      DrawString(g, x1, x2, barData.Substring(1, 6));

      x1 = GetWidth(FPattern.Substring(0, 3 + 24 + 5 - 1));
      x2 = GetWidth(FPattern.Substring(0, 3 + 24 + 5 + 24));
      DrawString(g, x1, x2, barData.Substring(7, 6));
    }

    internal override string GetPattern()
    {
      int LK;
      Text = DoCheckSumming(Text, 13);
      string tmp = Text;
      LK = CharToInt(tmp[0]);
      tmp = tmp.Substring(1, 12);

      string result = "A0A";   // Startcode

      for (int i = 0; i <= 5; i++)
      {
        int idx = CharToInt(tmp[i]);

        switch (tabelle_ParityEAN13[LK, i])
        {
          case "A":
            result += tabelle_EAN_A[idx];
            break;
          case "B":
            result += tabelle_EAN_B[idx];
            break;
          case "C":
            result += tabelle_EAN_C[idx];
            break;
        }
      }

      result += "0A0A0";   //Center Guard Pattern

      for (int i = 6; i < 12; i++)
      {
        result += tabelle_EAN_C[CharToInt(tmp[i])];
      }  

      result += "A0A";   // Stopcode
      return result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarcodeEAN13"/> class with default settings.
    /// </summary>
    public BarcodeEAN13()
    {
      FExtra1 = 8;
    }
  }
}
