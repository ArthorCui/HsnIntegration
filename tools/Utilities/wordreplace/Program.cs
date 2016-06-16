using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace wordreplace
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                ShowUsage();
                return;
            }

            WordApplication word = null;
            try
            {
                string templateFilename = args[0];
                string outputFilename = args[1];
                string tokens = args[2];
                string values = args[3];
                string[] tokenList = tokens.Split('|');
                string[] valueList = values.Split('|');
                if (tokenList.Length != valueList.Length)
                    throw new Exception("The token list and value list do not contain the same number of items.");
                
                if (File.Exists(outputFilename))
                    File.Delete(outputFilename);

                word = new WordApplication();
                WordDocument doc = word.OpenDocument(templateFilename);
                doc.Activate();

                for (int i = 0; i < tokenList.Length; i++)
                {
                    word.Replace(tokenList[i], valueList[i]);
                }

                doc.SaveAs(outputFilename);
                word.CloseDocuments();
                Console.WriteLine("Token replacement completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
                Environment.Exit(1);
            }
            finally
            {
                if (word != null)
                    word.Quit();
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine(@"WordReplace.  Replaces tokens in a Microsoft Word document.

Usage:
wordreplace <template filename> <output filename> <tokens> <values>
template filename      The name of the Word document containing tokens.
output filename        The name of the resulting Word document containing replacements.
tokens                 A pipe (|) delimited list of tokens to replace.
value                  A pipe (|) delimited list of values for the corresponding tokens.

Notes:
Filenames specified must contain the absolute path to the file.

Example:
wordreplace ""C:\test_template.doc"" ""C:\test.doc"" ""${Version}|${Date}"" ""62.11.0.0|2009-12-22""
");
        }
    }
}
