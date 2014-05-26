using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using Zeta.Common;
using Zeta.Common.Plugins;
using ThreadState = System.Threading.ThreadState;

namespace LumberJack
{
    public class LumberJack : IPlugin
    {
        public static Version PluginVersion = new Version(1, 0, 19);

        public Version Version
        {
            get
            {
                return PluginVersion;
            }
        }
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();
        private static Stopwatch _stopWatch = new Stopwatch();
        private static Thread _deleteThread;
        private static Thread _compressThread;
        private static string _logFilePath = "";

        // Compress after 30 minutes of not being updated
        private static readonly TimeSpan CompressDurationAfter = new TimeSpan(1, 0, 0, 0);

        // Delete after 1 day
        private static readonly TimeSpan DeleteFileAfter = new TimeSpan(2, 0, 0, 0);

        // Wait 5 minutes in case other DB Processes are spinning up right now too

        private const string LockFileName = "lumberjack.lock";
        private static Process _currentProcess;
        private static string _myLockPid = "";
        private DateTime _threadLastRunTime = DateTime.MinValue;

        public void OnPulse()
        {
            if (string.IsNullOrEmpty(_logFilePath))
            {
                _logFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Logs");
                //_logFilePath = new FileInfo(Logging.LogFilePath).DirectoryName;
            }

            // Run cleanup 
            if (!(DateTime.UtcNow.Subtract(_threadLastRunTime).TotalMinutes > CompressDurationAfter.TotalMinutes))
                return;

            // Make sure we're the only bot doing log file management
            if (!CheckLock())
            {
                return;
            }

            CreateThreads();

            _threadLastRunTime = DateTime.UtcNow;
        }

        public string Author
        {
            get { return "rrrix"; }
        }

        public string Description
        {
            get { return "Automated Log File Management"; }
        }

        public Window DisplayWindow
        {
            get { return null; }
        }

        public string Name
        {
            get { return "LumberJack"; }
        }

        public void OnDisabled()
        {
            Log.Info("[LumberJack] Plugin disabled. ");

            ShutdownThreads();
        }

        public void OnEnabled()
        {
            Log.InfoFormat("[LumberJack] Plugin v{0} Enabled", Version);
        }

        public void OnInitialize()
        {
        }

        public void OnShutdown()
        {
            ShutdownThreads();
        }

        public bool Equals(IPlugin other)
        {
            return (other.Name == Name) && (other.Version == Version);
        }

        private static void CreateThreads()
        {
            if (_deleteThread == null || (_deleteThread != null && !_deleteThread.IsAlive))
            {
                _deleteThread = new Thread(Delete)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest,
                    Name = "LumberJack Delete"
                };
                _deleteThread.Start();
            }
            if (_compressThread == null || (_compressThread != null && !_compressThread.IsAlive))
            {
                _compressThread = new Thread(Compress)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest,
                    Name = "LumberJack Compress"
                };
                _compressThread.Start();
            }
        }

        private static bool CheckLock()
        {
            try
            {
                var lockFile = new FileInfo(_logFilePath + @"\" + LockFileName);

                if (_currentProcess == null)
                {
                    _currentProcess = Process.GetCurrentProcess();
                }

                if (lockFile.Exists && lockFile.LastWriteTimeUtc > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 15)))
                {
                    using (TextReader tr = new StreamReader(lockFile.FullName))
                    {
                        _myLockPid = tr.ReadToEnd();
                    }

                    if (_myLockPid.Trim() != _currentProcess.Id.ToString())
                        return false;

                    // We are doing log management                
                    // Create a new file, or overwrite the current file
                    UpdateLock(lockFile);
                    return true;
                    // We are not doing log management
                }
                // Lock file does not exist or is not updated, we're in control
                UpdateLock(lockFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Writes the current process id to the file <see cref="lockFile" />
        /// </summary>
        /// <param name="lockFile"></param>
        private static void UpdateLock(FileInfo lockFile)
        {
            using (TextWriter tw = new StreamWriter(lockFile.FullName, false))
            {
                tw.Write(_currentProcess.Id);
            }
        }

        /// <summary>
        ///     Delete files older than <see cref="DeleteFileAfter" />
        /// </summary>
        private static void Delete()
        {
            var di = new DirectoryInfo(_logFilePath);
            IEnumerable<int> buddypids = GetBuddyPids();
            IEnumerable<FileInfo> files =
                di.EnumerateFiles()
                    .Where(
                        f =>
                            f.LastWriteTimeUtc < DateTime.UtcNow.Subtract(DeleteFileAfter) &&
                            (f.Name.EndsWith(".txt") || f.Name.EndsWith(".gz") || f.Name.EndsWith(".zip")));
            foreach (FileInfo f in files)
            {
                try
                {
                    // Make sure the log is not attached to a currently running Demonbuddy Process
                    if (buddypids.Contains(GetPidFromFileInfoFileName(f)))
                        continue;

                    Log.InfoFormat("[LumberJack] Deleting log file {0}, age {1}", f.Name,
                        (DateTime.UtcNow - f.LastWriteTimeUtc));
                    f.Delete();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex.ToString());
                }
            }
        }

        /// <summary>
        ///     Compress files older than <see cref="CompressDurationAfter" /> but before <see cref="DeleteFileAfter" />
        /// </summary>
        private static void Compress()
        {
            var di = new DirectoryInfo(_logFilePath);
            IEnumerable<int> buddypids = GetBuddyPids();
            IEnumerable<FileInfo> files =
                di.EnumerateFiles()
                    .Where(
                        f =>
                            f.LastWriteTimeUtc > DateTime.UtcNow.Subtract(DeleteFileAfter) &&
                            f.LastWriteTimeUtc < DateTime.UtcNow.Subtract(CompressDurationAfter) && f.Name.EndsWith(".txt"));
            foreach (FileInfo f in files)
            {
                // Make sure the log is not attached to a currently running Demonbuddy Process
                if (buddypids.Contains(GetPidFromFileInfoFileName(f)))
                    continue;
                try
                {
                    using (FileStream infile = File.OpenRead(f.FullName))
                    {
                        string outfileName = f.FullName + ".gz";

                        if (File.Exists(outfileName))
                        {
                            continue;
                        }
                        using (FileStream outfile = File.OpenWrite(outfileName))
                        {
                            using (var compressor = new GZipStream(outfile, CompressionMode.Compress))
                            {
                                Log.InfoFormat("[LumberJack] Compressing log file {0}, age {1}", f.Name,
                                    (DateTime.UtcNow - f.LastWriteTimeUtc));

                                try
                                {
                                    int inByte = infile.ReadByte();
                                    while (inByte != -1)
                                    {
                                        compressor.WriteByte((byte)inByte);
                                        inByte = infile.ReadByte();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Debug(ex.ToString());
                                }
                            }
                        }
                    }

                    // Delete the file
                    f.Delete();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex.ToString());
                }
            }
        }

        /// <summary>
        ///     Extracts the process ID from the log file name
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private static int GetPidFromFileInfoFileName(FileInfo f)
        {
            try
            {
                int pid = -1;
                Int32.TryParse(f.Name.Substring(0, f.Name.IndexOf(" ")), out pid);
                return pid;
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
                return -1;
            }
        }

        /// <summary>
        ///     Returns a list of Process ID's for Currently running Demonbuddy's
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<int> GetBuddyPids()
        {
            return
                from p in Process.GetProcesses()
                where p.ProcessName == "Demonbuddy"
                select p.Id;
        }

        private static void ShutdownThreads()
        {
            if (_deleteThread != null && _deleteThread.ThreadState == ThreadState.Running)
            {
                Log.Debug("Shutting down Delete thread");
                _deleteThread.Abort();
            }
            if (_compressThread != null && _compressThread.ThreadState == ThreadState.Running)
            {
                Log.Debug("Shutting down Compression thread");
                _compressThread.Abort();
            }
        }
    }
}