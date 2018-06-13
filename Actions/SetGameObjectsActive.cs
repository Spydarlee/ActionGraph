using System.Collections.Generic;
using UnityEngine;

namespace ActionGraph
{
    public class SetGameObjectsActive : Action
    {
        // -------------------------------------------------------------------------------

        public List<GameObject> Targets;
        public bool             Activate = true;

        // -------------------------------------------------------------------------------

        public override string DisplayName
        {
            get { return (Activate) ? "Activate GameObjects" : "Deactivate GameObjects"; }
        }

        // -------------------------------------------------------------------------------

        protected override void OnStart()
        {
            foreach (var go in Targets)
            {
                go.SetActive(Activate);
            }

            FinishAction();
        }

        // -------------------------------------------------------------------------------
    }
}
