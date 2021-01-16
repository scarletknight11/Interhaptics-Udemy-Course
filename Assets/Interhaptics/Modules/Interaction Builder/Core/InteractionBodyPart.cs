using Interhaptics.Modules.Interaction_Builder.Core.Abstract;


namespace Interhaptics.Modules.Interaction_Builder.Core
{

    [UnityEngine.AddComponentMenu("Interhaptics/Interaction Builder/InteractionBodyPart")]
    public class InteractionBodyPart : AInteractionBodyPart
    {
        #region Variables
        private HandTracking.TrackedHand _trackedHand = null;
        #endregion

        #region Life Cycle
        protected virtual void Awake()
        {
            _trackedHand = gameObject.GetComponentInParent<HandTracking.TrackedHand>();
        }

        protected virtual void OnEnable()
        {
            if (_trackedHand)
                _trackedHand.OnHandComputed.AddListener(BodypartUpdate);
        }

        /// <summary>
        /// Disable BodyPartUpdate from Unity Update life cycle
        /// </summary>
        protected override void Update() 
        {
            if (!_trackedHand)
                base.Update();
        }

        protected virtual void OnDisable()
        {
            if (_trackedHand)
                _trackedHand.OnHandComputed.RemoveListener(BodypartUpdate);
        }
        #endregion

        #region Update Grabbing / Pinching Values
        protected override float UpdateRightGrabbingStrength()
        {
            return HandTracking.HandTrackingManager.Instance.RightHand.GrabStrength;
        }

        protected override float UpdateLeftGrabbingStrength()
        {
            return HandTracking.HandTrackingManager.Instance.LeftHand.GrabStrength;
        }

        protected override float UpdateRightPinchingStrength()
        {
            return HandTracking.HandTrackingManager.Instance.RightHand.PinchStrength;
        }

        protected override float UpdateLeftPinchingStrength()
        {
            return HandTracking.HandTrackingManager.Instance.LeftHand.PinchStrength;
        }
        #endregion
    }

}
