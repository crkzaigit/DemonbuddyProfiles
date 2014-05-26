using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Input;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.SNO;
using Zeta.Game.Internals.Actors;
using Zeta.XmlEngine;
using Zeta.TreeSharp;
using Zeta.Bot.Profile;

namespace GearSwap
{
    class ConfigHelpers
    {
        public static void PopulatePriorityList()
        //This populates our priority list in the config menu
        {
            try
            {
                for (int i = 0; i < Plugin.statuses.Count; i++)
                {
                    Config.priority.Items.Add(Plugin.statuses[i].GetName());

                }
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Unable to populate priority list!", e);
            }

        }
        public static void LoadPriorityList()
        //This will attempt to load the custom priority list if it exists.
        {
            try
            {
                string assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string priorityPath = Path.Combine(assemblyPath, "Settings", "GearSwap", GearSwapSettings.BattleTagName, "Priority.txt");
                if (File.Exists(priorityPath))
                {
                    StreamReader file = new StreamReader(priorityPath);
                    string line = "";
                    while ((line = file.ReadLine()) != null)
                    {
                        if (IsInDefaults(line))
                            Plugin.statuses.Add(new Condition(line));
                        else
                            Plugin.WriteToLog("Status: " + line + " does not exist! Skipping!");
                    }

                    file.Close();
                    if (Plugin.statuses.Count() > Plugin.defaultStatuses.Count)
                    {
                        Plugin.WriteToLog("Too many statuses in Priority.txt!  Reverting to default priority list!");
                        Plugin.statuses = Plugin.defaultStatuses;
                    }
                    else if (Plugin.statuses.Count() < Plugin.defaultStatuses.Count)
                    {
                        Plugin.WriteToLog("Priority List is missing some statuses!  Adding extra statuses to the end!  You may want to check the config menu to modify the new priorities!");
                        UpdateStatuses();
                    }
                    else
                        Plugin.WriteToLog("Priority List successfully loaded!");

                }
                else
                {
                    Plugin.statuses = Plugin.defaultStatuses;
                }
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Failed to load file Priority.txt! Using default priorities instead!", e);
                Plugin.statuses = Plugin.defaultStatuses;
            }
        }

        public static void UpdateStatuses()
        {
            //Adds any missing priorities to the end of the current status list.
            bool flag = false;
            for (int i = 0; i < Plugin.defaultStatuses.Count(); i++)
            {
                for (int j = 0; j < Plugin.statuses.Count(); j++)
                {
                    if (Plugin.statuses[j].GetName() == Plugin.defaultStatuses[i].GetName())
                        flag = true;
                }
                if (!flag)
                {
                    Plugin.statuses.Add(new Condition(Plugin.defaultStatuses[i].GetName()));
                }
                flag = false;
            }
            SavePriorityList();
        }
        public static void SavePriorityList()
        //Write the new priority list priority.txt
        {
            Plugin.WriteToLog("Writing new priority list to file Priority.txt!");
            try
            {
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string priorityPath = Path.Combine(exePath, "Settings", "GearSwap", GearSwapSettings.BattleTagName);
                if (!Directory.Exists(priorityPath))
                    Directory.CreateDirectory(priorityPath);
                string priorityFile = Path.Combine(priorityPath, "Priority.txt");
                if (File.Exists(priorityFile))
                    File.Delete(priorityFile);
                StreamWriter file = new StreamWriter(priorityFile);
                for (int i = 0; i < Plugin.statuses.Count; i++)
                {
                    file.WriteLine(Plugin.statuses[i].GetName());
                }
                file.Close();

            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Was Unable to write to file, you will need to open config and try again!", e);
            }
        }
        public static void UpdatePriorityList()
        //Write the new priority list priority.txt
        {
            for (int i = 0; i < Config.priority.Items.Count; i++)
            {
                Plugin.statuses[i].SetName(Config.priority.Items[i].ToString());
            }
            Plugin.priorityUpdated = false;

            Plugin.WriteToLog("Writing new priority list to file Priority.txt!");

            try
            {
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string priorityPath = Path.Combine(exePath, "Settings", "GearSwap", GearSwapSettings.BattleTagName);
                if (!Directory.Exists(priorityPath))
                    Directory.CreateDirectory(priorityPath);
                string priorityFile = Path.Combine(priorityPath, "Priority.txt");
                if (File.Exists(priorityFile))
                    File.Delete(priorityFile);
                StreamWriter file = new StreamWriter(priorityFile);
                for (int i = 0; i < Plugin.statuses.Count; i++)
                {
                    file.WriteLine(Plugin.statuses[i].GetName());
                }
                file.Close();

            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Was Unable to write to file, you will need to open config and try again!", e);
            }
        }

        public static bool IsInDefaults(string n)
        //searches the defaultStatus List and returns true if provided string is in there.
        {
            for (int i = 0; i < Plugin.defaultStatuses.Count; i++)
            {
                if (n == Plugin.defaultStatuses[i].GetName())
                    return true;

            }
            return false;
        }

        public static void IncreasePriority()
        //Increase priority in the config menu.
        {
            Plugin.priorityUpdated = true;
            MoveItem(-1);

        }

        public static void DecreasePriority()
        //Decrease priority in the config menu.
        {
            Plugin.priorityUpdated = true;
            MoveItem(1);
        }

        public static void MoveItem(int direction)
        {
            // Checking selected item
            if (Config.priority.SelectedItem == null || Config.priority.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = Config.priority.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= Config.priority.Items.Count)
                return; // Index out of range - nothing to do

            object selected = Config.priority.SelectedItem;

            // Removing removable element
            Config.priority.Items.Remove(selected);
            // Insert it in new position
            Config.priority.Items.Insert(newIndex, selected);
            // Restore selection
            Config.priority.SelectedIndex = newIndex;
        }
        public static void ResetDefaults()
        {
            Config.priority.Items.Clear();
            for (int i = 0; i < Plugin.defaultStatuses.Count; i++)
            {
                Config.priority.Items.Add(Plugin.defaultStatuses[i].GetName());
            }
            Plugin.priorityUpdated = true;
        }
    }
}