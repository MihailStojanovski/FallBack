using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using FastReport.Web;
using FastReport.Utils;

namespace FastReport.Forms
{
  internal partial class AspSelectDataSourceForm : BaseDialogForm
  {
    private WebReport FReport;

    public void Init(WebReport report, IReferenceService service)
    {
      FReport = report;
      tvDataSources.ImageList = Res.GetImages();
      
      List<string> reportDataSources = new List<string>();
      reportDataSources.AddRange(FReport.ReportDataSources.Split(new char[] { ';' }));

      object[] objects = service.GetReferences(typeof(System.Web.UI.IDataSource));
      foreach (object obj in objects)
      {
        System.Web.UI.Control dataSource = obj as System.Web.UI.Control;
        if (dataSource != null)
        {
          string dsName = dataSource.ID;
          TreeNode dsNode = tvDataSources.Nodes.Add(dsName);
          dsNode.Tag = dataSource;
          dsNode.ImageIndex = 53;
          dsNode.SelectedImageIndex = dsNode.ImageIndex;
          dsNode.Checked = reportDataSources.Contains(dsName);
        }
      }
    }

    private void AspSelectDataSourceForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      if (DialogResult == DialogResult.OK)
      {
        FReport.ReportDataSources = "";
        foreach (TreeNode node in tvDataSources.Nodes)
        {
          if (node.Checked)
            FReport.ReportDataSources += FReport.ReportDataSources == "" ? node.Text : ";" + node.Text;
        }
      }
    }

    public override void Localize()
    {
      base.Localize();
      MyRes res = new MyRes("Forms,SelectDataSource");
      Text = res.Get("");
    }

    public AspSelectDataSourceForm()
    {
      InitializeComponent();
      Localize();
    }
  }
}

