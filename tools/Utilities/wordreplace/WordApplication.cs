using Microsoft.Office.Interop.Word;
using System.Reflection;

namespace wordreplace
{
    /// <summary>
    /// Encapsulates basic functions of MS Word.
    /// </summary>
    class WordApplication
    {
        public static object Missing = System.Reflection.Missing.Value;
        private readonly Application _word;
        public WordApplication()
        {
            _word = new ApplicationClass();
            _word.Visible = false;
        }

        public WordDocument OpenDocument(string filename)
        {
            object tempFilename = filename;
            var doc = _word.Documents.Open(ref tempFilename, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing, ref Missing);
            return new WordDocument(doc);
        }

        public void Replace(string findText, string replaceText)
        {
            _word.Selection.HomeKey(ref Missing, ref Missing);
            object myFind = _word.Selection.Find;
            var parameters = new object[15];
            parameters[0] = findText;
            parameters[1] = Missing;
            parameters[2] = Missing;
            parameters[3] = Missing;
            parameters[4] = Missing;
            parameters[5] = Missing;
            parameters[6] = Missing;
            parameters[7] = Missing;
            parameters[8] = Missing;
            parameters[9] = replaceText;
            parameters[10] = WdReplace.wdReplaceAll;
            parameters[11] = Missing;
            parameters[12] = Missing;
            parameters[13] = Missing;
            parameters[14] = Missing;
            myFind.GetType().InvokeMember("Execute", BindingFlags.InvokeMethod, null, myFind, parameters);
        }

        public void CloseDocuments()
        {
            _word.Documents.Close(ref Missing, ref Missing, ref Missing);
        }

        public void Quit()
        {
#pragma warning disable
            _word.Quit(ref Missing, ref Missing, ref Missing);
#pragma warning enable
        }
    }
}
