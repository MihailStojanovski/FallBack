using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Utils;
using FastReport.Data;
using FastReport.Data.ConnectionEditors;
using System.Data.Common;

namespace FastReport.Forms
{
  internal partial class ConnectionForm : BaseDialogForm
  {
    private DataConnectionBase FConnection;
    private ConnectionEditorBase FConnectionEditor;
    private bool FEditMode;
    
    public DataConnectionBase Connection
    {
      get { return FConnection; }
      set { FConnection = value; }
    }
    
    public bool EditMode
    {
      get { return FEditMode; }
      set
      {
        FEditMode = value;
        if (value)
        {
          cbxConnections.Items.Clear();
          cbxConnections.Items.Add(RegisteredObjects.FindObject(Connection));
          cbxConnections.SelectedIndex = 0;
          cbxConnections.Enabled = false;
          cbAlwaysUse.Enabled = false;
        }
      }
    }
    
    private void EnumConnections()
    {
      List<ObjectInfo> registeredObjects = new List<ObjectInfo>();
      RegisteredObjects.Objects.EnumItems(registeredObjects);
      string lastUsed = Config.Root.FindItem("Forms").FindItem(Name).GetProp("ConnectionType");

      foreach (ObjectInfo info in registeredObjects)
      {
        if (info.Object != null && info.Object.IsSubclassOf(typeof(DataConnectionBase)))
        {
          cbxConnections.Items.Add(info);
          if (info.Object.Name == lastUsed)
          {
            cbxConnections.SelectedIndex = cbxConnections.Items.Count - 1;
            cbAlwaysUse.Checked = true;
          }  
        }  
      }
    }

    private bool TestConnection(bool showOkMessage)
    {
      string saveConnectionString = FConnection.ConnectionString;
      FConnection.ConnectionString = FConnectionEditor.ConnectionString;
      bool successful = true;
      string errorMsg = "";
      try
      {
        FConnection.TestConnection();
      }
      catch (Exception e)
      {
        successful = false;
        errorMsg = e.Message;
      }
      FConnection.ConnectionString = saveConnectionString;

      if (successful && showOkMessage)
        FRMessageBox.Information(Res.Get("Forms,Connection,TestSuccesful"));
      else if (!successful)
        FRMessageBox.Error(errorMsg);
      return successful;  
    }

    private void cbxConnections_DrawItem(object sender, DrawItemEventArgs e)
    {
      e.DrawBackground();
      if (e.Index >= 0)
      {
        ObjectInfo info = cbxConnections.Items[e.Index] as ObjectInfo;
        TextRenderer.DrawText(e.Graphics, Res.TryGet(info.Text), e.Font, e.Bounds.Location, e.ForeColor);
      }
    }

    private void ConnectionForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      if (cbAlwaysUse.Checked)
      {
        Config.Root.FindItem("Forms").FindItem(Name).SetProp("ConnectionType", 
          (cbxConnections.SelectedItem as ObjectInfo).Object.Name);
      }    
    }

    private void ConnectionForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (DialogResult == DialogResult.OK && FConnection != null && FConnectionEditor != null)
      {
        FConnection.LoginPrompt = cbLoginPrompt.Checked;
        if (FConnection.LoginPrompt || TestConnection(false))
          FConnection.ConnectionString = FConnectionEditor.ConnectionString;
        else
          e.Cancel = true;
      }  
    }

    private void cbxConnections_SelectedIndexChanged(object sender, EventArgs e)
    {
      cbAlwaysUse.Checked = false;
      SuspendLayout();
      if (FConnectionEditor != null)
        FConnectionEditor.Dispose();
      if (!FEditMode)
      {
        if (FConnection != null)
          FConnection.Dispose();
        FConnection = null;  
      
        Type connectionType = (cbxConnections.SelectedItem as ObjectInfo).Object;
        FConnection = Activator.CreateInstance(connectionType) as DataConnectionBase;
      }
      
      FConnectionEditor = FConnection.GetEditor();
      if (FConnectionEditor != null)
      {
        FConnectionEditor.Parent = this;
        FConnectionEditor.Location = new Point(0, gbSelect.Bottom);
        PerformAutoScale();
        FConnectionEditor.UpdateLayout();
        ClientSize = new Size(ClientSize.Width, 
          FConnectionEditor.Bottom + cbLoginPrompt.Height + btnOk.Height + gbSelect.Top * 5);
      }
      else
        ClientSize = new Size(ClientSize.Width, 
        gbSelect.Bottom + cbLoginPrompt.Height + btnOk.Height + gbSelect.Top * 5);

      ResumeLayout();
      Refresh();
      if (FConnection != null && FConnectionEditor != null)
      {
        FConnectionEditor.ConnectionString = FConnection.ConnectionString;
        cbLoginPrompt.Checked = FConnection.LoginPrompt;
      }  
      btnTest.Enabled = FConnection != null && FConnection.IsSqlBased;
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
      TestConnection(true);
    }

    public override void Localize()
    {
      base.Localize();
      MyRes res = new MyRes("Forms,Connection");
      Text = res.Get("");
      gbSelect.Text = res.Get("Select");
      cbAlwaysUse.Text = res.Get("AlwaysUse");
      cbLoginPrompt.Text = res.Get("LoginPrompt");
      btnTest.Text = res.Get("Test");
    }
    
    public ConnectionForm()
    {
      InitializeComponent();
      Localize();
      EnumConnections();
    }
  }
}

