using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FastReport.Utils
{
  /// <summary>
  /// The helper class used to create unique component names using the fastest method.
  /// </summary>
  /// <remarks>
  /// <para>Note: you can create unique component's name using its <b>CreateUniqueName</b> method.
  /// However, it is very slow and can't be used in some situations (when you create a report 
  /// layout in a code and have a lot of objects on a page).</para>
  /// </remarks>
  /// <example>This example demonstrates how to use this class.
  /// <code>
  /// FastNameCreator nameCreator = new FastNameCreator(Report.AllObjects);
  /// foreach (Base c in Report.AllObjects)
  /// {
  ///   if (c.Name == "")
  ///     nameCreator.CreateUniqueName(c);
  /// }
  /// </code>
  /// </example>
  public class FastNameCreator
  {
    private Hashtable FBaseNames;

    /// <summary>
    /// Creates the unique name for the given object.
    /// </summary>
    /// <param name="obj">The object to create name for.</param>
    public void CreateUniqueName(Base obj)
    {
      string baseName = obj.BaseName;
      int num = 1;
      if (FBaseNames.ContainsKey(baseName))
        num = (int)FBaseNames[baseName] + 1;

      obj.SetName(baseName + num.ToString());
      FBaseNames[baseName] = num;
    }

    /// <summary>
    /// Initializes a new instance of the <b>FastNameCreator</b> class with collection of 
    /// existing report objects.
    /// </summary>
    /// <param name="objects">The collection of existing report objects.</param>
    public FastNameCreator(ObjectCollection objects)
    {
      FBaseNames = new Hashtable();

      foreach (Base obj in objects)
      {
        string objName = obj.Name;
        if (!String.IsNullOrEmpty(objName))
        {
          // find numeric part
          int i = objName.Length - 1;
          while (objName[i] >= '0' && objName[i] <= '9')
          {
            i--;
          }
          
          if (i >= 0 && i < objName.Length - 1)
          {
            // get number
            string baseName = objName.Substring(0, i + 1);
            int num = int.Parse(objName.Substring(i + 1));
            if (FBaseNames.ContainsKey(baseName))
            {
              int maxNum = (int)FBaseNames[baseName];
              if (num < maxNum)
                num = maxNum;
            }
            FBaseNames[baseName] = num;
          }
          else
          {
            FBaseNames[objName] = 0;
          }
        }

        //if (objName != null && objName.Length > baseName.Length && objName.StartsWith(baseName))
        //{
        //  string numStr = objName.Substring(baseName.Length);
        //  bool isNumber = true;
        //  foreach (char ch in numStr)
        //  {
        //    if (ch < '0' || ch > '9')
        //    {
        //      isNumber = false;
        //      break;
        //    }
        //  }
          
        //  if (isNumber)
        //  {
        //    int num = int.Parse(numStr);
        //    if (FBaseNames.ContainsKey(baseName))
        //    {
        //      int maxNum = (int)FBaseNames[baseName];
        //      if (num < maxNum)
        //        num = maxNum;
        //    }
        //    FBaseNames[baseName] = num;
        //  }
        //}
      }
    }
  }
}
