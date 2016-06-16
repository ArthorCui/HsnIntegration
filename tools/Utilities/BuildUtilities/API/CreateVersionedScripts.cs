namespace BuildUtilities.API
{
// ReSharper disable UnusedMember.Global
    public static class CreateVersionedScripts
// ReSharper restore UnusedMember.Global
    {
        private static ICBuildLib.CreateVersionedScriptsPsake _createVersionedScriptsPsake;

// ReSharper disable UnusedMember.Global
        public static void CreateVersionedScriptsTask(string basePath, bool isIcMainClient, bool isIcMain, string schemaList, 
            string version)
// ReSharper restore UnusedMember.Global
        {
            _createVersionedScriptsPsake = new ICBuildLib.CreateVersionedScriptsPsake
                {
                    BasePath = basePath,
                    TFS_User = null,
                    TFS_Pass = null,
                    IsIcMainClient = isIcMainClient,
                    IsIcMain = isIcMain,
                    ProgramFiles = null,
                    SchemaList = schemaList,
                    TFExePath = null,
                    Version = version,
                    checkInStampedScripts = false,
                    VersionXmlFile = null
                };
            _createVersionedScriptsPsake.Execute();
        }
    }
}
