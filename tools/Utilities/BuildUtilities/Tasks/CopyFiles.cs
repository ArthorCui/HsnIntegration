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
        public class CopyFiles : AppDomainIsolatedTask
        {
            [Required]
            public string Path { get; set; }

            [Required]
            public string SearchPattern { get; set; }

            [Required]
            public string DestDirName { get; set; }

            public override bool Execute()
            {
                try
                {
                    Initialize(Log);
                    FileUtils.CopyFiles(Path, SearchPattern, DestDirName);
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
