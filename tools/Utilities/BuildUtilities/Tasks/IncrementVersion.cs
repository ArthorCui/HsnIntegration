namespace BuildUtilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using ReplaceTokensLib;

    public static partial class ICBuildLib
    {
        public class IncrementVersion : AppDomainIsolatedTask
        {
            [Required]
            public string TFS_User { get; set; }

            [Required]
            public string TFS_Pass { get; set; }

            [Required]
            public string VersionXmlFile { get; set; }

            [Required]
            public string VersionCsFile { get; set; }

            [Required]
            public string ProjectRoot { get; set; }

            [Required]
            public string ProgramFiles { get; set; }

            [Required]
            public string TFExePath { get; set; }

            public string Version { get; set; }

            private string configFile = null;
            private string projectName = null;
            private string newVersion = null;

            public override bool Execute()
            {
                try
                {
                    Initialize(base.Log, this.ProgramFiles, this.TFExePath);
                    checkoutVersionFiles();
                    incrementVersion();
                    checkinVersionfiles();
                }
                catch (Exception ex)
                {
                    Log.LogErrorFromException(ex, true, true, null);
                    return false;
                }
                return true;
            }

            private void checkoutVersionFiles()
            {
                if (string.IsNullOrEmpty(Version))
                    TFSUtils.tfsGet(VersionXmlFile, TFS_User, TFS_Pass);
                TFSUtils.tfsGet(VersionCsFile, TFS_User, TFS_Pass);
                if (string.IsNullOrEmpty(Version))
                    TFSUtils.tfsCheckout(VersionXmlFile, TFS_User, TFS_Pass);
                TFSUtils.tfsCheckout(VersionCsFile, TFS_User, TFS_Pass);

                // TODO: Remove Telenor-specific logic
                // For Telenor builds only, check out config files
                if (VersionXmlFile.Contains("Telenor"))
                {
                    configFile = ProjectRoot + @"\Applications\WindowsService\App.config";
                    TFSUtils.tfsGet(configFile, TFS_User, TFS_Pass);
                    TFSUtils.tfsCheckout(configFile, TFS_User, TFS_Pass);
                }
            }

            private void checkinVersionfiles()
            {
                string tfscheckincomment;
                if (string.IsNullOrEmpty(Version))
                {
                    tfscheckincomment =
                        string.Format("IncrementVersion script: Incremented {0} IC version to {1}", projectName,
                                      newVersion);
                    TFSUtils.tfsCheckin(VersionXmlFile, tfscheckincomment, TFS_User, TFS_Pass);
                }

                tfscheckincomment = string.Format("IncrementVersion script: Updated {0} IC version.cs to {1}", projectName, newVersion);
                TFSUtils.tfsCheckin(VersionCsFile, tfscheckincomment, TFS_User, TFS_Pass);

                // TODO: Remove Telenor-specific logic
                // For Telenor builds only, check in config files
                if (VersionXmlFile.Contains("Telenor"))
                {
                    tfscheckincomment = string.Format("IncrementVersion script: Updated {0} {1} to {2}", projectName, configFile.Substring(configFile.LastIndexOf('\\') + 1), newVersion);
                    TFSUtils.tfsCheckin(configFile, tfscheckincomment, TFS_User, TFS_Pass);
                }
            }

            private void incrementVersion()
            {
                if (!File.Exists(VersionXmlFile))
                    throw new Exception(string.Format("Unable to find version xml file '{0}'.", VersionXmlFile));

                projectName = FileUtils.GetValueFromVersionFile(VersionXmlFile, "//ProjectName");
                var incrementText = FileUtils.GetValueFromVersionFile(VersionXmlFile, "//Increment");
                var lastVersionText = FileUtils.GetValueFromVersionFile(VersionXmlFile, "//LastVersion");
                if (!string.IsNullOrEmpty(Version))
                    lastVersionText = Version;
                var versionElements = lastVersionText.Split(new char[] { '.' });
                var newmajor = int.Parse(versionElements[0]);
                var newminor = int.Parse(versionElements[1]);
                var newrevision = int.Parse(versionElements[2]);
                var newbuild = int.Parse(versionElements[3]);

                if (string.IsNullOrEmpty(Version))
                {
                    switch (incrementText.ToLower())
                    {
                        case "major":
                            newmajor++;
                            break;
                        case "minor":
                            newminor++;
                            break;
                        case "revision":
                            newrevision++;
                            break;
                        case "build":
                            newbuild++;
                            break;
                    }
                }

                newVersion = string.Format("{0}.{1}.{2}.{3}", newmajor, newminor, newrevision, newbuild);
                if (string.IsNullOrEmpty(Version))
                    FileUtils.UpdateValueInVersionFile(VersionXmlFile, "//LastVersion", newVersion);

                var lines = File.ReadAllLines(VersionCsFile);
                string newAssemblyFileVersion = null;

                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("[assembly: AssemblyVersion"))
                    {
                        string newAssemblyVersion;
                        string assemblyVersion;
                        GetAssemblyVersionInfo(lines[i], newmajor, newminor, newrevision, newbuild, out assemblyVersion, out newAssemblyVersion);
                        lines[i] = lines[i].Replace(assemblyVersion, newAssemblyVersion);
                    }
                    else if (lines[i].StartsWith("[assembly: AssemblyFileVersion"))
                    {
                        string assemblyFileVersion;
                        GetAssemblyVersionInfo(lines[i], newmajor, newminor, newrevision, newbuild, out assemblyFileVersion, out newAssemblyFileVersion);
                        lines[i] = lines[i].Replace(assemblyFileVersion, newAssemblyFileVersion);
                    }
                }

                File.WriteAllLines(VersionCsFile, lines);

                // TODO: Remove Telenor-specific logic
                // If Telenor build, update config file
                if (VersionXmlFile.Contains("Telenor"))
                {
                    var template = configFile + ".template";
                    var tokens = new Dictionary<string, string>();
                    tokens.Add("@@VERSION@@", newAssemblyFileVersion);
                    TokenUtils.ReplaceTokens(template, configFile, tokens);
                }
            }

            private void GetAssemblyVersionInfo(string line, int newmajor, int newminor, int newrevision, int newbuild, out string assemblyVersion, out string newAssemblyVersion)
            {
                assemblyVersion = line.Split('"')[1];
                var assemblyVersionMajor = assemblyVersion.Split('.')[0];
                var assemblyVersionMinor = assemblyVersion.Split('.')[1];
                var assemblyVersionRevision = assemblyVersion.Split('.')[2];
                var assemblyVersionBuild = assemblyVersion.Split('.')[3];
                if (assemblyVersionMajor != "*")
                    assemblyVersionMajor = newmajor.ToString();
                if (assemblyVersionMinor != "*")
                    assemblyVersionMinor = newminor.ToString();
                if (assemblyVersionRevision != "*")
                    assemblyVersionRevision = newrevision.ToString();
                if (assemblyVersionBuild != "*")
                    assemblyVersionBuild = newbuild.ToString();
                newAssemblyVersion = string.Format("{0}.{1}.{2}.{3}", assemblyVersionMajor, assemblyVersionMinor, assemblyVersionRevision, assemblyVersionBuild);
            }
        }
    }
}
