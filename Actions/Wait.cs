using UnityEngine;

namespace ActionGraph
{
    public class Wait : Action
    {
        // -------------------------------------------------------------------------------

        public float Duration = 1.0f;

        // -------------------------------------------------------------------------------

        public override string DisplayName
        {
            get { return "Wait " + Duration + " secs"; }
        }

        // -------------------------------------------------------------------------------

        protected override void OnUpdate()
        {
            if (mElapsedTime >= Duration)
            {
                Status = ActionStatus.Finished;
            }
        }

        // -------------------------------------------------------------------------------
    }
}
