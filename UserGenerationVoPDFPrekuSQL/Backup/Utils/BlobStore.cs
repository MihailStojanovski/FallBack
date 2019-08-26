using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FastReport.Utils
{
  internal class BlobStore : IDisposable
  {
    private List<BlobItem> FList;
    
    public int Count
    {
      get { return FList.Count; }
    }
    
    public int Add(Stream stream)
    {
      BlobItem item = new BlobItem(stream);
      FList.Add(item);
      return FList.Count - 1;
    }

    public Stream Get(int index)
    {
      Stream stream = FList[index].Stream;
      if (stream != null)
        stream.Position = 0;
      return stream;
    }

    public void Clear()
    {
      foreach (BlobItem b in FList)
      {
        b.Dispose();
      }
      FList.Clear();
    }
    
    public void Load(XmlItem rootItem)
    {
      Clear();
      for (int i = 0; i < rootItem.Count; i++)
      {
        Add(Converter.FromString(typeof(Stream), Converter.FromXml(rootItem[i].GetProp("Stream"))) as Stream);
      }
    }
    
    public void Save(XmlItem rootItem)
    {
      foreach (BlobItem item in FList)
      {
        XmlItem xi = rootItem.Add();
        xi.Name = "item";
        xi.SetProp("Stream", Converter.ToXml(item.Stream));
      }
    }
    
    public void Dispose()
    {
      Clear();
    }
    
    public BlobStore()
    {
      FList = new List<BlobItem>();
    }


    private class BlobItem : IDisposable
    {
      public Stream Stream;

      public void Dispose()
      {
        if (Stream != null)
          Stream.Dispose();
        Stream = null;
      }

      public BlobItem(Stream stream)
      {
        Stream = stream;
      }
    }

  }
}
