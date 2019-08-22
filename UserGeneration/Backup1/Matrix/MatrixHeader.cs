using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using FastReport.Utils;

namespace FastReport.Matrix
{
  /// <summary>
  /// Represents a collection of matrix header descriptors used in the <see cref="MatrixObject"/>.
  /// </summary>
  public class MatrixHeader : CollectionBase, IFRSerializable
  {
    private MatrixHeaderItem FRootItem;
    private int FNextIndex;
    private string FName;

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">Index of an element.</param>
    /// <returns>The element at the specified index.</returns>
    public MatrixHeaderDescriptor this[int index]
    {
      get { return List[index] as MatrixHeaderDescriptor; }
      set { List[index] = value; }
    }

    internal MatrixHeaderItem RootItem
    {
      get { return FRootItem; }
    }
    
    internal string Name
    {
      get { return FName; }
      set { FName = value; }
    }

    /// <summary>
    /// Adds the specified descriptors to the end of this collection.
    /// </summary>
    /// <param name="range">Array of descriptors to add.</param>
    public void AddRange(MatrixHeaderDescriptor[] range)
    {
      foreach (MatrixHeaderDescriptor s in range)
      {
        Add(s);
      }
    }

    /// <summary>
    /// Adds a descriptor to the end of this collection.
    /// </summary>
    /// <param name="value">Descriptor to add.</param>
    /// <returns>Index of the added descriptor.</returns>
    public int Add(MatrixHeaderDescriptor value)
    {
      return List.Add(value);
    }

    /// <summary>
    /// Inserts a descriptor into this collection at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which value should be inserted.</param>
    /// <param name="value">The descriptor to insert.</param>
    public void Insert(int index, MatrixHeaderDescriptor value)
    {
      List.Insert(index, value);
    }

    /// <summary>
    /// Removes the specified descriptor from the collection.
    /// </summary>
    /// <param name="value">Descriptor to remove.</param>
    public void Remove(MatrixHeaderDescriptor value)
    {
      int i = IndexOf(value);
      if (i != -1)
        List.RemoveAt(i);
    }

    /// <summary>
    /// Returns the zero-based index of the first occurrence of a descriptor.
    /// </summary>
    /// <param name="value">The descriptor to locate in the collection.</param>
    /// <returns>The zero-based index of the first occurrence of descriptor within 
    /// the entire collection, if found; otherwise, -1.</returns>
    public int IndexOf(MatrixHeaderDescriptor value)
    {
      return List.IndexOf(value);
    }

    /// <summary>
    /// Determines whether a descriptor is in the collection.
    /// </summary>
    /// <param name="value">The descriptor to locate in the collection.</param>
    /// <returns><b>true</b> if descriptor is found in the collection; otherwise, <b>false</b>.</returns>
    public bool Contains(MatrixHeaderDescriptor value)
    {
      return List.Contains(value);
    }

    /// <summary>
    /// Copies the elements of this collection to a new array. 
    /// </summary>
    /// <returns>An array containing copies of this collection elements. </returns>
    public MatrixHeaderDescriptor[] ToArray()
    {
      MatrixHeaderDescriptor[] result = new MatrixHeaderDescriptor[Count];
      for (int i = 0; i < Count; i++)
      {
        result[i] = this[i];
      }
      return result;
    }

    internal MatrixHeaderItem Find(object[] address, bool create, int dataRowNo)
    {
      // Note that the top header itself does not contain a value. 
      // It is used as a list of first-level headers only.
      MatrixHeaderItem rootItem = RootItem;

      for (int i = 0; i < address.Length; i++)
      {
        int index = rootItem.Find(address[i], this[i].Sort);
        if (index >= 0)
          rootItem = rootItem.Items[index];
        else if (create)
        {
          // create new item if necessary.
          MatrixHeaderItem newItem = new MatrixHeaderItem(rootItem);
          newItem.Value = address[i];
          newItem.TemplateColumn = this[i].TemplateColumn;
          newItem.TemplateRow = this[i].TemplateRow;
          newItem.TemplateCell = this[i].TemplateCell;
          newItem.DataRowNo = dataRowNo;
          newItem.PageBreak = this[i].PageBreak;

          // index is used as a cell address in a matrix
          if (i == address.Length - 1)
          {
            // create index for bottom-level header
            newItem.Index = FNextIndex;
            FNextIndex++;
          }

          rootItem.Items.Insert(~index, newItem);
          rootItem = newItem;
        }
        else
          return null;
      }

      return rootItem;
    }
    
    private void AddTotalItems(MatrixHeaderItem rootItem, int descriptorIndex, bool isTemplate)
    {
      if (descriptorIndex >= Count)
        return;
        
      foreach (MatrixHeaderItem item in rootItem.Items)
      {
        AddTotalItems(item, descriptorIndex + 1, isTemplate);
      }
      
      if (this[descriptorIndex].Totals && 
        (isTemplate || !this[descriptorIndex].SuppressTotals || rootItem.Items.Count > 1))
      {
        MatrixHeaderItem totalItem = new MatrixHeaderItem(rootItem);
        totalItem.IsTotal = true;
        totalItem.Value = rootItem.Value;
        totalItem.TemplateColumn = this[descriptorIndex].TemplateTotalColumn;
        totalItem.TemplateRow = this[descriptorIndex].TemplateTotalRow;
        totalItem.TemplateCell = this[descriptorIndex].TemplateTotalCell;
        rootItem.Items.Add(totalItem);
      }
    }
    
    internal void AddTotalItems(bool isTemplate)
    {
      AddTotalItems(RootItem, 0, isTemplate);
    }

    internal void Reset()
    {
      RootItem.Clear();
      FNextIndex = 0;
    }

    /// <inheritdoc/>
    public void Serialize(FRWriter writer)
    {
      writer.ItemName = Name;
      foreach (MatrixHeaderDescriptor d in this)
      {
        writer.Write(d);
      }
    }

    /// <inheritdoc/>
    public void Deserialize(FRReader reader)
    {
      Clear();
      while (reader.NextItem())
      {
        MatrixHeaderDescriptor d = new MatrixHeaderDescriptor();
        reader.Read(d);
        Add(d);
      }
    }

    internal MatrixHeader()
    {
      FRootItem = new MatrixHeaderItem(null);
    }
  }
}
