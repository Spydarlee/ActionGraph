using UnityEngine;
using Cinemachine;

namespace ActionGraph
{
    public class SetVirtualCameraActive : Action
    {
        // -------------------------------------------------------------------------------

        public Variable<CinemachineVirtualCamera>   Target;
        public bool                                 Activate = true;
        public bool                                 WaitForBlend = true;

        // -------------------------------------------------------------------------------

        private CinemachineBrain                    mCinemachineBrain = null;
        private bool                                mHasStartedBlending = false;

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
            if (mCinemachineBrain == null)
            {
                mCinemachineBrain = Object.FindObjectOfType<CinemachineBrain>();
            }

            if (Target != null && Target.Value != null)
            {
                Target.Value.gameObject.SetActive(Activate);
            }

            if (!WaitForBlend)
            {
                FinishAction();
            }

            mHasStartedBlending = false;
        }

        // -------------------------------------------------------------------------------

        protected override void OnUpdate()
        {
            mHasStartedBlending = (mHasStartedBlending || mCinemachineBrain.IsBlending);

            if (WaitForBlend && mHasStartedBlending && !mCinemachineBrain.IsBlending)
            {
                FinishAction();
            }
        }

        // -------------------------------------------------------------------------------
    }
}
