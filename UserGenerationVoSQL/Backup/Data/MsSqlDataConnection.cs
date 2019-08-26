using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using FastReport.Forms;
using FastReport.Data.ConnectionEditors;

namespace FastReport.Data
{
  /// <summary>
  /// Represents a connection to MS SQL database.
  /// </summary>
  /// <example>This example shows how to add a new connection to the report.
  /// <code>
  /// Report report1;
  /// MsSqlDataConnection conn = new MsSqlDataConnection();
  /// conn.ConnectionString = "your_connection_string";
  /// report1.Dictionary.Connections.Add(conn);
  /// conn.CreateAllTables();
  /// </code>
  /// </example>
  public class MsSqlDataConnection : DataConnectionBase
  {
    private void GetDBObjectNames(string name, List<string> list)
    {
      DataTable schema = null;
      using (DbConnection connection = GetConnection())
      {
        connection.Open();
        schema = connection.GetSchema("Tables", new string[] { null, null, null, name });
      }

      foreach (DataRow row in schema.Rows)
      {
        string tableName = row["TABLE_NAME"].ToString();
        string schemaName = row["TABLE_SCHEMA"].ToString();
        if (String.Compare(schemaName, "dbo") == 0)
          list.Add(tableName);
        else
          list.Add(schemaName + ".\"" + tableName + "\"");
      }
    }

    /// <inheritdoc/>
    protected override string GetConnectionStringWithLoginInfo(string userName, string password)
    {
      SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
      
      builder.IntegratedSecurity = false;
      builder.UserID = userName;
      builder.Password = password;
      
      return builder.ToString();
    }

    /// <inheritdoc/>
    public override string QuoteIdentifier(string value, DbConnection connection)
    {
      // already quoted? keep in mind GetDBObjectNames and non-dbo schema!
      if (!value.EndsWith("\""))
        value = "\"" + value + "\"";
      return value;
    }

    /// <inheritdoc/>
    public override string[] GetTableNames()
    {
      List<string> list = new List<string>();
      GetDBObjectNames("BASE TABLE", list);
      GetDBObjectNames("VIEW", list);
      return list.ToArray();
    }

    /// <inheritdoc/>
    public override DbConnection GetConnection()
    {
      return new SqlConnection(ConnectionString);
    }

    /// <inheritdoc/>
    public override DbDataAdapter GetAdapter(string selectCommand, DbConnection connection,
      CommandParameterCollection parameters)
    {
      SqlDataAdapter adapter = new SqlDataAdapter(selectCommand, connection as SqlConnection);
      foreach (CommandParameter p in parameters)
      {
        SqlParameter parameter = adapter.SelectCommand.Parameters.Add(p.Name, (SqlDbType)p.DataType, p.Size);
        object value = p.Value;
        if ((SqlDbType)p.DataType == SqlDbType.UniqueIdentifier && (value is Variant || value is String))
          value = new Guid(value.ToString());
        parameter.Value = value;
      }
      return adapter;
    }

    /// <inheritdoc/>
    public override ConnectionEditorBase GetEditor()
    {
      return new MsSqlConnectionEditor();
    }

    /// <inheritdoc/>
    public override Type GetParameterType()
    {
      return typeof(SqlDbType);
    }

    /// <inheritdoc/>
    public override int GetDefaultParameterType()
    {
      return (int)SqlDbType.VarChar;
    }

    /// <inheritdoc/>
    public override string GetConnectionId()
    {
      SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
      string info = builder.InitialCatalog;
      if (String.IsNullOrEmpty(info))
        info = builder.AttachDBFilename;
      return "MS SQL: " + info;
    }
  }
}
