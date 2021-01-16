// ReSharper disable RedundantNameQualifier
namespace Interhaptics.Assets.Pointers
{

    public class GrabSelectionPointer : FloatableValuePointer
    {

        #region SERIALIZED FIELDS
        [UnityEngine.Header("GrabSelectionPointer Settings")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("True if pointer is attached to a left hand. False otherwise")]
        private bool isLeft = false;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Threshold at which the selection is triggered")]
        [UnityEngine.Range(0, 1)]
        private float grabbingThreshold = .5f;
        #endregion


        #region PUBLIC MEMBERS
        // ReSharper disable once MemberCanBePrivate.Global
        public Interhaptics.HandTracking.Hand MyHand => isLeft
            ? Interhaptics.HandTracking.HandTrackingManager.Instance.LeftHand
            : Interhaptics.HandTracking.HandTrackingManager.Instance.RightHand;
        #endregion


        #region OVERRIDED METHODS
        public override float GetSelectionValue()
        {
            return MyHand.GrabStrength;
        }

        public override float GetSelectionThreshold()
        {
            return grabbingThreshold;
        }
        #endregion

    }

}