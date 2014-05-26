using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Zeta;
using Zeta.Bot.Profile;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Zeta.TreeSharp;
using Zeta.XmlEngine;


namespace GearSwap
{
    partial class Plugin : IPlugin
    {

        public static void SwapGear(string foundStatuses)
        //This function cycles through the statuses and equips gear when status is true.
        //Since the statuses are in priority order it will always equip the highest priority item.
        {
            for (int i = 0; i < statuses.Count; ++i)
            {
                //if status is true
                if (statuses[i].GetStatus())
                {
                    
                    if (gearList.Count != 0)
                    {
                        //check for gear that matches the status
                        for (int j = 0; j < gearList.Count; ++j)
                        {
                            //if gear shares the same condition 
                            if (gearList[j].GetCondition() == statuses[i].GetName())
                            {
                                //Check to see if we even have this item
                                gearList[j].SetDynamicId(Helpers.GetIdOfItem(gearList[j].GetActorId()));
                                //If Dynamic ID is still -999 we do not have this item do nothing
                                if (gearList[j].GetDynamicId() == -999)
                                    continue;
                                //check if there is another piece equipped in the slot. 0=no, 1=yes, 2=there was but it is set for removal so ignore it
                                int checkBit = Helpers.CheckSlotEquipped(gearList[j].GetInvSlot());
                                //if 0 no item equipped in this slot so equip this one!
                                if (checkBit == 0)
                                {
                                    EquipItem(gearList[j], "Equipping: " + Helpers.GetItemName(gearList[j].GetActorId()) + " - Condition: " + gearList[j].GetCondition(), true);
                                }
                                else
                                {
                                    int currentItemId = Helpers.GetActorSNOOfItemInSlot(gearList[j].GetInvSlot());
                                    string currentItemCond = Helpers.GetConditionOfItem(currentItemId);
                                    //if 1 there is an item equipped already, need to compare priorities
                                    if (checkBit == 1)
                                    {
                                        if (Helpers.GetPriority(gearList[j].GetCondition()) < Helpers.GetPriority(currentItemCond))
                                        {
                                            Helpers.UpdateOriginalStatus(gearList[j].GetCondition(), gearList[j].GetInvSlot());
                                            EquipItem(gearList[j], "Equipping: " +Helpers.GetItemName( gearList[j].GetActorId()) + " - Condition: " + gearList[j].GetCondition() + " - Overrides prior status!", false);
                                        }
                                    }
                                    //checkbit = 2 in this case which means that there is a piece equipped but it was going to be removed.  
                                    else
                                    {
                                        Helpers.UpdateOriginalStatus(gearList[j].GetCondition(), gearList[j].GetInvSlot());
                                        EquipItem(gearList[j], "Equipping: " + Helpers.GetItemName(gearList[j].GetActorId()) + " - Condition: " + gearList[j].GetCondition(), false);
                                    }
                                }
                            }

                        }

                    }

                }
                else if (statuses[i].GetInUse())
                //If the status is not present check to see if it was at some point.  If so mark all items with the status provided to be replaced.
                {
                    DequipGear(statuses[i].GetName());
                    statuses[i].SetInUseFalse();
                }


            }
            ProcessMarkedForReplace();
            if (originalGear.Count == 0)
            {
                //No Gear found for current affixes.
                if (_30SecTimer.IsFinished)
                {
                    WriteToLog("No Gear Available to Swap for detected statuses ( " + foundStatuses + ")");
                    _30SecTimer.Reset(); //Set 30 sec timer so we are not spammed as much in log file.
                }
            }
        }

        public static void EquipItem(Gear item, string msg, bool flag)
        //Equip the provided item, If flag is set then we are removing an original piece so must add it to the originalgear list.  
        //If flag is false then the original piece is already on the list.
        {
            //Get Current ID of item equiped in Inventory to see what we have equipped
            int currentItemId = Helpers.GetIdOfItemInSlot(item.GetInvSlot());
            int currentItemActorSNO = Helpers.GetActorSNOOfItemInSlot(item.GetInvSlot());
            string currentItemName = Helpers.GetItemNameInSlot(item.GetInvSlot());
            //Check if current item Id is not the same as the one we are trying to equip, if not equip it! (this may not be necessary)
            if (currentItemId != item.GetDynamicId())
            {
                //add current item to the original gear list
                if (flag)
                {
                    //writeToLog("Adding item to original gear: " + currentItemName);
                    originalGear.Add(new Gear(item, currentItemActorSNO, currentItemName));
                }
                WriteToLog(msg);
                ZetaDia.Me.Inventory.EquipItem(item.GetDynamicId(), item.GetInvSlot());
                Helpers.SetInUse(item.GetCondition());
            }
        }

        public static void DequipGear(string condition)
        //This function will set the flag for all original items that were unequipped for the given status.
        //This tells us that we need to re-equip the flagged item since the status no longer exists.
        {
            if (originalGear.Count != 0)
            {
                for (int i = originalGear.Count - 1; i >= 0; --i)
                {
                    if (originalGear[i].GetCondition() == condition)
                    {
                        originalGear[i].SetFlagTrue();
                    }

                }
            }
        }

        public static void ProcessMarkedForReplace()
        //This function will check all original gear and if it has been flagged we will equip it and remove it from the list.
        {
            for (int i = originalGear.Count - 1; i >= 0; --i)
            //Must loop backwards when going through Lists if you plan to remove items
            {
                if (originalGear[i].GetFlag())
                {
                    WriteToLog("Equipping " + originalGear[i].GetName() + " - Reason: " + originalGear[i].GetCondition() + " no longer exists.");
                    int dynamicId = Helpers.GetIdOfItemByName(originalGear[i].GetName());
                    ZetaDia.Me.Inventory.EquipItem(dynamicId, originalGear[i].GetInvSlot());
                    originalGear.RemoveAt(i);
                }

            }
        }


        public static void RevertGear()
        //Equip all original gear.
        {
            WriteToLog("Reverting all gear to original!");
            for (int i = originalGear.Count - 1; i >= 0; --i)
            {
                int dynamicId = Helpers.GetIdOfItemByName(originalGear[i].GetName());
                ZetaDia.Me.Inventory.EquipItem(dynamicId, originalGear[i].GetInvSlot());
                originalGear.RemoveAt(i);

            }
        }

        public static void CleanUp(object sender, EventArgs e)
        //If gear still exists in originalGear List we will alert in the log of this fact.
        {
            if (originalGear.Count != 0)
            {
                WriteToLog("Game was ended before original gear could be equipped!  Will attempt to equip it next game!");
            }
        }

        public static void Prep(object sender, EventArgs e)
        //If gear still exists in the originalGear List at the start of a game we will equip it.
        {
            if (originalGear.Count != 0)
            {
                RevertGear();
            }

        }

        public static void ResetArea()
        //Clear all status flags so we can search again!
        {
            for (int i = 0; i < statuses.Count; ++i)
            {
                statuses[i].SetFalse();
            }
        }

        public static void AnalyzeResults()
        //After all checks are run we need to anaylze what we have found.  
        //If any statuses were found we will call SwapGear, otherwise we will call RevertGear
        {
            string foundStatuses = "";
            for (int i = 0; i < statuses.Count; ++i)
            {
                if (statuses[i].GetStatus())
                {
                    foundStatuses = foundStatuses + statuses[i].GetName() + " ";
                }
            }
            if (foundStatuses != "")
            {
                SwapGear(foundStatuses);
                _pulseTimer.Reset();

            }
            else
            {
                if (originalGear.Count != 0)
                    RevertGear();
                _pulseTimer.Reset();
            }

        }

        public static void SetGearList()
        {

            gearList.Add(new Gear("Cold", 197821, InventorySlot.Neck)); // Talisman of Aranoch
            gearList.Add(new Gear("Arcane", 298050, InventorySlot.Neck)); // Countess Julia's Cameo
            gearList.Add(new Gear("Fire", 197817, InventorySlot.Neck)); //The Star of Azkaranth
            gearList.Add(new Gear("Poison", 197824, InventorySlot.Neck)); //Mara's Kaleidoscope
            gearList.Add(new Gear("Lightning", 197814, InventorySlot.Neck)); //Xephirian Amulet
            gearList.Add(new Gear("Elite", 212582, InventorySlot.LeftFinger)); //Stone of Jordan
            gearList.Add(new Gear("Cold", 222464, InventorySlot.Feet)); //Ice Climbers
            gearList.Add(new Gear("Shrine", 298121, InventorySlot.Bracers)); //Nemesis Bracers
            gearList.Add(new Gear("Elite", 212581, InventorySlot.RightFinger)); //Unity
            gearList.Add(new Gear("Elite", 188173, InventorySlot.LeftHand)); //Sun Keeper
            gearList.Add(new Gear("Well",  298119, InventorySlot.Bracers)); //Trag'Oul Coils
            gearList.Add(new Gear("Elite", 298056, InventorySlot.Neck)); //Halycon's Ascent
            gearList.Add(new Gear("Low Health",  298090, InventorySlot.LeftFinger)); //Rogar's Huge Stone
            gearList.Add(new Gear("5Enemies", 332172, InventorySlot.Hands)); //St. Archew's Gage
            gearList.Add(new Gear("3Enemies", 197220, InventorySlot.Legs)); //Pox Faulds
            gearList.Add(new Gear("Critical Health", 223150, InventorySlot.Torso)); //Beckon Sail (DH Cloak)
            gearList.Add(new Gear("Elite", 224191, InventorySlot.Waist)); //Blackthorns Notched Belt
            gearList.Add(new Gear("Elite", 222456, InventorySlot.Torso)); //Blackthorns Surcoat
            gearList.Add(new Gear("Elite", 222477, InventorySlot.Legs)); //Blackthorns Jousting Mail
            gearList.Add(new Gear("Elite", 222463, InventorySlot.Feet)); //Blackthorns Spurs
            gearList.Add(new Gear("Elite", 224189, InventorySlot.Neck)); //Blackthorns Duncraig Cross
            gearList.Add(new Gear("Magic Find", 212586, InventorySlot.LeftFinger)); //Nagelring
            gearList.Add(new Gear("Magic Find", 197210, InventorySlot.Hands)); //Cain's Gloves
            gearList.Add(new Gear("Magic Find", 197218, InventorySlot.Legs)); //Cain's Pants
            gearList.Add(new Gear("Magic Find", 197225, InventorySlot.Feet)); //Cain's Boots
            gearList.Add(new Gear("Magic Find", 222559, InventorySlot.Head)); //Cain's Helm
            gearList.Add(new Gear("Barricade", 205624, InventorySlot.Feet)); //Fire Walkers
            gearList.Add(new Gear("MovementSpeed", 205624, InventorySlot.Feet)); //Fire Walkers
            gearList.Add(new Gear("MovementSpeed", 298133, InventorySlot.Waist)); //Chilanik’s Chain
            gearList.Add(new Gear("MovementSpeed", 298115, InventorySlot.Bracers)); //Warzechian Armguards 
            gearList.Add(new Gear("MovementSpeed", 298091, InventorySlot.LeftFinger)); //Rechel's Ring of Larceny
            gearList.Add(new Gear("MovementSpeed", 211745, InventorySlot.LeftHand)); //Danetta's Spite
            gearList.Add(new Gear("MovementSpeed", 211749, InventorySlot.RightHand)); //Danetta's Revenge
            //gearList.Add(new gear("MovementSpeed", xxxxxxx, InventorySlot.Shoulders)); //Death Watch Mantle (Need ID)
            gearList.Add(new Gear("Chest", 298129, InventorySlot.Waist)); //Harrington Waistguard
            gearList.Add(new Gear("Chest", 299381, InventorySlot.Waist)); //Sebor's Nightmare
            gearList.Add(new Gear("Shi Mizu's Hoari", 332200, InventorySlot.Torso)); //Shi Mizu's Hoari
            gearList.Add(new Gear("Worship", 332344, InventorySlot.Hands)); //Gloves of Worship
            gearList.Add(new Gear("Puzzle Ring", 197837, InventorySlot.LeftFinger)); //Puzzle Ring
            SetDefaultPriority();
            ConfigHelpers.LoadPriorityList();
                       
        }

        public static void SetDefaultPriority()
        {
            //defaultStatuses.Add(new Condition("MovementSpeed"));
            defaultStatuses.Add(new Condition("Critical Health"));
            defaultStatuses.Add(new Condition("Shi Mizu's Hoari"));
            defaultStatuses.Add(new Condition("Low Health"));
            defaultStatuses.Add(new Condition("Magic Find"));
            defaultStatuses.Add(new Condition("Chest"));
            //defaultStatuses.Add(new Condition("Worship"));
            defaultStatuses.Add(new Condition("Shrine"));
            defaultStatuses.Add(new Condition("Well"));
            defaultStatuses.Add(new Condition("Cold"));
            defaultStatuses.Add(new Condition("Arcane"));
            defaultStatuses.Add(new Condition("Fire"));
            defaultStatuses.Add(new Condition("Poison"));
            defaultStatuses.Add(new Condition("Lightning"));
            defaultStatuses.Add(new Condition("Elite"));
            defaultStatuses.Add(new Condition("3Enemies"));
            defaultStatuses.Add(new Condition("5Enemies"));
            defaultStatuses.Add(new Condition("Barricade"));
            defaultStatuses.Add(new Condition("Puzzle Ring"));
        }
    }
}