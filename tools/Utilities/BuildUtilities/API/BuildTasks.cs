using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReplaceTokensLib;

namespace BuildUtilities.API
{
    public static partial class BuildTasks
    {
        public static void ReplaceTokens(string templateFile, string outputFile, Dictionary<string, string> tokens)
        {
            TokenUtils.ReplaceTokens(templateFile, outputFile, tokens);
        }

        public static void CreateVersionedScripts(string basePath, string tfsUser, string tfsPass, bool isIcMainClient, bool isIcMain, 
            string programFilesPath, string schemaList, string tfExePath, string version)
        {
            var createVersionedScripts = new ICBuildLib.CreateVersionedScriptsPsake
                {
                    BasePath = basePath,
                    IsIcMain = isIcMain,
                    IsIcMainClient = isIcMainClient,
                    ProgramFiles = programFilesPath,
                    SchemaList = schemaList,
                    TFExePath = tfExePath,
                    TFS_User = tfsUser,
                    TFS_Pass = tfsPass,
                    Version = version
                };
            createVersionedScripts.Execute();
        }

        public static ICBuildLib.TFSUtils TfsUtils;
    }
}
