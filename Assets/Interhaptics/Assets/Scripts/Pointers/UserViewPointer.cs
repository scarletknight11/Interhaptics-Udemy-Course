namespace Interhaptics.Assets.Pointers
{

    public class UserViewPointer : Pointer
    {

        #region OVERRIDED METHODS
        public override bool GetRenderLine()
        {
            return false;
        }

        public override UnityEngine.Vector3 GetOrigin()
        {
            // ReSharper disable RedundantNameQualifier
            return Interhaptics.HandTracking.HandTrackingManager.Instance.MainCamera.position;
        }

        public override UnityEngine.Vector3 GetPointingDirection(UnityEngine.Vector3 rayOrigin)
        {
            // ReSharper disable RedundantNameQualifier
            return transform.forward;
        }
        #endregion

    }

}
