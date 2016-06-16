namespace BuildUtilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static partial class ICBuildLib
    {

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
        public class TFSUtils
        {
            public static void tfsAdd(string filespec, string tfs_user, string tfs_pass)
            {
                FileUtils.RunCommand(TF_exe_path, string.Format("add \"{0}\" /login:{1},{2} /noprompt /recursive", filespec, tfs_user, tfs_pass));
            }

            public static void tfsGet(string filespec, string tfs_user, string tfs_pass)
            {
                FileUtils.RunCommand(TF_exe_path, string.Format("get \"{0}\" /login:{1},{2} /noprompt /recursive", filespec, tfs_user, tfs_pass));
            }

            public static void tfsCheckout(string filespec, string tfs_user, string tfs_pass)
            {
                FileUtils.RunCommand(TF_exe_path, string.Format("checkout \"{0}\" /login:{1},{2} /noprompt /recursive", filespec, tfs_user, tfs_pass));

                // According to http://youtrack.jetbrains.com/issue/TW-24589:
                // "Do you use agent-side checkout? By default TFS sets readonly attribute on all files."
                // This means we must manually unset the read-only flag after checkout.
                Console.Out.WriteLine("Unsetting readonly flag on {0}", filespec); 
                if (Directory.Exists(filespec))
                    FileUtils.UnsetReadOnly(filespec, "*.*", SearchOption.AllDirectories);
                else if (File.Exists(filespec))
                {
                    var filename = Path.GetFileName(filespec);
                    var path = Path.GetDirectoryName(filespec);
                    FileUtils.UnsetReadOnly(path, filename, SearchOption.TopDirectoryOnly);
                }
            }

            public static void tfsCheckin(string filespec, string tfscheckincomment, string tfs_user, string tfs_pass)
            {
                FileUtils.RunCommand(TF_exe_path, string.Format("checkin \"{0}\" /login:{1},{2} /noprompt /comment:\"{3}\" /recursive", filespec, tfs_user, tfs_pass, tfscheckincomment));
            }

            public static void tfsLabel(string filespec, string label, string tfs_user, string tfs_pass)
            {
                FileUtils.RunCommand(TF_exe_path, string.Format("label \"{0}\" \"{1}\" /login:{2},{3} /noprompt /recursive", label, filespec, tfs_user, tfs_pass));
            }

            public static void tfsBranch(string labelname, string sourcepath, string branchpath, bool getsource, string tfs_user, string tfs_pass)
            {
                string noget = getsource ? "" : " /noget";
                FileUtils.RunCommand(TF_exe_path, string.Format("branch /version:L\"{0}\" \"{1}\" \"{2}\" /login:{3},{4}{5} /noprompt", labelname, sourcepath, branchpath, tfs_user, tfs_pass, noget));
            }

            public static void tfsDecloak(string filespec, string tfs_user, string tfs_pass)
            {
                FileUtils.RunCommand(TF_exe_path, string.Format("workfold /decloak \"{0}\" /login:{1},{2}", filespec, tfs_user, tfs_pass));
            }

            public static void tfsCloak(string filespec, string tfs_user, string tfs_pass)
            {
                FileUtils.RunCommand(TF_exe_path, string.Format("workfold /cloak \"{0}\" /login:{1},{2}", filespec, tfs_user, tfs_pass));
            }

            public static List<string> tfsDir(string filespec, string tfs_user, string tfs_pass)
            {
                string output = FileUtils.RunCommand(TF_exe_path, string.Format("dir \"{0}\" /login:{1},{2}", filespec, tfs_user, tfs_pass));
                var dirs = new List<string>(output.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                if (dirs.Count == 0)
                {
                    Logger.LogMessage("While performing a tfs dir, no entries were found in TFS in the location '{0}'.", filespec);
                    return dirs;
                }

                dirs.RemoveAt(dirs.Count - 1);
                dirs.RemoveAt(0);
                return dirs;
            }

            public static void tfsDelete(string filespec, string tfs_user, string tfs_pass)
            {
                FileUtils.RunCommand(TF_exe_path, string.Format("delete \"{0}\" /login:{1},{2} /recursive", filespec, tfs_user, tfs_pass));
            }

            public static void tfsUndo(string filespec, string tfs_user, string tfs_pass)
            {
                FileUtils.RunCommand(TF_exe_path, string.Format("undo \"{0}\" /login:{1},{2} /recursive /noprompt", filespec, tfs_user, tfs_pass));
            }

        }
    }
}
