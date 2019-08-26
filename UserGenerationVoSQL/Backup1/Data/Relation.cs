using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.ComponentModel;
using FastReport.Utils;
using FastReport.Forms;
using FastReport.TypeConverters;

namespace FastReport.Data
{
  /// <summary>
  /// Represents a master-detail relation between two data sources.
  /// </summary>
  /// <remarks>
  /// To setup a relation, you must specify parent and child datasources. For a parent datasource,
  /// you must specify set of key columns; for child datasource, you must specify set of columns that
  /// relate to the parent key columns.
  /// <example>This example shows how to create relation between Customers and Orders tables:
  /// <code>
  /// Report report1;
  /// DataSourceBase customersTable = report1.Dictionary.DataSources.FindByAlias("Customers");
  /// DataSourceBase ordersTable = report1.Dictionary.DataSources.FindByAlias("Orders");
  /// Relation rel = new Relation();
  /// rel.Name = "customersOrders";
  /// rel.ParentDataSource = customersTable;
  /// rel.ChildDataSource = ordersTable;
  /// rel.ParentColumns = new string[] { "CustomerID" };
  /// rel.ParentColumns = new string[] { "CustomerID" };
  /// report1.Dictionary.Relations.Add(rel);
  /// </code>
  /// </example>
  /// </remarks>
  [TypeConverter(typeof(RelationConverter))]
  public class Relation : DataComponentBase, IHasEditor
  {
    #region Fields
    private DataSourceBase FParentDataSource;
    private DataSourceBase FChildDataSource;
    private string[] FParentColumns;
    private string[] FChildColumns;
    #endregion
    
    #region Properties
    /// <summary>
    /// Gets or sets the parent datasource.
    /// </summary>
    [Category("Data")]
    public DataSourceBase ParentDataSource
    {
      get { return FParentDataSource; }
      set { FParentDataSource = value; }
    }

    /// <summary>
    /// Gets or sets the child datasource.
    /// </summary>
    [Category("Data")]
    public DataSourceBase ChildDataSource
    {
      get { return FChildDataSource; }
      set { FChildDataSource = value; }
    }

    /// <summary>
    /// Gets or sets an array of parent datasource columns.
    /// </summary>
    /// <remarks>
    /// Note: both <see cref="ParentColumns"/> and <see cref="ChildColumns"/> must have the
    /// same number of elements.
    /// </remarks>
    [Category("Data")]
    public string[] ParentColumns
    {
      get { return FParentColumns; }
      set { FParentColumns = value; }
    }

    /// <summary>
    /// Gets or sets an array of child datasource columns.
    /// </summary>
    /// <remarks>
    /// Note: both <see cref="ParentColumns"/> and <see cref="ChildColumns"/> must have the
    /// same number of elements.
    /// </remarks>
    [Category("Data")]
    public string[] ChildColumns
    {
      get { return FChildColumns; }
      set { FChildColumns = value; }
    }
    #endregion

    #region Public Methods
    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
      base.Serialize(writer);
      writer.WriteRef("ParentDataSource", ParentDataSource);
      writer.WriteRef("ChildDataSource", ChildDataSource);
      writer.WriteValue("ParentColumns", ParentColumns);
      writer.WriteValue("ChildColumns", ChildColumns);    
      writer.WriteBool("Enabled", Enabled);
    }
    
    /// <summary>
    /// Invokes the relation editor.
    /// </summary>
    /// <returns><b>true</b> if relation was modified.</returns>
    public bool InvokeEditor()
    {
      using (RelationEditorForm form = new RelationEditorForm(this))
      {
        return form.ShowDialog() == DialogResult.OK;
      }
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Relation"/> class with default settings.
    /// </summary>
    public Relation()
    {
      SetFlags(Flags.CanEdit, true);
    }
  }
}
