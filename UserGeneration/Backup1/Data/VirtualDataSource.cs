using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FastReport.Data
{
  internal class VirtualDataSource : DataSourceBase
  {
    private int FVirtualRowsCount;

    #region Protected Methods
    /// <inheritdoc/>
    protected override object GetValue(Column column)
    {
      return null;
    }
    #endregion

    #region Public Methods
    public override void InitSchema()
    {
     // do nothing
    }

    public override void LoadData(ArrayList rows)
    {
      rows.Clear();
      for (int i = 0; i < FVirtualRowsCount; i++)
      {
        rows.Add(0);
      }
    }
    #endregion

    public VirtualDataSource(int virtualRowsCount)
    {
      FVirtualRowsCount = virtualRowsCount;
    }
  }
}
