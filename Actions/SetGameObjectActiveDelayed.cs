using UnityEngine;

namespace ActionGraph
{
    public class SetGameObjectActiveDelayed : Action
    {
        // -------------------------------------------------------------------------------

        public Variable<GameObject>     Target;
        public bool                     Activate = true;
        public float                    Delay = 1.0f;

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
                LeanTween.delayedCall(Delay, () => 
                {
                    Target.Value.SetActive(Activate);
                });
            }

            FinishAction();
        }

        // -------------------------------------------------------------------------------
    }
}
