using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FastReport.Design;
using FastReport.Forms;
using FastReport.Data;
using System.Web.UI.Design;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.ComponentModel;
using FastReport.Utils;
using FastReport.Design.StandardDesigner;

namespace FastReport.Web
{
   /// <summary>
   /// Represents the WebReport component designer.
   /// </summary>
   public class WebReportComponentDesigner : ControlDesigner
   {
       private DesignerActionListCollection _actionLists = null;

       /// <inheritdoc/>
       public override DesignerActionListCollection ActionLists
       {
           get
           {
               if (_actionLists == null)
               {
                   _actionLists = new DesignerActionListCollection();
                   _actionLists.AddRange(base.ActionLists);

                   // Add a custom DesignerActionList
                   _actionLists.Add(new ActionList(this));
               }
               return _actionLists;
           }
       }

       /// <summary>
       /// Represents the action list for WebReport component designer.
       /// </summary>
       public class ActionList : DesignerActionList
       {
           private WebReportComponentDesigner _parent;
           private DesignerActionItemCollection _items;

           /// <summary>
           /// Initializes a new instance of the ActionList class.
           /// </summary>
           /// <param name="parent">The parent designer.</param>
           public ActionList(WebReportComponentDesigner parent)
               : base(parent.Component)
           {
               _parent = parent;
           }

           /// <inheritdoc/>
           public override DesignerActionItemCollection GetSortedActionItems()
           {
               if (_items == null)
               {
                   _items = new DesignerActionItemCollection();
                   _items.Add(new DesignerActionMethodItem(this, "DesignReport", "Design Report...", true));
                   _items.Add(new DesignerActionMethodItem(this, "SelectDataSource", "Select Data Source...", true));
                 }
               return _items;
           }

           /// <summary>
           /// The "Design Report" action.
           /// </summary>
           public void DesignReport()
           {
               // Get a reference to the parent designer's associated control               
               WebReport webreport = (WebReport)_parent.Component;
               Report report = new Report();

               bool fileBased = false;
               if (!String.IsNullOrEmpty(webreport.ReportFile))
               {
                   string fileName = webreport.ReportFile;
                   fileName = WebUtils.MapPath(webreport.Site, fileName);
                   report.Load(fileName);
                   fileBased = true;
               }
               else if (!String.IsNullOrEmpty(webreport.ReportResourceString))
                   report.ReportResourceString = webreport.ReportResourceString;

               try
               {
                   webreport.RegisterData(report, webreport.Site);
               }
               catch (Exception ex)
               {
                 using (ExceptionForm form = new ExceptionForm(ex))
                 {
                   form.ShowDialog();
                 }
                 return;
               }
               using (DesignerForm designerForm = new DesignerForm())
               {
                 designerForm.Designer.Report = report;
                 designerForm.Designer.AskSave = fileBased;
                 designerForm.ShowInTaskbar = true;
                 designerForm.ShowDialog();
                 if (designerForm.Designer.Modified && !fileBased)
                 {
                   string oldValue = webreport.ReportResourceString;
                   webreport.ReportResourceString = report.SaveToStringBase64();
                   PropertyDescriptorCollection props = TypeDescriptor.GetProperties(webreport);
                   PropertyDescriptor prop = props.Find("ReportResourceString", false);
                   _parent.RaiseComponentChanged(prop, oldValue, webreport.ReportResourceString);
                 }
               }
           }

           /// <summary>
           /// The "Select Data Source" action.
           /// </summary>
           public void SelectDataSource()
           {
               using (AspSelectDataSourceForm form = new AspSelectDataSourceForm())
               {
                   WebReport webreport = (WebReport)_parent.Component;
                   string oldValue = webreport.ReportDataSources;
                   IReferenceService service = GetService(typeof(IReferenceService)) as IReferenceService;
                   form.Init(webreport, service);
                   if (form.ShowDialog() == DialogResult.OK)
                   {
                       PropertyDescriptorCollection props = TypeDescriptor.GetProperties(webreport);
                       PropertyDescriptor prop = props.Find("ReportDataSources", false);
                       _parent.RaiseComponentChanged(prop, oldValue, webreport.ReportDataSources);
                   }
               }
           }
       }
   }
   
   internal class ReportFileEditor : UrlEditor
   {
     protected override string Caption
     {
       get
       {
         return "Select Report File";
       }
     }

     protected override string Filter
     {
       get
       {
         return Res.Get("FileFilters,Report");
       }
     }
   }

   internal class LocalizationFileEditor : UrlEditor
   {
     protected override string Caption
     {
       get
       {
         return "Select Localization File";
       }
     }

     protected override string Filter
     {
       get
       {
         return Res.Get("FileFilters,Localization");
       }
     }
   }
 }
