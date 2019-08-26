using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Windows.Forms;
using FastReport.Design;
using FastReport.Forms;
using System.ComponentModel.Design.Serialization;
using System.CodeDom;
using FastReport.Data;
using System.Data;
using FastReport.Utils;
using FastReport.Design.StandardDesigner;

namespace FastReport
{
  internal class ReportComponentDesigner : ComponentDesigner
  {
    private Report FReport;

    
    public override void Initialize(IComponent component)
    {
      base.Initialize(component);
      FReport = component as Report;
    }

    public override void DoDefaultAction()
    {
      DesignVerb(this, EventArgs.Empty);
    }

    public override DesignerVerbCollection Verbs
    {
      get
      {
        return new DesignerVerbCollection(new DesignerVerb[] { 
          new DesignerVerb("Design Report...", new EventHandler(DesignVerb)),
          new DesignerVerb("Select Data Source...", new EventHandler(SelectDataSourceVerb)) });
      }
    }
    
    private void RaiseReportChanged()
    {
      PropertyDescriptorCollection props = TypeDescriptor.GetProperties(FReport);
      PropertyDescriptor prop = props.Find("ReportResourceString", false);
      RaiseComponentChanged(prop, "", FReport.ReportResourceString);
    }

    private void DesignVerb(object sender, EventArgs e)
    {
      if (FReport.Dictionary.DataSources.Count == 0 && FReport.Dictionary.Connections.Count == 0)
        SelectDataSourceVerb(sender, e);
      else
        FReport.Dictionary.ReRegisterData();  

      try
      {
        using (DesignerForm designerForm = new DesignerForm())
        {
          designerForm.Designer.Report = FReport;
          designerForm.Designer.AskSave = !FReport.StoreInResources;
          designerForm.ShowInTaskbar = true;
          designerForm.ShowDialog();
          if (designerForm.Designer.Modified && FReport.StoreInResources)
            RaiseReportChanged();
        }
      }
      catch (Exception ex)
      {
        using (ExceptionForm form = new ExceptionForm(ex))
        {
          form.ShowDialog();
        }
      }  

      DesignerActionUIService designerActionUISvc = 
        GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
      if (designerActionUISvc != null)
        designerActionUISvc.HideUI(FReport);
    }

    private void SelectDataSourceVerb(object sender, EventArgs e)
    {
      using (SelectDataSourceForm form = new SelectDataSourceForm())
      {
        IReferenceService service = GetService(typeof(IReferenceService)) as IReferenceService;
        form.Init(FReport, service);
        if (form.ShowDialog() == DialogResult.OK)
          RaiseReportChanged();
      }
    }
  }


  internal class ReportCodeDomSerializer : CodeDomSerializer
  {
    public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
    {
      CodeDomSerializer baseClassSerializer = (CodeDomSerializer)manager.
        GetSerializer(typeof(Report).BaseType, typeof(CodeDomSerializer));

      return baseClassSerializer.Deserialize(manager, codeObject);
    }

    public override object Serialize(IDesignerSerializationManager manager, object value)
    {
      CodeDomSerializer baseClassSerializer = (CodeDomSerializer)manager.
        GetSerializer(typeof(Report).BaseType, typeof(CodeDomSerializer));

      object codeObject = baseClassSerializer.Serialize(manager, value);
      if (codeObject is CodeStatementCollection)
      {
        CodeStatementCollection statements = (CodeStatementCollection)codeObject;
        Report report = value as Report;
        List<string> addedItems = new List<string>();

        foreach (Dictionary.RegDataItem item in report.Dictionary.RegisteredItems)
        {
          string dsName = "";
          if (item.Data is DataTable)
            dsName = (item.Data as DataTable).DataSet.Site.Name;
          else if (item.Data is BindingSource)
            dsName = (item.Data as BindingSource).Site.Name;

          if (dsName != "" && !addedItems.Contains(dsName))
          {
            CodeExpression[] args = new CodeExpression[] {
              new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dsName),
              new CodePrimitiveExpression(dsName) };
            
            CodeExpression invokeExpression = new CodeMethodInvokeExpression(
              new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), report.Site.Name),
              "RegisterData",
              args);
            statements.Add(new CodeExpressionStatement(invokeExpression));
            addedItems.Add(dsName);
          }
        }
      }

      return codeObject;
    }
  }
  
}
