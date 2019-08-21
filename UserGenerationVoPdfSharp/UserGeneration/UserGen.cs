using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
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
            int i = 1;
            Random rand = new Random();
            StringBuilder builder = new StringBuilder();
            // Read the file 
            System.IO.StreamReader usersReader =
                new System.IO.StreamReader(@"C:\Users\mihail\Documents\ReadngFrom\Users.txt");


            while ((line = usersReader.ReadLine()) != null)
            {

                names = line.Split(null);
                // Username has Username with first letter of name + surname
                username = names[0][0] + names[1];
                builder.Append("User : " + line + "\n");
                builder.Append("Username : " + username + "\n");
                // Generated password from PasswordGenerator class
                builder.Append("Password : " + PasswordGenerator.generatePassword(rand) + "\n");

                if (args.Length == 0 || int.Parse(args[0]) == 0)
                {
                    //Create a new PDF document
                    PdfDocument document = new PdfDocument();

                    //Add a page to the document
                    PdfPage page = document.Pages.Add();

                    //Create PDF graphics for the page
                    XGraphics graph = XGraphics.FromPdfPage(page);

                    //Set the standard font
                    XFont font = new XFont("Verdana", 12, XFontStyle.Bold);

                    XTextFormatter tf = new XTextFormatter(graph);
                    tf.DrawString(builder.ToString(), font, XBrushes.Black,
                    new XRect(0, 0, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);

                    document.Save(@"C:\Users\mihail\Documents\CreatedFiles\GeneratedUser" + i + ".pdf");

                    builder.Clear();

                    i++;
                }

            }

                usersReader.Close();
                if (args.Length > 0 && int.Parse(args[0]) == 1)
                {
                    //Create a new PDF document
                    PdfDocument document = new PdfDocument();

                    //Add a page to the document
                    PdfPage page = document.Pages.Add();

                    //Create PDF graphics for the page
                    XGraphics graph = XGraphics.FromPdfPage(page);

                    //Set the standard font
                    XFont font = new XFont("Verdana", 12, XFontStyle.Bold);

                    //Draw the text
                    XTextFormatter tf = new XTextFormatter(graph);
                    tf.DrawString(builder.ToString(), font, XBrushes.Black,
                    new XRect(0, 0, page.Width.Point, page.Height.Point), XStringFormats.TopLeft);
                    
                    
                    //Save the document
                    document.Save(@"C:\Users\mihail\Documents\CreatedFiles\GeneratedUsers.pdf");
                }




            }
        }
    }
