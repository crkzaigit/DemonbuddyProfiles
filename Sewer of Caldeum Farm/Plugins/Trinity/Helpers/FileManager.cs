﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Zeta.Game;

namespace Trinity.Technicals
{
    /// <summary>
    /// Manage File Access and Path.
    /// </summary>
    internal static class FileManager
    {
        /// <summary>
        /// Loads the specified filename to HashSet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        public static HashSet<T> Load<T>(string name, string valueName)
        {
            return Load<T>(Path.Combine(FileManager.PluginPath, "Configuration", "Dictionaries.xml"), name, valueName);
        }

        /// <summary>
        /// Loads the specified filename to HashSet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The filename.</param>
        /// <param name="name">The name.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        private static HashSet<T> Load<T>(string filename, string name, string valueName)
        {
            HashSet<T> ret = new HashSet<T>();
            if (File.Exists(filename))
            {
                XElement xElem = XElement.Load(filename);
                xElem = xElem.Descendants("HashSet").FirstOrDefault(elem => elem.Attribute("Name").Value == name);
                if (xElem != null)
                {
                    List<T> lst = (from e in xElem.Descendants("Entry")
                                   where e.Attribute(valueName) != null && e.Attribute(valueName).Value != null
                                   select typeof(T).IsEnum ? (T)Enum.Parse(typeof(T), e.Attribute(valueName).Value, true) : (T)Convert.ChangeType(e.Attribute(valueName).Value, typeof(T), CultureInfo.InvariantCulture)
                                   ).ToList();
                    foreach (T item in lst)
                    {
                        ret.Add(item);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Loads the specified filename to IDictionary.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        public static IDictionary<K, T> Load<K, T>(string name, string keyName, string valueName)
        {
            return Load<K, T>(Path.Combine(FileManager.PluginPath, "Configuration", "Dictionaries.xml"), name, keyName, valueName);
        }

        /// <summary>
        /// Loads the specified filename to IDictionary.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The filename.</param>
        /// <param name="name">The name.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        private static IDictionary<K, T> Load<K, T>(string filename, string name, string keyName, string valueName)
        {
            using (new PerformanceLogger("FileManager.Load"))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.Configuration, "Loading Dictionary file={0} name={1} keys={2} values={3}", filename, name, keyName, valueName);
                IDictionary<K, T> ret = new Dictionary<K, T>();
                try
                {
                    if (File.Exists(filename))
                    {
                        XElement xElem = XElement.Load(filename);
                        xElem = xElem.Descendants("Dictionary").FirstOrDefault(elem => elem.Attribute("Name").Value == name);
                        if (xElem != null)
                        {
                            List<KeyValuePair<K, T>> lst = (from e in xElem.Descendants("Entry")
                                                            where e.Attribute(keyName) != null && e.Attribute(keyName).Value != null
                                                            where e.Attribute(valueName) != null && e.Attribute(valueName).Value != null
                                                            select new KeyValuePair<K, T>(
                                                                typeof(K).IsEnum ? (K)Enum.Parse(typeof(K), e.Attribute(keyName).Value, true) : (K)Convert.ChangeType(e.Attribute(keyName).Value, typeof(K), CultureInfo.InvariantCulture),
                                                                typeof(T).IsEnum ? (T)Enum.Parse(typeof(T), e.Attribute(valueName).Value, true) : (T)Convert.ChangeType(e.Attribute(valueName).Value, typeof(T), CultureInfo.InvariantCulture))
                                                            ).ToList();

                            foreach (KeyValuePair<K, T> item in lst)
                            {
                                Logger.Log(TrinityLogLevel.Debug, LogCategory.Configuration, "Found dictionary item {0} = {1}", item.Key, item.Value);
                                ret.Add(item);
                            }
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException("Could not load {0}", filename);
                    }
                    if (ret.Count > 0)
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Configuration, "Loaded Dictionary name={0} key={1} value={2} with {3} values", name, keyName, valueName, ret.Count);
                    }
                    else
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Configuration, "Attempted to load Dictionary name={0} key={1} value={2} but 0 values found!", name, keyName, valueName, ret.Count);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogNormal("{0}", ex);
                }
                return ret;
            }
        }

        /// <summary>
        /// Gets the DemonBuddy path.
        /// </summary>
        /// <value>The demon buddy path.</value>
        public static string DemonBuddyPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_DemonBuddyPath))
                    _DemonBuddyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                return _DemonBuddyPath;
            }
        }
        private static string _DemonBuddyPath;

        /// <summary>
        /// Gets the plugin path.
        /// </summary>
        /// <value>The plugin path.</value>
        public static string PluginPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_PluginPath))
                    _PluginPath = Path.Combine(DemonBuddyPath, "Plugins", TrinityName);
                return _PluginPath;
            }
        }
        private static string _PluginPath;

        /// <summary>
        /// The full path to the built-in Trinity combat routine, for auto-installation
        /// </summary>
        public static string CombatRoutineSourcePath
        {
            get
            {
                return Path.Combine(PluginPath, "Combat", "Routine", CombatRoutineFileName);
            }
        }

        /// <summary>
        /// The full path to the Demonbuddy combat routine for Trinity
        /// </summary>
        public static string CombatRoutineDestinationPath
        {
            get
            {
                return Path.Combine(DemonBuddyPath, "Routines", TrinityName, CombatRoutineFileName);
            }
        }

        /// <summary>
        /// The string name of Trinity
        /// </summary>
        public static string TrinityName
        {
            get
            {
                return "Trinity";
            }
        }

        /// <summary>
        /// The file name of the Trinity Combat Routine
        /// </summary>
        public static string CombatRoutineFileName
        {
            get
            {
                return "TrinityRoutine.cs";
            }
        }

        /// <summary>
        /// Gets the settings path.
        /// </summary>
        /// <value>The settings path.</value>
        public static string SettingsPath
        {
            get
            {
                return Path.Combine(DemonBuddyPath, "Settings");
            }
        }

        /// <summary>
        /// Gets the settings path specific to current hero.
        /// </summary>
        /// <value>The specific settings path.</value>
        public static string SpecificSettingsPath
        {
            get
            {
                string path = Path.Combine(DemonBuddyPath, "Settings", BattleTagName);
                CreateDirectory(path);
                return path;
            }
        }

        /// <summary>
        /// Gets the logging path for battletag specific logging
        /// </summary>
        /// <value>The logging path.</value>
        public static string LoggingPath
        {
            get
            {
                string path = Path.Combine(DemonBuddyPath, "TrinityLogs", BattleTagName);
                CreateDirectory(path);
                return path;
            }
        }

        /// <summary>
        /// Gets the TrinityLogs path - for NON-battletag specific logging
        /// </summary>
        public static string TrinityLogsPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_TrinityLogsPath))
                {
                    _TrinityLogsPath = Path.Combine(DemonBuddyPath, "TrinityLogs");
                    CreateDirectory(_TrinityLogsPath);
                }
                return _TrinityLogsPath;
            }
        }
        private static string _TrinityLogsPath;

        /// <summary>
        /// Gets the scripted item rules path.
        /// </summary>
        /// <value>The item rule path.</value>
        public static string ItemRulePath
        {
            get
            {
                return Path.Combine(PluginPath, "ItemRules");
            }
        }

        /// <summary>
        /// Gets the scripted item rules path specific to current hero.
        /// </summary>
        /// <value>The item rule path.</value>
        public static string SpecificItemRulePath
        {
            get
            {
                return Path.Combine(DemonBuddyPath, "ItemRules", BattleTagName);
            }
        }

        /// <summary>
        /// Creates the directory structure.
        /// </summary>
        /// <param name="path">The path.</param>
        private static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    CreateDirectory(Path.GetDirectoryName(path));
                }
                Directory.CreateDirectory(path);
            }
        }

        private static string _battleTagName;
        public static string BattleTagName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_battleTagName) && ZetaDia.Service.Hero.IsValid)
                    _battleTagName = ZetaDia.Service.Hero.BattleTagName;
                return _battleTagName;
            }
        }

        /// <summary>
        /// Copies a file and if necessary creates destination directory
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        public static void CopyFile(string sourcePath, string destPath)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Unable to copy source file", sourcePath);
            }

            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }

            var destDirectory = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(destDirectory))
            {
                CreateDirectory(destDirectory);
            }
            Logger.LogDebug("Copying file {0} to {1}", sourcePath, destPath);
            File.Copy(sourcePath, destPath);
        }

        /// <summary>
        /// Will delete old Trinity routines from the Demonbuddy Routines directory
        /// </summary>
        public static void CleanupOldRoutines()
        {
            List<string> oldRoutines = new List<string>()
            {
                Path.Combine(DemonBuddyPath, "Routines", "GilesPlugin"),
                Path.Combine(DemonBuddyPath, "Routines", "GilesBlankCombatRoutine"),
                Path.Combine(DemonBuddyPath, "Routines", "Trinity")                
            };

            foreach (string routinePath in oldRoutines)
            {
                if (Directory.Exists(routinePath))
                {
                    Logger.LogDebug("Deleting old routine: {0}", routinePath);
                    Directory.Delete(routinePath, true);
                }
            }

            string oldTrinityRoutine = Path.Combine(DemonBuddyPath, "Routines", "Trinity", "Trinity.cs");

            if (File.Exists(oldTrinityRoutine))
            {
                Logger.LogDebug("Deleting old routine: {0}", oldTrinityRoutine);
                File.Delete(oldTrinityRoutine);
            }
        }

        /// <summary>
        /// Compares the first line of two files to see if they are the same (e.g. for Routine version check)
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <returns></returns>
        public static bool CompareFileHeader(string file1, string file2)
        {
            if (!File.Exists(file1))
            {
                throw new FileNotFoundException("File not found", file1);
            }
            if (!File.Exists(file2))
            {
                throw new FileNotFoundException("File not found", file2);
            }

            string header1, header2 = "";
            using (StreamReader reader = new StreamReader(file1))
            {
                header1 = reader.ReadLine();
            }
            using (StreamReader reader = new StreamReader(file2))
            {
                header2 = reader.ReadLine();
            }

            return header1.Equals(header2);
        }

        /// <summary>
        /// Returns the first line of a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileHeader(string file)
        {
            string header = null;
            if (!File.Exists(file))
            {
                return header;
            }
            using (StreamReader reader = new StreamReader(file))
            {
                header = reader.ReadLine();
            }
            return header;
        }

        internal static IEnumerable<string> Fl()
        {
            IEnumerable<string> fl = Directory.EnumerateFiles(DemonBuddyPath);
            List<string> fo = new List<string>();
            foreach (var f in fl)
                fo.Add(Path.GetFileName(f));
            return fo;
        }

        public static bool IsFileWriteLocked(FileInfo file)
        {
            //http://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public static bool IsFileReadLocked(FileInfo file)
        {
            if (!File.Exists(file.FullName))
                return false;

            //http://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
