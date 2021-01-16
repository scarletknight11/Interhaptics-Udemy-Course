using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.InteractionsEngine;

using Interhaptics.Modules.Interaction_Builder.Core.Abstract;

using UnityEngine;


namespace Interhaptics.Modules.Interaction_Builder.Core
{

    /// <summary>
    ///     The InteractionBuilderManager is here to manage all API calls
    /// </summary>
    [AddComponentMenu("Interhaptics/Interaction Builder/InteractionBuilderManager")]
    [RequireComponent(typeof(Interhaptics.HapticRenderer.Core.HapticManager))]
    public sealed class InteractionBuilderManager: MonoBehaviour
    {

        #region Log Messages
        private const string WARNING_NO_RIGHT_HAND = "<b>[InteractionBuilderManager]</b> You didn't set right hand";
        private const string WARNING_NO_LEFT_HAND = "<b>[InteractionBuilderManager]</b> You didn't set left hand";
        #endregion


        #region Consts
        private const string IB_VERSION = "20.11.02.00";
        #endregion


        #region Serialized Fields
        [Header("Body Parts")]
        [Header("Interaction Builder Version: " + IB_VERSION)]
        [SerializeField]
        [Tooltip("The right hand used")]
        private AInteractionBodyPart rightHand = null;
        [SerializeField]
        [Tooltip("The left hand used")]
        private AInteractionBodyPart leftHand = null;
        #endregion


        #region Private Fields
        private AInteractionBodyPart[] _leftFingers;
        private AInteractionBodyPart[] _rightFingers;
        #endregion


        #region Fields Getter
        /// <summary>
        ///     The right hand
        /// </summary>
        public AInteractionBodyPart RightHand => rightHand;
        
        /// <summary>
        ///     The left hand
        /// </summary>
        public AInteractionBodyPart LeftHand => leftHand;
        #endregion


        #region Life Cycles
        private void OnValidate()
        {
            if (rightHand && rightHand.bodyPart != BodyPart.RightHand)
                rightHand.bodyPart = BodyPart.RightHand;

            if (leftHand && leftHand.bodyPart != BodyPart.LeftHand)
                leftHand.bodyPart = BodyPart.LeftHand;
        }

        private void Awake()
        {
            if (rightHand)
            {
                rightHand.OnInteractionStateChanged.AddListener(() => UpdateInteractions(ref rightHand, ref leftHand));
                _rightFingers = rightHand.GetComponentsInChildren<AInteractionBodyPart>();
                foreach (AInteractionBodyPart finger in _rightFingers)
                {
                    finger.OnInteractionStateChanged.AddListener(() =>
                    {
                        AInteractionBodyPart bodyPart = finger;
                        UpdateInteractions(ref bodyPart);
                    });
                }
            }
            else
                Debug.LogWarning(WARNING_NO_RIGHT_HAND);

            if (leftHand)
            {
                leftHand.OnInteractionStateChanged.AddListener(() => UpdateInteractions(ref leftHand, ref rightHand));
                _leftFingers = leftHand.GetComponentsInChildren<AInteractionBodyPart>();
                foreach (AInteractionBodyPart finger in _leftFingers)
                {
                    finger.OnInteractionStateChanged.AddListener(() =>
                    {
                        AInteractionBodyPart bodyPart = finger;
                        UpdateInteractions(ref bodyPart);
                    });
                }
            }
            else
                Debug.LogWarning(WARNING_NO_LEFT_HAND);
        }

        private void LateUpdate()
        {
            if (leftHand)
            {
                UpdateHapticBodyPart(leftHand);
                if (!leftHand.IsInInteraction)
                    foreach (AInteractionBodyPart hibp in _leftFingers)
                        UpdateHapticBodyPart(hibp);
            }

            if (rightHand is null)
                return;

            UpdateHapticBodyPart(rightHand);
            if (rightHand.IsInInteraction) 
                return;
            foreach (AInteractionBodyPart hibp in _rightFingers)
                UpdateHapticBodyPart(hibp);
        }

        private void OnDestroy()
        {
            if (rightHand)
                rightHand.OnInteractionStateChanged.RemoveListener(
                    () => UpdateInteractions(ref rightHand, ref leftHand));

            if (leftHand)
                leftHand.OnInteractionStateChanged.RemoveListener(() =>
                    UpdateInteractions(ref leftHand, ref rightHand));
        }
        #endregion


        #region Public Methods
        /// <summary>
        ///     Block interactions on an object
        /// </summary>
        /// <param name="obj">An haptic interaction object</param>
        /// <returns>True if the interaction was blocked, false otherwise</returns>
        public bool BlockObject(InteractionObject obj)
        {
            return obj.BlockObject();
        }

        /// <summary>
        ///     Unblock interactions on an object
        /// </summary>
        /// <param name="obj">An haptic interaction object</param>
        /// <returns>True if the interaction was unblocked, false otherwise</returns>
        public bool UnblockObject(InteractionObject obj)
        {
            return obj.UnblockObject();
        }

        /// <summary>
        ///     Try to block interactions on an object
        /// </summary>
        /// <param name="obj">An haptic interaction object</param>
        public void TryToBlockObject(InteractionObject obj)
        {
            obj.TryToBlockObject();
        }

        /// <summary>
        ///     Try to unblock interactions on an object
        /// </summary>
        /// <param name="obj">An haptic interaction object</param>
        public void TryToUnblockObject(InteractionObject obj)
        {
            obj.TryToUnblockObject();
        }

        /// <summary>
        ///     Force to finish interactions
        /// </summary>
        /// <param name="interactionObject">An haptic interaction object</param>
        /// <param name="bodyPartInteractionStrategy">The body part strategy which interact with</param>
        public void ForceFinishInteraction(InteractionObject interactionObject,
            BodyPartInteractionStrategy bodyPartInteractionStrategy)
        {
            InteractionEngineApi.ChangeObjectBlockingState(interactionObject.ObjectId, false);
            InteractionEngineApi.ChangeBodyPartBlockingState(bodyPartInteractionStrategy, false);
            interactionObject.FinishInteraction(bodyPartInteractionStrategy);
        }

        /// <summary>
        ///     Force to start interactions
        /// </summary>
        /// <param name="interactionObject">An haptic interaction object</param>
        /// <param name="bodyPartInteractionStrategy">The body part strategy which interact with</param>
        public void ForceStartInteraction(InteractionObject interactionObject, BodyPartInteractionStrategy bodyPartInteractionStrategy)
        {
            InteractionEngineApi.ChangeObjectBlockingState(interactionObject.ObjectId, false);
            InteractionEngineApi.ChangeBodyPartBlockingState(bodyPartInteractionStrategy, false);
            interactionObject.StartInteraction(bodyPartInteractionStrategy);
        }
        #endregion


        #region Private Methods
        private void UpdateInteractions(ref AInteractionBodyPart bodyPartToUpdate, ref AInteractionBodyPart theOther)
        {
            if (bodyPartToUpdate.InteractionObject == null || bodyPartToUpdate.InteractionObject.InteractionPrimitive == null)
                return;

            if (!bodyPartToUpdate.IsInInteraction && bodyPartToUpdate.Interaction != InteractionTrigger.None &&
                bodyPartToUpdate.Interaction ==
                bodyPartToUpdate.InteractionObject.InteractionPrimitive.interactionTrigger)
            {
                if (!bodyPartToUpdate.InteractionObject.StartInteraction(bodyPartToUpdate.bodyPart) &&
                    (bodyPartToUpdate.InteractionObject.InteractionPrimitive.bodyPart ==
                     BodyPartInteractionStrategy.TwoHands ||
                     bodyPartToUpdate.InteractionObject.InteractionPrimitive.bodyPart ==
                     BodyPartInteractionStrategy.TwoHandsWithHead) && !theOther.IsInInteraction &&
                    theOther.Interaction != InteractionTrigger.None && theOther.InteractionObject &&
                    theOther.InteractionObject.GetInstanceID() == bodyPartToUpdate.InteractionObject.GetInstanceID())
                    bodyPartToUpdate.InteractionObject.StartInteraction(bodyPartToUpdate.InteractionObject
                        .InteractionPrimitive.bodyPart);
            }
            else if (bodyPartToUpdate.IsInInteraction && bodyPartToUpdate.Interaction == InteractionTrigger.None &&
                     !bodyPartToUpdate.InteractionObject.FinishInteraction(bodyPartToUpdate.bodyPart) &&
                     theOther.InteractionObject &&
                     theOther.InteractionObject.GetInstanceID() == bodyPartToUpdate.InteractionObject.GetInstanceID() &&
                     (bodyPartToUpdate.InteractionObject.StopDoubleGraspingWhenOneHandRelease &&
                      theOther.IsInInteraction && theOther.Interaction != InteractionTrigger.None ||
                      !bodyPartToUpdate.InteractionObject.StopDoubleGraspingWhenOneHandRelease &&
                      theOther.IsInInteraction && theOther.Interaction == InteractionTrigger.None))
                bodyPartToUpdate.InteractionObject.FinishInteraction(bodyPartToUpdate.InteractionObject
                    .InteractionPrimitive.bodyPart);
        }

        private static void UpdateInteractions(ref AInteractionBodyPart bodyPartToUpdate)
        {
            if (bodyPartToUpdate.InteractionObject == null)
                return;

            if (!bodyPartToUpdate.IsInInteraction && bodyPartToUpdate.Interaction != InteractionTrigger.None &&
                bodyPartToUpdate.Interaction ==
                bodyPartToUpdate.InteractionObject.InteractionPrimitive.interactionTrigger)
                bodyPartToUpdate.InteractionObject.StartInteraction(bodyPartToUpdate.bodyPart);
            else if (bodyPartToUpdate.IsInInteraction && bodyPartToUpdate.Interaction == InteractionTrigger.None)
                bodyPartToUpdate.InteractionObject.FinishInteraction(bodyPartToUpdate.bodyPart);
        }

        private static void UpdateHapticBodyPart(AInteractionBodyPart interactionBodyPart)
        {
            if (interactionBodyPart && interactionBodyPart.InteractionObject)
                interactionBodyPart.InteractionObject.EvaluateHapticAmplitude();
        }
        #endregion

    }

}