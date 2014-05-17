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



    partial class gearSwap : IPlugin
    {
        public static void dumpActorSNO()
        {
            ZetaDia.Actors.Update();
            using (ZetaDia.Memory.SaveCacheState())
            {
                foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
                {
                    Log.Info("[GearSwap] Item: " + i.Name + " - ActorSNO: " + i.ActorSNO);
                }
                foreach (ACDItem i in ZetaDia.Me.Inventory.Backpack)
                {
                    Log.Info("[GearSwap] Item: " + i.Name + " - ActorSNO: " + i.ActorSNO);
                }
            }
        }

        public static void printActorInfo()
        {
            ZetaDia.Actors.Update();
            using (ZetaDia.Memory.SaveCacheState())
            {
                foreach (ACD obj in ZetaDia.Actors.GetActorsOfType<ACD>(true, false))
                {
                    writeToLog(obj.Name + " : " + obj.MonsterInfo.MonsterType);
                }
            }
        }
        public void printItemStats()
        {
            foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
            {
                writeToLog(i.ActorSNO + " " + i.Name);
            }

        }

        //function to return the DynamicId of the named item if it is in your inventory or equipped
        public static int getIdOfItem(int id)
        {
            //First Check Equipped
            foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
            {
                if (i.ActorSNO == id)
                {
                    return i.DynamicId;
                }
            }
            foreach (ACDItem i in ZetaDia.Me.Inventory.Backpack)
            {
                if (i.ActorSNO == id)
                {
                    return i.DynamicId;
                }
            }
            return -999;
        }

        public static int getIdOfItemByName(string name)
        {

            foreach (ACDItem i in ZetaDia.Me.Inventory.Backpack)
            {
                if (i.Name == name)
                {
                    return i.DynamicId;
                }
            }
            foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
            {
                if (i.Name == name)
                {
                    return i.DynamicId;
                }
            }

            return -999;
        }

        public static string getItemName(int id)
        {

            foreach (ACDItem i in ZetaDia.Me.Inventory.Backpack)
            {
                if (i.ActorSNO == id)
                {
                    return i.Name;
                }
            }
            foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
            {
                if (i.ActorSNO == id)
                {
                    return i.Name;
                }
            }

            return "Not Found";
        }


        //function to return dynamicId of item in specific slot
        public static int getIdOfItemInSlot(InventorySlot slot)
        {
            foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
            {
                if (i.InventorySlot == slot)
                {
                    return i.DynamicId;
                }
            }
            return -999;
        }

        public static string getItemNameInSlot(InventorySlot slot)
        {
            foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
            {
                if (i.InventorySlot == slot)
                {
                    return i.Name;
                }
            }
            return "";
        }


        public static int getActorSNOOfItemInSlot(InventorySlot slot)
        {
            foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
            {
                if (i.InventorySlot == slot)
                {
                    return i.ActorSNO;
                }
            }
            return 0;
        }

        //check to see if there is an item in the inventory with the provided dynamic id
        public static bool existsInInventory(int id)
        {
            foreach (ACDItem i in ZetaDia.Me.Inventory.Backpack)
            {
                if (i.DynamicId == id)
                {
                    return true;
                }
            }
            return false;
        }

        public static void setTrue(string condition)
        {
            for (int i = 0; i < statuses.Count; ++i)
            {
                if (statuses[i].getName() == condition)
                    statuses[i].setTrue();
            }
        }

        public static void setInUse(string condition)
        {
            for (int i = 0; i < statuses.Count; ++i)
            {
                if (statuses[i].getName() == condition)
                    statuses[i].setInUseTrue();
            }
        }


        public static int checkSlotEquipped(InventorySlot slot)
        {

            if (originalGear.Count == 0)
                return 0;
            else
            {
                for (int i = 0; i < originalGear.Count; ++i)
                {
                    //check if another higher piece is equipped in the same slot
                    if (originalGear[i].getInvSlot() == slot)
                    {
                        if (originalGear[i].getFlag())
                            return 2;
                        else
                            return 1;
                    }
                }
            }
            return 0;

        }



        public static int getPriority(string name)
        {
            for (int i = 0; i < statuses.Count; i++)
            {
                if (statuses[i].getName() == name)
                    return i;
            }
            return -1;
        }

        public static string getConditionOfItem(int id)
        {
            for (int i = 0; i < gearList.Count; i++)
            {
                if (gearList[i].getActorId() == id)
                    return gearList[i].getCondition();
            }
            return "";
        }

        public static void updateOriginalStatus(string cond, InventorySlot slot)
        {
            //writeToLog("Updating Status of Item in Slot: " + slot + " to " + cond);
            for (int i = 0; i < originalGear.Count; i++)
            {
                if (originalGear[i].getInvSlot() == slot)
                {
                    originalGear[i].setCond(cond);
                    //writeToLog("Item found: " + originalGear[i].getItemName() + " - Setting Condition");
                    //printGear(originalGear);
                }
            }
        }

        public static void printGear(List<gear> gearSet)
        {
            for (int i = 0; i < gearSet.Count; i++)
            {
                writeToLog(gearSet[i].display());

            }
        }

        public static void swapGear(string foundStatuses)
        {
            for (int i = 0; i < statuses.Count; ++i)
            {
                //if status is true
                if (statuses[i].getStatus())
                {
                    //check for gear 
                    if (gearList.Count != 0)
                    {
                        for (int j = 0; j < gearList.Count; ++j)
                        {
                            //if gear shares the same condition 
                            if (gearList[j].getCondition() == statuses[i].getName())
                            {
                                //Check to see if we even have this item
                                gearList[j].setDynamicId(getIdOfItem(gearList[j].getActorId()));
                                //If Dynamic ID is still -999 we do not have this item do nothing
                                if (gearList[j].getDynamicId() == -999)
                                    continue;
                                int checkBit = checkSlotEquipped(gearList[j].getInvSlot());
                                //if another higher priority item is not already equipped in the same slot, equip this one 
                                if (checkBit == 0)
                                {
                                    equipItem(gearList[j], "Equipping: " + getItemName(gearList[j].getActorId()) + " - Condition: " + gearList[j].getCondition(), i, true);
                                }
                                else
                                {
                                    int currentItemId = getActorSNOOfItemInSlot(gearList[j].getInvSlot());
                                    string currentItemCond = getConditionOfItem(currentItemId);
                                    if (checkBit == 1)
                                    {
                                        if (getPriority(gearList[j].getCondition()) < getPriority(currentItemCond))
                                        {
                                            updateOriginalStatus(gearList[j].getCondition(), gearList[j].getInvSlot());
                                            equipItem(gearList[j], "Equipping: " +getItemName( gearList[j].getActorId()) + " - Condition: " + gearList[j].getCondition() + " - Overrides prior status!", i, false);
                                        }
                                    }
                                    else
                                    {
                                        updateOriginalStatus(gearList[j].getCondition(), gearList[j].getInvSlot());
                                        equipItem(gearList[j], "Equipping: " + getItemName(gearList[j].getActorId()) + " - Condition: " + gearList[j].getCondition(), i, false);
                                    }
                                }
                            }

                        }

                    }

                }
                else if (statuses[i].getInUse())
                {
                    dequipGear(statuses[i].getName());
                    statuses[i].setInUseFalse();
                }


            }
            processMarkedForReplace();
            if (originalGear.Count == 0)
            {
                //No Gear found for current affixes.
                if (_30SecTimer.IsFinished)
                {
                    writeToLog("No Gear Available to Swap for detected statuses ( " + foundStatuses + ")");
                    _30SecTimer.Reset(); //Set 30 sec timer so we are not spammed as much in log file.
                }
            }
        }

        public static void equipItem(gear item, string msg, int priority, bool flag)
        {
            //item.setDynamicId(getIdOfItem(item.getActorId()));
            //If Dynamic ID is still -999 we do not have this item do nothing
            //if (item.getDynamicId() == -999)
            //    return;

            //Get Current ID of item equiped in Inventory to see what we have equipped
            int currentItemId = getIdOfItemInSlot(item.getInvSlot());
            int currentItemActorSNO = getActorSNOOfItemInSlot(item.getInvSlot());
            string currentItemName = getItemNameInSlot(item.getInvSlot());
            //Check if current item Id is not the same as the one we are trying to equip, if not equip it!
            if (currentItemId != item.getDynamicId())
            {
                //add current item to the original gear list
                if (flag)
                {
                    //writeToLog("Adding item to original gear: " + currentItemName);
                    originalGear.Add(new gear(item, currentItemActorSNO, currentItemName));
                }
                writeToLog(msg);
                ZetaDia.Me.Inventory.EquipItem(item.getDynamicId(), item.getInvSlot());
                setInUse(item.getCondition());
            }
        }

        public static void dequipGear(string condition)
        {
            if (originalGear.Count != 0)
            {
                for (int i = originalGear.Count - 1; i >= 0; --i)
                {
                    if (originalGear[i].getCondition() == condition)
                    {
                        originalGear[i].setFlagTrue();
                        //writeToLog(originalGear[i].getItemName() + " has been marked for removal!");
                    }

                }
            }
        }

        public static void processMarkedForReplace()
        {
            for (int i = originalGear.Count - 1; i >= 0; --i)
            {
                if (originalGear[i].getFlag())
                {
                    writeToLog("Equipping " + originalGear[i].getName() + " - Reason: " + originalGear[i].getCondition() + " no longer exists.");
                    int dynamicId = getIdOfItemByName(originalGear[i].getName());
                    ZetaDia.Me.Inventory.EquipItem(dynamicId, originalGear[i].getInvSlot());
                    originalGear.RemoveAt(i);
                }

            }
        }


        //Equip all original gear
        public static void revertGear()
        {
            writeToLog("Reverting all gear to original!");
            for (int i = originalGear.Count - 1; i >= 0; --i)
            {
                int dynamicId = getIdOfItemByName(originalGear[i].getName());
                ZetaDia.Me.Inventory.EquipItem(dynamicId, originalGear[i].getInvSlot());
                originalGear.RemoveAt(i);

            }
        }

        public static void cleanUp(object sender, EventArgs e)
        {
            if (originalGear.Count != 0)
            {
                writeToLog("Game was ended before original gear could be equipped!  Will attempt to equip it next game!");
            }
        }

        public static void prep(object sender, EventArgs e)
        {
            if (originalGear.Count != 0)
            {
                revertGear();
            }

        }

        public static void resetArea()
        {
            for (int i = 0; i < statuses.Count; ++i)
            {
                statuses[i].setFalse();
            }
        }

        //check if there is an elite/rare in the area.
        

        public static void analyzeResults()
        {
            string foundStatuses = "";
            for (int i = 0; i < statuses.Count; ++i)
            {
                if (statuses[i].getStatus())
                {
                    foundStatuses = foundStatuses + statuses[i].getName() + " ";
                }
            }
            if (foundStatuses != "")
            {
                swapGear(foundStatuses);
                _1SecTimer.Reset();

            }
            else
            {
                if (originalGear.Count != 0)
                    revertGear();
                _1SecTimer.Reset();
            }

        }

        public static void increasePriority()
        {
            priorityUpdated = true;
            moveItem(-1);
        
        }

        public static void decreasePriority()
        {
            priorityUpdated = true;
            moveItem(1);
        }

        public static void moveItem(int direction)
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

        public static void populatePriorityList()
        {
            for (int i = 0; i < statuses.Count; i++)
            {
                Config.priority.Items.Add(statuses[i].getName());

            }

        }
        public static void loadPriorityList()
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
                        if (isInDefaults(line))
                            statuses.Add(new condition(line));
                        else
                            writeToLog("Status: " + line + " does not exist! Skipping!");
                    }

                    file.Close();
                    if (statuses.Count() > defaultStatuses.Count)
                    {
                        writeToLog("Too many statuses in Priority.txt!  Reverting to default priority list!");
                        statuses = defaultStatuses;
                    }
                    else if (statuses.Count() < defaultStatuses.Count)
                    {
                        writeToLog("Priority List is missing some statuses!  Adding extra statuses to the end!  You may want to check the config menu to modify the new priorities!");
                        updateStatuses();
                    }
                    else
                        writeToLog("Priority List successfully loaded!");

                }
                else
                {
                    statuses = defaultStatuses;
                }
            }
            catch (Exception e)
            {
                writeToLog("Failed to load file Priority.txt! Using default priorities instead!", e);
                statuses = defaultStatuses;
            }
        }

        public static void updateStatuses()
        {
            bool flag = false;
            for (int i = 0; i < defaultStatuses.Count; i++)
            {
                for (int j = 0; j < statuses.Count; i++)
                {
                    if (statuses[j].getName() == defaultStatuses[i].getName())
                        flag = true;
                }
                if (!flag)
                {
                    statuses.Add(new condition(defaultStatuses[i].getName()));
                }
                flag = false;
            }
        }

        public static void updatePriorityList()
        {
            for (int i = 0; i < Config.priority.Items.Count; i++)
            {
                statuses[i].setName(Config.priority.Items[i].ToString());
            }
            priorityUpdated = false;

            writeToLog("Writing new priority list to file Priority.txt!");

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
                for (int i = 0; i < statuses.Count; i++)
                {
                    file.WriteLine(statuses[i].getName());
                }
                file.Close();

            }
            catch (Exception e)
            {
                writeToLog("Was Unable to write to file, you will need to open config and try again!");
                writeToLog("Was Unable to write to file, you will need to open config and try again!", e);
            }




        }

        public static bool isInDefaults(string n)
        {
            for (int i = 0; i < defaultStatuses.Count; i++)
            {
                if (n == defaultStatuses[i].getName())
                    return true;

            }
            return false;
        }

        public static void setGearList()
        {

            gearList.Add(new gear("Cold", 197821, InventorySlot.Neck)); // Talisman of Aranoch
            gearList.Add(new gear("Arcane", 298050, InventorySlot.Neck)); // Countess Julia's Cameo
            gearList.Add(new gear("Fire", 197817, InventorySlot.Neck)); //The Star of Azkaranth
            gearList.Add(new gear("Poison", 197824, InventorySlot.Neck)); //Mara's Kaleidoscope
            gearList.Add(new gear("Lightning", 197814, InventorySlot.Neck)); //Xephirian Amulet
            gearList.Add(new gear("Elite", 212582, InventorySlot.LeftFinger)); //Stone of Jordan
            gearList.Add(new gear("Cold", 222464, InventorySlot.Feet)); //Ice Climbers
            gearList.Add(new gear("Shrine", 298121, InventorySlot.Bracers)); //Nemesis Bracers
            gearList.Add(new gear("Elite", 212581, InventorySlot.RightFinger)); //Unity
            gearList.Add(new gear("Elite", 188173, InventorySlot.LeftHand)); //Sun Keeper
            gearList.Add(new gear("Well",  298119, InventorySlot.Bracers)); //Trag'Oul Coils
            gearList.Add(new gear("Elite", 298056, InventorySlot.Neck)); //Halycon's Ascent
            gearList.Add(new gear("Low Health",  298090, InventorySlot.LeftFinger)); //Rogar's Huge Stone
            gearList.Add(new gear("5Enemies", 332172, InventorySlot.Hands)); //St. Archew's Gage
            gearList.Add(new gear("3Enemies", 197220, InventorySlot.Legs)); //Pox Faulds
            gearList.Add(new gear("Critical Health", 223150, InventorySlot.Torso)); //Beckon Sail (DH Cloak)
            gearList.Add(new gear("Elite", 224191, InventorySlot.Waist)); //Blackthorns Notched Belt
            gearList.Add(new gear("Elite", 222456, InventorySlot.Torso)); //Blackthorns Surcoat
            gearList.Add(new gear("Elite", 222477, InventorySlot.Legs)); //Blackthorns Jousting Mail
            gearList.Add(new gear("Elite", 222463, InventorySlot.Feet)); //Blackthorns Spurs
            gearList.Add(new gear("Elite", 224189, InventorySlot.Neck)); //Blackthorns Duncraig Cross
            gearList.Add(new gear("Magic Find", 212586, InventorySlot.LeftFinger)); //Nagelring
            gearList.Add(new gear("Magic Find", 197210, InventorySlot.Hands)); //Cain's Gloves
            gearList.Add(new gear("Magic Find", 197218, InventorySlot.Legs)); //Cain's Pants
            gearList.Add(new gear("Magic Find", 197225, InventorySlot.Feet)); //Cain's Boots
            gearList.Add(new gear("Magic Find", 222559, InventorySlot.Head)); //Cain's Helm
            gearList.Add(new gear("Barricade", 205624, InventorySlot.Feet)); //Fire Walkers
            gearList.Add(new gear("MovementSpeed", 205624, InventorySlot.Feet)); //Fire Walkers
            gearList.Add(new gear("MovementSpeed", 298133, InventorySlot.Waist)); //Chilanik’s Chain
            gearList.Add(new gear("MovementSpeed", 298115, InventorySlot.Bracers)); //Warzechian Armguards 
            gearList.Add(new gear("MovementSpeed", 298091, InventorySlot.LeftFinger)); //Rechel's Ring of Larceny
            gearList.Add(new gear("MovementSpeed", 211745, InventorySlot.LeftHand)); //Danetta's Spite
            gearList.Add(new gear("MovementSpeed", 211749, InventorySlot.RightHand)); //Danetta's Revenge
            //gearList.Add(new gear("MovementSpeed", xxxxxxx, InventorySlot.Shoulders)); //Death Watch Mantle (Need ID)
            gearList.Add(new gear("Chest", 298129, InventorySlot.Waist)); //Harrington Waistguard
            setDefaultPriority();
            loadPriorityList();
                       
        }

        public static void setDefaultPriority()
        {
            defaultStatuses.Add(new condition("MovementSpeed"));
            defaultStatuses.Add(new condition("Critical Health"));
            defaultStatuses.Add(new condition("Low Health"));
            defaultStatuses.Add(new condition("Magic Find"));
            defaultStatuses.Add(new condition("Chest"));
            defaultStatuses.Add(new condition("Shrine"));
            defaultStatuses.Add(new condition("Well"));
            defaultStatuses.Add(new condition("Cold"));
            defaultStatuses.Add(new condition("Arcane"));
            defaultStatuses.Add(new condition("Fire"));
            defaultStatuses.Add(new condition("Poison"));
            defaultStatuses.Add(new condition("Lightning"));
            defaultStatuses.Add(new condition("Elite"));
            defaultStatuses.Add(new condition("3Enemies"));
            defaultStatuses.Add(new condition("5Enemies"));
            defaultStatuses.Add(new condition("Barricade"));
        }
    }
}