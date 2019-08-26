using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Utils;
using FastReport.Forms;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using ADODB;

namespace FastReport.Data.ConnectionEditors
{
  internal partial class OleDbConnectionEditor : ConnectionEditorBase
  {
    // imported from MS OLE DB Service Component 1.0 Type library
    [ComImport, TypeLibType((short)0x1040), Guid("2206CCB2-19C1-11D1-89E0-00C04FD7A829")]
    private interface IDataSourceLocator
    {
      [DispId(0x60020000)]
      int hWnd { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x60020000)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x60020000)] set; }
      [return: MarshalAs(UnmanagedType.IDispatch)]
      [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x60020002)]
      object PromptNew();
      [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x60020003)]
      bool PromptEdit([In, Out, MarshalAs(UnmanagedType.IDispatch)] ref object ppADOConnection);
    }

    [ComImport, TypeLibType((short)2), ClassInterface((short)0), Guid("2206CDB2-19C1-11D1-89E0-00C04FD7A829"), ComConversionLoss]
    private class DataLinksClass : IDataSourceLocator
    {
      [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x60020003)]
      public virtual extern bool PromptEdit([In, Out, MarshalAs(UnmanagedType.IDispatch)] ref object ppADOConnection);
      [return: MarshalAs(UnmanagedType.IDispatch)]
      [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x60020002)]
      public virtual extern object PromptNew();
      [DispId(0x60020000)]
      public virtual extern int hWnd { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x60020000)] get; [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x60020000)] set; }
    }

    private string GetConnectionString(string connectionString)
    {
      DataLinksClass dataLinks = new DataLinksClass();
      _Connection connection = null;

      if (String.IsNullOrEmpty(connectionString))
      {
        connection = dataLinks.PromptNew() as _Connection;
        if (connection == null)
          return "";
        return connection.ConnectionString;
      }

      connection = new ConnectionClass();
      connection.ConnectionString = connectionString;
      object objConnection = connection;
      if (dataLinks.PromptEdit(ref objConnection))
        return connection.ConnectionString;
      return connectionString;
    }

    private void Localize()
    {
      MyRes res = new MyRes("ConnectionEditors,OleDb");
      gbConnection.Text = res.Get("ConnectionString");
      btnBuild.Text = res.Get("Build");
    }

    private void btnBuild_Click(object sender, EventArgs e)
    {
      tbConnection.Text = GetConnectionString(tbConnection.Text);
    }

    protected override string GetConnectionString()
    {
      return tbConnection.Text;
    }

    protected override void SetConnectionString(string value)
    {
      tbConnection.Text = value;
    }

    public override void UpdateLayout()
    {
      tbConnection.Height = gbConnection.Height - tbConnection.Top - tbConnection.Left;
    }

    public OleDbConnectionEditor()
    {
      InitializeComponent();
      Localize();
    }
  }
}