using Interhaptics.HandTracking;
using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.Modules.Interaction_Builder.Core;
using Interhaptics.Modules.Interaction_Builder.Core.Abstract;
using UnityEngine;
using UnityEngine.Events;

namespace Interhaptics.ObjectSnapper.core
{
    [UnityEngine.AddComponentMenu("Interhaptics/Object Snapper/IBSnappableActor")]
    [RequireComponent(typeof(TrackedHand))]
    public class IBSnappableActor : ASnappableActor
    {
        #region Constants 
        private const string ERROR_NoCorrespondingInteractionBodyPart = "No corresponding InteractionBodypart found on this scene";
        #endregion

        #region Variables
        [SerializeField] private InteractionBodyPart interactionBodyPart = null;

        private TrackedHand _trackedHand = null;
        private UnityAction<AInteractionBodyPart, InteractionObject> _externalControlOn = null;
        private UnityAction<AInteractionBodyPart, InteractionObject> _externalControlOff = null;
        //private UnityAction _handUpdateAction = null;
        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            //Get the TrackedHand
            _trackedHand = gameObject.GetComponent<TrackedHand>();
            if (interactionBodyPart && ((interactionBodyPart.BodyPart == BodyPart.LeftHand && _trackedHand.IsLeft) || (interactionBodyPart.BodyPart == BodyPart.RightHand && !_trackedHand.IsLeft)))
            {
                _externalControlOn = (_, interactionObject) => this.OnInteractionStateChanging(interactionObject != null ? interactionObject.GetComponent<SnappingObject>() : null);
                _externalControlOff = (_, __) => this.OnInteractionStateChanging(null);
            }
            else
                Debug.LogError(ERROR_NoCorrespondingInteractionBodyPart, this);
        }

        protected virtual void OnEnable()
        {
            if (!interactionBodyPart)
                return;

            interactionBodyPart.OnInteractionStartEvent.AddListener(_externalControlOn);
            interactionBodyPart.OnInteractionFinishEvent.AddListener(_externalControlOff);
        }

        //TO FIX (Update on TackedHandEvent)
        protected virtual void LateUpdate()
        {
            this.RefreshSnapping();
        }

        protected virtual void OnDisable()
        {
            if (!interactionBodyPart)
                return;
        }
        #endregion

        #region Overrides
        protected override void OnSubscribeSnappingObject(SnappingObject snappingObject)
        {
            base.OnSubscribeSnappingObject(snappingObject);

            if (_trackedHand)
                _trackedHand.RefreshPose = false;
        }

        protected override void OnUnsubscribeSnappingObject(SnappingObject snappingObject)
        {
            base.OnUnsubscribeSnappingObject(snappingObject);

            if (_trackedHand)
                _trackedHand.RefreshPose = true;
        }
        #endregion
    }
}
