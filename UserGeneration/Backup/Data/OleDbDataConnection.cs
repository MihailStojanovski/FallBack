using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using FastReport.Utils;
using FastReport.Forms;
using FastReport.Data.ConnectionEditors;

namespace FastReport.Data
{
  /// <summary>
  /// Represents a connection to any OLE DB database.
  /// </summary>
  /// <example>This example shows how to add a new connection to the report.
  /// <code>
  /// Report report1;
  /// OleDbDataConnection conn = new OleDbDataConnection();
  /// conn.ConnectionString = "your_connection_string";
  /// report1.Dictionary.Connections.Add(conn);
  /// conn.CreateAllTables();
  /// </code>
  /// </example>
  public class OleDbDataConnection : DataConnectionBase
  {
    /// <inheritdoc/>
    protected override string GetConnectionStringWithLoginInfo(string userName, string password)
    {
      OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(ConnectionString);

      builder.Remove("User ID");
      builder.Add("User ID", userName);

      builder.Remove("Password");
      builder.Add("Password", password);

      return builder.ToString();
    }

    /// <inheritdoc/>
    public override string QuoteIdentifier(string value, DbConnection connection)
    {
      // OleDb is universal connection, so we need quoting dependent on used database type
      using (OleDbCommandBuilder builder = new OleDbCommandBuilder())
      {
        return builder.QuoteIdentifier(value, connection as OleDbConnection);
      }
    }

    /// <inheritdoc/>
    public override DbConnection GetConnection()
    {
      return new OleDbConnection(ConnectionString);
    }

    /// <inheritdoc/>
    public override DbDataAdapter GetAdapter(string selectCommand, DbConnection connection,
      CommandParameterCollection parameters)
    {
      OleDbDataAdapter adapter = new OleDbDataAdapter(selectCommand, connection as OleDbConnection);
      foreach (CommandParameter p in parameters)
      {
        OleDbParameter parameter = adapter.SelectCommand.Parameters.Add(p.Name, (OleDbType)p.DataType, p.Size);
        parameter.Value = p.Value;
      }
      return adapter;
    }

    /// <inheritdoc/>
    public override ConnectionEditorBase GetEditor()
    {
      return new OleDbConnectionEditor();
    }

    /// <inheritdoc/>
    public override Type GetParameterType()
    {
      return typeof(OleDbType);
    }

    /// <inheritdoc/>
    public override int GetDefaultParameterType()
    {
      return (int)OleDbType.VarChar;
    }

    /// <inheritdoc/>
    public override string GetConnectionId()
    {
      return "OleDb: " + ConnectionString;
    }
  }
}
