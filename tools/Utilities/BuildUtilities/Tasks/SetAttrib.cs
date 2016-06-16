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
        public class SetAttrib : AppDomainIsolatedTask
        {
            [Required]
            public bool ReadOnly { get; set; }

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
                    SearchOption searchOption = TopDirOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
                    if (ReadOnly)
                        FileUtils.SetReadOnly(Path, SearchPattern, searchOption);
                    else
                        FileUtils.UnsetReadOnly(Path, SearchPattern, searchOption);

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
