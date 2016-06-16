namespace BuildUtilities
{
    using System.Diagnostics;
    using System.IO;
    using System.Xml;

    public static partial class ICBuildLib
    {
        /// <summary>
        /// TODO: Update summary.
        /// </summary>
        private static class FileUtils
        {
            public static string RunCommand(string command, string args)
            {
                LogMsg("Executing command '{0}' with arguments '{1}'", command, args);

                var p = new Process
                    {
                        StartInfo = new ProcessStartInfo(command, args)
                            {
                                UseShellExecute = false,
                                RedirectStandardOutput = true
                            }
                    };
                p.Start();
                var output = p.StandardOutput.ReadToEnd();

                if (!string.IsNullOrEmpty(output))
                    LogMsg("Command returned the following output:\r\n{0}", output);
                p.WaitForExit(300000);

                return output;
            }

            public static string GetValueFromVersionFile(string versionXmlFile, string XPath)
            {
                LogMsg("Getting value '{0}' from version file '{1}'", XPath, versionXmlFile);

                XmlNode node = GetVersionXmlDoc(versionXmlFile).SelectSingleNode(XPath);
                return node == null ? "" : node.InnerText;
            }

            private static XmlDocument GetVersionXmlDoc(string versionXmlFile)
            {
                LogMsg("Getting version file '{0}'", versionXmlFile);

                XmlDocument docVersion = null;
                try
                {
                    versionDocLock.EnterReadLock();
                    if (versionDocList.ContainsKey(versionXmlFile))
                        docVersion = versionDocList[versionXmlFile];
                    else
                    {
                        versionDocLock.ExitReadLock();
                        versionDocLock.EnterWriteLock();

                        if (versionDocList.ContainsKey(versionXmlFile))
                        {
                            docVersion = versionDocList[versionXmlFile];
                        }
                        else
                        {
                            docVersion = new XmlDocument();
                            docVersion.Load(versionXmlFile);

                            versionDocList.Add(versionXmlFile, docVersion);
                        }
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (versionDocLock.IsWriteLockHeld)
                        versionDocLock.ExitWriteLock();
                    if (versionDocLock.IsReadLockHeld)
                        versionDocLock.ExitReadLock();
                }

                return docVersion;
            }

            public static void UpdateValueInVersionFile(string versionXmlFile, string XPath, string newValue)
            {
                LogMsg("Updating element '{0}' in version file '{1}' to new value '{2}'", XPath, versionXmlFile, newValue);

                var docVersion = GetVersionXmlDoc(versionXmlFile);
                var node = docVersion.SelectSingleNode(XPath);
                node.InnerText = newValue;
                docVersion.Save(versionXmlFile);
            }

            public static void CopyFiles(string path, string searchPattern, string destDirName)
            {
                if (!Directory.Exists(path))
                    return;

                LogMsg("Copying files in directory '{0}' with search pattern '{1}' to destination directory '{2}'", path, searchPattern, destDirName);

                foreach (string file in Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly))
                {
                    FileInfo fi = new FileInfo(file);
                    string destFileName = Path.Combine(destDirName, fi.Name);
                    fi.CopyTo(destFileName, true);
                }
            }

            public static void MoveFiles(string path, string searchPattern, string destDirName)
            {
                if (!Directory.Exists(path))
                    return;

                LogMsg("Moving files in directory '{0}' with search pattern '{1}' to destination directory '{2}'", path, searchPattern, destDirName);

                foreach (string file in Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly))
                {
                    FileInfo fi = new FileInfo(file);
                    string destFileName = Path.Combine(destDirName, fi.Name);
                    fi.MoveTo(destFileName);
                }
            }

            public static void MoveFiles(string path, string searchPattern, string destDirName, string newExtension)
            {
                if (!Directory.Exists(path))
                    return;

                LogMsg("Copying files in directory '{0}' with search pattern '{1}' to destination directory '{2}' with new extension '{3}'", path, searchPattern, destDirName, newExtension);

                foreach (string file in Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly))
                {
                    FileInfo fi = new FileInfo(file);
                    string destFileName = Path.Combine(destDirName, fi.Name);
                    destFileName = Path.ChangeExtension(destFileName, newExtension);
                    fi.MoveTo(destFileName);
                }
            }

            public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
            {
                LogMsg("Copying directory '{0}' to destination directory '{1}' with copySubDirs value '{2}'", sourceDirName, destDirName, copySubDirs);

                DirectoryInfo dir = new DirectoryInfo(sourceDirName);
                DirectoryInfo[] dirs = dir.GetDirectories();

                // If the source directory does not exist, throw an exception.
                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
                }

                // If the destination directory does not exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }


                // Get the file contents of the directory to copy.
                FileInfo[] files = dir.GetFiles();

                foreach (FileInfo file in files)
                {
                    // Create the path to the new copy of the file.
                    string temppath = Path.Combine(destDirName, file.Name);

                    // Copy the file.
                    file.CopyTo(temppath, true);
                }

                // If copySubDirs is true, copy the subdirectories.
                if (copySubDirs)
                {

                    foreach (DirectoryInfo subdir in dirs)
                    {
                        // Create the subdirectory.
                        string temppath = Path.Combine(destDirName, subdir.Name);

                        // Copy the subdirectories.
                        CopyDirectory(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }

            public static void DeleteFiles(string path, string searchPattern, SearchOption searchOption)
            {
                LogMsg("Deleting files in directory '{0}' with search pattern '{1}' and search option '{2}'", path, searchPattern, searchOption);

                foreach (string file in Directory.GetFiles(path, searchPattern, searchOption))
                    File.Delete(file);

                if (searchOption == SearchOption.AllDirectories)
                {
                    string[] dirs = Directory.GetDirectories(path, searchPattern, searchOption);
                    foreach (var dir in dirs)
                        DeleteFiles(dir, searchPattern, searchOption);
                }
            }

            public static void DeleteDir(string path)
            {
                LogMsg("Deleting directory '{0}'", path);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }

            public static void SetReadOnly(string path, string searchPattern, SearchOption searchOption)
            {
                LogMsg("Setting readonly flag in directory '{0}' with search pattern '{1}' and search option '{2}'", path, searchPattern, searchOption);

                foreach (var file in Directory.GetFiles(path, searchPattern, searchOption))
                    File.SetAttributes(file, FileAttributes.ReadOnly);

                if (searchOption != SearchOption.AllDirectories) return;
                foreach (var dir in Directory.GetDirectories(path, searchPattern, searchOption))
                    SetReadOnly(dir, searchPattern, searchOption);
            }

            public static void UnsetReadOnly(string path, string searchPattern, SearchOption searchOption)
            {
                LogMsg("Unsetting readonly flag in directory '{0}' with search pattern '{1}' and search option '{2}'", path, searchPattern, searchOption);

                foreach (var file in Directory.GetFiles(path, searchPattern, searchOption))
                    File.SetAttributes(file, FileAttributes.Normal);

                if (searchOption != SearchOption.AllDirectories) return;
                foreach (var dir in Directory.GetDirectories(path, searchPattern, searchOption))
                    UnsetReadOnly(dir, searchPattern, searchOption);
            }


            public static string GetVersionNumber(string dllFilePath)
            {
                LogMsg("Getting version number from file '{0}'", dllFilePath);

                var versionInfo = FileVersionInfo.GetVersionInfo(dllFilePath);
                return versionInfo.FileVersion;
            }
        }
    }
}
