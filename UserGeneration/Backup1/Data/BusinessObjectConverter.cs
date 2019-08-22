using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FastReport.Utils;

namespace FastReport.Data
{
  /// <summary>
  /// <b>Obsolete</b>. Specifies a set of flags used to convert business objects into datasources.
  /// </summary>
  [Flags]
  public enum BOConverterFlags
  {
    /// <summary>
    /// Specifies no actions.
    /// </summary>
    None,
    
    /// <summary>
    /// Allows using the fields of a business object.
    /// </summary>
    AllowFields,
    
    /// <summary>
    /// Allows using properties of a business object with <b>BrowsableAttribute</b> only.
    /// </summary>
    BrowsableOnly
  }


  /// <summary>
  /// Specifies a kind of property.
  /// </summary>
  public enum PropertyKind
  {
    /// <summary>
    /// Specifies the property of a simple type (such as integer).
    /// </summary>
    Simple,

    /// <summary>
    /// Specifies the complex property such as class with own properties.
    /// </summary>
    Complex,

    /// <summary>
    /// Specifies the property which is a list of objects (is of IEnumerable type).
    /// </summary>
    Enumerable
  }


  internal class BusinessObjectConverter
  {
    private Dictionary FDictionary;
    private int FNestingLevel;
    private int FMaxNestingLevel;
    
    private PropertyKind GetPropertyKind(Type type)
    {
      PropertyKind kind = PropertyKind.Complex;
      if (type.IsValueType ||
        type == typeof(string) ||
        type == typeof(byte[]) ||
        typeof(Image).IsAssignableFrom(type))
      {
        kind = PropertyKind.Simple;
      }
      else if (typeof(IEnumerable).IsAssignableFrom(type))
      {
        kind = PropertyKind.Enumerable;
      }

      GetPropertyKindEventArgs args = new GetPropertyKindEventArgs(type, kind);
      Config.ReportSettings.OnGetBusinessObjectPropertyKind(null, args);
      return args.PropertyKind;
    }

    private bool IsSimpleType(Type type)
    {
      return GetPropertyKind(type) == PropertyKind.Simple;
    }

    private bool IsEnumerable(Type type)
    {
      return GetPropertyKind(type) == PropertyKind.Enumerable;
    }

    private bool IsLoop(Column column, Type type)
    {
      while (column != null)
      {
        if (column.DataType == type)
          return true;
        column = column.Parent as Column;
      }
      return false;
    }

    private bool IsLoop(TreeNode node, Type type)
    {
      while (node != null)
      {
        Column column = node.Tag as Column;
        if (column.DataType == type)
          return true;
        node = node.Parent;
      }
      return false;
    }

    private PropertyDescriptorCollection GetProperties(Column column)
    {
      using (BindingSource source = new BindingSource())
      {
        source.DataSource = column.Reference != null ? column.Reference : column.DataType;
        
        PropertyDescriptorCollection properties = source.GetItemProperties(null);
        PropertyDescriptorCollection filteredProperties = new PropertyDescriptorCollection(null);

        foreach (PropertyDescriptor prop in properties)
        {
          FilterPropertiesEventArgs args = new FilterPropertiesEventArgs(prop);
          Config.ReportSettings.OnFilterBusinessObjectProperties(source.DataSource, args);
          if (!args.Skip)
            filteredProperties.Add(args.Property);
        }
        
        return filteredProperties;
      }
    }

    private Column CreateListValueColumn(Column column)
    {
      Type itemType = ListBindingHelper.GetListItemType(column.DataType);

      // find existing column
      Column childColumn = column.FindByPropName("Value");

      // column not found, create new one with default settings
      if (childColumn == null)
      {
        childColumn = new Column();
        childColumn.Name = "Value";
        childColumn.Enabled = IsSimpleType(itemType);
        childColumn.SetBindableControlType(itemType);
      }

      childColumn.DataType = itemType;
      childColumn.PropName = "Value";
      childColumn.PropDescriptor = null;

      return childColumn;
    }

    private void GetReference(Column column, Column childColumn)
    {
      object obj = null;
      if (column is BusinessObjectDataSource)
      {
        IEnumerable enumerable = column.Reference as IEnumerable;
        if (enumerable != null)
        {
          IEnumerator enumerator = enumerable.GetEnumerator();
          while (enumerator.MoveNext())
          {
            obj = enumerator.Current;
          }
        }
      }
      else
      {
        obj = column.Reference;
      }

      if (obj != null)
      {
        try
        {
          childColumn.Reference = childColumn.PropDescriptor.GetValue(obj);
        }
        catch
        {
          childColumn.Reference = null;
        }
      }
    }

    private void CreateInitialObjects(Column column)
    {
      if (FNestingLevel >= FMaxNestingLevel)
        return;
      FNestingLevel++;

      PropertyDescriptorCollection properties = GetProperties(column);
      foreach (PropertyDescriptor prop in properties)
      {
        Type type = prop.PropertyType;
        bool isSimpleProperty = IsSimpleType(type);
        bool isEnumerable = IsEnumerable(type);

        Column childColumn = null;
        if (isEnumerable)
          childColumn = new BusinessObjectDataSource();
        else
          childColumn = new Column();
        column.Columns.Add(childColumn);

        childColumn.Name = isEnumerable ? FDictionary.CreateUniqueName(prop.Name) : prop.Name;
        childColumn.Alias = prop.DisplayName;
        childColumn.DataType = type;
        childColumn.PropName = prop.Name;
        childColumn.PropDescriptor = prop;
        childColumn.SetBindableControlType(type);
        childColumn.Enabled = !isEnumerable || FNestingLevel < FMaxNestingLevel;
        GetReference(column, childColumn);

        if (!isSimpleProperty)
          CreateInitialObjects(childColumn);
      }

      FNestingLevel--;
    }

    private void AddNode(TreeNodeCollection nodes, Column column)
    {
      TreeNode node = new TreeNode();
      node.Text = column.Alias;
      node.Checked = column.Enabled;
      node.Tag = column;
      node.ImageIndex = column.GetImageIndex();
      node.SelectedImageIndex = node.ImageIndex;
      nodes.Add(node);

      // handle nested nodes
      if (!IsSimpleType(column.DataType))
      {
        if (node.Checked)
        {
          // node is enabled? Discover its subnodes
          DiscoverNode(node);
        }
        else
        {
          // add empty node to enable expansion
          AddEmptyNode(node);
        }
      }
    }

    // adds an empty child node to allow node expansion
    private void AddEmptyNode(TreeNode node)
    {
      TreeNode emptyNode = new TreeNode();
      node.Nodes.Add(emptyNode);
    }

    // discovers the tree node structure (create subnodes)
    private void DiscoverNode(TreeNode node)
    {
      Column column = node.Tag as Column;

      // create tree nodes based on info from business object. Use existing Column only to 
      // correct the checked & text properties. This will guarantee that we have tree 
      // that match actual schema
      PropertyDescriptorCollection properties = GetProperties(column);
      if (properties.Count > 0)
      {
        foreach (PropertyDescriptor prop in properties)
        {
          Type type = prop.PropertyType;
          bool isSimpleProperty = IsSimpleType(type);
          bool isEnumerable = IsEnumerable(type);

          // find existing column
          Column childColumn = column.FindByPropName(prop.Name);

          // column not found, create new one with default settings
          if (childColumn == null)
          {
            if (isEnumerable)
              childColumn = new BusinessObjectDataSource();
            else
              childColumn = new Column();

            childColumn.Name = prop.Name;
            childColumn.Alias = prop.DisplayName;
            childColumn.SetBindableControlType(type);

            // enable column if it is simple property (such as int), or it is class-type
            // property that will not lead to loop. The latter is needed to enable all nested 
            // properties automatically
            childColumn.Enabled = isSimpleProperty || (!isEnumerable && !IsLoop(node, type));
          }

          // update column's DataType - the schema may be changed 
          childColumn.DataType = prop.PropertyType;
          childColumn.PropName = prop.Name;
          childColumn.PropDescriptor = prop;
          GetReference(column, childColumn);

          AddNode(node.Nodes, childColumn);
        }
      }
      else if (IsEnumerable(column.DataType))
      {
        Column childColumn = CreateListValueColumn(column);
        AddNode(node.Nodes, childColumn);
      }
    }

    // create initial n-level structure
    public void CreateInitialObjects(Column column, int maxNestingLevel)
    {
      FMaxNestingLevel = maxNestingLevel;
      CreateInitialObjects(column);
    }

    // update existing columns - add new, delete non-existent, update PropDescriptor
    public void UpdateExistingObjects(Column column, int maxNestingLevel)
    {
      FMaxNestingLevel = maxNestingLevel;
      UpdateExistingObjects(column);
    }

    private void UpdateExistingObjects(Column column)
    {
      FNestingLevel++;

      // reset property descriptors to determine later which columns are outdated
      foreach (Column c in column.Columns)
      {
        c.PropDescriptor = null;
      }

      PropertyDescriptorCollection properties = GetProperties(column);
      if (properties.Count > 0)
      {
        foreach (PropertyDescriptor prop in properties)
        {
          Type type = prop.PropertyType;
          bool isSimpleProperty = IsSimpleType(type);
          bool isEnumerable = IsEnumerable(type);

          // find existing column
          Column childColumn = column.FindByPropName(prop.Name);

          // column not found, create new one
          if (childColumn == null)
          {
            if (isEnumerable)
              childColumn = new BusinessObjectDataSource();
            else
              childColumn = new Column();
            column.Columns.Add(childColumn);

            childColumn.Name = isEnumerable ? FDictionary.CreateUniqueName(prop.Name) : prop.Name;
            childColumn.Alias = prop.DisplayName;
            childColumn.SetBindableControlType(type);

            // enable column if it is simple property, or max nesting level is not reached
            childColumn.Enabled = isSimpleProperty || FNestingLevel < FMaxNestingLevel;
          }

          // update column's prop data - the schema may be changed 
          childColumn.DataType = prop.PropertyType;
          childColumn.PropName = prop.Name;
          childColumn.PropDescriptor = prop;
          GetReference(column, childColumn);

          if (childColumn.Enabled && !isSimpleProperty)
            UpdateExistingObjects(childColumn);
        }
      }
      else if (IsEnumerable(column.DataType))
      {
        CreateListValueColumn(column);
      }

      // remove non-existent columns
      for (int i = 0; i < column.Columns.Count; i++)
      {
        Column c = column.Columns[i];
        // delete columns with empty descriptors, except the "Value" columns
        if (c.PropDescriptor == null && c.PropName != "Value")
        {
          column.Columns.RemoveAt(i);
          i--;
        }
      }

      FNestingLevel--;
    }

    // creates the tree based on the datasource structure
    public void CreateTree(TreeNodeCollection nodes, Column dataSource)
    {
      AddNode(nodes, dataSource);
    }

    public void CheckNode(TreeNode node)
    {
      Column column = node.Tag as Column;
      column.Enabled = node.Checked;
      node.Nodes.Clear();

      if (!IsSimpleType(column.DataType))
      {
        if (node.Checked)
        {
          DiscoverNode(node);
          node.Expand();
        }
        else
        {
          AddEmptyNode(node);
          node.Collapse();
        }
      }
    }

    // creates the datasource structure based on the tree
    public void CreateDataSource(TreeNode node)
    {
      Column column = node.Tag as Column;
      // clear the Columns collection. Do not use Clear method because it will
      // destroy the objects.
      while (column.Columns.Count > 0)
      {
        column.Columns.RemoveAt(0);
      }

      foreach (TreeNode childNode in node.Nodes)
      {
        Column childColumn = childNode.Tag as Column;
        bool isDataSource = childColumn is BusinessObjectDataSource;

        if (childNode.Checked)
        {
          // fix datasource name
          if (isDataSource)
          {
            string saveAlias = childColumn.Alias;
            childColumn.Name = FDictionary.CreateUniqueName(childColumn.Name);
            childColumn.Alias = saveAlias;
          }
          
          column.Columns.Add(childColumn);
          CreateDataSource(childNode);
        }
        else if (!isDataSource)
        {
          column.Columns.Add(childColumn);
        }
      }
    }

    public BusinessObjectConverter(Dictionary dictionary)
    {
      FDictionary = dictionary;
    }
  }
}
