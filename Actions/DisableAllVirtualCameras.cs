using Cinemachine;
using UnityEngine;

namespace ActionGraph
{
    public class DisableAllVirtualCameras : Action
    {
        // ------------------------------------------------------------------------------

        public CameraController             CameraControllerTarget;
        public CinemachineVirtualCamera     CameraToIgnore = null;

        // -------------------------------------------------------------------------------

        protected override void OnStart()
        {
            if (CameraControllerTarget == null)
            {
                CameraControllerTarget = GameObject.FindObjectOfType<CameraController>();
            }

            CameraControllerTarget.DisableAllVirtualCameras(CameraToIgnore);
            FinishAction();
        }

        // -------------------------------------------------------------------------------
    }
}