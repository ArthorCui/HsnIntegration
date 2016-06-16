namespace BuildUtilities
{
    using System;
    using System.IO;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public static partial class ICBuildLib
    {
        /// <summary>
        /// TODO: Update summary.
        /// </summary>
// ReSharper disable UnusedMember.Global
        public class DeleteFiles : AppDomainIsolatedTask
// ReSharper restore UnusedMember.Global
        {
            [Required]
            public string Path { get; set; }

            [Required]
            public string SearchPattern { get; set; }

            [Required]
            public bool TopDirOnly { get; set; }

            public override bool Execute()
            {
                try
                {
                    Initialize(Log);
                    var searchOption = TopDirOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
                    if (Directory.Exists(Path))
                        FileUtils.DeleteFiles(Path, SearchPattern, searchOption);
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
