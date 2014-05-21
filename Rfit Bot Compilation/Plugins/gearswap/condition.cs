using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GearSwap
{
    class Condition
    {
        private string cond { get; set; }
        private bool status { get; set; }
        private bool inUse { get; set; }

        public Condition(string newCond)
        {
            cond = newCond;
            status = false;
            inUse = false;
        }

        public void SetTrue()
        {
            status = true;
        }

        public void SetFalse()
        {
            status = false;
        }
        public void SetInUseTrue()
        {
            inUse = true;
        }

        public void SetInUseFalse()
        {
            inUse = false;
        }

        public string GetName()
        {
            return cond;
        }

        public bool GetStatus()
        {
            return status;
        }
        public bool GetInUse()
        {
            return inUse;
        }
        public void SetName(string n)
        {
            cond = n;
        }
    }
}

