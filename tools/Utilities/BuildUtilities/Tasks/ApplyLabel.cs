namespace BuildUtilities
{
    using System;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public static partial class ICBuildLib
    {
        public class ApplyLabel : AppDomainIsolatedTask
        {
            [Required]
            public string TFS_User { get; set; }

            [Required]
            public string TFS_Pass { get; set; }

            public string VersionXmlFile { get; set; }

            [Required]
            public string ProjectRoot { get; set; }

            [Required]
            public string ProgramFiles { get; set; }

            [Required]
            public string TFExePath { get; set; }

            public string Version { get; set; }

            public string Label { get; set; }

            public override bool Execute()
            {
                try
                {
                    Initialize(base.Log, this.ProgramFiles, this.TFExePath);

                    string labelText = null;
                    if (!string.IsNullOrEmpty(Version) && !string.IsNullOrEmpty(Label))
                    {
                        labelText = Label.Replace("${LastVersion}", Version);
                    }
                    else
                    {
                        string lastVersionText = FileUtils.GetValueFromVersionFile(VersionXmlFile, "//LastVersion");
                        labelText = FileUtils.GetValueFromVersionFile(VersionXmlFile, "//Label");
                        labelText = labelText.Replace("${LastVersion}", lastVersionText);
                    }
                    TFSUtils.tfsLabel(ProjectRoot, labelText, TFS_User, TFS_Pass);
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
