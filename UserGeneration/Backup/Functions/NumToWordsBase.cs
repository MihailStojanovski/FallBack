// Based on RSDN RusCurrency class
using System;
using System.Text;
using System.Collections;

namespace FastReport.Functions
{
  internal abstract class NumToWordsBase
  {
    #region Private Methods
    private string Str(decimal value, WordInfo senior, WordInfo junior)
    {
      bool minus = false;
      if (value < 0) 
      { 
        value = -value; 
        minus = true; 
      }

      int n = (int)value;
      int remainder = (int)((value - n + 0.005m) * 100);
      StringBuilder r = new StringBuilder();

      if (n == 0)
        r.Append(GetZero() + " " + senior.Many + " ");
      else
      {
        if (n % 1000 != 0)
          r.Append(Str1000(n, senior));
        else
          r.Append(senior.Many + " ");

        n /= 1000;
        r.Insert(0, Str1000(n, GetThousands()));

        n /= 1000;
        r.Insert(0, Str1000(n, GetMillions()));

        n /= 1000;
        r.Insert(0, Str1000(n, GetMilliards()));

        n /= 1000;
        r.Insert(0, Str1000(n, GetTrillions()));
      }

      if (minus) 
        r.Insert(0, GetMinus() + " ");

      if (junior != null)
      {
        r.Append(GetDecimalSeparator() + remainder.ToString("00 "));
        r.Append(Case(remainder, junior));
      }
   
      r[0] = char.ToUpper(r[0]);
      return r.ToString();
    }

    private string Str1000(int value, WordInfo info)
    {
      bool hasMore = value >= 1000;
      value = value % 1000;
      if (value == 0) 
        return "";

      StringBuilder r = new StringBuilder();
      // add hundred
      string hund = GetHund(info.Male, value / 100);
      if (hund != "")
        r.Append(hund);
      
      // decide whether to use the 100-10 separator or not
      string sep100_10 = Get100_10Separator();
      if (!hasMore && hund == "")
        sep100_10 = "";

      value = value % 100;
      if (value < Get20())
      {
        // value is less than 20, get fixed words
        string frac20 = GetFrac20(info.Male, value);
        if (frac20 != "")
          r.Append(sep100_10 + frac20);
      }
      else
      {
        // value is greater than 20, get tens separately
        string ten = GetTen(info.Male, value / 10);
        string frac10 = GetFrac20(info.Male, value % 10);
        
        // decide whether to use 10-1 separator or not
        if (ten != "" && frac10 != "")
          r.Append(sep100_10 + ten + Get10_1Separator() + frac10);
        else if (ten != "")
          r.Append(sep100_10 + ten);
        else if (frac10 != "")
          r.Append(sep100_10 + frac10);
      }

      // add currency/group word
      r.Append(" ");
      r.Append(Case(value, info));

      // make the result starting with letter and ending with space
      if (r.Length != 0) 
        r.Append(" ");
      return r.ToString().TrimStart(new char[] { ' ' });
    }
    #endregion

    #region Protected Methods
    protected abstract string GetFrac20(bool male, int value);
    protected abstract string GetTen(bool male, int value);
    protected abstract string GetHund(bool male, int value);
    protected abstract WordInfo GetThousands();
    protected abstract WordInfo GetMillions();
    protected abstract WordInfo GetMilliards();
    protected abstract WordInfo GetTrillions();
    protected abstract CurrencyInfo GetCurrency(string currencyName);
    protected abstract string GetZero();
    protected abstract string GetMinus();

    protected virtual int Get20()
    {
      return 20;
    }
    
    protected virtual string GetDecimalSeparator()
    {
      return "";
    }

    protected virtual string Get10_1Separator()
    {
      return " ";
    }

    protected virtual string Get100_10Separator()
    {
      return " ";
    }

    protected virtual string Case(int value, WordInfo info)
    {
      if (value == 1)
        return info.One;
      return info.Many;
    }
    #endregion

    #region Public Methods
    public string ConvertCurrency(decimal value, string currencyName)
    {
      try
      {
        CurrencyInfo currency = GetCurrency(currencyName);
        return Str(value, currency.Senior, currency.Junior);
      }
      catch
      {
        throw new Exception("Currency \"" + currencyName + "\" is not defined in the \"" + GetType().Name + "\" converter.");
      }
    }

    public string ConvertNumber(decimal value, bool male, string one, string two, string many)
    {
      return Str(value, new WordInfo(male, one, two, many), null);
    }

    public string ConvertNumber(decimal value, bool male, 
      string seniorOne, string seniorTwo, string seniorMany,
      string juniorOne, string juniorTwo, string juniorMany)
    {
      return Str(value, 
        new WordInfo(male, seniorOne, seniorTwo, seniorMany), 
        new WordInfo(male, juniorOne, juniorTwo, juniorMany));
    }
    #endregion
  }

  internal class WordInfo
  {
    public bool Male;
    public string One;
    public string Two;
    public string Many;

    public WordInfo(bool male, string one, string two, string many)
    {
      Male = male;
      One = one;
      Two = two;
      Many = many;
    }

    public WordInfo(string one, string many)
      : this(true, one, many, many)
    {
    }

    public WordInfo(string all)
      : this(true, all, all, all)
    {
    }
  }

  internal class CurrencyInfo
  {
    public WordInfo Senior;
    public WordInfo Junior;

    public CurrencyInfo(WordInfo senior, WordInfo junior)
    {
      Senior = senior;
      Junior = junior;
    }
  }
}