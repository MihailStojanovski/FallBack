using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FastReport.Utils;
using FastReport.Data;
using System.Collections;
using FastReport.Design;

namespace FastReport.Forms
{
  internal partial class DataWizardForm : BaseWizardForm
  {
    private Report FReport;
    private LastConnections FConnections;
    private DataConnectionBase FConnection;
    private bool FUpdating;
    private bool FEditMode;
    
    public DataConnectionBase Connection
    {
      get { return FConnection; }
      set
      {
        if (value != FConnection)
        {
          if (FConnection != null)
          {
            FReport.Dictionary.Connections.Remove(FConnection);
            FConnection.Clear();
          }  
          FConnection = value;
          if (value != null)
          {
            FConnections.Add(FConnection);
            FReport.Dictionary.Connections.Add(FConnection);
            UpdateConnectionsCombo();
          }
        }
        if (value != null)
          tbConnString.Text = value.ConnectionString;
        btnEditConnection.Enabled = value != null;  
      }
    }
    
    public bool EditMode
    {
      get { return FEditMode; }
      set
      {
        FEditMode = value;
        if (value)
        {
          cbxConnections.Enabled = false;
          btnNewConnection.Enabled = false;
          tbConnName.Text = Connection.Name;
        }
      }
    }

    public override int VisiblePanelIndex
    {
      get { return base.VisiblePanelIndex; }
      set
      {
        base.VisiblePanelIndex = value;
        if (value == 1)
          UpdateTree();
      }
    }

    private bool ApplicationConnectionMode
    {
      get { return Config.DesignerSettings.ApplicationConnection != null; }
    }

    private void cbxConnections_DrawItem(object sender, DrawItemEventArgs e)
    {
      e.DrawBackground();
      if (e.Index >= 0)
      {
        try
        {
          DataConnectionBase connection = cbxConnections.Items[e.Index] as DataConnectionBase;
          TextRenderer.DrawText(e.Graphics, connection.GetConnectionId(), e.Font, e.Bounds.Location, e.ForeColor);
        }
        catch
        {
        }
      }
    }
    
    private void btnConnString_Click(object sender, EventArgs e)
    {
      if (!tbConnString.Visible)
      {
        btnConnString.Image = Res.GetImage(184);
        tbConnString.Visible = true;
      }
      else
      {
        btnConnString.Image = Res.GetImage(183);
        tbConnString.Visible = false;
      }
    }

    private void UpdateConnectionsCombo()
    {
      FUpdating = true;
      cbxConnections.Items.Clear();
      cbxConnections.Items.AddRange(FConnections.ToArray());
      if (cbxConnections.Items.Count > 0)
        cbxConnections.SelectedIndex = 0;
      FUpdating = false;  
    }

    private void cbxConnections_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (FUpdating)
        return;
      FUpdating = true;
      Connection = cbxConnections.SelectedItem as DataConnectionBase;
      FUpdating = false;  
    }

    private void btnNewConnection_Click(object sender, EventArgs e)
    {
      using (ConnectionForm form = new ConnectionForm())
      {
        if (form.ShowDialog() == DialogResult.OK)
          Connection = form.Connection;
      }
    }

    private void btnEditConnection_Click(object sender, EventArgs e)
    {
      using (ConnectionForm form = new ConnectionForm())
      {
        form.Connection = Connection;
        form.EditMode = true;
        if (form.ShowDialog() == DialogResult.OK)
          Connection = form.Connection;
      }
    }

    private void btnSort_Click(object sender, EventArgs e)
    {
      tvTables.Sort();
    }

    private void btnAddTable_Click(object sender, EventArgs e)
    {
      TableDataSource table = new TableDataSource();
      table.Name = FReport.Dictionary.CreateUniqueName("Table");
      table.Alias = FReport.Dictionary.CreateUniqueAlias(table.Alias);
      table.Connection = Connection;
      using (QueryWizardForm form = new QueryWizardForm(table))
      {
        if (form.ShowDialog() != DialogResult.OK)
          table.Dispose();
        else
          UpdateTree();
      }
    }

    private void UpdateTree()
    {
      tvTables.BeginUpdate();
      tvTables.Nodes.Clear();

      lblWait.Location = new Point(tvTables.Left + 1, tvTables.Top + tvTables.Height / 2 - 32);
      lblWait.Size = new Size(tvTables.Width - 2, 32);
      lblWait.Visible = true;
      lblWait.Refresh();
      try
      {
        FConnection.CreateAllTables();
      }
      catch
      {
      }
      lblWait.Visible = false;
      btnAddQuery.Enabled = Connection.IsSqlBased;

      DataHelper.CreateDataTree(FReport.Dictionary, Connection, tvTables.Nodes);
      tvTables.EndUpdate();
    }

    private void DataWizardForm_Shown(object sender, EventArgs e)
    {
      // needed for 120dpi mode
      tbConnString.Height = pnDatabase.Height - tbConnString.Top - 12;
    }

    private void DataWizardForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      Done();
      Config.SaveFormState(this);
    }

    private void ReadConnections()
    {
      if (Config.DesignerSettings.CustomConnections.Count > 0)
      {
        FConnections.Deserialize(Config.DesignerSettings.CustomConnections);
      }
      else
      {
        XmlItem root = Config.Root.FindItem("Designer").FindItem("LastConnections");
        using (FRReader reader = new FRReader(null, root))
        {
          reader.Read(FConnections);
        }
      }  

      if (FConnections.Count > 0)
        Connection = FConnections[0];
    }
    
    private void Init()
    {
      picIcon.Image = ResourceLoader.GetBitmap("datawizard.png");
      btnConnString.Image = Res.GetImage(183);
      tvTables.ImageList = Res.GetImages();

      if (ApplicationConnectionMode)
      {
        if (FReport.Dictionary.Connections.Count == 0)
        {
          Connection = Activator.CreateInstance(Config.DesignerSettings.ApplicationConnectionType) as DataConnectionBase;
          Connection.Name = FReport.Dictionary.CreateUniqueName("Connection");
        }
        else
          Connection = FReport.Dictionary.Connections[0];

        Connection.ConnectionString = Config.DesignerSettings.ApplicationConnection.ConnectionString;
        EditMode = true;
        VisiblePanelIndex = 1;
        btnPrevious.Visible = false;
        btnNext.Visible = false;
      }
      else
      {
        VisiblePanelIndex = 0;
        tbConnName.Text = FReport.Dictionary.CreateUniqueName("Connection");
        ReadConnections();
      }
    }
    
    private void Done()
    {
      if (DialogResult == DialogResult.OK)
      {
        Connection.Name = tbConnName.Text;
        
        // enable items that we have checked in the table tree
        foreach (TreeNode tableNode in tvTables.Nodes)
        {
          TableDataSource table = tableNode.Tag as TableDataSource;
          if (table != null)
          {
            table.Enabled = tableNode.Checked;

            foreach (TreeNode colNode in tableNode.Nodes)
            {
              Column column = colNode.Tag as Column;
              if (column != null)
                column.Enabled = colNode.Checked;
            }
          }
        }
        
        XmlItem root = Config.Root.FindItem("Designer").FindItem("LastConnections");
        using (FRWriter writer = new FRWriter(root))
        {
          root.Clear();
          writer.Write(FConnections);
        }
      }

      if (ApplicationConnectionMode)
      {
        // in this mode, we don't have to store connection string in the report
        Connection.ConnectionString = "";
      }
      if (DialogResult == DialogResult.OK || EditMode)
        FConnections.Remove(Connection);
      FConnections.Dispose();
    }

    public override void Localize()
    {
      base.Localize();
      MyRes res = new MyRes("Forms,DataWizard");
      Text = res.Get("");
      pnDatabase.Text = res.Get("Page1");
      pnTables.Text = res.Get("Page2");
      lblWhichData.Text = res.Get("WhichData");
      lblHint.Text = res.Get("Hint");
      btnNewConnection.Text = res.Get("NewConnection");
      btnEditConnection.Text = res.Get("EditConnection");
      lblConnName.Text = res.Get("ConnectionName");
      lblConnString.Text = res.Get("ConnectionString");
      lblWhichTables.Text = res.Get("WhichTables");
      lblWait.Text = res.Get("Wait");
      btnSort.Text = res.Get("SortTables");
      btnAddQuery.Text = res.Get("AddQuery");
    }
    
    public DataWizardForm(Report report)
    {
      FReport = report;
      FConnections = new LastConnections();
      InitializeComponent();
      Localize();
      Init();
      Config.RestoreFormState(this);
    }
  
  
    private class LastConnections : IDisposable, IFRSerializable
    {
      private List<DataConnectionBase> FConnections;

      public DataConnectionBase this[int index]
      {
        get { return FConnections[index]; }
      }
      
      public int Count
      {
        get { return FConnections.Count; }
      }
      
      public void Serialize(FRWriter writer)
      {
        List<string> savedConnections = new List<string>();
        writer.ItemName = "LastConnections";
        foreach (DataConnectionBase connection in FConnections)
        {
          if (savedConnections.IndexOf(connection.GetConnectionId()) == -1)
          {
            writer.SaveChildren = false;
            writer.Write(connection);
            savedConnections.Add(connection.GetConnectionId());  
          }  
        }
      }

      public void Deserialize(FRReader reader)
      {
        while (reader.NextItem())
        {
          try
          {
            DataConnectionBase connection = reader.Read() as DataConnectionBase;
            if (connection != null)
              FConnections.Add(connection);
          }
          catch
          {
          }    
        }
      }
      
      public void Deserialize(List<ConnectionEntry> connections)
      {
        foreach (ConnectionEntry ce in connections)
        {
          try
          {
            DataConnectionBase connection = 
              Activator.CreateInstance(ce.Type) as DataConnectionBase;
            connection.ConnectionString = ce.ConnectionString;
            FConnections.Add(connection);
          }
          catch
          {
          }
        }
      }
      
      public void Add(DataConnectionBase connection)
      {
        for (int i = 0; i < FConnections.Count; i++)
        {
          try
          {
            if (FConnections[i].GetConnectionId() == connection.GetConnectionId())
            {
              FConnections.RemoveAt(i);
              break;
            }
          }
          catch
          {
          }
        }
        FConnections.Insert(0, connection);
      }
      
      public void Remove(DataConnectionBase connection)
      {
        if (FConnections.Contains(connection))
          FConnections.Remove(connection);
      }
      
      public DataConnectionBase[] ToArray()
      {
        return FConnections.ToArray();
      }

      public void Dispose()
      {
        foreach (DataConnectionBase connection in FConnections)
        {
          connection.Dispose();
        }
        FConnections.Clear();
      }

      public LastConnections()
      {
        FConnections = new List<DataConnectionBase>();
      }
    }
  }
}

