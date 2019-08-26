using FastReport;
using FastReport.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserGeneration
{
    class UserGen
    {
        static void Main(string[] args)
        {
            string line;
            string[] names;
            string username;
            Random rand = new Random();
            StringBuilder builder = new StringBuilder();
            // Reading the file 
            System.IO.StreamReader usersReader = null;
            try
            {
                usersReader =
                    new System.IO.StreamReader(@"ReadingFrom\Users.txt");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in reading file");
            }

            // Generating Report
            Report report = new Report();

            ReportPage page = new ReportPage();
            page.CreateUniqueName();
            page.TopMargin = 10.0f;
            page.LeftMargin = 10.0f;
            page.RightMargin = 10.0f;
            page.BottomMargin = 10.0f;
            // Title Creation
            page.ReportTitle = new ReportTitleBand();
            page.ReportTitle.CreateUniqueName();
            page.ReportTitle.Height = 4.0f * Units.Centimeters;
            // Populationg title with text
            TextObject titleText = new TextObject();
            titleText.CreateUniqueName();
            titleText.Left = 1.0f * Units.Centimeters;
            titleText.Top = 1.0f * Units.Centimeters;
            titleText.Width = 17.0f * Units.Centimeters;
            titleText.Height = 2.0f * Units.Centimeters;
            titleText.HorzAlign = HorzAlign.Center;
            titleText.VertAlign = VertAlign.Center;
            titleText.Font = new Font("Arial", 14.0f, FontStyle.Bold);
            titleText.TextColor = Color.Black;
            titleText.Text = "Users and Passwords";
            page.ReportTitle.Objects.Add(titleText);


            while ((line = usersReader.ReadLine()) != null)
            {
                // Names gets the first name in names[0] and the last in names[1]
                names = line.Split(null);
                // Username has Username with first letter of name + surname
                username = names[0][0] + names[1];
                builder.Append("User: " + line + "\n");
                builder.Append("Username: " + username + "\n");
                // Generated password from PasswordGenerator class
                builder.Append("Password: " + PasswordGenerator.generatePassword(rand) + "\n");

                // Creating the databand for the report
                DataBand band = new DataBand();
                page.Bands.Add(band);
                band.CreateUniqueName();
                band.Height = 2f * Units.Centimeters;

                TextObject bandText = new TextObject();
                bandText.CreateUniqueName();
                bandText.Bounds = new RectangleF(0.0f * Units.Centimeters, 0.0f,
                20.0f * Units.Centimeters, 2f * Units.Centimeters);
                bandText.HorzAlign = HorzAlign.Left;
                bandText.VertAlign = VertAlign.Center;
                bandText.Text = builder.ToString();
                band.AddChild(bandText);

                // Called if we want multiple pdf files with one user on each
                if (args.Length > 0 && int.Parse(args[0]) == 1)
                {
                    // Creates the unique name for each pdf and calls the report generation method
                    ReportGen.GenerateMultipleReports(@"..\..\..\CreatedFiles\Multiple\" + names[0]+"_"+names[1] + ".pdf", builder);
                    Console.WriteLine("Finished User : "+ line);
                }
                builder.Clear();

            }

            usersReader.Close();
            // Export of the pdf file with all users on it
            if (args.Length == 0 || int.Parse(args[0]) == 0)
            {
                report.Pages.Add(page);
                report.Prepare();

                FastReport.Export.Pdf.PDFExport rtfexport = new FastReport.Export.Pdf.PDFExport();

                using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                {
                    report.Export(rtfexport, mStream);
                    report.Export(rtfexport, @"..\..\..\CreatedFiles\One\Users.pdf");

                }
                Process.Start(@"..\..\..\CreatedFiles\One\Users.pdf");
            }

            Console.WriteLine("Finished with generating Usernames and Passwords");
            Console.ReadKey();
            

        }
        }
    }
