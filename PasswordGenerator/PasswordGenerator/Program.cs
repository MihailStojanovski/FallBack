using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.Drawing;

namespace PasswordGenerator
{
    class Program
    {
        static void Main(string[] args)
        {

            //Create a new PDF document
            PdfDocument document = new PdfDocument();

            //Add a page to the document
            PdfPage page = document.Pages.Add();

            //Create PDF graphics for the page
            PdfGraphics graphics = page.Graphics;

            //Set the standard font
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);

            StringBuilder builder = new StringBuilder("", 8);
            StringBuilder fileBuilder = new StringBuilder();
            Random rand = new Random();
            int lim = int.Parse(args[0]);
            for (int i = 0;i < lim ; i++)
            {
                // 2 Digits
                builder.Append(rand.Next(0, 9));
                builder.Append(rand.Next(0, 9));

                // 2 Uppercase
                builder.Append((char)rand.Next(65, 90));
                builder.Append((char)rand.Next(65, 90));

                // 3 Lowercase
                builder.Append((char)rand.Next(97, 122));
                builder.Append((char)rand.Next(97, 122));
                builder.Append((char)rand.Next(97, 122));

                // 1 Special
                char[] spc = { ' ', '!', '"', '#', '$', '%', '&', '`', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', (char)92, ']', '^', '_', '`', '{', '|', '}', '~', (char)44, (char)39 };
                builder.Append(spc[rand.Next(spc.Length)]);

                fileBuilder.Append(new string(builder.ToString().
                OrderBy(s => (rand.Next(2) % 2) == 0).ToArray())+"\n");
                builder.Clear();
            }

            string Filedata = fileBuilder.ToString();

            //Draw the text
            graphics.DrawString(Filedata, font, PdfBrushes.Black, new PointF(0, 0));

            //Save the document
            document.Save(@"C:\Users\mihail\Documents\CreatedFiles\Output.pdf");

            //Close the document
            document.Close(true);
           
            System.Diagnostics.Process.Start(@"C:\Users\mihail\Documents\CreatedFiles\Output.pdf");
            
        }


        
    }
}
