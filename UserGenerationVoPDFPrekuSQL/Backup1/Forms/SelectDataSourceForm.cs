using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using FastReport.Utils;
using System.Collections;
using FastReport.Data;

namespace FastReport.Forms
{
  internal partial class SelectDataSourceForm : BaseDialogForm
  {
    private Report FReport;

    public void Init(Report report, IReferenceService service)
    {
      FReport = report;
      tvAvailableDs.ImageList = Res.GetImages();
      object[] objects = service.GetReferences();
      
      TreeNode datasetsNode = tvAvailableDs.Nodes.Add(Res.Get("Forms,SelectDataSource,ProjectDs"));
      datasetsNode.ImageIndex = 66;
      datasetsNode.SelectedImageIndex = datasetsNode.ImageIndex;

      TreeNode otherNode = tvAvailableDs.Nodes.Add(Res.Get("Forms,SelectDataSource,OtherDs"));
      otherNode.ImageIndex = 66;
      otherNode.SelectedImageIndex = otherNode.ImageIndex;

      foreach (object obj in objects)
      {
        if (obj is DataSet)
        {
          DataSet dataset = obj as DataSet;
          TreeNode dsNode = datasetsNode.Nodes.Add(dataset.Site.Name);
          dsNode.Tag = dataset;
          dsNode.ImageIndex = 53;
          dsNode.SelectedImageIndex = dsNode.ImageIndex;
          
          foreach (DataTable table in dataset.Tables)
          {
            TreeNode tableNode = dsNode.Nodes.Add(table.TableName);
            tableNode.Tag = table;
            tableNode.ImageIndex = 222;
            tableNode.SelectedImageIndex = tableNode.ImageIndex;
          }
        }
        else if (obj is BindingSource)
        {
          BindingSource source = obj as BindingSource;
          TreeNode dsNode = otherNode.Nodes.Add(source.Site.Name);
          dsNode.Tag = source;
          dsNode.ImageIndex = 53;
          dsNode.SelectedImageIndex = dsNode.ImageIndex;
        }
      }
      
      if (datasetsNode.Nodes.Count == 0)
        tvAvailableDs.Nodes.Remove(datasetsNode);

      if (otherNode.Nodes.Count == 0)
        tvAvailableDs.Nodes.Remove(otherNode);

      if (tvAvailableDs.Nodes.Count > 0)
        tvAvailableDs.Nodes[0].Expand();
      UpdateSelectedDs();
      UpdateButtons();
    }
    
    private void UpdateSelectedDs()
    {
      tvSelectedDs.CreateNodes(FReport.Dictionary);
      foreach (Relation relation in FReport.Dictionary.Relations)
      {
        relation.Enabled = relation.ParentDataSource.Enabled && relation.ChildDataSource.Enabled;
      }
    }
    
    private void UpdateButtons()
    {
      btnAdd.Enabled = tvAvailableDs.SelectedNode != null && tvAvailableDs.SelectedNode.Tag != null;
      btnRemove.Enabled = tvSelectedDs.SelectedNode != null;
      btnRemoveAll.Enabled = tvSelectedDs.Nodes.Count > 0;
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
      TreeNode node = tvAvailableDs.SelectedNode;
      
      if (node.Tag is BindingSource)
      {
        BindingSource source = node.Tag as BindingSource;
        FReport.Dictionary.RegisterBusinessObject(source, source.Site.Name, 1, true);
      }  
      else
      {
        DataSet dataset = null;
        if (node.Tag is DataSet)
          dataset = node.Tag as DataSet;
        else if (node.Tag is DataTable)
          dataset = (node.Tag as DataTable).DataSet;

        if (dataset != null)
        {
          FReport.Dictionary.RegisterData(dataset, dataset.Site.Name, node.Tag is DataSet);
          if (node.Tag is DataTable)
            FReport.Dictionary.FindDataTableSource(node.Tag as DataTable).Enabled = true;
        }
        else
          FReport.Dictionary.RegisterData(node.Tag, node.Text, true);
      }
      
      UpdateSelectedDs();
      UpdateButtons();
    }

    private void btnRemove_Click(object sender, EventArgs e)
    {
      TreeNode node = tvSelectedDs.SelectedNode;
      DataComponentBase comp = node.Tag as DataComponentBase;
      if (comp != null)
        comp.Enabled = false;
      
      UpdateSelectedDs();
      UpdateButtons();
    }

    private void btnRemoveAll_Click(object sender, EventArgs e)
    {
      while (FReport.Dictionary.DataSources.Count > 0)
      {
        FReport.Dictionary.DataSources[0].Dispose();
      }
      
      UpdateSelectedDs();
      UpdateButtons();
    }

    private void tvAvailableDs_AfterSelect(object sender, TreeViewEventArgs e)
    {
      UpdateButtons();
    }

    public override void Localize()
    {
      base.Localize();
      MyRes res = new MyRes("Forms,SelectDataSource");
      Text = res.Get("");
      lblHint.Text = res.Get("Hint");
      lblAvailableDs.Text = res.Get("AvailableDs");
      lblSelectedDs.Text = res.Get("SelectedDs");
      btnOk.Text = Res.Get("Buttons,Close");
    }
    
    public SelectDataSourceForm()
    {
      InitializeComponent();
      Localize();
    }
  }
}

