using UnityEngine;

namespace ActionGraph
{
    public class DebugLog : Action
    {
        // -------------------------------------------------------------------------------

        public string Message = "";

        // -------------------------------------------------------------------------------

        public override string DisplayName
        {
            get { return "Log '" + Message + "'"; }
        }

        // -------------------------------------------------------------------------------

        protected override void OnStart()
        {
            Debug.Log(Message);
            FinishAction();
        }

        // -------------------------------------------------------------------------------
    }
}
