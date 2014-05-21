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
    class Helpers
    {

        public static void DumpActorSNO()
        //Print ActorSNO of all items in inventory and equipped
        {
            try
            {
                ZetaDia.Actors.Update();
                using (ZetaDia.Memory.SaveCacheState())
                {
                    Plugin.WriteToLog("Equipped Items:");
                    foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
                    {
                        Plugin.WriteToLog("Item: " + i.Name + " - ActorSNO: " + i.ActorSNO);
                    }
                    Plugin.WriteToLog("Inventory Items:");
                    foreach (ACDItem i in ZetaDia.Me.Inventory.Backpack)
                    {
                        Plugin.WriteToLog("Item: " + i.Name + " - ActorSNO: " + i.ActorSNO);
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Unable to parse inventory!", e);
            }
        }
        public static int GetIdOfItem(int SNO)
        //Return the DynamicId of the named item if it is in your inventory (Do not check equipped items)
        {
            try
            {
                foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
                {
                    if (i.ActorSNO == SNO)
                    {
                        return i.DynamicId;
                    }
                }
                foreach (ACDItem i in ZetaDia.Me.Inventory.Backpack)
                {
                    if (i.ActorSNO == SNO)
                    {
                        return i.DynamicId;
                    }
                }
            }
            catch(Exception e)
            {
                Plugin.WriteToLog("Unable to parse inventory for item with ActorSNO: " + SNO, e);
            }
            return -999;
        }

        public static int GetIdOfItemByName(string name)
        //Searches your inventory and equipped items for an item with the provided name then returns its dynamicID, it will return -999 if you do not have this item.
        {
            try
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
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Unable to parse inventory for item with name: " + name, e);
            }

            return -999;
        }
        public static string GetItemName(int SNO)
        //Searches your equipped items and inventory for an item with the ActorSNO provided.  Then returns it's name.  
        //Will return "Not Found" if item is not found.
        {
            try
            {

                foreach (ACDItem i in ZetaDia.Me.Inventory.Backpack)
                {
                    if (i.ActorSNO == SNO)
                    {
                        return i.Name;
                    }
                }
                foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
                {
                    if (i.ActorSNO == SNO)
                    {
                        return i.Name;
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Unable to parse inventory for item with ActorSNO: " + SNO, e);
            }
            return "Not Found";
        }

        public static string GetItemNameInSlot(InventorySlot slot)
        //Searches your equipped items for an item in a provided slot.  Then returns it's name.  
        //Will return "Not Found" if item is not found.
        {
            try
            {
                foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
                {
                    if (i.InventorySlot == slot)
                    {
                        return i.Name;
                    }
                }
            }
            catch (Exception e) 
            {
                Plugin.WriteToLog("Unable to parse inventory for item in slot: " + slot.ToString(), e);
            }
            return "Not Found";
        }
        public static int GetIdOfItemInSlot(InventorySlot slot)
        //Returns the dynamicId of the item in the provided slot, returns -999 if no item is found.
        {
            try
            {
                foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
                {
                    if (i.InventorySlot == slot)
                    {
                        return i.DynamicId;
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Unable to parse inventory for item in slot: " + slot.ToString(), e);
            }
            return -999;
        }
        public static int GetActorSNOOfItemInSlot(InventorySlot slot)
        //Returns the ActorSNO of the item equipped in a provided slot.  Returns 0 if no item found.
        {
            try
            {
                foreach (ACDItem i in ZetaDia.Me.Inventory.Equipped)
                {
                    if (i.InventorySlot == slot)
                    {
                        return i.ActorSNO;
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Unable to parse inventory for item in slot: " + slot.ToString(), e);
            }
            return 0;
        }
        public static void SetTrue(string condition)
        //Sets the provided condition's status to true
        {
            for (int i = 0; i < Plugin.statuses.Count; ++i)
            {
                if (Plugin.statuses[i].GetName() == condition)
                    Plugin.statuses[i].SetTrue();
            }
        }
        public static void SetInUse(string condition)
        //Sets the provided condition's inuse flag to true.
        {
            for (int i = 0; i < Plugin.statuses.Count; ++i)
            {
                if (Plugin.statuses[i].GetName() == condition)
                    Plugin.statuses[i].SetInUseTrue();
            }
        }
        public static int CheckSlotEquipped(InventorySlot slot)
        //This checks if there is currently a swap piece equipped in the provided slot.  If not returns 0.  
        //If the piece is set to be replaced we return 2.
        //If not set to be replaced we will return 1.
        {

            if (Plugin.originalGear.Count == 0)
                return 0;
            else
            {
                for (int i = 0; i < Plugin.originalGear.Count; ++i)
                {
                    //check if another higher piece is equipped in the same slot
                    if (Plugin.originalGear[i].GetInvSlot() == slot)
                    {
                        if (Plugin.originalGear[i].GetFlag())
                            return 2;
                        else
                            return 1;
                    }
                }
            }
            return 0;
        }
        public static int GetPriority(string name)
        //Returns the index of the status (the lower the status the higher the priority)
        {
            for (int i = 0; i < Plugin.statuses.Count; i++)
            {
                if (Plugin.statuses[i].GetName() == name)
                    return i;
            }
            return -1;
        }
        public static string GetConditionOfItem(int SNO)
        //Returns the condition applied to the item in gearList with SNO provided.  If not found returns "";
        {
            for (int i = 0; i < Plugin.gearList.Count; i++)
            {
                if (Plugin.gearList[i].GetActorId() == SNO)
                    return Plugin.gearList[i].GetCondition();
            }
            return "";
        }
        public static void UpdateOriginalStatus(string cond, InventorySlot slot)
        //This updates the original gear in the provided slot with the most up to date condition provided.
        //This is used when we have a lower status equipped and replace it with a higher priority item. 
        //The original item needs to reflect the highest priority in case a lower priority is no longer present.
        {
            for (int i = 0; i < Plugin.originalGear.Count; i++)
            {
                if (Plugin.originalGear[i].GetInvSlot() == slot)
                {
                    Plugin.originalGear[i].SetCond(cond);
                }
            }
        }
    }
}