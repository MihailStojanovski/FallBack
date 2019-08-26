using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace FastReport.Functions
{
  internal class NumToWordsRu : NumToWordsBase
  {
    private static Dictionary<string, CurrencyInfo> currencyList;

    private static string[] frac20 =
    {
      "", "один", "два", "три", "четыре", "пять", "шесть",
      "семь", "восемь", "девять", "десять", "одиннадцать",
      "двенадцать", "тринадцать", "четырнадцать", "пятнадцать",
      "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать"
    };

    private static string[] tens =
    {
      "", "десять", "двадцать", "тридцать", "сорок", "пятьдесят",
      "шестьдесят", "семьдесят", "восемьдесят", "девяносто"
    };

    private static string[] hunds =
    {
      "", "сто", "двести", "триста", "четыреста",
      "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот"
    };

    private static WordInfo thousands = new WordInfo(false, "тысяча", "тысячи", "тысяч");
    private static WordInfo millions = new WordInfo(true, "миллион", "миллиона", "миллионов");
    private static WordInfo milliards = new WordInfo(true, "миллиард", "миллиарда", "миллиардов");
    private static WordInfo trillions = new WordInfo(true, "триллион", "триллиона", "триллионов");

    protected override string GetFrac20(bool male, int value)
    {
      string result = frac20[value];
      if (!male)
      {
        if (value == 1)
          return "одна";
        if (value == 2)
          return "две";
      }
      return result;
    }

    protected override string GetTen(bool male, int value)
    {
      return tens[value];
    }

    protected override string GetHund(bool male, int value)
    {
      return hunds[value];
    }

    protected override WordInfo GetThousands()
    {
      return thousands;
    }

    protected override WordInfo GetMillions()
    {
      return millions;
    }

    protected override WordInfo GetMilliards()
    {
      return milliards;
    }

    protected override WordInfo GetTrillions()
    {
      return trillions;
    }

    protected override CurrencyInfo GetCurrency(string currencyName)
    {
      currencyName = currencyName.ToUpper();
      return currencyList[currencyName];
    }

    protected override string GetZero()
    {
      return "ноль";
    }

    protected override string GetMinus()
    {
      return "минус";
    }

    protected override string Case(int value, WordInfo info)
    {
      if (value > Get20())
        value = value % 10;

      switch (value)
      {
        case 1:
          return info.One;

        case 2:
        case 3:
        case 4:
          return info.Two;

        default:
          return info.Many;
      }
    }

    static NumToWordsRu()
    {
      currencyList = new Dictionary<string, CurrencyInfo>();
      currencyList.Add("RUR", new CurrencyInfo(
        new WordInfo(true, "рубль", "рубля", "рублей"),
        new WordInfo(false, "копейка", "копейки", "копеек")));
      currencyList.Add("UAH", new CurrencyInfo(
        new WordInfo(false, "гривна", "гривны", "гривен"),
        new WordInfo(false, "копейка", "копейки", "копеек")));
      currencyList.Add("EUR", new CurrencyInfo(
        new WordInfo(true, "евро", "евро", "евро"),
        new WordInfo(true, "евроцент", "евроцента", "евроцентов")));
      currencyList.Add("USD", new CurrencyInfo(
        new WordInfo(true, "доллар", "доллара", "долларов"),
        new WordInfo(true, "цент", "цента", "центов")));
    }
  }
}