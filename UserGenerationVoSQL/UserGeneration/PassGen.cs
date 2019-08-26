using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserGeneration
{
    class PasswordGenerator
    {
        public PasswordGenerator()
        {

        }
        public static string generatePassword(Random rand)
            {
                StringBuilder builder = new StringBuilder("", 8);
                StringBuilder passGenBuilder = new StringBuilder();
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

                passGenBuilder.Append(new string(builder.ToString().
                OrderBy(s => (rand.Next(2) % 2) == 0).ToArray()) + "\n");
                builder.Clear();


                string passGen = passGenBuilder.ToString();
                passGenBuilder.Clear();
                return passGen;

            }



        }
    }

