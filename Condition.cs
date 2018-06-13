using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActionGraph
{
    public class Condition
    {
        // -------------------------------------------------------------------------------

        public virtual bool Check()
        {
            Debug.Log("Default Condition - returning true");
            return true;
        }

        // -------------------------------------------------------------------------------

        public virtual void OnGUI() { }

        // -------------------------------------------------------------------------------
    }
}
