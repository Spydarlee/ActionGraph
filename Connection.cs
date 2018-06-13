using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActionGraph
{
    public class Connection
    {
        // -------------------------------------------------------------------------------

        public enum ConditionRequirements
        {
            All,
            Any
        }

        // -------------------------------------------------------------------------------

        public ConditionRequirements    ConditionRequirement;
        public List<Condition>          Conditions = new List<Condition>();
        public Node                     StartNode = null;
        public Node                     EndNode = null;

        // -------------------------------------------------------------------------------

        public bool CheckConditions()
        {
            int numChecksPassed = 0;

            if (Conditions.Count > 0 )
            {
                foreach (var condition in Conditions)
                {
                    var result = condition.Check();
                    numChecksPassed += (result) ? 1 : 0;
                }
            }
            else
            {
                return true;
            }

            return (ConditionRequirement == ConditionRequirements.All && numChecksPassed == Conditions.Count) ||
                   (ConditionRequirement == ConditionRequirements.Any && numChecksPassed >= 1);

        }

        // -------------------------------------------------------------------------------
    }
}