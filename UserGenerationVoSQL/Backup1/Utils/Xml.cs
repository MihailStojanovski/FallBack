using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;

namespace FastReport.Utils
{
  /// <summary>
  /// Represents a xml node.
  /// </summary>
  public class XmlItem : IDisposable
  {
    private List<XmlItem> FItems;
    private XmlItem FParent;
    private string FName;
    private string FText;
    private string FValue;

    /// <summary>
    /// Gets a number of children in this node.
    /// </summary>
    public int Count
    {
      get { return FItems == null ? 0 : FItems.Count; }
    }

    /// <summary>
    /// Gets a list of children in this node.
    /// </summary>
    public List<XmlItem> Items
    {
      get 
      {
        if (FItems == null)
          FItems = new List<XmlItem>();
        return FItems; 
      }
    }

    /// <summary>
    /// Gets a child node with specified index.
    /// </summary>
    /// <param name="index">Index of node.</param>
    /// <returns>The node with specified index.</returns>
    public XmlItem this[int index]
    {
      get { return Items[index]; }
    }

    /// <summary>
    /// Gets or sets the node name.
    /// </summary>
    /// <remarks>
    /// This property will return "Node" for a node like <c>&lt;Node Text="" Left="0"/&gt;</c>
    /// </remarks>
    public string Name
    {
      get { return FName; }
      set { FName = value; }
    }

    /// <summary>
    /// Gets or sets the parent for this node.
    /// </summary>
    public XmlItem Parent
    {
      get { return FParent; }
      set 
      { 
        if (FParent != value)
        {
          if (FParent != null)
            FParent.Items.Remove(this);
          if (value != null)
            value.Items.Add(this);  
        }
        FParent = value; 
      }
    }

    /// <summary>
    /// Gets or sets the node text.
    /// </summary>
    /// <remarks>
    /// This property will return "Text="" Left="0"" for a node like <c>&lt;Node Text="" Left="0"/&gt;</c>
    /// </remarks>
    public string Text
    {
      get { return FText; }
      set { FText = value; }
    }

    /// <summary>
    /// Gets or sets the node value.
    /// </summary>
    /// <remarks>
    /// This property will return "ABC" for a node like <c>&lt;Node&gt;ABC&lt;/Node&gt;</c>
    /// </remarks>
    public string Value
    {
      get { return FValue; }
      set { FValue = value; }
    }

    /// <summary>
    /// Gets the root node which owns this node.
    /// </summary>
    public XmlItem Root
    {
      get
      {
        XmlItem result = this;
        while (result.Parent != null)
        {
          result = result.Parent;
        }  
        return result;
      }
    }

    /// <summary>
    /// Clears the child nodes of this node.
    /// </summary>
    public void Clear()
    {
      if (FItems != null)
      {
        while (Items.Count > 0)
        {
          Items[0].Dispose();
        }  
        FItems = null;
      }
    }

    /// <summary>
    /// Adds a new child node to this node.
    /// </summary>
    /// <returns>The new child node.</returns>
    public XmlItem Add()
    {
      XmlItem result = new XmlItem();
      AddItem(result);
      return result;
    }

    /// <summary>
    /// Adds a specified node to this node.
    /// </summary>
    /// <param name="item">The node to add.</param>
    public void AddItem(XmlItem item)
    {
      item.Parent = this;
    }

    /// <summary>
    /// Inserts a specified node to this node.
    /// </summary>
    /// <param name="index">Position to insert.</param>
    /// <param name="item">Node to insert.</param>
    public void InsertItem(int index, XmlItem item)
    {
      AddItem(item);
      Items.RemoveAt(Count - 1);
      Items.Insert(index, item);
    }

    /// <summary>
    /// Finds the node with specified name.
    /// </summary>
    /// <param name="name">The name of node to find.</param>
    /// <returns>The node with specified name, if found; <b>null</b> otherwise.</returns>
    public int Find(string name)
    {
      for (int i = 0; i < Count; i++)
      {
        if (String.Compare(Items[i].Name, name, true) == 0)
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Finds the node with specified name.
    /// </summary>
    /// <param name="name">The name of node to find.</param>
    /// <returns>The node with specified name, if found; the new node otherwise.</returns>
    /// <remarks>
    /// This method adds the node with specified name to the child nodes if it cannot find the node.
    /// </remarks>
    public XmlItem FindItem(string name)
    {
      XmlItem result = null;
      int i = Find(name);
      if (i == -1)
      {
        result = Add();
        result.Name = name;
      }
      else
        result = Items[i];
      return result;
    }

    /// <summary>
    /// Gets the index of specified node in the child nodes list.
    /// </summary>
    /// <param name="item">The node to find.</param>
    /// <returns>Zero-based index of node, if found; <b>-1</b> otherwise.</returns>
    public int IndexOf(XmlItem item)
    {
      return Items.IndexOf(item);
    }

    /// <summary>
    /// Gets a property with specified name.
    /// </summary>
    /// <param name="index">The property name.</param>
    /// <returns>The value of property, if found; empty string otherwise.</returns>
    /// <remarks>
    /// This property will return "0" when you request the "Left" property for a node 
    /// like <c>&lt;Node Text="" Left="0"/&gt;</c>
    /// </remarks>
    public string GetProp(string index)
    {
      string result = "";
      int i = FText.ToUpper().IndexOf(index.ToUpper() + "=\"");
      if (i != -1 && (i == 0 || FText[i - 1] == 32))
      {
        result = FText.Substring(i + index.Length + 2);
        result = Converter.FromXml(result.Substring(0, result.IndexOf("\"")));
      }
      else
        result = "";
      return result;
    }

    /// <summary>
    /// Sets the value for a specified property.
    /// </summary>
    /// <param name="index">The property name.</param>
    /// <param name="value">Value to set.</param>
    /// <remarks>
    /// For example, you have a node like <c>&lt;Node Text="" Left="0"/&gt;</c>. When you set the
    /// "Text" property to "test", the node will be <c>&lt;Node Text="test" Left="0"/&gt;</c>.
    /// If property with specified name is not exist, it will be added.
    /// </remarks>
    public void SetProp(string index, string value)
    {
      string s = index + "=\"" + Converter.ToXml(value) + "\"";
      int i = FText.ToUpper().IndexOf(index.ToUpper() + "=\"");
      if (i != -1 && (i == 0 || FText[i - 1] == 32))
      {
        int j = i + index.Length + 2;
        while (j <= FText.Length && FText[j] != 34)
          j++;
        FText = FText.Remove(i, j - i + 1);
      }
      else
      {
        i = FText.Length + 1;
        s = " " + s;
      }
      FText = i > FText.Length ? FText + s : FText.Insert(i, s);
    }

    /// <summary>
    /// Disposes the node and all its children.
    /// </summary>
    public void Dispose()
    {
      Clear();
      Parent = null;
    }

    /// <summary>
    /// Initializes a new instance of the <b>XmlItem</b> class with default settings.
    /// </summary>
    public XmlItem() 
    {
      FName = "";
      FText = "";
      FValue = "";
    }
  }


  /// <summary>
  /// Represents a xml document that contains the root xml node.
  /// </summary>
  /// <remarks>
  /// Use <b>Load</b> and <b>Save</b> methods to load/save the document. To access the root node
  /// of the document, use the <see cref="Root"/> property.
  /// </remarks>
  public class XmlDocument : IDisposable
  {
    private bool FAutoIndent;
    private XmlItem FRoot;

    /// <summary>
    /// Gets or sets a value indicating whether is necessary to indent the document
    /// when saving it to a file/stream.
    /// </summary>
    public bool AutoIndent
    {
      get { return FAutoIndent; }
      set { FAutoIndent = value; }
    }

    /// <summary>
    /// Gets the root node of the document.
    /// </summary>
    public XmlItem Root
    {
      get { return FRoot; }
    }

    /// <summary>
    /// Clears the document.
    /// </summary>
    public void Clear()
    {
      FRoot.Clear();
    }

    /// <summary>
    /// Saves the document to a stream.
    /// </summary>
    /// <param name="stream">Stream to save to.</param>
    public void Save(Stream stream)
    {
      XmlWriter wr = new XmlWriter(stream);
      wr.AutoIndent = FAutoIndent;
      wr.Write(FRoot);
    }

    /// <summary>
    /// Loads the document from a stream.
    /// </summary>
    /// <param name="stream">Stream to load from.</param>
    public void Load(Stream stream)
    {
      XmlReader rd = new XmlReader(stream);
      FRoot.Clear();
      rd.Read(FRoot);
    }

    /// <summary>
    /// Saves the document to a file.
    /// </summary>
    /// <param name="fileName">The name of file to save to.</param>
    public void Save(string fileName)
    {
      FileStream s = new FileStream(fileName, FileMode.Create);
      Save(s);
      s.Close();
    }

    /// <summary>
    /// Loads the document from a file.
    /// </summary>
    /// <param name="fileName">The name of file to load from.</param>
    public void Load(string fileName)
    {
      FileStream s = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      Load(s);
      s.Close();
    }

    /// <summary>
    /// Disposes resources used by the document.
    /// </summary>
    public void Dispose()
    {
      FRoot.Dispose();
    }

    /// <summary>
    /// Initializes a new instance of the <b>XmlDocument</b> class with default settings.
    /// </summary>
    public XmlDocument()
    {
      FRoot = new XmlItem();
    }
  }

  internal class XmlReader
  {
    private StringBuilder FBuilder;
    private StreamReader FReader;
    private Stream FStream;
    private string FLastName;
    private enum ReadState { FindLeft, FindRight, FindComment, FindCloseItem, Done }
    private enum ItemState { Begin, End, Complete }
    private int FSymbolInBuffer;

    private ItemState ReadItem(XmlItem item)
    {
      FBuilder.Length = 0;
      ReadState state = ReadState.FindLeft;
      int comment = 0;
      int i = 0;

      do
      {
        int c = FSymbolInBuffer == -1 ? FReader.Read() : FSymbolInBuffer;
        FSymbolInBuffer = -1;
        if (c == -1)
          RaiseException();

        if (state == ReadState.FindLeft)
        {
          if (c == '<')
            state = ReadState.FindRight;
          else if (c != ' ' && c != '\r' && c != '\n' && c != '\t')
            RaiseException();  
        }
        else if (state == ReadState.FindRight)
        {
          if (c == '>')
          {
            state = ReadState.Done;
            break;
          }
          else if (c == '<')
            RaiseException();
          else
          {
            FBuilder.Append((char)c);
            i++;
            if (i == 3 && FBuilder[0] == '!' && FBuilder[1] == '-' && FBuilder[2] == '-')
            {
              state = ReadState.FindComment;
              comment = 0;
              i = 0;
              FBuilder.Length = 0;
            }
          }
        }
        else if (state == ReadState.FindComment)
        {
          if (comment == 2)
          {
            if (c == '>')
              state = ReadState.FindLeft;
          }
          else
          {
            if (c == '-')
              comment++;
            else
              comment = 0;
          }
        }
      }
      while (true);

      if (FBuilder[FBuilder.Length - 1] == ' ')
        FBuilder.Length--;

      string itemText = FBuilder.ToString();
      ItemState itemState = ItemState.Begin;
      if (itemText.StartsWith("/"))
      {
        itemText = itemText.Substring(1);
        itemState = ItemState.End;
      }  
      else if (itemText.EndsWith("/"))
      {
        itemText = itemText.Substring(0, itemText.Length - 1);
        itemState = ItemState.Complete;
      }  

      item.Name = itemText;
      item.Text = "";

      i = itemText.IndexOf(" ");
      if (i != -1)
      {
        item.Name = itemText.Substring(0, i);
        item.Text = itemText.Substring(i + 1);
      }
      
      return itemState;
    }

    private bool ReadValue(XmlItem item)
    {
      FBuilder.Length = 0;
      ReadState state = ReadState.FindLeft;
      string lastName = "</" + FLastName + ">";
      int lastNameLength = lastName.Length;

      do
      {
        int c = FReader.Read();
        if (c == -1)
          RaiseException();

        FBuilder.Append((char)c);
        if (state == ReadState.FindLeft)
        {
          if (c == '<')
          {
            FSymbolInBuffer = '<';
            return false;
          }  
          else if (c != ' ' && c != '\r' && c != '\n' && c != '\t')
            state = ReadState.FindCloseItem;
        }
        else if (state == ReadState.FindCloseItem)
        {
          if (FBuilder.Length >= lastNameLength)
          {
            bool match = true;
            for (int j = 0; j < lastNameLength; j++)
            {
              if (FBuilder[FBuilder.Length - lastNameLength + j] != lastName[j])
              {
                match = false;
                break;
              }
            }

            if (match)
            {
              FBuilder.Length -= lastNameLength;
              item.Value = FBuilder.ToString();
              return true;
            }
          }
        }
      }
      while (true);
    }

    private bool DoRead(XmlItem rootItem)
    {
      ItemState itemState = ReadItem(rootItem);
      FLastName = rootItem.Name;

      if (itemState == ItemState.End)
        return true;
      else if (itemState == ItemState.Complete)
        return false;  

      if (ReadValue(rootItem))
        return false;

      bool done = false;
      do
      {
        XmlItem childItem = new XmlItem();
        done = DoRead(childItem);
        if (!done)
          rootItem.AddItem(childItem);
        else
          childItem.Dispose();
      }
      while (!done);

      if (FLastName != "" && String.Compare(FLastName, rootItem.Name, true) != 0)
        RaiseException();

      return false;
    }

    private void RaiseException()
    {
      throw new FileFormatException();
    }

    private void ReadHeader()
    {
      XmlItem item = new XmlItem();
      ReadItem(item);
      if (item.Name.IndexOf("?xml") != 0)
        RaiseException();
    }

    public void Read(XmlItem item)
    {
      ReadHeader();
      DoRead(item);
    }

    public XmlReader(Stream stream)
    {
      FStream = stream;
      FBuilder = new StringBuilder();
      FReader = new StreamReader(FStream, Encoding.UTF8);
      FLastName = "";
      FSymbolInBuffer = -1;
    }
  }


  internal class XmlWriter
  {
    private bool FAutoIndent;
    private Stream FStream;
    private StreamWriter FWriter;

    public bool AutoIndent
    {
      get { return FAutoIndent; }
      set { FAutoIndent = value; }
    }

    private void WriteLn(string s)
    {
      if (!FAutoIndent)
        FWriter.Write(s);
      else
        FWriter.Write(s + "\r\n");
    }

    private string Dup(int num)
    {
      string s = "";
      return s.PadLeft(num);
    }

    private void WriteItem(XmlItem item, int level)
    {
      string s;

      // start
      if (!FAutoIndent)
        s = "<" + item.Name;
      else
        s = Dup(level) + "<" + item.Name;

      // text
      if (item.Text != "")
      {
        if (item.Text.StartsWith(" "))
          s += item.Text;
        else
          s += " " + item.Text;
      }

      // end
      if (item.Count == 0 && item.Value == "")
        s += "/>";
      else
        s += ">";

      // value
      if (item.Count == 0 && item.Value != "")
        s += item.Value + "</" + item.Name + ">";
        
      WriteLn(s);
    }

    private void DoWrite(XmlItem rootItem, int level)
    {
      if (!FAutoIndent)
        level = 0;

      WriteItem(rootItem, level);
      for (int i = 0; i < rootItem.Count; i++)
        DoWrite(rootItem[i], level + 2);
      
      if (rootItem.Count > 0)
      {
        if (!FAutoIndent)
          WriteLn("</" + rootItem.Name + ">");
        else
          WriteLn(Dup(level) + "</" + rootItem.Name + ">");
      }    
    }

    private void WriteHeader()
    {
      WriteLn("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
    }

    public void Write(XmlItem rootItem)
    {
      WriteHeader();
      DoWrite(rootItem, 0);
      FWriter.Flush();
    }

    public XmlWriter(Stream stream)
    {
      FStream = stream;
      FWriter = new StreamWriter(FStream, Encoding.UTF8);
    }
  }
}