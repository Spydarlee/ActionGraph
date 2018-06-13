using UnityEngine;

namespace ActionGraph
{
    public class SetGameObjectActive : Action
    {
        // -------------------------------------------------------------------------------

        public Variable<GameObject>     Target;
        public bool                     Activate = true;

        // -------------------------------------------------------------------------------

        public override string DisplayName
        {
            get
            {
                var displayName = (Activate) ? "Activate " : "Deactivate ";

                if (Target != null)
                {
                    displayName += Target.ToString();
                }

                return displayName;
            }
        }

        // -------------------------------------------------------------------------------

        protected override void OnStart()
        {
            if (Target != null && Target.Value != null)
            {
                Target.Value.SetActive(Activate);
            }

            FinishAction();
        }

        // -------------------------------------------------------------------------------
    }
}
