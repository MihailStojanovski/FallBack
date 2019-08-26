using System;
using System.Collections.Generic;
using System.Text;
using FastReport.Utils;
using System.Collections;

namespace FastReport.Matrix
{
  /// <summary>
  /// Contains a set of properties and methods to hold and manipulate the matrix descriptors.
  /// </summary>
  /// <remarks>
  /// This class contains three collections of descriptors such as <see cref="Columns"/>,
  /// <see cref="Rows"/> and <see cref="Cells"/>. Use collections' methods to add/remove
  /// descriptors. When you are done, call the <see cref="MatrixObject.BuildTemplate"/>
  /// method to refresh the matrix.
  /// <para/>To fill a matrix in code, use the <b>AddValue</b> method.
  /// </remarks>
  public class MatrixData
  {
    #region Fields
    private MatrixHeader FColumns;
    private MatrixHeader FRows;
    private MatrixCells FCells;
    #endregion
    
    #region Properties
    /// <summary>
    /// Gets a collection of column descriptors.
    /// </summary>
    /// <remarks>
    /// Note: after you change something in this collection, call the 
    /// <see cref="MatrixObject.BuildTemplate"/> method to refresh the matrix.
    /// </remarks>
    public MatrixHeader Columns
    {
      get { return FColumns; }
    }

    /// <summary>
    /// Gets a collection of row descriptors.
    /// </summary>
    /// <remarks>
    /// Note: after you change something in this collection, call the 
    /// <see cref="MatrixObject.BuildTemplate"/> method to refresh the matrix.
    /// </remarks>
    public MatrixHeader Rows
    {
      get { return FRows; }
    }

    /// <summary>
    /// Gets a collection of data cell descriptors.
    /// </summary>
    /// <remarks>
    /// Note: after you change something in this collection, call the 
    /// <see cref="MatrixObject.BuildTemplate"/> method to refresh the matrix.
    /// </remarks>
    public MatrixCells Cells
    {
      get { return FCells; }
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// Clears all descriptors.
    /// </summary>
    public void Clear()
    {
      Columns.Reset();
      Rows.Reset();
      Cells.Reset();
    }
    
    /// <summary>
    /// Adds a value in the matrix.
    /// </summary>
    /// <param name="columnValues">Array of column values.</param>
    /// <param name="rowValues">Array of row values.</param>
    /// <param name="cellValues">Array of data values.</param>
    /// <remarks>
    /// The number of elements in an array passed to this method must be the same as
    /// a number of descriptors in the appropriate collection. That is, if your matrix
    /// has one column, two row and one cell descriptors (in <b>Columns</b>, <b>Rows</b> and
    /// <b>Cells</b> collections respectively), you have to pass one-element array for the
    /// <b>columnValues</b> param, two-element array for the <b>rowValues</b> and one-element
    /// array for the <b>cellValues</b>.
    /// </remarks>
    /// <example>This example demonstrates how to fill a simple matrix that contains one column,
    /// row and cell.
    /// <code>
    /// MatrixObject matrix;
    /// matrix.Data.AddValue(
    ///   new object[] { 1996 },
    ///   new object[] { "Andrew Fuller" }, 
    ///   new object[] { 123.45f });
    /// 
    /// // this will produce the following result:
    /// //               |   1996   |
    /// // --------------+----------+
    /// // Andrew Fuller |    123.45|
    /// // --------------+----------+
    /// </code>
    /// </example>
    public void AddValue(object[] columnValues, object[] rowValues, object[] cellValues)
    {
      AddValue(columnValues, rowValues, cellValues, 0);
    }

    /// <summary>
    /// Adds a value in the matrix.
    /// </summary>
    /// <param name="columnValues">Array of column values.</param>
    /// <param name="rowValues">Array of row values.</param>
    /// <param name="cellValues">Array of data values.</param>
    /// <param name="dataRowNo">Datasource row index.</param>
    /// <remarks>
    /// See the <see cref="AddValue(object[],object[],object[])"/> method for more details.
    /// </remarks>
    public void AddValue(object[] columnValues, object[] rowValues, object[] cellValues, int dataRowNo)
    {
      MatrixHeaderItem column = Columns.Find(columnValues, true, dataRowNo);
      MatrixHeaderItem row = Rows.Find(rowValues, true, dataRowNo);
      Cells.AddValue(column.Index, row.Index, cellValues);
    }

    internal ArrayList GetValues(int columnIndex, int rowIndex, int cellIndex)
    {
      return Cells.GetValues(columnIndex, rowIndex, cellIndex);
    }
    #endregion

    internal MatrixData()
    {
      FColumns = new MatrixHeader();
      FColumns.Name = "MatrixColumns";
      FRows = new MatrixHeader();
      FRows.Name = "MatrixRows";
      FCells = new MatrixCells();
      FCells.Name = "MatrixCells";
    }
  }
}
