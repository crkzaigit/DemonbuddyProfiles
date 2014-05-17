using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GearSwap
{
    class condition
    {
        private string cond { get; set; }
        private bool status { get; set; }
        private bool inUse { get; set; }

        public condition(string newCond)
        {
            cond = newCond;
            status = false;
            inUse = false;
        }

        public void setTrue()
        {
            status = true;
        }

        public void setFalse()
        {
            status = false;
        }
        public void setInUseTrue()
        {
            inUse = true;
        }

        public void setInUseFalse()
        {
            inUse = false;
        }

        public string getName()
        {
            return cond;
        }

        public bool getStatus()
        {
            return status;
        }
        public bool getInUse()
        {
            return inUse;
        }
        public void setName(string n)
        {
            cond = n;
        }
    }
}

