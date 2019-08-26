using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using FastReport.Utils;
using System.Windows.Forms;
using FastReport.Engine;

namespace FastReport.Preview
{
  internal class PreparedPage : IDisposable
  {
    private XmlItem FXmlItem;
    private PreparedPages FPreparedPages;
    private SizeF FPageSize;
    private long FTempFilePosition;
    private bool FUploaded;

    public XmlItem Xml
    {
      get { return FXmlItem; }
      set 
      { 
        FXmlItem = value; 
        value.Parent = null;
      }
    }

    public SizeF PageSize
    {
      get
      {
        if (FPageSize.IsEmpty)
        {
          ReportPage page = ReadObject(null, FXmlItem, false) as ReportPage;
          FPageSize = new SizeF(page.PaperWidth * Units.Millimeters, page.PaperHeight * Units.Millimeters);
        }
        return FPageSize;  
      }
    }
    
    public int CurPosition
    {
      get { return FXmlItem.Count; }
    }
    
    private bool UseFileCache
    {
      get { return FPreparedPages.Report.UseFileCache; }
    }

    private bool DoAdd(Base c, XmlItem item)
    {
      if (c == null)
        return false;
      ReportEngine engine = FPreparedPages.Report.Engine;
      if (c is ReportComponentBase)
      {
        if (engine != null && !engine.CanPrint(c as ReportComponentBase))
          return false;
      }

      item = item.Add();
      using (FRWriter writer = new FRWriter(item))
      {
        writer.SerializeTo = SerializeTo.Preview;
        writer.SaveChildren = false;
        writer.BlobStore = FPreparedPages.BlobStore;
        writer.Write(c);
      }
      if (engine != null)
        engine.AddObjectToProcess(c, item);
      
      if ((c.Flags & Flags.CanWriteChildren) == 0)
      {
        ObjectCollection childObjects = c.ChildObjects;
        foreach (Base obj in childObjects)
        {
          DoAdd(obj, item);
        }
      }
      
      return true;
    }

    private Base ReadObject(Base parent, XmlItem item, bool readChildren)
    {
      string objName = item.Name;
      
      // try to find the object in the dictionary
      Base obj = FPreparedPages.Dictionary.GetObject(objName);
      
      // object not found, objName is type name
      if (obj == null)
      {
        Type type = RegisteredObjects.FindType(objName);
        if (type == null)
          return null;
        obj = Activator.CreateInstance(type) as Base;
      }
      obj.SetRunning(true);
      
      // read object's properties
      if (!String.IsNullOrEmpty(item.Text))
      {
        using (FRReader reader = new FRReader(null, item))
        {
          reader.ReadChildren = false;
          reader.BlobStore = FPreparedPages.BlobStore;
          // since the BlobStore is shared resource, lock it to avoid problems with multi-thread access.
          // this may happen in the html export that uses several threads to export one report.
          lock (reader.BlobStore)
          {
            reader.Read(obj);
          }  
        }
      }
    
      if (readChildren)
      {
        for (int i = 0; i < item.Count; i++)
        {
          ReadObject(obj, item[i], true);
        }
      }

      obj.Parent = parent;
      return obj;
    }

    private void PostProcessPageObjects(Base obj)
    {
      obj.ExtractMacros();
      if (obj is BandBase)
        (obj as BandBase).UpdateWidth();
      
      ObjectCollection childObjects = obj.ChildObjects;
      foreach (Base c in childObjects)
      {
        PostProcessPageObjects(c);
      }
    }

    public bool AddBand(BandBase band)
    {
      return DoAdd(band, FXmlItem);
    }

    public ReportPage GetPage(int pageIndex)
    {
      if (UseFileCache && FUploaded)
      {
        FPreparedPages.TempFile.Position = FTempFilePosition;
        XmlReader reader = new XmlReader(FPreparedPages.TempFile);
        reader.Read(FXmlItem);
      }
      
      ReportPage page = ReadObject(null, FXmlItem, true) as ReportPage;
      page.SetReport(FPreparedPages.Report);
      PostProcessPageObjects(page);

      if (UseFileCache && FUploaded)
        FXmlItem.Clear();
      return page;
    }

    public void Upload()
    {
      if (UseFileCache && !FUploaded)
      {
        FPreparedPages.TempFile.Seek(0, SeekOrigin.End);
        FTempFilePosition = FPreparedPages.TempFile.Position;

        XmlWriter writer = new XmlWriter(FPreparedPages.TempFile);
        writer.Write(FXmlItem);

        FXmlItem.Clear();
        FUploaded = true;
      }
    }

    public XmlItem CutObjects(int index)
    {
      XmlItem result = new XmlItem();
      while (FXmlItem.Count > index)
      {
        result.AddItem(FXmlItem[index]);
      }
      return result;
    }
    
    public void PasteObjects(XmlItem objects, float x, float y)
    {
      if (objects.Count > 0)
      {
        // get the top object's location
        float pastedX = (objects[0].GetProp("l") != "") ? 
          Converter.StringToFloat(objects[0].GetProp("l")) : 0;
        float pastedY = (objects[0].GetProp("t") != "") ? 
          Converter.StringToFloat(objects[0].GetProp("t")) : 0;

        float deltaX = x - pastedX;
        float deltaY = y - pastedY;

        while (objects.Count > 0)
        {
          XmlItem obj = objects[0];

          // shift the object's location
          float objX = (obj.GetProp("l") != "") ? 
            Converter.StringToFloat(obj.GetProp("l")) : 0;
          float objY = (obj.GetProp("t") != "") ?
            Converter.StringToFloat(obj.GetProp("t")) : 0;
          obj.SetProp("l", Converter.ToString(objX + deltaX));
          obj.SetProp("t", Converter.ToString(objY + deltaY));
          
          // add object to a page
          FXmlItem.AddItem(obj);
        }
      }
    }

    public float GetLastY()
    {
      float result = 0;

      for (int i = 0; i < FXmlItem.Count; i++)
      {
        XmlItem xi = FXmlItem[i];

        BandBase obj = FPreparedPages.Dictionary.GetOriginalObject(xi.Name) as BandBase;
        if (obj != null && !(obj is PageFooterBand) && !(obj is OverlayBand))
        {
          string s = xi.GetProp("t");
          float top = (s != "") ? Converter.StringToFloat(s) : obj.Top;
          s = xi.GetProp("h");
          float height = (s != "") ? Converter.StringToFloat(s) : obj.Height;

          if (top + height > result)
            result = top + height;
        }    
      }

      return result;
    }

    public bool ContainsBand(Type bandType)
    {
      for (int i = 0; i < FXmlItem.Count; i++)
      {
        XmlItem xi = FXmlItem[i];

        BandBase obj = FPreparedPages.Dictionary.GetOriginalObject(xi.Name) as BandBase;
        if (obj != null && obj.GetType() == bandType)
          return true;
      }

      return false;
    }

    public bool ContainsBand(string bandName)
    {
      for (int i = 0; i < FXmlItem.Count; i++)
      {
        XmlItem xi = FXmlItem[i];

        BandBase obj = FPreparedPages.Dictionary.GetOriginalObject(xi.Name) as BandBase;
        if (obj != null && obj.Name == bandName)
          return true;
      }

      return false;
    }

    public void Dispose()
    {
      FXmlItem.Dispose();
    }
    
    public PreparedPage(ReportPage page, PreparedPages preparedPages)
    {
      FPreparedPages = preparedPages;
      FXmlItem = new XmlItem();

      // page == null when we load prepared report from a file
      if (page != null)
      {
        using (FRWriter writer = new FRWriter(FXmlItem))
        {
          writer.SerializeTo = SerializeTo.Preview;
          writer.SaveChildren = false;
          writer.Write(page);
        }

        FPageSize = new SizeF(page.PaperWidth * Units.Millimeters, page.PaperHeight * Units.Millimeters);
      }
    }
  }
}
