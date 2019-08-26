using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;

namespace FastReport.Utils
{
  /// <summary>
  /// The reader used to deserialize object's properties from a report file.
  /// </summary>
  public class FRReader : IDisposable
  {
    #region Fields
    private XmlDocument FDoc;
    private XmlItem FRoot;
    private XmlItem FCurItem;
    private XmlItem FCurRoot;
    private List<PropInfo> FProps;
    private string FErrors;
    private List<FixupInfo> FFixups;
    private Report FReport;
    private BlobStore FBlobStore;
    private bool FReadChildren;
    #endregion

    #region Properties
    /// <summary>
    /// Gets a string that contains errors occured during the load.
    /// </summary>
    public string Errors
    {
      get { return FErrors; }
    }
    
    internal BlobStore BlobStore
    {
      get { return FBlobStore; }
      set { FBlobStore = value; }
    }
    
    /// <summary>
    /// Gets the current item name.
    /// </summary>
    public string ItemName
    {
      get { return FCurItem.Name; }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whther is necessary to read the object's children.
    /// </summary>
    public bool ReadChildren
    {
      get { return FReadChildren; }
      set { FReadChildren = value; }
    }
    #endregion

    #region Private Methods
    private void GetProps()
    {
      FProps = new List<PropInfo>();
      if (FCurRoot != null)
      {
        string s = FCurRoot.Text;
        int len = s.Length;
        int i = 0;
        while (i < len)
        {
          // skip spaces
          while (i < len && s[i] == ' ')
            i++;
          int start = i;
          // find '='
          while (i < len && s[i] != '=')
            i++;
          if (i < len)
          {
            // get property name
            string name = s.Substring(start, i - start);
            if (name == "") break;
            // find first '"'
            while (i < len && s[i] != '"')
              i++;
            start = i + 1;
            i++;
            // find second '"'
            while (i < len && s[i] != '"')
              i++;
            // get property value between first and second '"'  
            string value = s.Substring(start, i - start);

            // add the pair of name/value to the prop list
            FProps.Add(new PropInfo(name, value));
            i++;
          }
        }
      }
    }

    private string PropName(string name)
    {
      return ShortProperties.GetFullName(name);
    }

    private string PropValue(string name)
    {
      int i = IndexOf(name);
      if (i != -1)
        return FProps[i].Value;
      return "";
    }
    
    private int IndexOf(string name)
    {
      int result = -1;
      for (int i = 0; i < FProps.Count; i++)
      {
        if (String.Compare(FProps[i].Name, name, true) == 0)
        {
          result = i;
          break;
        }
      }
      return result;
    }

    private void FixupReferences()
    {
      if (FReport == null)
        return;
      foreach (FixupInfo fixup in FFixups)
      {
        PropertyInfo pi = fixup.Obj.GetType().GetProperty(fixup.Name);
        if (pi != null)
          pi.SetValue(fixup.Obj, FReport.FindObject(fixup.Value), null);
      }
      FFixups.Clear();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Reads the specified object.
    /// </summary>
    /// <param name="obj">The object to read.</param>
    /// <remarks>
    /// The object must implement the <see cref="IFRSerializable"/> interface. This method
    /// invokes the <b>Deserialize</b> method of the object.
    /// </remarks>
    /// <example>This example demonstrates the use of <b>ReadProperties</b>, <b>ReadChildren</b>,
    /// <b>NextItem</b>, <b>Read</b> methods.
    /// <code>
    /// public void Deserialize(FRReader reader)
    /// {
    ///   // read simple properties like "Text", complex properties like "Border.Lines"
    ///   reader.ReadProperties(this);
    /// 
    ///   // moves the current reader item
    ///   while (reader.NextItem())
    ///   {
    ///     // read the "Styles" collection
    ///     if (String.Compare(reader.ItemName, "Styles", true) == 0)
    ///       reader.Read(Styles);
    ///     else if (reader.ReadChildren)
    ///     {
    ///       // if read of children is enabled, read them
    ///       Base obj = reader.Read();
    ///       if (obj != null)
    ///          obj.Parent = this;
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    public void Read(IFRSerializable obj)
    {
      XmlItem saveCurItem = FCurItem;
      XmlItem saveCurRoot = FCurRoot;
      List<PropInfo> saveProps = FProps;
      try
      {
        if (FCurItem == null)
          FCurItem = FRoot;
        FCurRoot = FCurItem;
        GetProps();
        obj.Deserialize(this);
      }
      finally
      {
        FCurItem = saveCurItem;
        FCurRoot = saveCurRoot;
        FProps = saveProps;
      }
    }

    /// <summary>
    /// Reads an object from current xml node.
    /// </summary>
    /// <returns>The object.</returns>
    /// <remarks>
    /// This method creates an instance of object described by the current xml node, then invokes
    /// its <b>Deserialize</b> method.
    /// </remarks>
    /// <example>This example demonstrates the use of <b>ReadProperties</b>, <b>ReadChildren</b>,
    /// <b>NextItem</b>, <b>Read</b> methods.
    /// <code>
    /// public void Deserialize(FRReader reader)
    /// {
    ///   // read simple properties like "Text", complex properties like "Border.Lines"
    ///   reader.ReadProperties(this);
    /// 
    ///   // moves the current reader item
    ///   while (reader.NextItem())
    ///   {
    ///     // read the "Styles" collection
    ///     if (String.Compare(reader.ItemName, "Styles", true) == 0)
    ///       reader.Read(Styles);
    ///     else if (reader.ReadChildren)
    ///     {
    ///       // if read of children is enabled, read them
    ///       Base obj = reader.Read();
    ///       if (obj != null)
    ///          obj.Parent = this;
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    public IFRSerializable Read()
    {
      XmlItem saveCurItem = FCurItem;
      XmlItem saveCurRoot = FCurRoot;
      List<PropInfo> saveProps = FProps;
      IFRSerializable result = null;
      
      try
      {
        if (FCurItem == null)
          FCurItem = FRoot;
        FCurRoot = FCurItem;
        GetProps();

        if (FReport != null && FReport.IsAncestor)
          result = FReport.FindObject(ReadStr("Name"));
        if (result == null && FCurItem.Name != "inherited")
        {
          Type type = RegisteredObjects.FindType(FCurItem.Name);
          if (type != null)
          {
            result = Activator.CreateInstance(type) as IFRSerializable;
            if (result is Report)
              FReport = result as Report;
          }  
          else
          {
            throw new ClassException(FCurItem.Name);
          }  
        }  
        if (result != null)
          result.Deserialize(this);
      }
      finally
      {
        FCurItem = saveCurItem;
        FCurRoot = saveCurRoot;
        FProps = saveProps;
      }
      
      return result;
    }

    /// <summary>
    /// Reads properties of specified object.
    /// </summary>
    /// <param name="obj">The object to read.</param>
    /// <remarks>
    /// This method reads simple properties like "Text", "Border.Lines" etc. for specified object.
    /// To read nested properties like collections, you should override the <see cref="Base.DeserializeSubItems"/>
    /// method of an object.
    /// </remarks>
    /// <example>This example demonstrates the use of <b>ReadProperties</b>, <b>ReadChildren</b>,
    /// <b>NextItem</b>, <b>Read</b> methods.
    /// <code>
    /// public void Deserialize(FRReader reader)
    /// {
    ///   // read simple properties like "Text", complex properties like "Border.Lines"
    ///   reader.ReadProperties(this);
    /// 
    ///   // moves the current reader item
    ///   while (reader.NextItem())
    ///   {
    ///     // read the "Styles" collection
    ///     if (String.Compare(reader.ItemName, "Styles", true) == 0)
    ///       reader.Read(Styles);
    ///     else if (reader.ReadChildren)
    ///     {
    ///       // if read of children is enabled, read them
    ///       Base obj = reader.Read();
    ///       if (obj != null)
    ///          obj.Parent = this;
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    public void ReadProperties(object obj)
    {
      // speed optimization, for use in the preview mode
      if (obj is TextObject && FProps.Count == 1 && FProps[0].Name == "x")
      {
        (obj as TextObject).Text = Converter.FromXml(FProps[0].Value);
        return;
      }
      
      for (int i = 0; i < FProps.Count; i++)
      {
        string name = FProps[i].Name;
        string value = FProps[i].Value;

        // check multiple properties like Frame.LeftLine.Typ
        object obj1 = obj;
        int len = name.Length;
        int start = 0;
        int j = 0;
        // find '.'
        while (j < len && name[j] != '.')
          j++;
        if (j < len)
        {
          while (j < len)
          {
            // get subproperty
            PropertyInfo pi = obj1.GetType().GetProperty(name.Substring(start, j - start));
            if (pi == null) break;
            obj1 = pi.GetValue(obj1, null);  
            
            // find next '.'
            start = j + 1;
            j++;
            while (j < len && name[j] != '.')
              j++;
          }
          name = name.Substring(start);
        }

        try
        {
          name = PropName(name);
          PropertyInfo pi = obj1.GetType().GetProperty(name);
          if (pi == null) 
            continue;

          if (value == "null")
            pi.SetValue(obj1, value, null);
          else
          {  
            if (pi.PropertyType == typeof(string))
            {
              pi.SetValue(obj1, Converter.FromXml(value), null);
            }
            else if (pi.PropertyType.IsClass && pi.PropertyType.IsSubclassOf(typeof(Base)))
            {
              // it's a reference
              FFixups.Add(new FixupInfo(obj1 as Base, name, Converter.FromXml(value)));
            }
            else
            {
              pi.SetValue(obj1, Converter.FromString(pi.PropertyType, Converter.FromXml(value)), null);
            }
          }
        }  
        catch (Exception e)
        {
          FErrors += e.Message + "\r\n";
        }
      }
    }

    /// <summary>
    /// Moves the current xml item.
    /// </summary>
    /// <returns><b>false</b> if there is no more items to move on; <b>true</b> otherwise.</returns>
    /// <remarks>
    /// This method is used to read child objects.
    /// </remarks>
    /// <example>This example demonstrates the use of <b>ReadProperties</b>, <b>ReadChildren</b>,
    /// <b>NextItem</b>, <b>Read</b> methods.
    /// <code>
    /// public void Deserialize(FRReader reader)
    /// {
    ///   // read simple properties like "Text", complex properties like "Border.Lines"
    ///   reader.ReadProperties(this);
    /// 
    ///   // moves the current reader item
    ///   while (reader.NextItem())
    ///   {
    ///     // read the "Styles" collection
    ///     if (String.Compare(reader.ItemName, "Styles", true) == 0)
    ///       reader.Read(Styles);
    ///     else if (reader.ReadChildren)
    ///     {
    ///       // if read of children is enabled, read them
    ///       Base obj = reader.Read();
    ///       if (obj != null)
    ///          obj.Parent = this;
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    public bool NextItem()
    {
      if (FCurItem == FCurRoot)
      {
        if (FCurRoot.Count > 0)
        {
          FCurItem = FCurRoot[0];
          return true;
        }
        else
          return false;
      }  
      else
      {
        int i = FCurRoot.IndexOf(FCurItem);
        if (i < FCurRoot.Count - 1)
        {
          FCurItem = FCurRoot[i + 1];
          return true;
        }  
        else
          return false;
      }  
    }
    
    /// <summary>
    /// Checks if current item has specified property.
    /// </summary>
    /// <param name="name">The property name to check.</param>
    /// <returns><b>true</b> if current item has specified property.</returns>
    public bool HasProperty(string name)
    {
      return IndexOf(name) != -1;
    }

    /// <summary>
    /// Reads the string property.
    /// </summary>
    /// <param name="name">Name of property.</param>
    /// <returns>Property value.</returns>
    public string ReadStr(string name)
    {
      return Converter.FromXml(PropValue(name));
    }

    /// <summary>
    /// Reads the boolean property.
    /// </summary>
    /// <param name="name">Name of property.</param>
    /// <returns>Property value.</returns>
    public bool ReadBool(string name)
    {
      return PropValue(name) == "1" ? true : false;
    }

    /// <summary>
    /// Reads the integer property.
    /// </summary>
    /// <param name="name">Name of property.</param>
    /// <returns>Property value.</returns>
    public int ReadInt(string name)
    {
      return int.Parse(PropValue(name));
    }

    /// <summary>
    /// Reads the float property.
    /// </summary>
    /// <param name="name">Name of property.</param>
    /// <returns>Property value.</returns>
    public float ReadFloat(string name)
    {
      return (float)Converter.FromString(typeof(float), PropValue(name));
    }

    /// <summary>
    /// Reads the enum property.
    /// </summary>
    /// <param name="name">Name of property.</param>
    /// <param name="typ">Type of property.</param>
    /// <returns>Property value.</returns>
    public object ReadValue(string name, Type typ)
    {
      string propValue = PropValue(name);
      if (propValue == "null")
        return null;
      return Converter.FromString(typ, Converter.FromXml(propValue));
    }

    /// <summary>
    /// Reads the standalone property value.
    /// </summary>
    /// <returns>Property value.</returns>
    public string ReadPropertyValue()
    {
      return Converter.FromXml(FCurItem.Value);
    }

    /// <summary>
    /// Disposes the reader, fixups the property references.
    /// </summary>
    public void Dispose()
    {
      FixupReferences();
      FDoc.Dispose();
    }

    /// <summary>
    /// Loads the xml items from a stream.
    /// </summary>
    /// <param name="stream">The stream to load from.</param>
    public void Load(Stream stream)
    {
      FDoc.Load(stream);
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <b>FRReader</b> class with specified report.
    /// </summary>
    /// <param name="report">Reference to a report.</param>
    public FRReader(Report report)
    {
      FDoc = new XmlDocument();
      FRoot = FDoc.Root;
      FFixups = new List<FixupInfo>();
      FErrors = "";
      FReport = report;
      FReadChildren = true;
    }

    /// <summary>
    /// Initializes a new instance of the <b>FRReader</b> class with specified report and xml item with
    /// contents to read.
    /// </summary>
    /// <param name="report">Reference to a report.</param>
    /// <param name="root">Xml item with contents to read.</param>
    public FRReader(Report report, XmlItem root) : this(report)
    {
      FRoot = root;
    }


    private class FixupInfo
    {
      public Base Obj;
      public string Name;
      public string Value;

      public FixupInfo(Base obj, string name, string value)
      {
        Obj = obj;
        Name = name;
        Value = value;
      }
    }

    private class PropInfo
    {
      public string Name;
      public string Value;

      public PropInfo(string name, string value)
      {
        Name = name;
        Value = value;
      }
    }
  }
}    


