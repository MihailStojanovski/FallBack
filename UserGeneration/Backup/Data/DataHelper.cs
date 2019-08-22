using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Collections;
using System.Drawing;
using FastReport.Utils;
using System.Reflection;
using FastReport.Dialog;

namespace FastReport.Data
{
  internal static class DataHelper
  {
    private static void AddColumns(TreeNodeCollection root, ColumnCollection columns, bool enabledOnly, bool showColumns)
    {
      foreach (Column column in columns)
      {
        if (!enabledOnly || column.Enabled)
        {
          TreeNode node = new TreeNode(column.Alias);
          node.Tag = column;
          node.Checked = column.Enabled;

          int imageIndex = column.GetImageIndex();
          node.ImageIndex = imageIndex;
          node.SelectedImageIndex = imageIndex;

          AddColumns(node.Nodes, column.Columns, enabledOnly, showColumns);

          bool isDataSource = column is DataSourceBase;
          bool addNode = showColumns || isDataSource || node.Nodes.Count > 0;

          if (addNode)
            root.Add(node);
        }
      }
    }

    private static void AddDataSource(Dictionary dictionary, DataSourceBase data, TreeNodeCollection root, 
      bool enabledOnly, bool showRelations, bool showColumns)
    {
      AddDataSource(dictionary, data, root, null, new ArrayList(), enabledOnly, showRelations, showColumns, false);
    }
    
    private static void AddDataSource(Dictionary dictionary, DataSourceBase data, TreeNodeCollection root, 
      Relation rel, ArrayList processed, bool enabledOnly, bool showRelations, bool showColumns, 
      bool useRelationName)
    {
      if (data == null)
        return;
        
      TreeNode dataNode = root.Add(rel != null && useRelationName ? rel.Alias : data.Alias);
      dataNode.Tag = rel != null ? (object)rel : (object)data;
      dataNode.Checked = data.Enabled;
      dataNode.ImageIndex = rel != null ? 58 : 222;
      dataNode.SelectedImageIndex = dataNode.ImageIndex;
      
      bool alreadyProcessed = processed.Contains(data);
      processed.Add(data);

      // process relations
      if (showRelations && !alreadyProcessed)
      {
        foreach (Relation relation in dictionary.Relations)
        {
          if ((!enabledOnly || relation.Enabled) && relation.ChildDataSource == data)
          {
            useRelationName = GetNumberOfRelations(dictionary, relation.ParentDataSource, relation.ChildDataSource) > 1;
            AddDataSource(dictionary, relation.ParentDataSource, dataNode.Nodes, relation, processed, 
              enabledOnly, true, showColumns, useRelationName);
          }    
        }
      }

      // add columns and/or nested datasources
      AddColumns(dataNode.Nodes, data.Columns, enabledOnly, showColumns);
      
      processed.Remove(data);
    }

    private static void AddParameter(Parameter par, TreeNodeCollection root)
    {
      TreeNode parNode = root.Add(par.Name);
      parNode.Tag = par;
      parNode.ImageIndex = par.Parameters.Count == 0 ? GetTypeImageIndex(par.DataType) : 234;
      parNode.SelectedImageIndex = parNode.ImageIndex;

      foreach (Parameter p in par.Parameters)
      {
        AddParameter(p, parNode.Nodes);
      }
    }

    private static void AddVariable(Parameter par, TreeNodeCollection root)
    {
      TreeNode parNode = root.Add(par.Name);
      parNode.Tag = par;
      parNode.ImageIndex = GetTypeImageIndex(par.DataType);
      parNode.SelectedImageIndex = parNode.ImageIndex;

      foreach (Parameter p in par.Parameters)
      {
        AddParameter(p, parNode.Nodes);
      }
    }

    private static void CreateFunctionsTree(Report report, ObjectInfo rootItem, TreeNodeCollection rootNode)
    {
      foreach (ObjectInfo item in rootItem.Items)
      {
        string text = "";
        int imageIndex = 66;

        MethodInfo func = item.Function;
        if (func != null)
        {
          text = func.Name;
          // if this is an overridden function, show its parameters
          if (rootItem.Name == text)
            text = report.CodeHelper.GetMethodSignature(func, false);
          imageIndex = GetTypeImageIndex(func.ReturnType);
        }
        else
        {
          // it's a category
          text = Res.TryGet(item.Text);
        }

        TreeNode node = rootNode.Add(text);
        node.Tag = func;
        node.ImageIndex = imageIndex;
        node.SelectedImageIndex = imageIndex;

        if (item.Items.Count > 0)
          CreateFunctionsTree(report, item, node.Nodes);
      }
    }

    public static void CreateDataTree(Dictionary dictionary, TreeNodeCollection root, bool enabledOnly,
      bool relations, bool dataSources, bool columns)
    {
      if (dataSources)
      {
        // create connection node and enumerate connection tables
        foreach (DataConnectionBase connection in dictionary.Connections)
        {
          TreeNode connNode = root.Add(connection.Name);
          connNode.Tag = connection;
          connNode.Checked = true;
          connNode.ImageIndex = 53;
          connNode.SelectedImageIndex = connNode.ImageIndex;

          foreach (TableDataSource data in connection.Tables)
          {
            if (!enabledOnly || data.Enabled)
              AddDataSource(dictionary, data, connNode.Nodes, enabledOnly, relations, columns);
          }
        }
        
        // process regular tables
        foreach (DataSourceBase data in dictionary.DataSources)
        {
          if (!enabledOnly || data.Enabled)
            AddDataSource(dictionary, data, root, enabledOnly, relations, columns);
        }
      }
      else if (relations)
      {
        // display relations only (used by the Relation type editor)
        foreach (Relation relation in dictionary.Relations)
        {
          TreeNode relNode = root.Add(relation.Alias + " (" + 
            relation.ParentDataSource.Alias + "->" + relation.ChildDataSource.Alias + ")");
          relNode.Tag = relation;
          relNode.Checked = true;
          relNode.ImageIndex = 58;
          relNode.SelectedImageIndex = relNode.ImageIndex;
        }
      }
    }

    public static void CreateDataTree(Dictionary dictionary, DataConnectionBase connection, 
      TreeNodeCollection root)
    {
      foreach (TableDataSource data in connection.Tables)
      {
        AddDataSource(dictionary, data, root, false, false, true);
        // fix the node text if table is not a query: display the TableName instead of Alias
        TreeNode node = root[root.Count - 1];
        if (String.IsNullOrEmpty(data.SelectCommand))
          node.Text = data.TableName.Replace("\"", "");
      }
    }
    
    public static void CreateParametersTree(ParameterCollection parameters, TreeNodeCollection root)
    {
      foreach (Parameter p in parameters)
      {
        AddParameter(p, root);
      }
    }

    public static void CreateVariablesTree(ParameterCollection parameters, TreeNodeCollection root)
    {
      foreach (Parameter p in parameters)
      {
        AddVariable(p, root);
      }
    }

    public static void CreateTotalsTree(TotalCollection totals, TreeNodeCollection root)
    {
      foreach (Total s in totals)
      {
        TreeNode sumNode = root.Add(s.Name);
        sumNode.Tag = s;
        sumNode.ImageIndex = 132;
        sumNode.SelectedImageIndex = sumNode.ImageIndex;
      }
    }

    public static void CreateFunctionsTree(Report report, TreeNodeCollection root)
    {
      CreateFunctionsTree(report, RegisteredObjects.FindFunctionsRoot(), root);
    }

    public static void CreateDialogControlsTree(Report report, TreeNodeCollection root)
    {
      TreeNode dialogNode = null;
      
      foreach (Base c in report.AllObjects)
      {
        if (c is DialogControl)
        {
          PropertyInfo bindableProp = (c as DialogControl).BindableProperty;
          if (bindableProp != null)
          {
            if (dialogNode == null)
            {
              dialogNode = root.Add(Res.Get("Designer,ToolWindow,Dictionary,DialogControls"));
              dialogNode.ImageIndex = 136;
              dialogNode.SelectedImageIndex = dialogNode.ImageIndex;
            }

            TreeNode node = dialogNode.Nodes.Add(c.Name + "." + bindableProp.Name);
            node.ImageIndex = GetTypeImageIndex(bindableProp.PropertyType);
            node.SelectedImageIndex = node.ImageIndex;
            node.Tag = c;
          }
        }
      }
    }

    public static DataSourceBase GetDataSource(Dictionary dictionary, string complexName)
    {
      if (String.IsNullOrEmpty(complexName))
        return null;
      
      string[] names = complexName.Split(new char[] { '.' });
      DataSourceBase data = dictionary.FindByAlias(names[0]) as DataSourceBase;
      if (data == null)
        return null;

      Column column = data;
      
      // take into account nested tables. Table may even be nested into Column.
      for (int i = 1; i < names.Length; i++)
      {
        Column childColumn = column.Columns.FindByAlias(names[i]);
        if (childColumn == null)
          break;

        if (childColumn is DataSourceBase)
          data = childColumn as DataSourceBase;
        column = childColumn;
      }  

      return data;
    }

    public static Column GetColumn(Dictionary dictionary, string complexName)
    {
      if (String.IsNullOrEmpty(complexName))
        return null;

      string[] names = complexName.Split(new char[] { '.' });
      // the first part of complex name is always datasource name.
      DataSourceBase data = dictionary.FindByAlias(names[0]) as DataSourceBase;
      
      return GetColumn(dictionary, data, names, false);
    }

    public static Column GetColumn(Dictionary dictionary, DataSourceBase data, string[] names, bool initRelation)
    {
      // Process the following cases:
      // - Table.Column
      // - Table.NestedTable.Column (specific to BO)
      // - Table.RelatedTable.Column
      // - Table.Column where Column has '.' inside (specific to MSSQL)
      // - Table.ComplexColumn.Column (specific to BO)

      if (data == null || names.Length < 2)
        return null;

      // search for relation
      int i = 1;
      for (; i < names.Length; i++)
      {
        bool found = false;
        foreach (Relation r in dictionary.Relations)
        {
          if (r.ChildDataSource == data &&
            (r.ParentDataSource != null && r.ParentDataSource.Alias == names[i] || r.Alias == names[i]))
          {
            data = r.ParentDataSource;
            if (initRelation)
              data.FindParentRow(r);
            found = true;
            break;
          }
        }

        // nothing found, break and try column name.
        if (!found)
          break;
      }

      // the rest is column name.
      ColumnCollection columns = data.Columns;

      // try full name first
      string columnName = "";
      for (int j = i; j < names.Length; j++)
      {
        columnName += (columnName == "" ? "" : ".") + names[j];
      }

      Column column = columns.FindByAlias(columnName);
      if (column != null)
        return column;

      // try nested columns
      for (int j = i; j < names.Length; j++)
      {
        column = columns.FindByAlias(names[j]);
        if (column == null)
          return null;
        columns = column.Columns;
      }

      return column;
    }

    public static bool IsValidColumn(Dictionary dictionary, string complexName)
    {
      return GetColumn(dictionary, complexName) != null;
    }

    // Checks if the specified column name is simple column.
    // The column is simple if it belongs to the datasource, and that datasource
    // is simple as well. This check is needed when we prepare a script for compile.
    // Simple columns are handled directly by the Report component, so they should be
    // skipped when generating the expression handler code.
    public static bool IsSimpleColumn(Dictionary dictionary, string complexName)
    {
      Column column = GetColumn(dictionary, complexName);
      return column != null && column.Parent is DataSourceBase && 
        (column.Parent as DataSourceBase).FullName + "." + column.Alias == complexName;
    }
    
    public static int GetNumberOfRelations(Dictionary dictionary, DataSourceBase parent, DataSourceBase child)
    {
      int result = 0;
      foreach (Relation rel in dictionary.Relations)
      {
        if (rel.ParentDataSource == parent && rel.ChildDataSource == child)
          result++;
      }
      
      return result;
    }

    public static bool IsValidParameter(Dictionary dictionary, string complexName)
    {
      string[] names = complexName.Split(new char[] { '.' });
      Parameter par = dictionary.Parameters.FindByName(names[0]);
      if (par == null)
      {
        if (names.Length == 1)
          par = dictionary.SystemVariables.FindByName(names[0]);
        return par != null;
      }  
        
      for (int i = 1; i < names.Length; i++)
      {
        par = par.Parameters.FindByName(names[i]);
        if (par == null)
          return false;
      }
      return true;
    }

    public static bool IsValidTotal(Dictionary dictionary, string name)
    {
      Total sum = dictionary.Totals.FindByName(name);
      return sum != null;
    }

    public static Parameter GetParameter(Dictionary dictionary, string complexName)
    {
      string[] names = complexName.Split(new char[] { '.' });
      Parameter par = dictionary.Parameters.FindByName(names[0]);
      if (par == null)
      {
        par = dictionary.SystemVariables.FindByName(names[0]);
        return par;
      }  

      for (int i = 1; i < names.Length; i++)
      {
        par = par.Parameters.FindByName(names[i]);
        if (par == null)
          return null;
      }
      return par;
    }

    public static Parameter CreateParameter(Dictionary dictionary, string complexName)
    {
      string[] names = complexName.Split(new char[] { '.' });
      ParameterCollection parameters = dictionary.Parameters;
      Parameter par = null;
      
      for (int i = 0; i < names.Length; i++)
      {
        par = parameters.FindByName(names[i]);
        if (par == null)
        {
          par = new Parameter(names[i]);
          parameters.Add(par);
        }
        parameters = par.Parameters;
      }
      
      return par;
    }

    public static object GetTotal(Dictionary dictionary, string name)
    {
      Total sum = dictionary.Totals.FindByName(name);
      if (sum != null)
        return sum.Value;
      return null;  
    }

    public static Type GetColumnType(Dictionary dictionary, string complexName)
    {
      Column column = GetColumn(dictionary, complexName);
      return column.DataType;
    }

    public static int GetTypeImageIndex(Type dataType)
    {
      int index = 224;
      if (dataType == typeof(string) || dataType == typeof(char))
        index = 223;
      else if (dataType == typeof(float) || dataType == typeof(double))
        index = 225;
      else if (dataType == typeof(decimal))
        index = 226;
      else if (dataType == typeof(DateTime) || dataType == typeof(TimeSpan))
        index = 227;
      else if (dataType == typeof(bool))
        index = 228;
      else if (dataType == typeof(byte[]) || dataType == typeof(Image) || dataType == typeof(Bitmap))
        index = 229;
      return index;
    }
    
    public static Relation FindRelation(Dictionary dictionary, DataSourceBase parent, DataSourceBase child)
    {
      foreach (Relation relation in dictionary.Relations)
      {
        if (relation.ParentDataSource == parent && relation.ChildDataSource == child)
          return relation;
      }
      return null;
    }
  }
}
