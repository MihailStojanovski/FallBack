using System;
using System.Collections.Generic;
using System.Text;

namespace FastReport.Barcode
{
  /// <summary>
  /// Generates the Codabar barcode.
  /// </summary>
  public class BarcodeCodabar : LinearBarcodeBase
  {
    private struct Codabar
    {
      public string c;
      public string data;
      public Codabar(string c, string data)
      {
        this.c = c;
        this.data = data;
      }
    }

    private static Codabar[] tabelle_cb = {
      new Codabar("1", "5050615"),
      new Codabar("2", "5051506"),
      new Codabar("3", "6150505"),
      new Codabar("4", "5060515"),
      new Codabar("5", "6050515"),
      new Codabar("6", "5150506"),
      new Codabar("7", "5150605"),
      new Codabar("8", "5160505"),
      new Codabar("9", "6051505"),
      new Codabar("0", "5050516"),
      new Codabar("-", "5051605"),
      new Codabar("$", "5061505"),
      new Codabar(":", "6050606"),
      new Codabar("/", "6060506"),
      new Codabar(".", "6060605"),
      new Codabar("+", "5060606"),
      new Codabar("A", "5061515"),
      new Codabar("B", "5151506"),
      new Codabar("C", "5051516"),
      new Codabar("D", "5051615") };

    /// <inheritdoc/>
    public override bool IsNumeric
    {
      get { return false; }
    }

    private int FindBarItem(string c)
    {
      for (int i = 0; i < tabelle_cb.Length; i++)
      {
        if (c == tabelle_cb[i].c)
          return i;
      }
      return -1;
    }

    internal override string GetPattern()
    {
      string result = tabelle_cb[FindBarItem("A")].data + "0";

      foreach (char c in Text)
      {
        int idx = FindBarItem(c.ToString());
        result += tabelle_cb[idx].data + "0";
      }
      
      result += tabelle_cb[FindBarItem("B")].data;
      return result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarcodeCodabar"/> class with default settings.
    /// </summary>
    public BarcodeCodabar()
    {
      FRatioMin = 2;
      FRatioMax = 3;
    }
  }
}
