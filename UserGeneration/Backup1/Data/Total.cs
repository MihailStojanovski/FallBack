using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using FastReport.Utils;
using FastReport.TypeEditors;
using FastReport.Forms;

namespace FastReport.Data
{
  /// <summary>
  /// Specifies the total type.
  /// </summary>
  public enum TotalType
  {
    /// <summary>
    /// The total returns sum of values.
    /// </summary>
    Sum,

    /// <summary>
    /// The total returns minimal value.
    /// </summary>
    Min,

    /// <summary>
    /// The total returns maximal value.
    /// </summary>
    Max,

    /// <summary>
    /// The total returns average value.
    /// </summary>
    Avg,

    /// <summary>
    /// The total returns number of values.
    /// </summary>
    Count
  }
  
  /// <summary>
  /// Represents a total that is used to calculate aggregates such as Sum, Min, Max, Avg, Count.
  /// </summary>
  public class Total : Base, IHasEditor
  {
    #region Fields
    private TotalType FTotalType;
    private string FExpression;
    private DataBand FEvaluator;
    private BandBase FPrintOn;
    private string FEvaluateCondition;
    private bool FIncludeInvisibleRows;
    private bool FResetAfterPrint;
    private bool FResetOnReprint;
    // engine
    private object FValue;
    private int FCount;
    private bool FKeeping;
    private Total FKeepTotal;
    private TotalCollection FSubTotals;
    private const string subPrefix = "_sub";
    #endregion
    
    #region Properties
    /// <summary>
    /// Gets or sets the total type.
    /// </summary>
    [DefaultValue(TotalType.Sum)]
    [Category("Data")]
    public TotalType TotalType
    {
      get { return FTotalType; }
      set { FTotalType = value; }
    }

    /// <summary>
    /// Gets or sets the expression used to calculate the total.
    /// </summary>
    [Editor(typeof(ExpressionEditor), typeof(UITypeEditor))]
    [Category("Data")]
    public string Expression
    {
      get { return FExpression; }
      set { FExpression = value; }
    }
    
    /// <summary>
    /// Gets or sets the evaluator databand.
    /// </summary>
    /// <remarks>
    /// The total will be calculated for each row of this band.
    /// </remarks>
    [Category("Data")]
    public DataBand Evaluator
    {
      get { return FEvaluator; }
      set { FEvaluator = value; }
    }
    
    /// <summary>
    /// This property is kept for compatibility only.
    /// </summary>
    [Category("Data")]
    [Browsable(false)]
    public BandBase Resetter
    {
      get { return FPrintOn; }
      set { FPrintOn = value; }
    }

    /// <summary>
    /// Gets or sets the band to print the total on.
    /// </summary>
    /// <remarks>
    /// The total will be resetted after the specified band has been printed.
    /// </remarks>
    [Category("Data")]
    public BandBase PrintOn
    {
      get { return FPrintOn; }
      set { FPrintOn = value; }
    }

    /// <summary>
    /// Gets or sets a value that determines whether the total should be resetted after print.
    /// </summary>
    [DefaultValue(true)]
    [Category("Behavior")]
    public bool ResetAfterPrint
    {
      get { return FResetAfterPrint; }
      set { FResetAfterPrint = value; }
    }

    /// <summary>
    /// Gets or sets a value that determines whether the total should be resetted if printed 
    /// on repeated band (i.e. band with "RepeatOnEveryPage" flag).
    /// </summary>
    [DefaultValue(true)]
    [Category("Behavior")]
    public bool ResetOnReprint
    {
      get { return FResetOnReprint; }
      set { FResetOnReprint = value; }
    }

    /// <summary>
    /// Gets or sets the condition which tells the total to evaluate.
    /// </summary>
    [Editor(typeof(ExpressionEditor), typeof(UITypeEditor))]
    [Category("Data")]
    public string EvaluateCondition
    {
      get { return FEvaluateCondition; }
      set { FEvaluateCondition = value; }
    }
    
    /// <summary>
    /// Gets or sets a value that determines if invisible rows of the <b>Evaluator</b> should
    /// be included into the total's value.
    /// </summary>
    [DefaultValue(false)]
    [Category("Behavior")]
    public bool IncludeInvisibleRows
    {
      get { return FIncludeInvisibleRows; }
      set { FIncludeInvisibleRows = value; }
    }

    /// <summary>
    /// This property is not relevant to this class.
    /// </summary>
    [Browsable(false)]
    public new Restrictions Restrictions
    {
      get { return base.Restrictions; }
      set { base.Restrictions = value; }
    }

    /// <summary>
    /// Gets the value of total.
    /// </summary>
    [Browsable(false)]
    public object Value
    {
      get { return GetValue(); }
    }

    private bool IsPageFooter
    {
      get 
      { 
        return PrintOn is PageFooterBand || PrintOn is ColumnFooterBand ||
          ((PrintOn is HeaderFooterBandBase) && (PrintOn as HeaderFooterBandBase).RepeatOnEveryPage); 
      }
    }

    private bool IsInsideHierarchy
    {
      get 
      {
        return Report.Engine.HierarchyLevel > 1 &&
          !Name.StartsWith(subPrefix) &&
          PrintOn != null && PrintOn.ParentDataBand != null && PrintOn.ParentDataBand.IsHierarchical;
      }
    }
    #endregion

    #region Private Methods
    private object GetValue()
    {
      if (IsInsideHierarchy)
      {
        Total subTotal = FindSubTotal(subPrefix + Report.Engine.HierarchyLevel.ToString());
        return subTotal.Value;
      }

      if (TotalType == TotalType.Avg)
      {
        if (FValue == null || FCount == 0)
          return 0;
        return new Variant(FValue) / FCount;
      }
      else if (TotalType == TotalType.Count)
        return FCount;
      return FValue;
    }

    private void AddValue(object value)
    {
      if (FValue == null)
        FValue = value;
      else if (value != null)
      {
        switch (TotalType)
        {
          case TotalType.Sum:
          case TotalType.Avg:
            FValue = ((Variant)(new Variant(FValue) + new Variant(value))).Value;
            break;

          case TotalType.Min:
            IComparable val1 = FValue as IComparable;
            IComparable val2 = value as IComparable;
            if (val1 != null && val2 != null && val1.CompareTo(val2) > 0)
              FValue = value;
            break;

          case TotalType.Max:
            val1 = FValue as IComparable;
            val2 = value as IComparable;
            if (val1 != null && val2 != null && val1.CompareTo(val2) < 0)
              FValue = value;
            break;
        }
      }
    }

    private Total FindSubTotal(string name)
    {
      Total result = FSubTotals.FindByName(name);
      if (result == null)
      {
        result = this.Clone();
        result.Name = name;
        FSubTotals.Add(result);
      }

      return result;
    }
    #endregion
    
    #region Public Methods
    /// <inheritdoc/>
    public override void Assign(Base source)
    {
      BaseAssign(source);
    }

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
      Total c = writer.DiffObject as Total;
      base.Serialize(writer);

      if (TotalType != c.TotalType)
        writer.WriteValue("TotalType", TotalType);
      if (Expression != c.Expression)
        writer.WriteStr("Expression", Expression);
      if (Evaluator != c.Evaluator)
        writer.WriteRef("Evaluator", Evaluator);
      if (PrintOn != c.PrintOn)
        writer.WriteRef("PrintOn", PrintOn);
      if (ResetAfterPrint != c.ResetAfterPrint)
        writer.WriteBool("ResetAfterPrint", ResetAfterPrint);
      if (ResetOnReprint != c.ResetOnReprint)
        writer.WriteBool("ResetOnReprint", ResetOnReprint);
      if (EvaluateCondition != c.EvaluateCondition)
        writer.WriteStr("EvaluateCondition", EvaluateCondition);
      if (IncludeInvisibleRows != c.IncludeInvisibleRows)
        writer.WriteBool("IncludeInvisibleRows", IncludeInvisibleRows);
    }

    /// <inheritdoc/>
    public bool InvokeEditor()
    {
      using (TotalEditorForm form = new TotalEditorForm(Report.Designer))
      {
        form.Total = this;
        return form.ShowDialog() == DialogResult.OK;
      }
    }
    
    internal Total Clone()
    {
      Total total = new Total();
      total.SetReport(Report);
      total.TotalType = TotalType;
      total.Expression = Expression;
      total.Evaluator = Evaluator;
      total.PrintOn = PrintOn;
      total.ResetAfterPrint = ResetAfterPrint;
      total.ResetOnReprint = ResetOnReprint;
      total.EvaluateCondition = EvaluateCondition;
      total.IncludeInvisibleRows = IncludeInvisibleRows;
      return total;
    }
    #endregion
    
    #region Report Engine
    /// <inheritdoc/>
    public override string[] GetExpressions()
    {
      List<string> expressions = new List<string>();
      if (!String.IsNullOrEmpty(Expression))
        expressions.Add(Expression);
      if (!String.IsNullOrEmpty(EvaluateCondition))
        expressions.Add(EvaluateCondition);
      return expressions.ToArray();
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      FValue = null;
      FCount = 0;
    }
    
    internal void AddValue()
    {
      if (IsInsideHierarchy)
      {
        Total subTotal = FindSubTotal(subPrefix + Report.Engine.HierarchyLevel.ToString());
        subTotal.AddValue();
        return;
      }
      
      if (!Evaluator.Visible && !IncludeInvisibleRows)
        return;
      if (!String.IsNullOrEmpty(EvaluateCondition) && (bool)Report.Calc(EvaluateCondition) == false)
        return;
      if (TotalType != TotalType.Count && String.IsNullOrEmpty(Expression))
        return;  
      
      if (FKeeping)
      {
        FKeepTotal.AddValue();
        return;
      }

      object value = TotalType == TotalType.Count ? null : Report.Calc(Expression);
      AddValue(value);
      if (TotalType != TotalType.Avg || value != null)
        FCount++;
    }

    internal void ResetValue()
    {
      if (IsInsideHierarchy)
      {
        Total subTotal = FindSubTotal(subPrefix + Report.Engine.HierarchyLevel.ToString());
        subTotal.ResetValue();
        return;
      }

      Clear();
    }

    internal void StartKeep()
    {
      if (!IsPageFooter || FKeeping)
        return;
      FKeeping = true;

      FKeepTotal = Clone();
    }

    internal void EndKeep()
    {
      if (!IsPageFooter || !FKeeping)
        return;
      FKeeping = false;

      AddValue(FKeepTotal.FValue);
      FCount += FKeepTotal.FCount;
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Total"/> class with default settings.
    /// </summary>
    public Total()
    {
      FExpression = "";
      FEvaluateCondition = "";
      FResetAfterPrint = true;
      FSubTotals = new TotalCollection(null);
      SetFlags(Flags.CanCopy, false);
    }
  }
}
