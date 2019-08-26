using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using FastReport.Utils;
using System.CodeDom;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;

namespace FastReport.Data
{
  /// <summary>
  /// This class stores all report data items such as datasources, connections, relations, parameters,
  /// system variables.
  /// </summary>
  /// <remarks>
  /// You can access the report dictionary via <b>Report.Dictionary</b> property.
  /// </remarks>
  public class Dictionary : Base, IParent
  {
    #region Fields
    private ConnectionCollection FConnections;
    private DataSourceCollection FDataSources;
    private RelationCollection FRelations;
    private ParameterCollection FParameters;
    private SystemVariables FSystemVariables;
    private TotalCollection FTotals;
    private List<RegDataItem> FRegisteredItems;
    #endregion

    #region Properties
    /// <summary>
    /// Gets a collection of connection objects available in a report.
    /// </summary>
    public ConnectionCollection Connections
    {
      get { return FConnections; }
    }
    
    /// <summary>
    /// Gets a collection of datasources available in a report. 
    /// </summary>
    /// <remarks>
    /// Usually you don't need to use this property. It contains only datasources 
    /// registered using the <b>RegisterData</b> method. All other datasources are contained 
    /// in connection objects and may be accessed via <see cref="Connections"/> property. 
    /// </remarks>
    public DataSourceCollection DataSources
    {
      get { return FDataSources; }
    }

    /// <summary>
    /// Gets a collection of relations.
    /// </summary>
    public RelationCollection Relations
    {
      get { return FRelations; }
    }
    
    /// <summary>
    /// Gets a collection of parameters.
    /// </summary>
    /// <remarks>
    /// Another way to access parameters is to use the <b>Report.Parameters</b> property
    /// which is actually a shortcut to this property. You also may use the <b>Report.GetParameter</b>
    /// and <b>Report.GetParameterValue</b> methods.
    /// </remarks>
    public ParameterCollection Parameters
    {
      get { return FParameters; }
    }
    
    /// <summary>
    /// Gets a collection of system variables like Date, PageNofM etc.
    /// </summary>
    /// <remarks>
    /// Another way to access a system variable is to use the <b>Report.GetVariableValue</b> method.
    /// </remarks>
    public SystemVariables SystemVariables
    {
      get { return FSystemVariables; }
    }
    
    /// <summary>
    /// Gets a collection of totals.
    /// </summary>
    /// <remarks>
    /// Another way to get a total value is to use the <b>Report.GetTotalValue</b> method.
    /// </remarks>
    public TotalCollection Totals
    {
      get { return FTotals; }
    }
    
    internal List<RegDataItem> RegisteredItems
    {
      get { return FRegisteredItems; }
    }
    #endregion

    #region Private Methods
    private RegDataItem FindRegisteredItem(object data)
    {
      foreach (RegDataItem item in FRegisteredItems)
      {
        if (item.Data == data)
          return item;
      }
      return null;
    }

    private RegDataItem FindRegisteredItem(string name)
    {
      foreach (RegDataItem item in FRegisteredItems)
      {
        if (item.Name == name)
          return item;
      }
      return null;
    }

    private void AddRegisteredItem(object data, string name)
    {
      if (FindRegisteredItem(data) == null)
        FRegisteredItems.Add(new RegDataItem(data, name));
    }
    #endregion
    
    #region Public Methods
    /// <inheritdoc/>
    public override void Assign(Base source)
    {
      BaseAssign(source);
    }

    internal void RegisterDataSet(DataSet data, string referenceName, bool enabled)
    {
      foreach (DataTable table in data.Tables)
      {
        RegisterDataTable(table, referenceName + "." + table.TableName, enabled);
      }
      foreach (DataRelation relation in data.Relations)
      {
        RegisterDataRelation(relation, referenceName + "." + relation.RelationName, enabled);
      }
    }

    internal void RegisterDataTable(DataTable table, string referenceName, bool enabled)
    {
      AddRegisteredItem(table, referenceName);

      TableDataSource source = FindDataComponent(referenceName) as TableDataSource;
      if (source != null)
      {
        source.Reference = table;
        source.InitSchema();
        source.RefreshColumns(true);
      }
      else
      {
        // check tables inside connections. Are we trying to replace the connection table
        // with table provided by an application?
        source = FindByAlias(referenceName) as TableDataSource;
        if (source != null && (source.Connection != null || source.IgnoreConnection))
        {
          source.IgnoreConnection = true;
          source.Reference = table;
          source.InitSchema();
          source.RefreshColumns(true);
        }
        else
        {
          source = new TableDataSource();
          source.ReferenceName = referenceName;
          source.Reference = table;
          source.Name = CreateUniqueName(referenceName.Contains(".") ? table.TableName : referenceName);
          source.Alias = CreateUniqueAlias(source.Alias);
          source.Enabled = enabled;
          source.InitSchema();
          DataSources.Add(source);
        }
      }
    }

    internal void RegisterDataView(DataView view, string referenceName, bool enabled)
    {
      AddRegisteredItem(view, referenceName);

      ViewDataSource source = FindDataComponent(referenceName) as ViewDataSource;
      if (source != null)
      {
        source.Reference = view;
        source.InitSchema();
        source.RefreshColumns();
      }
      else
      {
        source = new ViewDataSource();
        source.ReferenceName = referenceName;
        source.Reference = view;
        source.Name = CreateUniqueName(referenceName);
        source.Alias = CreateUniqueAlias(source.Alias);
        source.Enabled = enabled;
        source.InitSchema();
        DataSources.Add(source);
      }
    }

    internal void RegisterDataRelation(DataRelation relation, string referenceName, bool enabled)
    {
      AddRegisteredItem(relation, referenceName);
      if (FindDataComponent(referenceName) != null)
        return;

      Relation rel = new Relation();
      rel.ReferenceName = referenceName;
      rel.Reference = relation;
      rel.Name = relation.RelationName;
      rel.Enabled = enabled;
      rel.ParentDataSource = FindDataTableSource(relation.ParentTable);
      rel.ChildDataSource = FindDataTableSource(relation.ChildTable);
      string[] parentColumns = new string[relation.ParentColumns.Length];
      string[] childColumns = new string[relation.ChildColumns.Length];
      for (int i = 0; i < relation.ParentColumns.Length; i++)
      {
        parentColumns[i] = relation.ParentColumns[i].Caption;
      }
      for (int i = 0; i < relation.ChildColumns.Length; i++)
      {
        childColumns[i] = relation.ChildColumns[i].Caption;
      }
      rel.ParentColumns = parentColumns;
      rel.ChildColumns = childColumns;
      Relations.Add(rel);
    }

    internal void RegisterBusinessObject(IEnumerable data, string referenceName, int maxNestingLevel, bool enabled)
    {
      AddRegisteredItem(data, referenceName);

      Type dataType = data.GetType();
      if (data is BindingSource)
      {
        if ((data as BindingSource).DataSource is Type)
          dataType = ((data as BindingSource).DataSource as Type);
        else
          dataType = (data as BindingSource).DataSource.GetType();
      }
      
      BusinessObjectConverter converter = new BusinessObjectConverter(this);
      BusinessObjectDataSource source = FindDataComponent(referenceName) as BusinessObjectDataSource;
      if (source != null)
      {
        source.Reference = data;
        source.DataType = dataType;
        converter.UpdateExistingObjects(source, maxNestingLevel);
      }
      else
      {
        source = new BusinessObjectDataSource();
        source.ReferenceName = referenceName;
        source.Reference = data;
        source.DataType = dataType;
        source.Name = CreateUniqueName(referenceName);
        source.Alias = CreateUniqueAlias(source.Alias);
        source.Enabled = enabled;
        DataSources.Add(source);

        converter.CreateInitialObjects(source, maxNestingLevel);
      }
    }

    internal void RegisterData(object data, string name, bool enabled)
    {
      if (data is DataSet)
      {
        RegisterDataSet(data as DataSet, name, enabled);
      }
      else if (data is DataTable)
      {
        RegisterDataTable(data as DataTable, name, enabled);
      }
      else if (data is DataView)
      {
        RegisterDataView(data as DataView, name, enabled);
      }
      else if (data is DataRelation)
      {
        RegisterDataRelation(data as DataRelation, name, enabled);
      }
      else if (data is IEnumerable)
      {
        RegisterBusinessObject(data as IEnumerable, name, 1, enabled);
      }
    }
    
    /// <summary>
    /// Unregisters the previously registered data.
    /// </summary>
    /// <param name="data">The application data.</param>
    public void UnregisterData(object data)
    {
      UnregisterData(data, "Data");
    }

    /// <summary>
    /// Unregisters the previously registered data.
    /// </summary>
    /// <param name="data">The application data.</param>
    /// <param name="name">The name of the data.</param>
    /// <remarks>
    /// You must specify the same <b>data</b> and <b>name</b> as when you call <b>RegisterData</b>.
    /// </remarks>
    public void UnregisterData(object data, string name)
    {
      for (int i = 0; i < FRegisteredItems.Count; i++)
      {
        RegDataItem item = FRegisteredItems[i];
        if (item.Name == name)
        {
          FRegisteredItems.RemoveAt(i);
          break;
        }
      }

      DataComponentBase comp = FindDataComponent(name);
      if (comp != null)
        comp.Dispose();

      if (data is DataSet)
      {
        foreach (DataTable table in (data as DataSet).Tables)
        {
          UnregisterData(table, name + "." + table.TableName);
        }
        foreach (DataRelation relation in (data as DataSet).Relations)
        {
          UnregisterData(relation, name + "." + relation.RelationName);
        }
      }
    }

    internal void ReRegisterData()
    {
      ReRegisterData(this);
    }

    internal void ReRegisterData(Dictionary dictionary)
    {
      // re-register all data items. It is needed after load or "new report" operations
      foreach (RegDataItem item in FRegisteredItems)
      {
        dictionary.RegisterData(item.Data, item.Name, false);
      }
    }

    internal void ClearRegisteredData()
    {
      FRegisteredItems.Clear();
    }
    
    /// <summary>
    /// Enables or disables relations between data tables.
    /// </summary>
    /// <remarks>
    /// Call this method if you create master-detail report from code. This method enables
    /// relation between two data tables which <b>Enabled</b> flag is set to <b>true</b>. Relations
    /// whose parent and child tables are disabled, gets disabled too.
    /// </remarks>
    public void UpdateRelations()
    {
      foreach (Relation relation in Relations)
      {
        relation.Enabled = relation.ParentDataSource != null && relation.ParentDataSource.Enabled &&
          relation.ChildDataSource != null && relation.ChildDataSource.Enabled;
      }
    }

    /// <summary>
    /// Creates unique name for data item such as connection, datasource, relation, parameter or total.
    /// </summary>
    /// <param name="name">The base name.</param>
    /// <returns>The new unique name.</returns>
    /// <remarks>
    /// Use this method to create unique name of the data item. It is necessary when you create new
    /// items in code to avoid conflicts with existing report items.
    /// <example>This example show how to add a new parameter:
    /// <code>
    /// Report report1;
    /// Parameter par = new Parameter();
    /// par.Name = report1.Dictionary.CreateUniqueName("Parameter");
    /// report1.Parameters.Add(par);
    /// </code>
    /// </example>
    /// </remarks>
    public string CreateUniqueName(string name)
    {
      string baseName = name;
      int i = 1;
      while (FindByName(name) != null || Report.FindObject(name) != null)
      {
        name = baseName + i.ToString();
        i++;
      }
      return name;
    }

    /// <summary>
    /// Creates unique alias for data item such as connection, datasource or relation.
    /// </summary>
    /// <param name="alias">The base alias.</param>
    /// <returns>The new unique alias.</returns>
    /// <remarks>
    /// Use this method to create unique alias of the data item. It is necessary when you create new
    /// items in code to avoid conflicts with existing report items.
    /// <example>This example show how to add a new table:
    /// <code>
    /// Report report1;
    /// DataConnectionBase conn = report1.Dictionary.Connections.FindByName("Connection1");
    /// TableDataSource table = new TableDataSource();
    /// table.TableName = "Employees";
    /// table.Name = report1.Dictionary.CreateUniqueName("EmployeesTable");
    /// table.Alias = report1.Dictionary.CreateUniqueAlias("Employees");
    /// conn.Tables.Add(table);
    /// </code>
    /// </example>
    /// </remarks>
    public string CreateUniqueAlias(string alias)
    {
      string baseAlias = alias;
      int i = 1;
      while (FindByAlias(alias) != null)
      {
        alias = baseAlias + i.ToString();
        i++;
      }
      return alias;
    }
    
    /// <summary>
    /// Finds a data item such as connection, datasource, relation, parameter or total by its name.
    /// </summary>
    /// <param name="name">The item's name.</param>
    /// <returns>The data item if found; otherwise, <b>null</b>.</returns>
    public Base FindByName(string name)
    {
      foreach (Base c in AllObjects)
      {
        if (c is DataConnectionBase || c is DataSourceBase || c is Relation || 
          (c is Parameter && c.Parent == this) || c is Total)
        {
          if (name == c.Name)
            return c;
        }
      }
      return null;
    }

    /// <summary>
    /// Finds a data item such as connection, datasource or relation by its alias.
    /// </summary>
    /// <param name="alias">The item's alias.</param>
    /// <returns>The data item if found; otherwise, <b>null</b>.</returns>
    public DataComponentBase FindByAlias(string alias)
    {
      foreach (Base c in AllObjects)
      {
        if (c is DataConnectionBase || c is Relation)
        {
          if (alias == (c as DataComponentBase).Alias)
            return c as DataComponentBase;
        }
        else if (c is DataSourceBase)
        {
          if (alias == (c as DataSourceBase).FullName)
            return c as DataSourceBase;
        }
      }
      return null;
    }

    internal DataSourceBase FindDataTableSource(DataTable table)
    {
      foreach (DataSourceBase c in DataSources)
      {
        if (c is TableDataSource && c.Reference == table)
          return c;
      }
      return null;
    }

    internal DataComponentBase FindDataComponent(string referenceName)
    {
      foreach (Base c in AllObjects)
      {
        DataComponentBase data = c as DataComponentBase;
        if (data != null && data.ReferenceName == referenceName)
          return data;
      }
      return null;
    }

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
      writer.ItemName = ClassName;
      ObjectCollection childObjects = ChildObjects;
      foreach (Base c in childObjects)
      {
        if (c is Parameter || c is Total || (c is DataComponentBase && (c as DataComponentBase).Enabled))
          writer.Write(c);
      }
    }

    /// <inheritdoc/>
    public override void Deserialize(FRReader reader)
    {
      base.Deserialize(reader);
      ReRegisterData();
    }

    /// <summary>
    /// Saves the dictionary to a stream.
    /// </summary>
    /// <param name="stream">Stream to save to.</param>
    public void Save(Stream stream)
    {
      using (FRWriter writer = new FRWriter())
      {
        writer.Write(this);
        writer.Save(stream);
      }
    }

    /// <summary>
    /// Saves the dictionary to a file.
    /// </summary>
    /// <param name="fileName">The name of a file to save to.</param>
    public void Save(string fileName)
    {
      using (FileStream f = new FileStream(fileName, FileMode.Create))
      {
        Save(f);
      }
    }

    /// <summary>
    /// Loads the dictionary from a stream.
    /// </summary>
    /// <param name="stream">The stream to load from.</param>
    public void Load(Stream stream)
    {
      Clear();
      using (FRReader reader = new FRReader(Report))
      {
        reader.Load(stream);
        reader.Read(this);
      }
    }

    /// <summary>
    /// Loads the dictionary from a file.
    /// </summary>
    /// <param name="fileName">The name of a file to load from.</param>
    public void Load(string fileName)
    {
      using (FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        Load(f);
      }
    }

    /// <summary>
    /// Merges this dictionary with another <b>Dictionary</b>.
    /// </summary>
    /// <param name="source">Another dictionary to merge the data from.</param>
    public void Merge(Dictionary source)
    {
      // Report object is needed to handle save/load of dictionary correctly.
      // Some dictionary objects (such as relations) may contain references to other objects.
      // In order to clone them correctly, we need a parent Report object, because
      // reader uses it to fixup references.
      
      using (Report cloneReport = new Report())
      {
        Dictionary clone = cloneReport.Dictionary;
        clone.AssignAll(source, true);
        
        foreach (Base c in clone.ChildObjects)
        {
          Base my = FindByName(c.Name);
          if (my != null)
            my.Dispose();
          c.Parent = this;
        }
        
        source.ReRegisterData(this);
        ReRegisterData();
      }  
    }
    #endregion

    #region IParent Members
    /// <inheritdoc/>
    public bool CanContain(Base child)
    {
      return child is DataConnectionBase || child is DataSourceBase || child is Relation || child is Parameter ||
        child is Total;
    }

    /// <inheritdoc/>
    public void GetChildObjects(ObjectCollection list)
    {
      foreach (DataConnectionBase c in Connections)
      {
        list.Add(c);
      }
      foreach (DataSourceBase c in DataSources)
      {
        list.Add(c);
      }
      foreach (Relation c in Relations)
      {
        list.Add(c);
      }
      foreach (Parameter c in Parameters)
      {
        list.Add(c);
      }
      foreach (Total c in Totals)
      {
        list.Add(c);
      }
    }

    /// <inheritdoc/>
    public void AddChild(Base child)
    {
      if (child is DataConnectionBase)
        Connections.Add(child as DataConnectionBase);
      else if (child is DataSourceBase)
        DataSources.Add(child as DataSourceBase);
      else if (child is Relation)
        Relations.Add(child as Relation);
      else if (child is Parameter)
        Parameters.Add(child as Parameter);
      else if (child is Total)
        Totals.Add(child as Total);  
    }

    /// <inheritdoc/>
    public void RemoveChild(Base child)
    {
      if (child is DataConnectionBase)
        Connections.Remove(child as DataConnectionBase);
      else if (child is DataSourceBase)
        DataSources.Remove(child as DataSourceBase);
      else if (child is Relation)
        Relations.Remove(child as Relation);
      else if (child is Parameter)
        Parameters.Remove(child as Parameter);
      else if (child is Total)
        Totals.Remove(child as Total);
    }

    /// <inheritdoc/>
    public int GetChildOrder(Base child)
    {
      return 0;
    }

    /// <inheritdoc/>
    public void SetChildOrder(Base child, int order)
    {
      // do nothing
    }

    /// <inheritdoc/>
    public void UpdateLayout(float dx, float dy)
    {
      // do nothing
    }
    #endregion
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Dictionary"/> class with default settings.
    /// </summary>
    public Dictionary()
    {
      FConnections = new ConnectionCollection(this);
      FDataSources = new DataSourceCollection(this);
      FRelations = new RelationCollection(this);
      FParameters = new ParameterCollection(this);
      FSystemVariables = new SystemVariables(this);
      FTotals = new TotalCollection(this);
      FRegisteredItems = new List<RegDataItem>();
    }
    
    internal class RegDataItem
    {
      public object Data;
      public string Name;

      public RegDataItem(object data, string name)
      {
        Data = data;
        Name = name;
      }
    }
  }
}
