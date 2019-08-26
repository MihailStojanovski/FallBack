using System;
using System.Collections.Generic;
using System.Text;
using FastReport.Utils;
using FastReport.Data;
using FastReport.Dialog;
using FastReport.Table;
using FastReport.Wizards;
using FastReport.Export.Image;
using FastReport.Export.RichText;
using FastReport.Export.Xml;
using FastReport.Export.Html;
using FastReport.Export.Mht;
using FastReport.Export.Odf;
using FastReport.Export.Pdf;
using FastReport.Export.Text;
using FastReport.Export.Csv;
using FastReport.Export.Dbf;
using FastReport.Barcode;
using FastReport.Matrix;
using FastReport.Format;
using FastReport.Design;
using FastReport.MSChart;
using FastReport.Functions;
using FastReport.Export.OoXML;

namespace FastReport
{
  /// <summary>
  /// The FastReport.dll assembly initializer.
  /// </summary>
  public sealed class AssemblyInitializer : AssemblyInitializerBase
  {
    /// <summary>
    /// Registers all standard objects, wizards, export filters.
    /// </summary>
    public AssemblyInitializer()
    {
      // report
      RegisteredObjects.AddReport(typeof(Report), 134);

      // pages
#if (!Basic)
      RegisteredObjects.AddPage(typeof(DialogPage), "DialogPage", 136);
#endif
      RegisteredObjects.AddPage(typeof(ReportPage), "ReportPage", 135);

      // data items
      RegisteredObjects.Add(typeof(Column), "", 0);
      RegisteredObjects.Add(typeof(CommandParameter), "", 0);
      RegisteredObjects.Add(typeof(Relation), "", 0);
      RegisteredObjects.Add(typeof(Parameter), "", 0);
      RegisteredObjects.Add(typeof(Total), "", 0);
      RegisteredObjects.Add(typeof(TableDataSource), "", 0);
      RegisteredObjects.Add(typeof(ViewDataSource), "", 0);
      RegisteredObjects.Add(typeof(BusinessObjectDataSource), "", 0);
#if (!Basic)
      RegisteredObjects.AddConnection(typeof(MsAccessDataConnection));
      RegisteredObjects.AddConnection(typeof(XmlDataConnection));
      RegisteredObjects.AddConnection(typeof(OleDbDataConnection));
      RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
      RegisteredObjects.AddConnection(typeof(OdbcDataConnection));
#endif

      // formats
      RegisteredObjects.Add(typeof(BooleanFormat), "", 0);
      RegisteredObjects.Add(typeof(CurrencyFormat), "", 0);
      RegisteredObjects.Add(typeof(CustomFormat), "", 0);
      RegisteredObjects.Add(typeof(DateFormat), "", 0);
      RegisteredObjects.Add(typeof(GeneralFormat), "", 0);
      RegisteredObjects.Add(typeof(NumberFormat), "", 0);
      RegisteredObjects.Add(typeof(PercentFormat), "", 0);
      RegisteredObjects.Add(typeof(TimeFormat), "", 0);

      // dialog controls
#if (!Basic)
      RegisteredObjects.Add(typeof(ButtonControl), "DialogPage", 115);
      RegisteredObjects.Add(typeof(CheckBoxControl), "DialogPage", 116);
      RegisteredObjects.Add(typeof(CheckedListBoxControl), "DialogPage", 148);
      RegisteredObjects.Add(typeof(ComboBoxControl), "DialogPage", 119);
      RegisteredObjects.Add(typeof(GridControl), "DialogPage", 122);
      RegisteredObjects.Add(typeof(DataSelectorControl), "DialogPage", 128);
      RegisteredObjects.Add(typeof(DateTimePickerControl), "DialogPage", 120);
      RegisteredObjects.Add(typeof(GroupBoxControl), "DialogPage", 143);
      RegisteredObjects.Add(typeof(LabelControl), "DialogPage", 112);
      RegisteredObjects.Add(typeof(ListBoxControl), "DialogPage", 118);
      RegisteredObjects.Add(typeof(ListViewControl), "DialogPage", 203);
      RegisteredObjects.Add(typeof(MaskedTextBoxControl), "DialogPage", 147);
      RegisteredObjects.Add(typeof(MonthCalendarControl), "DialogPage", 145);
      RegisteredObjects.Add(typeof(NumericUpDownControl), "DialogPage", 146);
      RegisteredObjects.Add(typeof(PanelControl), "DialogPage", 144);
      RegisteredObjects.Add(typeof(PictureBoxControl), "DialogPage", 103);
      RegisteredObjects.Add(typeof(RadioButtonControl), "DialogPage", 117);
      RegisteredObjects.Add(typeof(RichTextBoxControl), "DialogPage", 205);
      RegisteredObjects.Add(typeof(TextBoxControl), "DialogPage", 113);
      RegisteredObjects.Add(typeof(TreeViewControl), "DialogPage", 204);
#endif

      // bands
      RegisteredObjects.Add(typeof(ReportTitleBand), "", 154, "Objects,Bands,ReportTitle");
      RegisteredObjects.Add(typeof(ReportSummaryBand), "", 155, "Objects,Bands,ReportSummary");
      RegisteredObjects.Add(typeof(PageHeaderBand), "", 156, "Objects,Bands,PageHeader");
      RegisteredObjects.Add(typeof(PageFooterBand), "", 157, "Objects,Bands,PageFooter");
      RegisteredObjects.Add(typeof(ColumnHeaderBand), "", 158, "Objects,Bands,ColumnHeader");
      RegisteredObjects.Add(typeof(ColumnFooterBand), "", 159, "Objects,Bands,ColumnFooter");
      RegisteredObjects.Add(typeof(DataHeaderBand), "", 160, "Objects,Bands,DataHeader");
      RegisteredObjects.Add(typeof(DataFooterBand), "", 161, "Objects,Bands,DataFooter");
      RegisteredObjects.Add(typeof(DataBand), "", 162, "Objects,Bands,Data");
      RegisteredObjects.Add(typeof(GroupHeaderBand), "", 163, "Objects,Bands,GroupHeader");
      RegisteredObjects.Add(typeof(GroupFooterBand), "", 164, "Objects,Bands,GroupFooter");
      RegisteredObjects.Add(typeof(ChildBand), "", 165, "Objects,Bands,Child");
      RegisteredObjects.Add(typeof(OverlayBand), "", 166, "Objects,Bands,Overlay");

      // report objects
      RegisteredObjects.Add(typeof(TextObject), "ReportPage", 102);
      RegisteredObjects.Add(typeof(PictureObject), "ReportPage", 103);
      RegisteredObjects.AddCategory("ReportPage,Shapes", 106, "Objects,Shapes");
      RegisteredObjects.Add(typeof(LineObject), "ReportPage,Shapes", 105, "Objects,Shapes,Line", 0, true);
      RegisteredObjects.Add(typeof(LineObject), "ReportPage,Shapes", 107, "Objects,Shapes,DiagonalLine", 1, true);
      RegisteredObjects.Add(typeof(LineObject), "ReportPage,Shapes", 150, "Objects,Shapes,DiagonalLine", 2, true);
      RegisteredObjects.Add(typeof(LineObject), "ReportPage,Shapes", 151, "Objects,Shapes,DiagonalLine", 3, true);
      RegisteredObjects.Add(typeof(LineObject), "ReportPage,Shapes", 152, "Objects,Shapes,DiagonalLine", 4, true);
      RegisteredObjects.Add(typeof(ShapeObject), "ReportPage,Shapes", 108, "Objects,Shapes,Rectangle", 0);
      RegisteredObjects.Add(typeof(ShapeObject), "ReportPage,Shapes", 109, "Objects,Shapes,RoundRectangle", 1);
      RegisteredObjects.Add(typeof(ShapeObject), "ReportPage,Shapes", 110, "Objects,Shapes,Ellipse", 2);
      RegisteredObjects.Add(typeof(ShapeObject), "ReportPage,Shapes", 111, "Objects,Shapes,Triangle", 3);
      RegisteredObjects.Add(typeof(ShapeObject), "ReportPage,Shapes", 131, "Objects,Shapes,Diamond", 4);
      RegisteredObjects.Add(typeof(SubreportObject), "ReportPage", 104);
#if (!Basic)
      RegisteredObjects.Add(typeof(TableObject), "ReportPage", 127);
      RegisteredObjects.Add(typeof(TableColumn), "", 215);
      RegisteredObjects.Add(typeof(TableRow), "", 216);
      RegisteredObjects.Add(typeof(TableCell), "", 214);
      RegisteredObjects.Add(typeof(MatrixObject), "ReportPage", 142);
      RegisteredObjects.Add(typeof(BarcodeObject), "ReportPage", 123);
      RegisteredObjects.Add(typeof(RichObject), "ReportPage", 126);
      RegisteredObjects.Add(typeof(CheckBoxObject), "ReportPage", 124);
      RegisteredObjects.Add(typeof(MSChartObject), "ReportPage", 125);
      RegisteredObjects.Add(typeof(MSChartSeries), "", 130);
      RegisteredObjects.Add(typeof(ZipCodeObject), "ReportPage", 129);
      RegisteredObjects.Add(typeof(CellularTextObject), "ReportPage", 121);
//      RegisteredObjects.Add(typeof(CrossBandObject), "ReportPage", 11, "Cross-band line", 0);
//      RegisteredObjects.Add(typeof(CrossBandObject), "ReportPage", 11, "Cross-band rectangle", 1);
#endif

      // wizards
      RegisteredObjects.AddWizard(typeof(BlankReportWizard), 134, "Wizards,BlankReport", false);
      RegisteredObjects.AddWizard(typeof(InheritedReportWizard), 134, "Wizards,InheritedReport", false);
      RegisteredObjects.AddWizard(typeof(StandardReportWizard), 133, "Wizards,StandardReport", false);
      RegisteredObjects.AddWizard(typeof(LabelWizard), 133, "Wizards,Label", false);
      RegisteredObjects.AddWizard(typeof(NewPageWizard), 135, "Wizards,NewPage", true);
#if (!Basic)
      RegisteredObjects.AddWizard(typeof(NewDialogWizard), 136, "Wizards,NewDialog", true);
      RegisteredObjects.AddWizard(typeof(NewDataSourceWizard), 137, "Wizards,NewDataSource", true);
#endif

      // exports
#if (Basic)
      RegisteredObjects.AddExport(typeof(ImageExport), "Export,Image,File");
#else      
      RegisteredObjects.AddExport(typeof(PDFExport), "Export,Pdf,File", 201);
      RegisteredObjects.AddExport(typeof(RTFExport), "Export,RichText,File", 190);
      RegisteredObjects.AddExport(typeof(HTMLExport), "Export,Html,File");
      RegisteredObjects.AddExport(typeof(MHTExport), "Export,Mht,File");
      RegisteredObjects.AddExport(typeof(XMLExport), "Export,Xml,File", 191);
      RegisteredObjects.AddExport(typeof(Excel2007Export), "Export,Xlsx,File", 191);
      RegisteredObjects.AddExport(typeof(PowerPoint2007Export), "Export,Pptx,File");
      RegisteredObjects.AddExport(typeof(ODSExport), "Export,Ods,File");
      //      RegisteredObjects.AddExport(typeof(ODTExport), "Export,Odt,File");
      RegisteredObjects.AddExport(typeof(CSVExport), "Export,Csv,File");
      RegisteredObjects.AddExport(typeof(DBFExport), "Export,Dbf,File");
      RegisteredObjects.AddExport(typeof(TextExport), "Export,Text,File");
      RegisteredObjects.AddExport(typeof(ImageExport), "Export,Image,File");
#endif

#if true
      //      RegisteredObjects.AddExport(typeof(OOXMLWordExport), "Export,Docx,File");
#endif

      // functions
      RegisteredObjects.AddCategory("Functions", -1, "");
      StdFunctions.Register();
    }
  }
}
