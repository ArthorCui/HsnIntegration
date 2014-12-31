using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Xml;

namespace Utilities.XlstScript
{
    public class XSLTStylesheetScripting
    {
        public void Transform(string fileName, string stylesheet)
        {
            //Create the XslTransform and load the style sheet.
            XslTransform xslt = new XslTransform();
            xslt.Load(stylesheet);

            //Load the XML data file.
            XPathDocument doc = new XPathDocument(fileName);

            //Create an XmlTextWriter to output to the console.
            XmlTextWriter writer = new XmlTextWriter(Console.Out);
            writer.Formatting = Formatting.Indented;

            //Transform the file.
            xslt.Transform(doc, null, writer, null);
            writer.Close();
        }
    }
}
