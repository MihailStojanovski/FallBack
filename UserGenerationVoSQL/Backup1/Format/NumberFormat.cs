using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using FastReport.Utils;
using System.Globalization;

namespace FastReport.Format
{
  /// <summary>
  /// Defines how numeric values are formatted and displayed.
  /// </summary>
  public class NumberFormat : FormatBase
  {
    #region Fields
    private bool FUseLocale;
    private int FDecimalDigits;
    private string FDecimalSeparator;
    private string FGroupSeparator;
    private int FNegativePattern;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets a value that determines whether to use system locale settings to format a value.
    /// </summary>
    [DefaultValue(true)]
    public bool UseLocale
    {
      get { return FUseLocale; }
      set { FUseLocale = value; }
    }

    /// <summary>
    /// Gets or sets the number of decimal places to use in numeric values. 
    /// </summary>
    [DefaultValue(2)]
    public int DecimalDigits
    {
      get { return FDecimalDigits; }
      set { FDecimalDigits = value; }
    }

    /// <summary>
    /// Gets or sets the string to use as the decimal separator in numeric values.
    /// </summary>
    public string DecimalSeparator
    {
      get { return FDecimalSeparator; }
      set { FDecimalSeparator = value; }
    }

    /// <summary>
    /// Gets or sets the string that separates groups of digits to the left of the decimal in numeric values. 
    /// </summary>
    public string GroupSeparator
    {
      get { return FGroupSeparator; }
      set { FGroupSeparator = value; }
    }

    /// <summary>
    /// Gets or sets the format pattern for negative numeric values.
    /// </summary>
    /// <remarks>This property can have one of the values in the following table. 
    /// The symbol <i>n</i> is a number.
    /// <list type="table">
    ///   <listheader><term>Value</term><description>Associated Pattern</description></listheader>
    ///   <item><term>0</term><description>(n)</description></item>
    ///   <item><term>1</term><description>-n</description></item>
    ///   <item><term>2</term><description>- n</description></item>
    ///   <item><term>3</term><description>n-</description></item>
    ///   <item><term>4</term><description>n -</description></item>
    /// </list>
    /// </remarks>
    [DefaultValue(0)]
    public int NegativePattern
    {
      get { return FNegativePattern; }
      set { FNegativePattern = value; }
    }
    #endregion

    #region Public Methods
    /// <inheritdoc/>
    public override FormatBase Clone()
    {
      NumberFormat result = new NumberFormat();
      result.UseLocale = UseLocale;
      result.DecimalDigits = DecimalDigits;
      result.DecimalSeparator = DecimalSeparator;
      result.GroupSeparator = GroupSeparator;
      result.NegativePattern = NegativePattern;
      return result;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      NumberFormat f = obj as NumberFormat;
      return f != null &&
        UseLocale == f.UseLocale &&
        DecimalDigits == f.DecimalDigits &&
        DecimalSeparator == f.DecimalSeparator &&
        GroupSeparator == f.GroupSeparator &&
        NegativePattern == f.NegativePattern;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <inheritdoc/>
    public override string FormatValue(object value)
    {
      if (value is Variant)
        value = ((Variant)value).Value;
      if (UseLocale)
        return String.Format("{0:n}", value);
      else
      {
        NumberFormatInfo info = new NumberFormatInfo();
        info.NumberDecimalDigits = DecimalDigits;
        info.NumberDecimalSeparator = DecimalSeparator;
        info.NumberGroupSizes = new int[] { 3 };
        info.NumberGroupSeparator = GroupSeparator;
        info.NumberNegativePattern = NegativePattern;
        return String.Format(info, "{0:n}", new object[] { value });
      }
    }

    internal override string GetSampleValue()
    {
      return FormatValue(-12345f);
    }

    internal override void Serialize(FRWriter writer, string prefix, FormatBase format)
    {
      base.Serialize(writer, prefix, format);
      NumberFormat c = format as NumberFormat;

      if (c == null || UseLocale != c.UseLocale)
        writer.WriteBool(prefix + "UseLocale", UseLocale);
      if (!UseLocale)
      {  
        if (c == null || DecimalDigits != c.DecimalDigits)
          writer.WriteInt(prefix + "DecimalDigits", DecimalDigits);
        if (c == null || DecimalSeparator != c.DecimalSeparator)
          writer.WriteStr(prefix + "DecimalSeparator", DecimalSeparator);
        if (c == null || GroupSeparator != c.GroupSeparator)
          writer.WriteStr(prefix + "GroupSeparator", GroupSeparator);
        if (c == null || NegativePattern != c.NegativePattern)
          writer.WriteInt(prefix + "NegativePattern", NegativePattern);
      }
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <b>NumberFormat</b> class with default settings. 
    /// </summary>
    public NumberFormat()
    {
      UseLocale = true;
      DecimalDigits = 2;
      DecimalSeparator = ".";
      GroupSeparator = ",";
    }
  }
}
