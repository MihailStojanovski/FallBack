using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using FastReport.Utils;
using FastReport.Data;

namespace FastReport.Dialog
{
  /// <summary>
  /// Represents a Windows control that allows the user to select a date and a time and to display the date and time with a specified format.
  /// Wraps the <see cref="System.Windows.Forms.DateTimePicker"/> control.
  /// </summary>
  public class DateTimePickerControl : DataFilterBaseControl
  {
    private DateTimePicker FDateTimePicker;

    #region Properties
    /// <summary>
    /// Gets an internal <b>DateTimePicker</b>.
    /// </summary>
    [Browsable(false)]
    public DateTimePicker DateTimePicker
    {
      get { return FDateTimePicker; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Value property has been set with a valid date/time value and the displayed value is able to be updated.
    /// Wraps the <see cref="System.Windows.Forms.DateTimePicker.Checked"/> property.
    /// </summary>
    [DefaultValue(false)]
    [Category("Behavior")]
    public bool Checked
    {
      get { return DateTimePicker.Checked; }
      set { DateTimePicker.Checked = value; }
    }

    /// <summary>
    /// Gets or sets the custom date/time format string.
    /// Wraps the <see cref="System.Windows.Forms.DateTimePicker.CustomFormat"/> property.
    /// </summary>
    [Category("Data")]
    public string CustomFormat
    {
      get { return DateTimePicker.CustomFormat; }
      set { DateTimePicker.CustomFormat = value; }
    }

    /// <summary>
    /// Gets or sets the alignment of the drop-down calendar on the DateTimePicker control.
    /// Wraps the <see cref="System.Windows.Forms.DateTimePicker.DropDownAlign"/> property.
    /// </summary>
    [DefaultValue(LeftRightAlignment.Left)]
    [Category("Appearance")]
    public LeftRightAlignment DropDownAlign 
    {
      get { return DateTimePicker.DropDownAlign; }
      set { DateTimePicker.DropDownAlign = value; }
    }

    /// <summary>
    /// Gets or sets the format of the date and time displayed in the control.
    /// Wraps the <see cref="System.Windows.Forms.DateTimePicker.Format"/> property.
    /// </summary>
    [DefaultValue(DateTimePickerFormat.Long)]
    [Category("Data")]
    public DateTimePickerFormat Format
    {
      get { return DateTimePicker.Format; }
      set { DateTimePicker.Format = value; }
    }

    /// <summary>
    /// Gets or sets the maximum date and time that can be selected in the control.
    /// Wraps the <see cref="System.Windows.Forms.DateTimePicker.MaxDate"/> property.
    /// </summary>
    [Category("Data")]
    public DateTime MaxDate
    {
      get { return DateTimePicker.MaxDate; }
      set { DateTimePicker.MaxDate = value; }
    }

    /// <summary>
    /// Gets or sets the minimum date and time that can be selected in the control.
    /// Wraps the <see cref="System.Windows.Forms.DateTimePicker.MinDate"/> property.
    /// </summary>
    [Category("Data")]
    public DateTime MinDate
    {
      get { return DateTimePicker.MinDate; }
      set { DateTimePicker.MinDate = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a check box is displayed to the left of the selected date.
    /// Wraps the <see cref="System.Windows.Forms.DateTimePicker.ShowCheckBox"/> property.
    /// </summary>
    [DefaultValue(false)]
    [Category("Appearance")]
    public bool ShowCheckBox
    {
      get { return DateTimePicker.ShowCheckBox; }
      set { DateTimePicker.ShowCheckBox = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a spin button control (also known as an up-down control) is used to adjust the date/time value.
    /// Wraps the <see cref="System.Windows.Forms.DateTimePicker.ShowUpDown"/> property.
    /// </summary>
    [DefaultValue(false)]
    [Category("Appearance")]
    public bool ShowUpDown
    {
      get { return DateTimePicker.ShowUpDown; }
      set { DateTimePicker.ShowUpDown = value; }
    }

    /// <summary>
    /// Gets or sets the date/time value assigned to the control.
    /// Wraps the <see cref="System.Windows.Forms.DateTimePicker.Value"/> property.
    /// </summary>
    [Category("Data")]
    public DateTime Value
    {
      get { return DateTimePicker.Value; }
      set { DateTimePicker.Value = value; }
    }

    /// <summary>
    /// This property is not relevant to this class.
    /// </summary>
    [Browsable(false)]
    public override string Text
    {
      get { return base.Text; }
      set { base.Text = value; }
    }
    #endregion

    #region Protected Methods
    /// <inheritdoc/>
    protected override void OnLeave()
    {
      base.OnLeave();
      OnFilterChanged();
    }

    /// <inheritdoc/>
    protected override bool ShouldSerializeBackColor()
    {
      return BackColor != SystemColors.Window;
    }

    /// <inheritdoc/>
    protected override bool ShouldSerializeForeColor()
    {
      return ForeColor != SystemColors.WindowText;
    }

    /// <inheritdoc/>
    protected override SelectionPoint[] GetSelectionPoints()
    {
      return new SelectionPoint[] { 
        new SelectionPoint(AbsLeft - 2, AbsTop + Height / 2, SizingPoint.LeftCenter),
        new SelectionPoint(AbsLeft + Width + 1, AbsTop + Height / 2, SizingPoint.RightCenter) };
    }

    /// <inheritdoc/>
    protected override object GetValue()
    {
      if (Format == DateTimePickerFormat.Long || Format == DateTimePickerFormat.Short)
        return new DateTime(Value.Year, Value.Month, Value.Day);
      return Value;
    }
    #endregion

    #region Public Methods
    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
      DateTimePickerControl c = writer.DiffObject as DateTimePickerControl;
      base.Serialize(writer);

      if (Checked != c.Checked)
        writer.WriteBool("Checked", Checked);
      if (CustomFormat != c.CustomFormat)
        writer.WriteStr("CustomFormat", CustomFormat);
      if (DropDownAlign != c.DropDownAlign)
        writer.WriteValue("DropDownAlign", DropDownAlign);
      if (Format != c.Format)
        writer.WriteValue("Format", Format);
      if (MaxDate != c.MaxDate)
        writer.WriteValue("MaxDate", MaxDate);
      if (MinDate != c.MinDate)
        writer.WriteValue("MinDate", MinDate);
      if (ShowCheckBox != c.ShowCheckBox)
        writer.WriteBool("ShowCheckBox", ShowCheckBox);
      if (ShowUpDown != c.ShowUpDown)
        writer.WriteBool("ShowUpDown", ShowUpDown);
      if (Value != c.Value)
        writer.WriteValue("Value", Value);
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <b>DateTimePickerControl</b> class with default settings. 
    /// </summary>
    public DateTimePickerControl()
    {
      FDateTimePicker = new DateTimePicker();
      Control = FDateTimePicker;
      BindableProperty = this.GetType().GetProperty("Value");
    }
  }
}
