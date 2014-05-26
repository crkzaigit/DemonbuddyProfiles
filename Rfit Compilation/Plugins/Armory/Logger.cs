

namespace Armory
{
    /// <summary>
    /// Facilitates Logging
    /// </summary>
    public class Logger
    {
        private static log4net.ILog DBLog = Zeta.Common.Logger.GetLoggerInstanceForType();

        private static string logName;
        private static string LogName
        {
            get
            {
                if (!string.IsNullOrEmpty(logName))
                    return logName;

                logName = "[" + Plugin.NAME + "] ";
                return logName;
            }
        }

        /// <summary>
        /// Write to Error (Quiet)
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            Error(message, null);
        }

        /// <summary>
        /// Write to Error (Quiet)
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message, params object[] args)
        {
            DBLog.ErrorFormat(LogName + message, args);
        }

        /// <summary>
        /// Write to Normal
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            Log(message, null);
        }

        /// <summary>
        /// Write to Normal
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message, params object[] args)
        {
            DBLog.InfoFormat(LogName + message, args);
        }

        /// <summary>
        /// Write to Verbose
        /// </summary>
        /// <param name="message"></param>
        public static void Verbose(string message)
        {
            Verbose(message, null);
        }

        /// <summary>
        /// Write to Verbose
        /// </summary>
        /// <param name="message"></param>
        public static void Verbose(string message, params object[] args)
        {
            DBLog.DebugFormat(LogName + message, args);
        }

        /// <summary>
        /// Write to Debug (Diagnostic)
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            Verbose(message, null);
        }

        /// <summary>
        /// Write to Debug (Diagnostic)
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message, params object[] args)
        {
            DBLog.DebugFormat(LogName + message, args);
        }
    }
}
