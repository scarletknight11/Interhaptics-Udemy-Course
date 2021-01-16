// ReSharper disable UnusedMember.Local
namespace Interhaptics.Assets.Pointers
{

    public class PinchViewfinderPointer : FloatableValuePointer
    {

        #region SERIALIZED FIELDS
        [UnityEngine.Header("Viewfinder Settings")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("First fingertip to find the ray origin")]
        private UnityEngine.Transform firstFingerTip;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Weight of this first fingertip for the lerping")]
        [UnityEngine.Range(0, 1)]
        private float firstFingerTipWeight = .5f;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Second fingertip to find the ray origin")]
        private UnityEngine.Transform secondFingerTip;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Distance between the two finger tips to trigger the custom selection")]
        private float fingerTipDistanceToTriggerSelection = .0015f;
        #endregion


        #region PUBLIC MEMBERS
        private float FirstFingerTipWeight
        {
            get => firstFingerTipWeight;
            set => firstFingerTipWeight = UnityEngine.Mathf.Clamp01(value);
        }
        private UnityEngine.Transform FirstFingerTip
        {
            get => firstFingerTip;
            set
            {
                if (value)
                    firstFingerTip = value;
            }
        }
        private UnityEngine.Transform SecondFingerTip
        {
            get => secondFingerTip;
            set
            {
                if (value)
                    secondFingerTip = value;
            }
        }
        #endregion


        #region OVERRIDED METHODS
        public override UnityEngine.Vector3 GetOrigin()
        {
            return UnityEngine.Vector3.Lerp(secondFingerTip.position, firstFingerTip.position, firstFingerTipWeight);
        }

        public override UnityEngine.Vector3 GetPointingDirection(UnityEngine.Vector3 rayOrigin)
        {
            // ReSharper disable RedundantNameQualifier
            return UnityEngine.Vector3.Lerp(
                (rayOrigin - Interhaptics.HandTracking.HandTrackingManager.Instance.MainCamera.position).normalized,
                transform.right, 0.5f);
        }

        public override float GetSelectionValue()
        {
            return UnityEngine.Vector3.Distance(firstFingerTip.position, secondFingerTip.position);
        }

        public override float GetSelectionThreshold()
        {
            return fingerTipDistanceToTriggerSelection;
        }
        #endregion

    }

}
