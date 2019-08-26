using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using FastReport.Utils;

namespace FastReport.Data
{
  /// <summary>
  /// Represents the collection of <see cref="Parameter"/> objects.
  /// </summary>
  public class ParameterCollection : FRCollectionBase
  {
    /// <summary>
    /// Gets or sets a parameter.
    /// </summary>
    /// <param name="index">The index of a parameter in this collection.</param>
    /// <returns>The parameter with specified index.</returns>
    public Parameter this[int index]
    {
      get { return List[index] as Parameter; }
      set { List[index] = value; }
    }

    /// <summary>
    /// Finds a parameter by its name.
    /// </summary>
    /// <param name="name">The name of a parameter.</param>
    /// <returns>The <see cref="Parameter"/> object if found; otherwise <b>null</b>.</returns>
    public Parameter FindByName(string name)
    {
      foreach (Parameter c in this)
      {
        if (c.Name == name)
          return c;
      }
      return null;
    }

    /// <summary>
    /// Returns an unique parameter name based on given name.
    /// </summary>
    /// <param name="name">The base name.</param>
    /// <returns>The unique name.</returns>
    public string CreateUniqueName(string name)
    {
      string baseName = name;
      int i = 1;
      while (FindByName(name) != null)
      {
        name = baseName + i.ToString();
        i++;
      }
      return name;
    }
    
    /// <summary>
    /// Copies the parameters from other collection.
    /// </summary>
    /// <param name="source">Parameters to copy from.</param>
    public void Assign(ParameterCollection source)
    {
      Clear();
      foreach (Parameter par in source)
      {
        Parameter thisParam = new Parameter(par.Name);
        Add(thisParam);

        thisParam.DataType = par.DataType;
        thisParam.Value = par.Value;
        thisParam.Expression = par.Expression;
        
        thisParam.Parameters.Assign(par.Parameters);
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterCollection"/> class with default settings.
    /// </summary>
    /// <param name="owner">The owner of this collection.</param>
    public ParameterCollection(Base owner) : base(owner)
    {
    }
  }
}
