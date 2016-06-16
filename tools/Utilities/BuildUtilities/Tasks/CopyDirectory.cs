namespace BuildUtilities
{
    using System;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public static partial class ICBuildLib
    {
        /// <summary>
        /// TODO: Update summary.
        /// </summary>
        public class CopyDirectory : AppDomainIsolatedTask
        {
            [Required]
            public string SourceDirName { get; set; }

            [Required]
            public string DestDirName { get; set; }

            [Required]
            public bool CopySubDirs { get; set; }

            public override bool Execute()
            {
                try
                {
                    Initialize(Log);
                    FileUtils.CopyDirectory(SourceDirName, DestDirName, CopySubDirs);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogErrorFromException(ex, true, true, null);
                    return false;
                }
            }
        }
    }
}
