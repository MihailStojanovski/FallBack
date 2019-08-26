using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using FastReport.Data;

namespace FastReport.TypeConverters
{
  internal class ParameterDataTypeConverter : TypeConverter
  {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      if (sourceType == typeof(string))
        return true;
      return base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      if (destinationType == typeof(string))
        return true;
      return base.CanConvertTo(context, destinationType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (context != null && context.Instance is CommandParameter && value is string)
      {
        CommandParameter parameter = context.Instance as CommandParameter;
        Type dataType = parameter.GetUnderlyingDataType;
        if (dataType != null)
          return (int)Enum.Parse(dataType, (string)value);
      }
      return base.ConvertFrom(context, culture, value);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
      if (context != null && context.Instance is CommandParameter && destinationType == typeof(string))
      {
        CommandParameter parameter = context.Instance as CommandParameter;
        Type dataType = parameter.GetUnderlyingDataType;
        if (dataType != null)
          return Enum.Format(dataType, value, "G");
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }
}
