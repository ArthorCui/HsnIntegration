// ReSharper disable CheckNamespace
namespace BuildUtilities
// ReSharper restore CheckNamespace
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;

// ReSharper disable ClassNeverInstantiated.Global
    public static partial class ICBuildLib
// ReSharper restore ClassNeverInstantiated.Global
    {
// ReSharper disable UnusedMember.Global
        public class DeleteDirectory : AppDomainIsolatedTask
// ReSharper restore UnusedMember.Global
        {
            [Required]
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
            public string Path { get; set; }
// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global

            public override bool Execute()
            {
                try
                {
                    Initialize(Log);
                    FileUtils.DeleteDir(Path);
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
