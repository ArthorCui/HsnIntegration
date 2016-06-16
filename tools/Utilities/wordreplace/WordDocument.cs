using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Word;

namespace wordreplace
{
    /// <summary>
    /// Represents an MS Word document.
    /// </summary>
    class WordDocument
    {
        private Document doc;
        internal WordDocument(Document document)
        {
            doc = document;
        }

        public void Activate()
        {
            doc.Activate();
        }

        public void SaveAs(string filename)
        {
            object tempFilename = filename;
            doc.SaveAs(ref tempFilename, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing, ref WordApplication.Missing);
        }
    }
}
