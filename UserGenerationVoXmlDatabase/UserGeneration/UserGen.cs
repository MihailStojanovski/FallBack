using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

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
            if (args.Length == 0 || int.Parse(args[0]) == 0)
            {
                XmlTextWriter xtw;
                xtw = new XmlTextWriter(@"C:\Users\mihail\Documents\CreatedFiles\Xml\UserNameAndPassword.xml", Encoding.UTF8);
                xtw.WriteStartDocument();
                xtw.WriteStartElement("User");
                xtw.WriteEndElement();
                xtw.Close();
            }


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

                if (args.Length == 0 || int.Parse(args[0]) == 0)
                {
                    XmlDocument xd = new XmlDocument();
                    FileStream lfile = new FileStream(@"C:\Users\mihail\Documents\CreatedFiles\Xml\UserNameAndPassword.xml", FileMode.Open);
                    xd.Load(lfile);
                    XmlElement user = xd.CreateElement("User");
                    user.SetAttribute("Name", nameBuilder.ToString());

                    XmlElement uName = xd.CreateElement("Username");
                    XmlText uNameText = xd.CreateTextNode(uNameBuilder.ToString());
                    uName.AppendChild(uNameText);
                    user.AppendChild(uName);

                    XmlElement pass = xd.CreateElement("Password");
                    XmlText passText = xd.CreateTextNode(passBuilder.ToString());
                    pass.AppendChild(passText);
                    user.AppendChild(pass);

                    xd.DocumentElement.AppendChild(user);

                    lfile.Close();
                    xd.Save(@"C:\Users\mihail\Documents\CreatedFiles\Xml\UserNameAndPassword.xml");
                }

                if (args.Length > 0 || int.Parse(args[0]) == 1)
                {
                    XmlTextWriter xtw;
                    xtw = new XmlTextWriter(@"C:\Users\mihail\Documents\CreatedFiles\Xml\UserNameAndPassword"+i+".xml", Encoding.UTF8);
                    xtw.WriteStartDocument();
                    xtw.WriteStartElement("User");
                    xtw.WriteEndElement();
                    xtw.Close();
                    XmlDocument xd = new XmlDocument();
                    FileStream lfile = new FileStream(@"C:\Users\mihail\Documents\CreatedFiles\Xml\UserNameAndPassword"+i+".xml", FileMode.Open);
                    xd.Load(lfile);
                    XmlElement user = xd.CreateElement("User");
                    user.SetAttribute("Name", nameBuilder.ToString());

                    XmlElement uName = xd.CreateElement("Username");
                    XmlText uNameText = xd.CreateTextNode(uNameBuilder.ToString());
                    uName.AppendChild(uNameText);
                    user.AppendChild(uName);

                    XmlElement pass = xd.CreateElement("Password");
                    XmlText passText = xd.CreateTextNode(passBuilder.ToString());
                    pass.AppendChild(passText);
                    user.AppendChild(pass);

                    xd.DocumentElement.AppendChild(user);

                    lfile.Close();
                    xd.Save(@"C:\Users\mihail\Documents\CreatedFiles\Xml\UserNameAndPassword"+i+".xml");
                    i++;
                }

                // Clearing the builders after creating a user
                nameBuilder.Clear();
                uNameBuilder.Clear();
                passBuilder.Clear();


            }

                usersReader.Close();
                

            }
        }
    }
