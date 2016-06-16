namespace BuildUtilities
{
    using System;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System.Xml.Linq;
    using System.Xml.XPath;

    public static partial class ICBuildLib
    {
        public class XmlSelect : AppDomainIsolatedTask
        {
            [Required]
            public string FileName { get; set; }

            [Required]
            public string XPath { get; set; }

            [Output]
            public string Value { get; set; }

            public override bool Execute()
            {
                try
                {
                    Initialize(base.Log);
                    Value = XDocument.Load(FileName).XPathSelectElement(XPath).Value;
                }
                catch (Exception ex)
                {
                    Log.LogErrorFromException(ex, true, true, null);
                    return false;
                }
                return true;
            }
        }
    }
}
