﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Zeta.Common;

namespace Trinity.Technicals
{
    /// <summary>
    /// Utilities help developer interact with DemonBuddy
    /// </summary>
    //[DebuggerStepThrough]
    internal class Logger
    {
        private static readonly log4net.ILog _Logger = Zeta.Common.Logger.GetLoggerInstanceForType();
        private static string prefix = "[Trinity]";

        public static string Prefix
        {
            get { return Logger.prefix; }
            set { Logger.prefix = value; }
        }

        private static Dictionary<LogCategory, string> _LastLogMessages = new Dictionary<LogCategory, string>();

        /// <summary>Logs the specified level.</summary>
        /// <param name="level">The logging level.</param>
        /// <param name="category">The category.</param>
        /// <param name="formatMessage">The format message.</param>
        /// <param name="args">The parameters used when format message.</param>
        public static void Log(TrinityLogLevel level, LogCategory category, string formatMessage, params object[] args)
        {
            if (category == LogCategory.UserInformation || level >= TrinityLogLevel.Error || (Trinity.Settings != null && Trinity.Settings.Advanced.LogCategories.HasFlag(category)))
            {
                string msg = string.Format(prefix + "{0} {1}", category != LogCategory.UserInformation ? "[" + category.ToString() + "]" : string.Empty, formatMessage);

                if (args.Length > 0)
                    msg = string.Format(msg, args);

                if (!_LastLogMessages.ContainsKey(category))
                    _LastLogMessages.Add(category, "");
                
                string lastMessage;
                if (_LastLogMessages.TryGetValue(category, out lastMessage) && lastMessage != msg)
                {
                    _LastLogMessages[category] = msg;

                    switch (level)
                    {
                        case TrinityLogLevel.Error:
                            _Logger.Error(msg);
                            break;
                        case TrinityLogLevel.Info:
                            _Logger.Info(msg);
                            break;
                        case TrinityLogLevel.Verbose:
                            _Logger.Debug(msg);
                            break;
                        case TrinityLogLevel.Debug:
                            LogToTrinityDebug(msg);
                            break;
                    }
                }
            }
        }

        /// <summary>Logs the message to Normal log level</summary>
        /// <param name="category">The category.</param>
        /// <param name="formatMessage">The format message.</param>
        /// <param name="args">The parameters used when format message.</param>
        public static void Log(LogCategory category, string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Info, category, formatMessage, args);
        }

        /// <summary>Logs the message to Normal log level</summary>
        /// <param name="category">The category.</param>
        /// <param name="formatMessage">The format message.</param>
        /// <param name="args">The parameters used when format message.</param>
        public static void Log(string formatMessage)
        {
            LogNormal(formatMessage, 0);
        }

        public static void Log(string formatMessage, params object[] args)
        {
            LogNormal(formatMessage, args);
        }

        /// <summary>
        /// Logs a message with Normal/UserInformation
        /// </summary>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogNormal(string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Info, LogCategory.UserInformation, formatMessage, args);
        }

        /// <summary>
        /// Logs a message with Normal/UserInformation
        /// </summary>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogVerbose(string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, formatMessage, args);
        }

        /// <summary>
        /// Logs a message as verbose with the specified category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogVerbose(LogCategory category, string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Verbose, category, formatMessage, args);
        }

        /// <summary>
        /// Logs a message with Normal/UserInformation
        /// </summary>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogDebug(string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Debug, LogCategory.UserInformation, formatMessage, args);
        }
        /// <summary>
        /// Logs a message with Normal/UserInformation
        /// </summary>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogDebug(LogCategory logCategory, string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Debug, logCategory, formatMessage, args);
        }

        /// <summary>
        /// Logs a message with Normal/UserInformation
        /// </summary>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogError(string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Error, LogCategory.UserInformation, formatMessage, args);
        }
        /// <summary>
        /// Logs a message with Normal/UserInformation
        /// </summary>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogError(LogCategory logCategory, string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Error, logCategory, formatMessage, args);
        }

        /// <summary>Converts <see cref="TrinityLogLevel"/> to <see cref="LogLevel"/>.</summary>
        /// <param name="level">The trinity logging level.</param>
        /// <returns>DemonBuddy logging level.</returns>
        private static LogLevel ConvertToLogLevel(TrinityLogLevel level)
        {
            LogLevel logLevel = LogLevel.Debug;
            switch (level)
            {
                case TrinityLogLevel.Error:
                    logLevel = LogLevel.Error;
                    break;
                case TrinityLogLevel.Info:
                    logLevel = LogLevel.Info;
                    break;
                case TrinityLogLevel.Verbose:
                    logLevel = LogLevel.Verbose;
                    break;
                case TrinityLogLevel.Debug:
                    logLevel = LogLevel.Debug;
                    break;
            }
            return logLevel;
        }

        public static string ListToString(System.Collections.Generic.List<object> list)
        {
            string result = "";
            for (int i = 0; i < list.Count; i++)
            {
                result += list[i];
                if (i < list.Count - 1)
                    result += ", ";
            }
            return result;
        }

        /*
         * Log4Net logging
         */

        private static ILog trinityLog;
        private static log4net.Filter.LoggerMatchFilter trinityFilter;
        private static PatternLayout trinityLayout;
        private static log4net.Repository.Hierarchy.Logger trinityLogger;
        private static FileAppender trinityAppender;
        private static Object _loglock = 0;

        private static void SetupLogger()
        {
            try
            {
                if (trinityLog == null)
                {
                    lock (_loglock)
                    {
                        _Logger.Info("Setting up Trinity Logging");
                        int myPid = Process.GetCurrentProcess().Id;
                        DateTime startTime = Process.GetCurrentProcess().StartTime;

                        trinityLayout = new PatternLayout("%date{HH:mm:ss.fff} %-5level %logger{1} %m%n");
                        trinityLayout.ActivateOptions();

                        trinityFilter = new log4net.Filter.LoggerMatchFilter();
                        trinityFilter.LoggerToMatch = "Trinity";
                        trinityFilter.AcceptOnMatch = true;
                        trinityFilter.ActivateOptions();

                        Hierarchy h = (Hierarchy)LogManager.GetRepository();
                        var appenders = h.GetAppenders();

                        foreach (var appender in appenders)
                        {
                            if (appender is FileAppender)
                            {
                                trinityAppender = appender as FileAppender;
                            }
                        }

                        trinityAppender.Layout = trinityLayout;
                        //trinityAppender.AddFilter(trinityFilter);
                        trinityAppender.LockingModel = new FileAppender.ExclusiveLock();
                        //trinityAppender.LockingModel = new FileAppender.InterProcessLock();

                        trinityAppender.ActivateOptions();

                        trinityLog = LogManager.GetLogger("TrinityDebug");
                        trinityLogger = ((log4net.Repository.Hierarchy.Logger)trinityLog.Logger);
                        trinityLogger.Additivity = false;
                        trinityLogger.AddAppender(trinityAppender);
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("Error setting up Trinity Logger:\n" + ex.ToString());
            }
        }

        public static void LogToTrinityDebug(string message, params object[] args)
        {

            try
            {
                SetupLogger();

                string data = "";

                if (message.Contains("{c:"))
                    data = message + " args: " + args;
                else if (args != null && args.Length > 0)
                    data = string.Format(message, args);
                else
                    data = message;

                trinityLog.Debug(data);
            }
            catch (Exception ex)
            {
                _Logger.Error("Error in LogToTrinityDebug: " + ex.ToString());
            }
        }
    }
}
