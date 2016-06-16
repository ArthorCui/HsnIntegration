// ReSharper disable CheckNamespace
namespace BuildUtilities
// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Build.Framework;
    using ReplaceTokensLib;

// ReSharper disable ClassNeverInstantiated.Global
    public static partial class ICBuildLib
// ReSharper restore ClassNeverInstantiated.Global
    {
// ReSharper disable UnusedMember.Global
        /// <summary>
        /// MSBuild-specific class
        /// </summary>
        public class CreateVersionedScripts : CreateVersionedScriptsPsake, ITask
// ReSharper restore UnusedMember.Global
        {
            public IBuildEngine BuildEngine { get; set; }
            public ITaskHost HostObject { get; set; }
        }

        /// <summary>
        /// Used by Psake
        /// </summary>
        public class CreateVersionedScriptsPsake 
        {
            public string TFS_User { get; set; }

            public string TFS_Pass { get; set; }

            public virtual string VersionXmlFile { get; set; }

            [Required]
            public string BasePath { get; set; }

            [Required]
            public string SchemaList { get; set; }

            [Required]
            public bool IsIcMainClient { get; set; }

            [Required]
            public bool IsIcMain { get; set; }

            public string ProgramFiles { get; set; }

            public string TFExePath { get; set; }

            public bool checkInStampedScripts = true;

            public string Version { get; set; }

            private string newDBVersion;

            public bool Execute()
            {
                Initialize(null, ProgramFiles, TFExePath);

                LogMsg(MessageImportance.High, "Starting Execute() method.");
                LogMsg(MessageImportance.High, @"TFS_User: {0}
TFS_Pass: {1}
VersionXmlFile: {2}
BasePath: {3}
SchemaList: {4}
IsIcMainClient: {5}
IsIcMain: {6}
ProgramFiles: {7}
Version: {8}", TFS_User, TFS_Pass, VersionXmlFile, BasePath, SchemaList, IsIcMainClient, IsIcMain, ProgramFiles, Version);

                newDBVersion = string.IsNullOrEmpty(Version)
                                   ? FileUtils.GetValueFromVersionFile(VersionXmlFile, "//LastVersion")
                                   : Version;
                var schemas = SchemaList.Split(',');
                var specialschemas = new[] {"_PreProcess", "SMP", "zzPostProcess"};
                var databaseobjectfolders = new[]
                    {
                        "ForeignKeys", "Functions", "Packages", "Procedures", "Triggers", "Views", "Tables", "Sequences",
                        "Types", "Jobs"
                    };
                var newICMainVersion = "";

                if (IsIcMain)
                {
                    var versionparts = newDBVersion.Split('.');
                    newICMainVersion = string.Format("{0}.{1}.{2}.{3}", versionparts[0].PadLeft(2, '0'),
                                                     versionparts[1].PadLeft(2, '0'),
                                                     versionparts[2].PadLeft(2, '0'), versionparts[3].PadLeft(2, '0'));
                }
                if (!IsIcMain)
                    CreateClientVersionedFoldersRootDirectories(specialschemas);

                foreach (var schema in schemas)
                {
                    if (IsIcMain)
                        CreateICMainVersionedFolders(schema, databaseobjectfolders, newICMainVersion);
                    else
                    {
                        CreateClientVersionedFolders(schema, specialschemas);

                        if (checkInStampedScripts)
                            CopyApprovedScripts(schema, specialschemas);

                        CreateBlankFiles(schema, specialschemas);
                    }

                    CopyDatabaseObjects(databaseobjectfolders, schema, newICMainVersion);

                    if (IsIcMainClient)
                        CopyICMainClientScripts(schema, databaseobjectfolders);
                }

                if (!IsIcMain)
                    CheckInClientScripts(schemas, specialschemas);

                return true;
            }

            private void CreateClientVersionedFoldersRootDirectories(IEnumerable<string> specialschemas)
            {
                LogMsg(MessageImportance.High, "CreateClientVersionedFoldersRootDirectories() called.");
                Directory.CreateDirectory(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}", BasePath, newDBVersion));
                foreach (var schema in specialschemas)
                    Directory.CreateDirectory(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}", BasePath, schema));
            }

            private void CreateBlankFiles(string schema, IEnumerable<string> specialschemas)
            {
                LogMsg(MessageImportance.High, "CreateBlankFiles() called.");

                // Make sure at least one .SQL file exists for each schema
                if (Directory.GetFiles(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}", BasePath, newDBVersion, schema), "*.sql", SearchOption.TopDirectoryOnly).Length == 0)
                {
                    LogMsg(MessageImportance.High, "Creating blank.sql.");
                    var blankFilePath = string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}\blank.sql", BasePath, newDBVersion, schema);
                    var fi = File.Create(blankFilePath);
                    fi.Close();
                }

                specialschemas.ToList().Where(specialschema => 
                    Directory.GetFiles(
                        string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}", BasePath, specialschema, schema), 
                            "*.sql", SearchOption.TopDirectoryOnly).Length == 0)
                        .ToList().ForEach(s => File.Create(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}\blank.sql", BasePath, s, schema)).Close());
            }

            protected virtual void CopyApprovedScripts(string schema, string[] specialschemas)
            {
                LogMsg(MessageImportance.High, "CopyApprovedScripts() called.");

                // Copy approved SQL scripts into today's build folders
                FileUtils.CopyFiles(string.Format(@"{0}\{1}\UpdateScripts\Approved", BasePath, schema), "*.approved", string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}\", BasePath, newDBVersion, schema));
                specialschemas.ToList().ForEach(specialschema => FileUtils.CopyFiles(string.Format(@"{0}\{1}\Data\{2}\Approved", BasePath, schema, 
                    specialschema), "*.approved", string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}\", BasePath, specialschema, schema)));

                // Rename .Approved to .SQL
                FileUtils.MoveFiles(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}", BasePath, newDBVersion, schema), "*.approved", string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}", BasePath, newDBVersion, schema), ".SQL");
                specialschemas.ToList().ForEach(specialschema => FileUtils.MoveFiles(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}", BasePath, 
                    specialschema, schema), "*.approved", string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}", BasePath, specialschema, schema), ".SQL"));
            }

            private void CreateClientVersionedFolders(string schema, IEnumerable<string> specialschemas)
            {
                LogMsg(MessageImportance.High, "CreateClientVersionedFolders() called.");

                // Create versioned schema folders for today's build
                Directory.CreateDirectory(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}", BasePath, newDBVersion, schema));
                specialschemas.ToList().ForEach(specialschema => Directory.CreateDirectory(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}", 
                    BasePath, specialschema, schema)));
            }

            private void CreateICMainVersionedFolders(string schema, IEnumerable<string> databaseobjectfolders, string newICMainVersion)
            {
                LogMsg(MessageImportance.High, "CreateICMainVersionedFolders() called.");

                // Create versioned schema folders for today's build
                Directory.CreateDirectory(string.Format(@"{0}\{1}\UpdateScripts\{2}\DatabaseObjects", BasePath, schema, newICMainVersion));
                databaseobjectfolders.ToList().ForEach(dbobjectfolder => Directory.CreateDirectory(
                    string.Format(@"{0}\{1}\UpdateScripts\{2}\DatabaseObjects\{3}", BasePath, schema, newICMainVersion, dbobjectfolder)));
            }

            private static string GetLogMessage(string listName, IEnumerable<string> list)
            {
                var logMessage = string.Format("Contents of {0}:\r\n", listName);
                logMessage += list.Aggregate("", (current, value) => current + Environment.NewLine + value);
                return logMessage;
            }

            private static string GetLogMessage(string dictName, IEnumerable<KeyValuePair<string, string>> dict)
            {
                var logMessage = string.Format("Contents of {0}:\r\n", dictName);
                logMessage += dict.Aggregate("", (current, keypair) => current + Environment.NewLine + string.Format("{0} = {1}", keypair.Key, keypair.Value));
                return logMessage;
            }

            private void CopyDatabaseObjects(IEnumerable<string> databaseobjectfolders, string schema, string newICMainVersion)
            {
                LogMsg(MessageImportance.High, "CopyDatabaseObjects() called.");

                // Copy database objects
                foreach (var databaseobjectfolder in databaseobjectfolders)
                {
                    // determine the dbscript folder
                    var dbscriptpath = string.Format(@"{0}\{1}\DatabaseObjects\{2}", BasePath, schema, databaseobjectfolder);

                    if (checkInStampedScripts)
                        // check out everything in the dbscript folder
                        TFSUtils.tfsCheckout(dbscriptpath, TFS_User, TFS_Pass);

                    // Determine output folder
                    var destscriptfolder = IsIcMain ? string.Format(@"{0}\{1}\UpdateScripts\{2}\DatabaseObjects\{3}", BasePath, schema, newICMainVersion, databaseobjectfolder) :
                        string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\_PreProcess\{1}", BasePath, schema);

                    if (checkInStampedScripts)
                        // check out everything in the destination folder.
                        TFSUtils.tfsCheckout(destscriptfolder, TFS_User, TFS_Pass);

                    LogMsg("Inside CopyDatabaseObjects(), checking if path '{0}' exists.", dbscriptpath);
                    if (Directory.Exists(dbscriptpath))
                    {
                        LogMsg("Inside CopyDatabaseObjects(), directory exists.  Looping through SQL files in this directory.");

                        var dbscripts = Directory.GetFiles(dbscriptpath, "*.sql", SearchOption.TopDirectoryOnly);

                        LogMsg(MessageImportance.High, GetLogMessage("dbscripts", dbscripts));

                        foreach (var dbscript in dbscripts)
                        {
                            LogMsg("Inside CopyDatabaseObjects().  Adding comments to script '{0}'.", dbscript);

                            // Add comments to script
                            var tokens = AddComments(dbscript);

                            var scriptpathparts = dbscript.Split('\\');
                            var destscriptpath = string.Format(@"{0}\{1}", destscriptfolder, scriptpathparts[scriptpathparts.Length - 1]);

                            var tokenString = tokens.Aggregate("", (current, token) => current + string.Format("key: {0}; value={1}\r\n", token.Key, token.Value));

                            LogMsg("Inside CopyDatabaseObjects().  Calling ReplaceTokens() with dbscript '{0}', destscriptpath '{1}', tokens '{2}'.", dbscript, destscriptpath, tokenString);

                            if (!checkInStampedScripts)
                                FileUtils.UnsetReadOnly(destscriptfolder, Path.GetFileName(destscriptpath), SearchOption.TopDirectoryOnly);

                            // Perform token replacement on script and copy to destination folder
                            TokenUtils.ReplaceTokens(dbscript, destscriptpath, tokens);
                        }
                    }

                    if (checkInStampedScripts)
                        // Undo original script (template)
                        TFSUtils.tfsUndo(dbscriptpath, TFS_User, TFS_Pass);
                }
            }

            protected virtual void CheckInClientScripts(string[] schemas, string[] specialschemas)
            {
                if (!checkInStampedScripts) return;

                LogMsg(MessageImportance.High, "CheckInClientScripts() called.");

                // TFS Add Files to installer area
                TFSUtils.tfsAdd(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\*.*", BasePath, newDBVersion), TFS_User, TFS_Pass);
                foreach (var specialschema in specialschemas)
                    foreach (var schema in schemas)
                        TFSUtils.tfsAdd(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}\*.sql", BasePath, specialschema, schema), TFS_User, TFS_Pass);

                TFSUtils.tfsCheckin(string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts", BasePath), "CreateAll script: Checking in moved scripts", TFS_User, TFS_Pass);

                foreach (var schema in schemas)
                {
                    // Remove old SQL files from TFS
                    TFSUtils.tfsDelete(string.Format(@"{0}\{1}\UpdateScripts\Approved\*.approved", BasePath, schema), TFS_User, TFS_Pass);
                    foreach (var specialschema in specialschemas)
                        TFSUtils.tfsDelete(string.Format(@"{0}\{1}\Data\{2}\Approved\*.approved", BasePath, schema, specialschema), TFS_User, TFS_Pass);
                }

                TFSUtils.tfsCheckin(BasePath, "CreateAll script: Deleting old approved scripts", TFS_User, TFS_Pass);
            }

            private void CopyICMainClientScripts(string schema, string[] databaseobjectfolders)
            {
                LogMsg(MessageImportance.High, "CopyICMainClientScripts() called.");

                LogMsg(MessageImportance.High, "Getting directory listing of {0}", string.Format(@"{0}\..\IC Main Scripts\{1}", BasePath, schema));

                // Copy IC Main scripts to setup folder
                var icMainFolders = Directory.GetDirectories(string.Format(@"{0}\..\IC Main Scripts\{1}", BasePath, schema), "*", SearchOption.TopDirectoryOnly);

                LogMsg(MessageImportance.High, GetLogMessage("icMainFolders", icMainFolders));

                if (icMainFolders.Length <= 0)
                {
                    LogMsg(MessageImportance.High, "No IC Main folders found, so exiting CopyICMainClientScripts() method.");
                    return;
                }

                var icMainFolderVersions = new SortedList<string, string>();

                foreach (var icMainFolder in icMainFolders)
                {
                    // Check out destination scripts if they exist
                    var tempDestPath = string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\{1}\{2}", BasePath, newDBVersion, schema);
                    if (checkInStampedScripts)
                        TFSUtils.tfsCheckout(tempDestPath, TFS_User, TFS_Pass);

                    // Copy scripts
                    LogMsg(MessageImportance.High, "Copying files matching '*.*' from '{0}' to '{1}'.", icMainFolder, tempDestPath);
                    FileUtils.CopyFiles(icMainFolder, "*.*", tempDestPath);

                    // Add folder to sorted list
                    var icMainFolderNameParts = icMainFolder.Split('\\');
                    var icMainFolderName = icMainFolderNameParts[icMainFolderNameParts.Length - 1];
                    icMainFolderVersions.Add(icMainFolderName, icMainFolder);
                }

                LogMsg(MessageImportance.High, GetLogMessage("icMainFolderVersions", icMainFolderVersions));

                var latestICMainFolder = string.Format(@"{0}\..\IC Main Scripts\{1}\{2}", BasePath, schema,
                                                       icMainFolderVersions.Last().Key);

                LogMsg(MessageImportance.High, "latestICMainFolder = {0}", latestICMainFolder);

                LogMsg(MessageImportance.High, GetLogMessage("databaseobjectfolders", databaseobjectfolders));

                // Copy IC Main DB objects to setup folder
                foreach (var tempLatestICMainDatabaseObjectFolder in databaseobjectfolders.Select(
                    databaseobjectfolder => string.Format(@"{0}\DatabaseObjects\{1}", latestICMainFolder, 
                        databaseobjectfolder)))
                {
                    if (checkInStampedScripts)
                        // Check out script
                        TFSUtils.tfsCheckout(tempLatestICMainDatabaseObjectFolder, TFS_User, TFS_Pass);

                    // Determine output path
                    var destscriptfolder = string.Format(@"{0}\..\Setup\DbUpgrade\DBScripts\_PreProcess\{1}", BasePath, schema);

                    if (checkInStampedScripts)
                        // Check out destination scripts
                        TFSUtils.tfsCheckout(destscriptfolder, TFS_User, TFS_Pass);

                    if (Directory.Exists(tempLatestICMainDatabaseObjectFolder))
                    {
                        foreach (var dbscript in Directory.GetFiles(tempLatestICMainDatabaseObjectFolder, "*.sql", SearchOption.TopDirectoryOnly))
                        {
                            // Add comments to script
                            var tokens = AddComments(dbscript);

                            // Determine output path
                            var scriptpathparts = dbscript.Split('\\');
                            var destscriptpath = string.Format(@"{0}\{1}", destscriptfolder, scriptpathparts[scriptpathparts.Length - 1]);

                            if (!checkInStampedScripts)
                                FileUtils.UnsetReadOnly(destscriptfolder, Path.GetFileName(destscriptpath), SearchOption.TopDirectoryOnly);

                            // Perform token replacement on script and copy to destination folder
                            TokenUtils.ReplaceTokens(dbscript, destscriptpath, tokens);
                        }
                    }
                    else
                    {
                        LogMsg(MessageImportance.High, "Directory not found: {0}", tempLatestICMainDatabaseObjectFolder);
                    }

                    if (checkInStampedScripts)
                        // Undo original script (template)
                        TFSUtils.tfsUndo(tempLatestICMainDatabaseObjectFolder, TFS_User, TFS_Pass);
                }
            }

            private Dictionary<string, string> AddComments(string dbscript)
            {
                LogMsg(MessageImportance.High, "AddComments() called.");

                //var dbscriptparts = dbscript.Split('\\');
                //var scriptname = dbscriptparts[dbscriptparts.Length - 1];
                //string objectname = null;
                //if (scriptname.Split('.').Length == 4)
                //{
                //    // Some of our db files now store the file with funny names now so that the filenames will sort in the order they need to execute.
                //    // The object name is still in the filename, but we have to pick it out differently than before.
                //    // example: D:\tfs\Integration\Main 6.2\Integration\CreateCI_DB\CI_CENTRAL\DatabaseObjects\Tables\000.50.APPLICATION_SETTINGS.sql
                //    // The desired object is simply APPLICATION_SETTINGS.
                //    objectname = scriptname.Split('.')[2];
                //}
                //else
                //{
                //    // if the filename does NOT have three "." then assume it's in the file name minus the path and extension.
                //    // example:  d:\folder1\folder2\MyTable.sql = object name MyTable.
                //    objectname = Path.GetFileNameWithoutExtension(scriptname);
                //}

                var icMainTokenList = new[] { "${Version}", "${ClientICMainVersion}", "${Changeset}", "${LastModifiedDate}"};
                var icMainClientTokenList = new[] { "${Version}", "${ClientICMainVersion}", "${Changeset}", "${LastModifiedDate}", "${ClientVersion}" };

                var tokenList = IsIcMainClient ? icMainClientTokenList : icMainTokenList;
                var tokens = tokenList.ToDictionary(token => token, token => string.Empty);
                tokens["${Version}"] = newDBVersion;

                if (IsIcMainClient)
                    tokens["${ClientVersion}"] = newDBVersion;

                if (checkInStampedScripts)
                    // Add comment to database object
                    TFSUtils.tfsCheckout(dbscript, TFS_User, TFS_Pass);
                else
                    FileUtils.UnsetReadOnly(Path.GetDirectoryName(dbscript), Path.GetFileName(dbscript), 
                        SearchOption.TopDirectoryOnly);
                TokenUtils.ReplaceTokens(dbscript, dbscript, tokens);
                return tokens;
            }
        }
    }
}
