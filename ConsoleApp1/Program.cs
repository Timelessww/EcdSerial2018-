using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            //List<string> listtext = new List<string>();
            //string str = "A-01";
            //string str1 = "A-06";
            //string str2 = "A-03";
            //string str3 = "A-04";
            //string str4 = "A-05";
            //listtext.Add(str);
            //listtext.Add(str1);
            //listtext.Add(str2);
            //listtext.Add(str3);
            //listtext.Add(str4);
            //foreach (var item in listtext)
            //{

            //    Console.WriteLine(item);
            //}
            //Console.WriteLine("--------------------------------------");
            string str = "A-01";
            String b1 = str.Substring(0, str.IndexOf("-"));
            String b2 = str.Substring(str.IndexOf("-") + 1);
            List<string> listtext1 = new List<string>();
            for (int i = 1; i <=101; i++)
            {
                if (i < 10)
                {
                    listtext1.Add(b1 + "-" + 0 +0+ i);
                }
             else  if (i < 100)
                {
                    listtext1.Add(b1 + "-" + 0 + i);
                }
                else
                { listtext1.Add(b1 + "-" + i); }
            }

            foreach (var item in listtext1)
            {

                Console.WriteLine(item);
            }



            //string[] sArray = str.Split('-');

            //Console.WriteLine(b1);
            //Console.WriteLine(b2);
            //foreach (string e in sArray)
            //{
            //    Console.WriteLine(e);
            //}
            Console.ReadKey();

          

        }
    }
}
