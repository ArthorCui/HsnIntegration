using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace BuildUtilities
{
// ReSharper disable ClassNeverInstantiated.Global
    public static partial class ICBuildLib
// ReSharper restore ClassNeverInstantiated.Global
    {
        private static readonly Dictionary<string, XmlDocument> versionDocList = new Dictionary<string, XmlDocument>();
        private static readonly ReaderWriterLockSlim versionDocLock = new ReaderWriterLockSlim();
        [ThreadStatic]
        private static TaskLoggingHelper Logger;

        private static string TF_exe_path;

        private static void Initialize(TaskLoggingHelper logger, string programFilesPath = null, string tfExePath = null)
        {
            Logger = logger;

            if (tfExePath == null && programFilesPath != null)
                TF_exe_path = string.Format(@"{0}\Microsoft Visual Studio 10.0\Common7\IDE\TF.exe", programFilesPath);
            else
                TF_exe_path = tfExePath;
        }

        /// <summary>
        /// Logs a message with the specified string.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">The arguments for formatting the message.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="message"/> is null.</exception>
        private static void LogMsg(string message, params object[] messageArgs)
        {
            if (Logger == null)
                Console.WriteLine(message, messageArgs);
            else
                try
                {
                    Logger.LogMessage(message, messageArgs);
                }
                catch (Exception)
                {
                    Logger = null;
                    LogMsg(message, messageArgs);
                }
        }

        /// <summary>
        /// Logs a message with the specified string and importance.
        /// </summary>
        /// <param name="importance">One of the enumeration values that specifies the importance of the message.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">The arguments for formatting the message.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="message"/> is null.</exception>
        private static void LogMsg(MessageImportance importance, string message, params object[] messageArgs)
        {
            if (Logger == null)
                Console.WriteLine(message, messageArgs);
            else
                try
                {
                    Logger.LogMessage(importance, message, messageArgs);
                }
                catch (Exception)
                {
                    Logger = null;
                    LogMsg(importance, message, messageArgs);
                }
        }

    }
}