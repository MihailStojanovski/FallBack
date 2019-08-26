using System;
using System.Diagnostics;

public class ReportGen
{
	public static void GenerateReport()
	{
        System.Data.SqlClient.SqlConnection sqlConnection1 =
                new System.Data.SqlClient.SqlConnection(@"Data Source=MIHAIL-PC\SQL2008R2;Initial Catalog=UserGeneration;Integrated Security=True");
        System.Data.SqlClient.SqlCommand cm = (System.Data.SqlClient.SqlCommand)sqlConnection1.CreateCommand();
        cm.CommandType = System.Data.CommandType.StoredProcedure;
        //se setira naziv sql procedurata sto se povikuva
        cm.CommandText = "dbo.getAllItems";

        System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cm);
        System.Data.DataSet ds = new System.Data.DataSet("Users");
        da.Fill(ds);

        FastReport.Report rpt = new FastReport.Report();

        try
        {
            string temPath = @"C:\Users\mihail\Documents\CreatedFiles\Reports\UsersReport.frx";
            rpt.Load(temPath);
        }
        catch (Exception ex)
        {
            throw new Exception("Извештајот не е вчитан!");
        }

        rpt.RegisterData(ds);
        rpt.GetDataSource("Users").Enabled = true;

        rpt.Prepare();

        FastReport.Export.Pdf.PDFExport rtfexport = new FastReport.Export.Pdf.PDFExport();

        try
        {
            using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
            {
                rpt.Export(rtfexport, mStream);
                rpt.Export(rtfexport, @"C:\Users\mihail\Documents\CreatedFiles\Reports\UsersReport.pdf");

            }
        }
        catch (Exception ex)
        {
            throw new Exception("Грешка при генерирање на прегледот. Ве молиме обидете се повторно.");
        }

        Process.Start(@"C:\Users\mihail\Documents\CreatedFiles\Reports\UsersReport.pdf");
    }
}
