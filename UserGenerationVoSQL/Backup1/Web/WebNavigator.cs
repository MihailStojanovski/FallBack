using System.Web.UI;
using System;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FastReport.Export.RichText;
using FastReport.Export.Xml;
using FastReport.Export.Odf;
using FastReport.Export.Pdf;
using FastReport.Export.Csv;
using FastReport.Export.Text;
using FastReport.Export.OoXML;
using FastReport.Export.Mht;
using System.IO;
using FastReport.Utils;

namespace FastReport.Web
{
    public partial class WebReport
    {
        private void RenderDesignModeNavigatorControls(HtmlTextWriter writer)
        {
            writer.WriteLine("<span id=\"WebReport1\" style=\"display:inline-block;border-color:" + this.BorderColor.ToString() + ";border-width:" + this.BorderWidth.Value.ToString() + "px;border-style:" + this.BorderStyle.ToString() + ";height:" + this.Height.ToString() + "px;width:" + this.Width.ToString() + "px;vertical-align:top;\">");
            if (ShowToolbar)
            {
                writer.WriteLine("<table width=\"100%\" bgcolor=\"" + System.Drawing.ColorTranslator.ToHtml(ToolbarColor) + "\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">");
                writer.WriteLine("<tr height=\"4\"/><td width=\"6\"></td></tr>");
                writer.WriteLine("<tr height=\"19\" valign=\"middle\" align=\"center\">");
                writer.WriteLine("<td width=\"6\">&nbsp;</td>");
                if (ShowExports)
                {
                    writer.WriteLine("<td width=\"105\" align=\"left\"><select style=\"font-family:Tahoma;font-size:8pt;width:100px;\">");
                    writer.WriteLine("  <option selected=\"selected\">Export to...</option>");
                    writer.WriteLine("</select></td>");
                    writer.WriteLine("<td width=\"16\"><img src=\"" + GetButtonDesignPath("Export") + "\" style=\"height:16px;width:16px;border-width:0px;\" /></td>");
                    writer.WriteLine("<td width=\"10\">&nbsp;</td>");
                }
                if (ShowPrint)
                {
                    writer.WriteLine("<td width=\"16\"><img src=\"" + GetButtonDesignPath("Print") + "\" style=\"height:16px;width:16px;border-width:0px;\" /></td>");
                    writer.WriteLine("<td width=\"10\">&nbsp;</td>");
                }
                if (ShowZoomButton)
                {
                    writer.WriteLine("<td width=\"105\" align=\"right\"><select style=\"font-family:Tahoma;font-size:8pt;width:100px;\">");
                    writer.WriteLine("  <option selected=\"selected\">100%</option>");
                    writer.WriteLine("</select></td>");
                    writer.WriteLine("<td width=\"10\">&nbsp;</td>");
                }
                if (ShowRefreshButton)
                {
                    writer.WriteLine("<td width=\"20\"><input type=\"image\" src=\"" + GetButtonDesignPath("Refresh") + "\" style=\"height:16px;width:16px;border-width:0px;\" /></td>");
                    writer.WriteLine("<td width=\"5\">&nbsp;</td>");
                }
                if (ShowFirstButton)
                  writer.WriteLine("<td width=\"20\"><input type=\"image\" src=\"" + GetButtonDesignPath("First") + "\" style=\"height:16px;width:16px;border-width:0px;\" /></td>");
                if (ShowPrevButton)
                  writer.WriteLine("<td width=\"20\"><input type=\"image\" src=\"" + GetButtonDesignPath("Prev") + "\" style=\"height:16px;width:16px;border-width:0px;\" /></td>");
                if (ShowPageNumber)
                {
                  writer.WriteLine("<td width=\"2\">&nbsp;</td>");
                  writer.WriteLine("<td width=\"30\"><input type=\"text\" value=\"1\" size=\"3\" style=\"font-family:Tahoma;font-size:8pt;width:40px;text-align:center;width:30;\" /></td>");
                  writer.WriteLine("<td width=\"40\"><span style=\"font-family:Tahoma;font-size:8pt;\"> of 1</span></td>");
                }
                if (ShowNextButton)
                  writer.WriteLine("<td width=\"20\"><input type=\"image\" src=\"" + GetButtonDesignPath("Next") + "\" style=\"height:16px;width:16px;border-width:0px;\" /></td>");
                if (ShowLastButton)
                  writer.WriteLine("<td width=\"20\"><input type=\"image\" src=\"" + GetButtonDesignPath("Last") + "\" style=\"height:16px;width:16px;border-width:0px;\" /></td>");
                writer.WriteLine("<td align=\"right\">&nbsp;</td>");
                writer.WriteLine("<td width=\"10\">&nbsp;</td>");
               
                writer.WriteLine("</tr>");

                writer.WriteLine("<tr height=\"5\"><td width=\"6\"></td></tr></table>");
            }
            writer.WriteLine("<div style=\"text-align:center; vertical-align: middle; " +
                "padding-left: " + Padding.Left.ToString() + "px; padding-right: " + Padding.Right.ToString() + 
                "px; padding-top: " + Padding.Top.ToString() + "px; padding-bottom: " + Padding.Bottom + "px; " +
                "font-weight: bold; font-family: Tahoma; font-size: 22px; color: #CCCCCC;\"" +
                ">FastReport .NET<br>ver." + Config.Version + "</div></span>");
        }

        private void CreateNavigatorControls()
        {
            this.Style.Add("vertical-align", "top");
            this.Style.Add("text-align", "left");
            //fix IE6 to expanded content
            this.Style.Add("margin", "1px");
            this.Style.Add("display", "-moz-inline-stack");
            this.Style.Add("display", "inline-block");
            this.Style.Add("_overflow", "hidden");
            this.Style.Add("zoom", "1");
            this.Style.Add("*display", "inline");

            if (ShowToolbar)
            {
                btnExport = new ImageButton();
                btnPrint = new ImageButton();
                cbbExportList = new DropDownList();
                cbbZoom = new DropDownList();
                btnFirst = new ImageButton();
                btnPrev = new ImageButton();
                tbPage = new TextBox();
                lblPages = new Label();
                btnNext = new ImageButton();
                btnLast = new ImageButton();
                btnRefresh = new ImageButton();

                ListItem item;
                if (ShowPdfExport && Config.FullTrust)
                {
                    item = new ListItem(Res.Get("Export,Pdf,File"), "pdf");
                    cbbExportList.Items.Add(item);
                }
                if (ShowRtfExport)
                {
                    item = new ListItem(Res.Get("Export,RichText,File"), "rtf");
                    cbbExportList.Items.Add(item);
                }
                if (ShowMhtExport)
                {
                    item = new ListItem(Res.Get("Export,Mht,File"), "mht");
                    cbbExportList.Items.Add(item);
                }
                if (ShowExcel2007Export && Config.FullTrust)
                {
                    item = new ListItem(Res.Get("Export,Xlsx,File"), "xlsx");
                    cbbExportList.Items.Add(item);
                }
                if (ShowPowerPoint2007Export && Config.FullTrust)
                {
                    item = new ListItem(Res.Get("Export,Pptx,File"), "pptx");
                    cbbExportList.Items.Add(item);
                }
                if (ShowXmlExcelExport)
                {
                    item = new ListItem(Res.Get("Export,Xml,File"), "xls");
                    cbbExportList.Items.Add(item);
                }
                if (ShowOdsExport && Config.FullTrust)
                {
                    item = new ListItem(Res.Get("Export,Ods,File"), "ods");
                    cbbExportList.Items.Add(item);
                }
                if (ShowCsvExport)
                {
                    item = new ListItem(Res.Get("Export,Csv,File"), "csv");
                    cbbExportList.Items.Add(item);
                }
                if (ShowTextExport)
                {
                    item = new ListItem(Res.Get("Export,Text,File"), "txt");
                    cbbExportList.Items.Add(item);
                }
                cbbExportList.Width = 120;
                cbbExportList.Font.Name = "Tahoma";
                cbbExportList.Font.Size = 8;
                cbbExportList.ToolTip = Res.Get("Web,Export");

                btnExport.ImageUrl = GetButtonImageURL("Export", true);
                btnExport.Width = 16;
                btnExport.Height = 16;
                btnExport.ToolTip = Res.Get("Web,Export");
                btnExport.Click += new ImageClickEventHandler(btnExport_Click);

                btnPrint.ImageUrl = GetButtonImageURL("Print", true);
                btnPrint.Width = 16;
                btnPrint.Height = 16;
                btnPrint.ToolTip = Res.Get("Web,Print");
                btnPrint.Click += new ImageClickEventHandler(btnPrint_Click);

                item = new ListItem("25%", "25");
                cbbZoom.Items.Add(item);
                item = new ListItem("50%", "50");
                cbbZoom.Items.Add(item);
                item = new ListItem("75%", "75");
                cbbZoom.Items.Add(item);
                item = new ListItem("100%", "100");
                cbbZoom.Items.Add(item);
                item = new ListItem("150%", "150");
                cbbZoom.Items.Add(item);
                item = new ListItem("200%", "200");
                cbbZoom.Items.Add(item);
                item = new ListItem("300%", "300");
                cbbZoom.Items.Add(item);

                if (this.Width.Type == UnitType.Pixel && this.Height.Type == UnitType.Pixel)
                {
                    item = new ListItem(Res.Get("Designer,Toolbar,Zoom,PageWidth"), "1000");
                    cbbZoom.Items.Add(item);
                    item = new ListItem(Res.Get("Designer,Toolbar,Zoom,WholePage"), "1001");
                    cbbZoom.Items.Add(item);
                }

                int zoomvalue = (int)Math.Round(Zoom * 100);
                int i;
                for (i = 0; i < cbbZoom.Items.Count; i++)
                    if (Convert.ToInt16(cbbZoom.Items[i].Value) == zoomvalue)
                        break;
                    else if (Convert.ToInt16(cbbZoom.Items[i].Value) > zoomvalue)
                    {
                        item = new ListItem(zoomvalue.ToString() + "%", zoomvalue.ToString());
                        cbbZoom.Items.Insert(i, item);
                        break;
                    }    
            
                cbbZoom.Width = 90;
                cbbZoom.Font.Name = "Tahoma";
                cbbZoom.Font.Size = 8;
                cbbZoom.AutoPostBack = true;
                cbbZoom.ToolTip = Res.Get("Web,Zoom");
                cbbZoom.SelectedIndexChanged += new EventHandler(cbbZoom_SelectedIndexChanged);

                btnFirst.Width = 16;
                btnFirst.Height = 16;
                btnFirst.CommandName = "first";
                btnFirst.ToolTip = Res.Get("Preview,First");

                btnPrev.Width = 16;
                btnPrev.Height = 16;
                btnPrev.CommandName = "prev";
                btnPrev.ToolTip = Res.Get("Preview,Prior");

                tbPage.Columns = 3;
                tbPage.Rows = 1;
                tbPage.Width = 30;
                tbPage.TextChanged += new EventHandler(tbPage_TextChanged);
                tbPage.AutoPostBack = true;
                tbPage.Font.Name = "Tahoma";
                tbPage.Font.Size = 8;
                tbPage.Style.Add("text-align", "center");
                tbPage.Style.Add("width", "30");

                lblPages.Font.Name = "Tahoma";
                lblPages.Font.Size = 8;

                btnNext.CommandName = "next";
                btnNext.ToolTip = Res.Get("Preview,Next");
                btnNext.Width = 16;
                btnNext.Height = 16;

                btnLast.CommandName = "last";
                btnLast.ToolTip = Res.Get("Preview,Last");
                btnLast.Width = 16;
                btnLast.Height = 16;

                btnRefresh.ImageUrl = GetButtonImageURL("Refresh", true);
                btnRefresh.ToolTip = Res.Get("Web,Refresh");
                btnRefresh.Width = 16;
                btnRefresh.Height = 16;
                btnRefresh.Click += new ImageClickEventHandler(btnRefresh_Click);

                // draw navigator toolbar
                tblNavigator = new HtmlTable();
                UnitConverter conv = new UnitConverter();
                tblNavigator.Width = this.Width.Value.ToString() + GetHtmlUnitName(this.Width);
                tblNavigator.Height = "28";
                tblNavigator.BgColor = System.Drawing.ColorTranslator.ToHtml(ToolbarColor);
                tblNavigator.CellPadding = 0;
                tblNavigator.CellSpacing = 0;
                tblNavigator.Border = 0;

                HtmlTableCell cell;
                HtmlTableRow row;

                // draw first row
                row = new HtmlTableRow();
                row.Height = "4";
                tblNavigator.Rows.Add(row);

                // draw second row
                row = new HtmlTableRow();
                row.Height = "19";
                row.VAlign = "middle";
                row.Align = "center";

                // left margin
                cell = new HtmlTableCell();
                cell.Width = "6";
                cell.InnerHtml = "&nbsp;";
                row.Cells.Add(cell);

                if (ShowExports)
                {
                    // export list combo-box
                    cell = new HtmlTableCell();
                    cell.Width = "125";
                    cell.Align = "left";
                    cell.Controls.Add(cbbExportList);
                    row.Cells.Add(cell);

                    // export picture
                    cell = new HtmlTableCell();
                    cell.Width = "16";
                    cell.Controls.Add(btnExport);
                    row.Cells.Add(cell);

                    // space 
                    cell = new HtmlTableCell();
                    cell.Width = "10";
                    cell.InnerHtml = "&nbsp;";
                    row.Cells.Add(cell);
                }

                if (ShowPrint && Config.FullTrust)
                {
                    // print picture
                    cell = new HtmlTableCell();
                    cell.Width = "16";
                    cell.Controls.Add(btnPrint);
                    row.Cells.Add(cell);

                    // space 
                    cell = new HtmlTableCell();
                    cell.Width = "10";
                    cell.InnerHtml = "&nbsp;";
                    row.Cells.Add(cell);
                }

                if (ShowZoomButton)
                {
                    // zoom
                    cell = new HtmlTableCell();
                    cell.Width = "95";
                    cell.Align = "right";
                    cell.Controls.Add(cbbZoom);
                    row.Cells.Add(cell);

                    // space 
                    cell = new HtmlTableCell();
                    cell.Width = "10";
                    cell.InnerHtml = "&nbsp;";
                    row.Cells.Add(cell);
                }

                if (ShowRefreshButton)
                {
                    // refresh button
                    cell = new HtmlTableCell();
                    cell.Width = "20";
                    cell.Controls.Add(btnRefresh);
                    row.Cells.Add(cell);

                    // space 
                    cell = new HtmlTableCell();
                    cell.Width = "5";
                    cell.InnerHtml = "&nbsp;";
                    row.Cells.Add(cell);
                }

                if (ShowFirstButton)
                {
                  // first button
                  cell = new HtmlTableCell();
                  cell.Width = "20";
                  cell.Controls.Add(btnFirst);
                  row.Cells.Add(cell);
                }
                if (ShowPrevButton)
                {
                  // previous button
                  cell = new HtmlTableCell();
                  cell.Width = "20";
                  cell.Controls.Add(btnPrev);
                  row.Cells.Add(cell);
                }

                if (ShowPageNumber)
                {
                  cell = new HtmlTableCell();
                  cell.Width = "2";
                  cell.InnerHtml = "&nbsp;";
                  row.Cells.Add(cell);

                  // page number text edit
                  cell = new HtmlTableCell();
                  cell.Width = "30";
                  cell.Controls.Add(tbPage);
                  row.Cells.Add(cell);

                  // label with total pages
                  cell = new HtmlTableCell();
                  cell.Width = "40";
                  cell.Controls.Add(lblPages);
                  row.Cells.Add(cell);
                }
                if (ShowNextButton)
                {
                  // next button
                  cell = new HtmlTableCell();
                  cell.Width = "20";
                  cell.Controls.Add(btnNext);
                  row.Cells.Add(cell);
                }
                if (ShowLastButton)
                {
                  // last button
                  cell = new HtmlTableCell();
                  cell.Width = "20";
                  cell.Controls.Add(btnLast);
                  row.Cells.Add(cell);
                }

                // ribbon cell
                cell = new HtmlTableCell();
                cell.Align = "right";
                cell.InnerHtml = "&nbsp;";
                row.Cells.Add(cell);

                // right margin
                cell = new HtmlTableCell();
                cell.Width = "6";
                cell.InnerHtml = "&nbsp;";
                row.Cells.Add(cell);

                tblNavigator.Rows.Add(row);

                // last row
                row = new HtmlTableRow();
                row.Height = "5";
                tblNavigator.Rows.Add(row);

                this.Controls.Add(tblNavigator);
            }

            // panel with report content                        
            
            div = new HtmlGenericControl("div");

            //if (this.Height.Type == UnitType.Pixel && this.Width.Type == UnitType.Pixel)
                div.Style.Add("overflow", "auto");

            if (this.Width.Type == UnitType.Pixel)
            {
                div.Style.Add("width", (this.Width.Value - Padding.Left - Padding.Right).ToString() + "px");
                if (Padding.Left > 0)
                    div.Style.Add("padding-left", Padding.Left.ToString() + "px");
                if (Padding.Right > 0)
                    div.Style.Add("padding-right", Padding.Right.ToString() + "px");
            }
            if (this.Height.Type == UnitType.Pixel)
            {
                div.Style.Add("height", (Convert.ToInt16(this.Height.Value) - Padding.Top - Padding.Bottom -
                    (ShowToolbar ? (int)Convert.ToInt16(tblNavigator.Height) : 0) - 2).ToString() + "px");
                if (Padding.Top > 0)
                    div.Style.Add("padding-top", Padding.Top.ToString() + "px");
                if (Padding.Bottom > 0)
                    div.Style.Add("padding-bottom", Padding.Bottom.ToString() + "px");
            }

            this.Controls.Add(div);
        }

        private string GetHtmlUnitName(Unit unit)
        {
            switch (unit.Type)
            {
                case UnitType.Pixel:
                    return "px";
                case UnitType.Percentage:
                    return "%";
                default:
                    return "";
            }
        }

        void btnExport_Click(object sender, ImageClickEventArgs e)
        {
            string guid = ReportGuid;
            if (CacheGet("frxExport" + guid) != null)
                CacheRemove("frxExport" + guid);
            if (TotalPages > 0)
            {
                string format = cbbExportList.SelectedValue;
                WebExportItem ExportItem = new WebExportItem();
                bool exported = false;
                if (format == "csv")
                {
                    CSVExport csvExport = new CSVExport();
                    csvExport.OpenAfterExport = false;
                    csvExport.Export(Report, ExportItem.File);
                    exported = true;
                }
                if (format == "txt")
                {
                    TextExport textExport = new TextExport();
                    textExport.OpenAfterExport = false;
                    textExport.AvoidDataLoss = true;
                    textExport.Export(Report, ExportItem.File);
                    exported = true;
                }
                if (format == "pdf")
                {
                    PDFExport pdfExport = new PDFExport();
                    pdfExport.OpenAfterExport = false;
                    pdfExport.Export(Report, ExportItem.File);
                    exported = true;
                }
                if (format == "rtf")
                {
                    RTFExport rtfExport = new RTFExport();
                    rtfExport.OpenAfterExport = false;
                    rtfExport.Export(Report, ExportItem.File);
                    exported = true;
                }
                if (format == "mht")
                {
                    MHTExport mhtExport = new MHTExport();
                    mhtExport.OpenAfterExport = false;
                    mhtExport.Export(Report, ExportItem.File);
                    exported = true;
                }
                else if (format == "xls")
                {
                    XMLExport xmlExport = new XMLExport();
                    xmlExport.OpenAfterExport = false;
                    xmlExport.Export(Report, ExportItem.File);
                    exported = true;
                }
                else if (format == "ods")
                {
                    ODSExport odsExport = new ODSExport();
                    odsExport.OpenAfterExport = false;
                    odsExport.Export(Report, ExportItem.File);
                    exported = true;
                }
                else if (format == "xlsx")
                {
                    Excel2007Export xlsxExport = new Excel2007Export();
                    xlsxExport.OpenAfterExport = false;
                    xlsxExport.Export(Report, ExportItem.File);
                    exported = true;
                }
                else if (format == "pptx")
                {
                    PowerPoint2007Export pptxExport = new PowerPoint2007Export();
                    pptxExport.OpenAfterExport = false;
                    pptxExport.Export(Report, ExportItem.File);
                    exported = true;
                }
                if (exported)
                {
                    ExportItem.Format = format;
                    ExportItem.FileName = Path.GetFileNameWithoutExtension(Report.FileName.Length == 0 ? "report" : Report.FileName) + "." + format;
                    CacheAdd("frxExport" + guid, ExportItem, null, 5);
                    this.Page.Response.Redirect(this.Page.Request.CurrentExecutionFilePath + "?" + FExportPrefix + "=" + guid);
                    this.Page.Response.End();
                }
            }
        }

        void btnPrint_Click(object sender, ImageClickEventArgs e)
        {
            string guid = ReportGuid;
            if (CacheGet("frxExport" + guid) != null)
                CacheRemove("frxExport" + guid);
            if (TotalPages > 0)
            {
                string format = "pdf";
                WebExportItem ExportItem = new WebExportItem();
                PDFExport pdfExport = new PDFExport();
                pdfExport.OpenAfterExport = false;
                pdfExport.Export(Report, ExportItem.File);
                ExportItem.Format = format;
                ExportItem.FileName = Path.GetFileNameWithoutExtension(Report.FileName.Length == 0 ? "report" : Report.FileName) + "." + format;
                CacheAdd("frxExport" + guid, ExportItem, null, 5);
//                this.Page.Response.Write("<script>window.open('" + this.Page.Request.Url.AbsoluteUri + "?" + FExportPrefix + "=" + guid + "', 'PDF Print');</script>");
                this.Page.Response.Redirect(this.Page.Request.CurrentExecutionFilePath + "?" + FExportPrefix + "=" + guid);
                this.Page.Response.End();
            }
        }

        void btnRefresh_Click(object sender, ImageClickEventArgs e)
        {
            Refresh();
        }

        void cbbZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            float z = 1;
            if (cbbZoom.SelectedValue == "1000")
                z = (float)Math.Round((float)(this.Width.Value - 20) / HTMLExport.PreparedPages[CurrentPage].Width, 2);
            else if (cbbZoom.SelectedValue == "1001")
                z = (float)Math.Round(Math.Min((float)(this.Width.Value - 20) / HTMLExport.PreparedPages[CurrentPage].Width,
                    (float)(this.Height.Value - 40) / HTMLExport.PreparedPages[CurrentPage].Height), 2);
            else
                z = (float)Convert.ToInt16(cbbZoom.SelectedValue) / 100;
            if (z != Zoom)
            {
                Zoom = z;
                HTMLDone = false;
                PrepareReport();
            }
        }

        private void tbPage_TextChanged(object sender, EventArgs e)
        {            
            SetPage(Convert.ToInt16(tbPage.Text) - 1);
        }
    }
}