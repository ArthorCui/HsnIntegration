namespace BuildUtilities
{
    using System;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System.Collections.Generic;
    using ReplaceTokensLib;

    public static partial class ICBuildLib
    {
        public class ReplaceTokens : AppDomainIsolatedTask
        {
            [Required]
            public string TemplateFile { get; set; }

            [Required]
            public string OutputFile { get; set; }

            [Required]
            public ITaskItem[] Tokens { get; set; }

            [Required]
            public ITaskItem[] Values { get; set; }

            public override bool Execute()
            {
                try
                {
                    Initialize(base.Log);

                    var tokens = new Dictionary<string, string>();
                    for (var i = 0; i < Tokens.Length; i++)
                        tokens.Add(Tokens[i].ItemSpec, Values[i].ItemSpec);
                    TokenUtils.ReplaceTokens(TemplateFile, OutputFile, tokens );
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
