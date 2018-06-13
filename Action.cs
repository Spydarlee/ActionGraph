using UnityEngine;

namespace ActionGraph
{
    // -------------------------------------------------------------------------------

    public enum ActionStatus
    {
        Executing,
        Finished
    }

    // -------------------------------------------------------------------------------

    public class Action
    {
        // -------------------------------------------------------------------------------

        public ActionStatus Status { get; set; }
        protected float     mElapsedTime = 0.0f;

        // -------------------------------------------------------------------------------

        public Action Clone()
        {
            return (Action)this.MemberwiseClone();
        }

        // -------------------------------------------------------------------------------

        public virtual string DisplayName
        {
            get {  return this.GetType().Name; }
        }

        // -------------------------------------------------------------------------------

        public void Start()
        {
            Status = ActionStatus.Executing;
            mElapsedTime = 0.0f;
            OnStart();
        }

        // -------------------------------------------------------------------------------

        public void Update()
        {
            OnUpdate();
            mElapsedTime += Time.deltaTime;
        }

        // -------------------------------------------------------------------------------

        public void Finish()
        {
            OnFinish();
        }

        // -------------------------------------------------------------------------------

        #if UNITY_EDITOR
        public virtual bool OnGUI() { return false; }
        #endif

    // -------------------------------------------------------------------------------

        protected virtual void OnStart() { Status = ActionStatus.Executing; }
        protected virtual void OnUpdate() { }
        protected virtual void OnFinish() { }

        // -------------------------------------------------------------------------------

        protected void FinishAction()
        {
            Status = ActionStatus.Finished;
        }

        // -------------------------------------------------------------------------------
    }
}
