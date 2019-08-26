using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Web;

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
            StringBuilder nameBuilder = new StringBuilder();
            StringBuilder uNameBuilder = new StringBuilder();
            StringBuilder passBuilder = new StringBuilder();
            // Read the file 
            System.IO.StreamReader usersReader =
                new System.IO.StreamReader(@"C:\Users\mihail\Documents\ReadngFrom\Users.txt");
            System.Data.SqlClient.SqlConnection sqlConnection1 =
                new System.Data.SqlClient.SqlConnection(@"Data Source=MIHAIL-PC\SQL2008R2;Initial Catalog=UserGeneration;Integrated Security=True");


            while ((line = usersReader.ReadLine()) != null)
            {

                names = line.Split(null);
                // Username has Username with first letter of name + surname
                username = names[0][0] + names[1];
                // Building the Name of the user
                nameBuilder.Append(line);
                // Building the Username
                uNameBuilder.Append(username);
                // Generated password from PasswordGenerator class
                passBuilder.Append(PasswordGenerator.generatePassword(rand));
                               
                // Assigning attributes for clarity
                int ID = i;
                string UN = uNameBuilder.ToString();
                string CN = nameBuilder.ToString();
                string PW = passBuilder.ToString();
                

                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "INSERT Users (PersonID, CompleteName, Username, Passwrd)";
                cmd.CommandText += "VALUES (@ID, @CN, @UN, @PW)";
                cmd.Connection = sqlConnection1;

                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.Parameters.AddWithValue("@CN", CN);
                cmd.Parameters.AddWithValue("@UN", UN);
                cmd.Parameters.AddWithValue("@PW", PW);

                // Opening connection, entering the data from the command and then closing it
                sqlConnection1.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                { }
                

                sqlConnection1.Close();

                i++;
                // Clearing the builders after creating a user
                nameBuilder.Clear();
                uNameBuilder.Clear();
                passBuilder.Clear();


                
            }

            usersReader.Close();

            ReportGen.GenerateReport();
            
        }
        }
    }
