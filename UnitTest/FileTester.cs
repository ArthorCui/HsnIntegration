using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace UnitTest
{
    public class FileTester
    {
        [Test]
        public void test_file_create_to_network()
        {
            var fileName = @"\\IC01\L_01_Import\BuildList_FUT9001_STB-DTH1.cd099561-a592-41d5-8127-802bf13926c5.txt";

            var record = "000091120000032505 000000000000000278340000001722";
            try
            {
                lock (fileName)
                {
                    if (!File.Exists(fileName))
                    {
                        using (StreamWriter w = File.CreateText(fileName))
                        {
                            w.WriteLine("");
                            w.WriteLine(record);
                        }
                    }
                    else
                    {
                        using (StreamWriter w = File.AppendText(fileName))
                        {
                            w.WriteLine(record);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
