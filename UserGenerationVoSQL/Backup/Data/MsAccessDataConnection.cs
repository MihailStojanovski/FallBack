using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data.Common;
using System.Data.OleDb;
using FastReport.Forms;
using FastReport.Utils;
using FastReport.Data.ConnectionEditors;

namespace FastReport.Data
{
  /// <summary>
  /// Represents a connection to MS Access database (.mdb file).
  /// </summary>
  /// <example>This example shows how to add a new connection to the report.
  /// <code>
  /// Report report1;
  /// MsAccessDataConnection conn = new MsAccessDataConnection();
  /// conn.DataSource = @"c:\data.mdb";
  /// report1.Dictionary.Connections.Add(conn);
  /// conn.CreateAllTables();
  /// </code>
  /// </example>
  public class MsAccessDataConnection : DataConnectionBase
  {
    /// <summary>
    /// Gets or sets the datasource file name.
    /// </summary>
    [Category("Data")]
    public string DataSource
    {
      get
      {
        OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(ConnectionString);
        return builder.DataSource;
      }
      set
      {
        OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(ConnectionString);
        builder.DataSource = value;
        ConnectionString = builder.ToString();
      }
    }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    [Category("Data")]
    public string UserName
    {
      get
      {
        OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(ConnectionString);
        object userName;
        builder.TryGetValue("User ID", out userName);
        return userName == null ? "" : userName.ToString();
      }
      set
      {
        OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(ConnectionString);
        if (!String.IsNullOrEmpty(value))
          builder.Add("User ID", value);
        else
          builder.Remove("User ID");
        ConnectionString = builder.ToString();
      }
    }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Category("Data")]
    public string Password
    {
      get
      {
        OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(ConnectionString);
        object password;
        builder.TryGetValue("Password", out password);
        return password == null ? "" : password.ToString();
      }
      set
      {
        OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(ConnectionString);
        if (!String.IsNullOrEmpty(value))
          builder.Add("Password", value);
        else
          builder.Remove("Password");
        ConnectionString = builder.ToString();
      }
    }

    /// <inheritdoc/>
    protected override void SetConnectionString(string value)
    {
      OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(value);
      builder.Provider = "Microsoft.Jet.OLEDB.4.0";
      base.SetConnectionString(builder.ToString());
    }

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
      return new MsAccessConnectionEditor();
    }

    /// <inheritdoc/>
    public override Type GetParameterType()
    {
      return typeof(OleDbType);
    }

    /// <inheritdoc/>
    public override string QuoteIdentifier(string value, DbConnection connection)
    {
      return "[" + value + "]";
    }

    /// <inheritdoc/>
    public override string GetConnectionId()
    {
      OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(ConnectionString);
      return "MsAccess: " + builder.DataSource;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MsAccessDataConnection"/> class with default settings.
    /// </summary>
    public MsAccessDataConnection()
    {
      ConnectionString = "";
    }
  }
}
