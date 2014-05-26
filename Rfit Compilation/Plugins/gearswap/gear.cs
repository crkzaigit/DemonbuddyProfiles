using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta;
using Zeta.Game;


namespace GearSwap
{
    class Gear
    {
        private string condition { get; set; }
        private int actorId { get; set; }
        private InventorySlot slot { get; set; }
        private int dynamicId { get; set; }
        private bool flagToReplace { get; set; }
        private string itemName { get; set; }

        public Gear(string newCond, int id, InventorySlot islot)
        {
            condition = newCond;
            actorId = id;
            slot = islot;
            itemName = "";
            dynamicId = -999;
            flagToReplace = false;
        }

        public Gear(string newCond, int id, InventorySlot islot, string name)
        {
            condition = newCond;
            actorId = id;
            slot = islot;
            itemName = name;
            dynamicId = -999;
            flagToReplace = false;
        }
        public Gear(Gear item)
        {
            condition = item.GetCondition();
            actorId = item.GetActorId();
            slot = item.GetInvSlot();
            itemName = item.GetName();
            dynamicId = -999;
            flagToReplace = false;
        }
        public Gear(Gear item, int newId)
        {
            condition = item.GetCondition();
            actorId = newId;
            slot = item.GetInvSlot();
            itemName = item.GetName();
            dynamicId = -999;
            flagToReplace = false;
        }
        public Gear(Gear item, int newId, string name)
        {
            condition = item.GetCondition();
            actorId = newId;
            slot = item.GetInvSlot();
            itemName = name;
            dynamicId = -999;
            flagToReplace = false;
        }

        public void SetName(string name)
        {
            itemName = name;
        }

        public string GetName()
        {
            return itemName;
        }

        public string Display()
        {
            return "Item: " + actorId + " - " + slot + " - Condition: " + condition + " - ID: " + dynamicId;
        }

        public void SetCond(string cond)
        {
            condition = cond;
        }

        public int GetActorId()
        {
            return actorId;
        }

        public InventorySlot GetInvSlot()
        {
            return slot;
        }

        public string GetCondition()
        {
            return condition;
        }

        public int GetDynamicId()
        {
            return dynamicId;
        }

        public void SetDynamicId(int dynId)
        {
            dynamicId = dynId;
        }

        public void SetFlagFalse()
        {
            flagToReplace = false;
        }
        public void SetFlagTrue()
        {
            flagToReplace = true;
        }
        public bool GetFlag()
        {
            return flagToReplace;
        }
    }
}

