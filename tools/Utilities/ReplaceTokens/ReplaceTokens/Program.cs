using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommandLine.OptParse;
using ReplaceTokensLib;

namespace ReplaceTokens
{
    class Program
    {
        private static string usage =
@"Usage: ReplaceTokens.exe [options]

Options:
-h --help                                                              Show this help message and exit
-m MACHINE_ANSWER_FILE, --machine-answer-file=MACHINE_ANSWER_FILE.xml  Path to answer file
-t TEMPLATE_FILE, --template-file=configfile.xml.template              Path to template file
-o OUTPUT_FILE, --output-file=configfile.xml                           Path to output file

Return codes:
  SUCCESS = 0
  ERROR = 1

If the return code is 1, a description of the error will be printed out.";

        static void Main(string[] args)
        {
            string[] testargs = new string[] { 
                @"-m C:\AnswerFile\MACHINE_ANSWER_FILE_MI.xml", 
                @"-t C:\Integration\Framework2.0\Framework\Trunk\Integration.MassImport\Integration.MassImport.exe.config.template", 
                @"-o C:\Integration\Framework2.0\Framework\Trunk\Integration.MassImport\Integration.MassImport.exe.config" 
            };

            //Uncomment for testing
            //args = testargs;

            if (args.Length < 3)
            {
                Console.WriteLine(usage);
                Environment.Exit(1);
            }
            Arguments arguments = new Arguments();
            Parser p = ParserFactory.BuildParser(arguments);
            p.OptStyle = OptStyle.Unix;
            // Parse the args
            p.Parse(args);
            try
            {
                List<string> tokensNotInAnswerFile = new List<string>();
                TokenUtils.ReplaceTokens(arguments.MachineAnswerFile, arguments.TemplateFile, arguments.OutputFile, out tokensNotInAnswerFile);
                Console.WriteLine("Token replacement completed successfully.\r\nAnswer file: {0}\r\nTemplate file: {1}\r\nOutput file: {2}",
                    arguments.MachineAnswerFile, arguments.TemplateFile, arguments.OutputFile);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                string message = string.Format("An error occurred while performing the token replacement.Please check the below information to fix the error.\r\n\r\n1) Answer file: {0}\r\n2)  Template file: {1}\r\n3)  Output file: {2}\r\n4)  Error message: {3}",
                    arguments.MachineAnswerFile, arguments.TemplateFile, arguments.OutputFile, ex.Message);
                Console.WriteLine(message);
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }
    }

    public class Arguments
    {
        public string MachineAnswerFile;
        public string TemplateFile;
        public string OutputFile;


        [ShortOptionName('h')]
        [LongOptionName("help")]
        [UseNameAsLongOption(false)]
        [OptDef(OptValType.Flag)]
        [Category("Help")]
        [Description("Show this help message and exit")]
        public string Help;

        [ShortOptionName('m')]
        [LongOptionName("machine-answer-file")]
        [UseNameAsLongOption(false)]
        [OptDef(OptValType.ValueReq)]
        [Category("Files")]
        [Description("")]
        public string MachineAnswerFileUnparsed
        {
            set
            {
                this.MachineAnswerFile = value;
            }
            get
            {
                return "";
            }
        }

        [ShortOptionName('t')]
        [LongOptionName("template-file")]
        [UseNameAsLongOption(false)]
        [OptDef(OptValType.ValueReq)]
        [Category("Files")]
        [Description("")]
        public string TemplateFileUnparsed
        {
            set
            {
                this.TemplateFile = value;
            }
            get
            {
                return "";
            }
        }

        [ShortOptionName('o')]
        [LongOptionName("output-file")]
        [UseNameAsLongOption(false)]
        [OptDef(OptValType.ValueReq)]
        [Category("Files")]
        [Description("")]
        public string OutputFileUnparsed
        {
            set
            {
                this.OutputFile = value;
            }
            get
            {
                return "";
            }
        }
    }
}
