using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta;
using Zeta.Game;


namespace GearSwap
{
    class gear
    {
        private string condition { get; set; }
        private int actorId { get; set; }
        private InventorySlot slot { get; set; }
        private int dynamicId { get; set; }
        private bool flagToReplace { get; set; }
        private string itemName { get; set; }

        public gear(string newCond, int id, InventorySlot islot)
        {
            condition = newCond;
            actorId = id;
            slot = islot;
            itemName = "";
            dynamicId = -999;
            flagToReplace = false;
        }

        public gear(string newCond, int id, InventorySlot islot, string name)
        {
            condition = newCond;
            actorId = id;
            slot = islot;
            itemName = name;
            dynamicId = -999;
            flagToReplace = false;
        }
        public gear(gear item)
        {
            condition = item.getCondition();
            actorId = item.getActorId();
            slot = item.getInvSlot();
            itemName = item.getName();
            dynamicId = -999;
            flagToReplace = false;
        }
        public gear(gear item, int newId)
        {
            condition = item.getCondition();
            actorId = newId;
            slot = item.getInvSlot();
            itemName = item.getName();
            dynamicId = -999;
            flagToReplace = false;
        }
        public gear(gear item, int newId, string name)
        {
            condition = item.getCondition();
            actorId = newId;
            slot = item.getInvSlot();
            itemName = name;
            dynamicId = -999;
            flagToReplace = false;
        }

        public void setName(string name)
        {
            itemName = name;
        }

        public string getName()
        {
            return itemName;
        }

        public string display()
        {
            return "Item: " + actorId + " - " + slot + " - Condition: " + condition + " - ID: " + dynamicId;
        }

        public void setCond(string cond)
        {
            condition = cond;
        }

        public int getActorId()
        {
            return actorId;
        }

        public InventorySlot getInvSlot()
        {
            return slot;
        }

        public string getCondition()
        {
            return condition;
        }

        public int getDynamicId()
        {
            return dynamicId;
        }

        public void setDynamicId(int dynId)
        {
            dynamicId = dynId;
        }

        public void setFlagFalse()
        {
            flagToReplace = false;
        }
        public void setFlagTrue()
        {
            flagToReplace = true;
        }
        public bool getFlag()
        {
            return flagToReplace;
        }
    }
}

