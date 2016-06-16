namespace BuildUtilities
{
    using System;
    using System.IO;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public static partial class ICBuildLib
    {
        public class CreateBranch : AppDomainIsolatedTask
        {
            [Required]
            public string TFS_User { get; set; }

            [Required]
            public string TFS_Pass { get; set; }

            [Required]
            public string VersionXmlFile { get; set; }

            [Required]
            public string ProjectRoot { get; set; }

            [Required]
            public string IntegrationRoot { get; set; }

            [Required]
            public string ProgramFiles { get; set; }

            [Required]
            public string TFExePath { get; set; }

            public string Version { get; set; }

            public override bool Execute()
            {
                try
                {
                    Initialize(base.Log, this.ProgramFiles, this.TFExePath);

                    string lastVersionText = Version ?? FileUtils.GetValueFromVersionFile(VersionXmlFile, "//LastVersion");
                    string labelText = FileUtils.GetValueFromVersionFile(VersionXmlFile, "//Label").Replace("${LastVersion}", lastVersionText);
                    string targetBranchText = FileUtils.GetValueFromVersionFile(VersionXmlFile, "//TargetTFSBranchPath").Replace("${LastVersion}", lastVersionText)
                        .Replace("${Year}", DateTime.Now.Year.ToString());
                    string serverBranchText = FileUtils.GetValueFromVersionFile(VersionXmlFile, "//LocalBranchPath").Replace("${LastVersion}", lastVersionText)
                        .Replace("${Year}", DateTime.Now.Year.ToString()).Replace("${Integration_Root}", IntegrationRoot);
                    bool isICMainBuild = false;
                    string isICMainBuildValue = FileUtils.GetValueFromVersionFile(VersionXmlFile, "//IsICMainBuild");
                    if (!string.IsNullOrEmpty(isICMainBuildValue))
                    {
                        switch (isICMainBuildValue)
                        {
                            case "0":
                                isICMainBuild = false;
                                break;
                            case "1":
                                isICMainBuild = true;
                                break;
                            default:
                                isICMainBuild = bool.Parse(isICMainBuildValue);
                                break;
                        }
                    }
                    string buildsFolderText = FileUtils.GetValueFromVersionFile(VersionXmlFile, "//BuildsFolder").Replace("${Integration_Root}", IntegrationRoot);

                    string[] schemas = new string[] { "CI_CENTRAL", "CI_BUSINESSDATA", "CI_QUEUEDATA" };

                    string tfscheckincomment = string.Format("CreateBranch script: Creating branch from label '{0}'", labelText);

                    // Decloak build folders
                    foreach (string folder in TFSUtils.tfsDir(buildsFolderText, TFS_User, TFS_Pass))
                        TFSUtils.tfsDecloak(string.Format(@"{0}\{1}", buildsFolderText, folder.Split(new char[] { '$' })[1]), TFS_User, TFS_Pass);

                    if (isICMainBuild)
                    {
                        // Generate full padded IC main version string
                        string[] versionparts = lastVersionText.Split(new char[] { '.' });
                        string icmainversion = string.Format("{0}.{1}.{2}.{3}", versionparts[0].PadLeft(2, '0'), versionparts[1].PadLeft(2, '0'),
                            versionparts[2].PadLeft(2, '0'), versionparts[3].PadLeft(2, '0'));

                        // Actually create the branch
                        TFSUtils.tfsBranch(labelText, ProjectRoot, targetBranchText, true, TFS_User, TFS_Pass);

                        string[] icMainSubDirs = new string[] {"Schemas", "es", "Oracle DLL", "Templates",
                            @"Transformers\ConditionalAccess\Common", @"Transformers\ConditionalAccess\Nagra\Aladin.1.4",
                            @"Transformers\ConditionalAccess\Nagra\Common", @"Transformers\ConditionalAccess\Nagra\Merlin.2.1",
                            @"Transformers\ConditionalAccess\Control.6.14", @"Transformers\ConditionalAccess\Control.6.14\Common"};

                        // Create the IC Main DLL folders
                        foreach (string icMainSubDir in icMainSubDirs)
                            Directory.CreateDirectory(string.Format(@"{0}\IC Main DLL\{1}", serverBranchText, icMainSubDir));

                        // Copy the IC Main DLLs over
                        FileUtils.CopyDirectory(string.Format(@"{0}\IC Main DLL", ProjectRoot), string.Format(@"{0}\IC Main DLL", serverBranchText), true);

                        foreach (string schema in schemas)
                        {
                            // Create the IC Main Scripts folders
                            Directory.CreateDirectory(string.Format(@"{0}\IC Main Scripts\{1}\{2}", serverBranchText, schema, icmainversion));

                            // Move the DatabaseObjects folders
                            Directory.Move(string.Format(@"{0}\CreateCI_DB\{1}\UpdateScripts\{2}\DatabaseObjects", ProjectRoot, schema, icmainversion),
                                string.Format(@"{0}\IC Main Scripts\{1}\{2}\DatabaseObjects", serverBranchText, schema, icmainversion));

                            // Copy ad-hoc scripts
                            FileUtils.CopyDirectory(string.Format(@"{0}\CreateCI_DB\{1}\UpdateScripts", ProjectRoot, schema),
                                string.Format(@"{0}\IC Main Scripts\{1}\{2}", serverBranchText, schema, icmainversion), true);

                            // Delete ad-hoc scripts from TFS
                            TFSUtils.tfsDelete(string.Format(@"{0}\CreateCI_DB\{1}\UpdateScripts\*.*", ProjectRoot, schema), TFS_User, TFS_Pass);

                            // Delete intermediary files
                            Directory.Delete(string.Format(@"{0}\CreateCI_DB\{1}\UpdateScripts\{2}", ProjectRoot, schema, icmainversion), true);
                        }

                        // Add branch DLLs and scripts to TFS
                        TFSUtils.tfsAdd(string.Format(@"{0}\IC Main DLL", serverBranchText), TFS_User, TFS_Pass);
                        TFSUtils.tfsAdd(string.Format(@"{0}\IC Main Scripts", serverBranchText), TFS_User, TFS_Pass);

                        // Check in files
                        TFSUtils.tfsCheckin(string.Format(@"{0}\CreateCI_DB", ProjectRoot), "CreateBranch script: Deleting IC Main update scripts", TFS_User, TFS_Pass);
                    }
                    else
                    {
                        TFSUtils.tfsBranch(labelText, ProjectRoot, targetBranchText, false, TFS_User, TFS_Pass);
                    }

                    TFSUtils.tfsCheckin(serverBranchText, tfscheckincomment, TFS_User, TFS_Pass);

                    // Cloak builds folders
                    foreach (string folder in TFSUtils.tfsDir(buildsFolderText, TFS_User, TFS_Pass))
                        TFSUtils.tfsCloak(string.Format(@"{0}\{1}", buildsFolderText, folder), TFS_User, TFS_Pass);
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
