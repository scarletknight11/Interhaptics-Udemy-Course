using Interhaptics.InteractionsEngine.Shared.Types;

using UnityEngine;


namespace Interhaptics.Modules.Interaction_Builder.Core
{

    /// <summary>
    ///     An interaction representation
    /// </summary>
    [CreateAssetMenu(fileName = "Interaction Primitive", menuName = "Interhaptics/Interaction Primitive",
        order = 10)]
    public class InteractionPrimitive : ScriptableObject
    {

        #region Default Values
        private const InteractionStrategy DEFAULT_INTERACTION_STRATEGY = InteractionStrategy.FreeMovement;
        private const InteractionTrigger DEFAULT_INTERACTION_TRIGGER = InteractionTrigger.PinchOrGrasp;
        private const BodyPartInteractionStrategy DEFAULT_BODY_PART_INTERACTION_STRATEGY =
            BodyPartInteractionStrategy.OneHand;

        private const bool DEFAULT_RENDER_STIFFNESS = true;
        private const bool DEFAULT_RENDER_TEXTURE = false;
        private const bool DEFAULT_RENDER_VIBRATION = false;
        private const VibrationEvaluationMode DEFAULT_VIBRATION_EVALUATION_MODE = VibrationEvaluationMode.SinceTheObjectCreation;

        private const float DEFAULT_DEGREE_VALUE_CORRESPONDING = 360;

        private const WorldSpace DEFAULT_WORLD_SPACE = WorldSpace.Local;

        private const float DEFAULT_MAXIMAL_CONSTRAINT = 0;
        private const bool DEFAULT_ENABLE_MAXIMAL_CONSTRAINT = false;
        private const float DEFAULT_MINIMAL_CONSTRAINT = 0;
        private const bool DEFAULT_ENABLE_MINIMAL_CONSTRAINT = false;
        #endregion


        #region Enums
        public enum VibrationEvaluationMode
        {
            SinceTheObjectCreation = 0,
            SinceTheInteractionBeginning = 1,
            OnlyOnTrigger = 2
        }
        #endregion


        #region Fields
        [Header("Interaction Settings")]
        [Tooltip("The interaction strategy will determine how the object will move in the space during an interaction")]
        public InteractionStrategy interactionStrategy = DEFAULT_INTERACTION_STRATEGY;
        [Tooltip("How the interaction will be triggered")]
        public InteractionTrigger interactionTrigger = DEFAULT_INTERACTION_TRIGGER;
        [Tooltip("With which body part the object will interact")]
        public BodyPartInteractionStrategy bodyPart = DEFAULT_BODY_PART_INTERACTION_STRATEGY;

        [Header("Haptic Interaction Settings")]
        [Tooltip("Your custom haptic material to use during the interaction, if no one is set the haptic feedback will be computed normally")]
        public TextAsset hapticInteractionFeedback = null;
        [Tooltip("True to render the stiffness, false otherwise")]
        public bool renderStiffness = DEFAULT_RENDER_STIFFNESS;
        [Tooltip("True to render texture, false otherwise")]
        public bool renderTexture = DEFAULT_RENDER_TEXTURE;
        [Tooltip("It will convert one texture metric unit into this degree value")]
        public float degreeValueCorresponding = DEFAULT_DEGREE_VALUE_CORRESPONDING;
        [Tooltip("True to render vibration, false otherwise")]
        public bool renderVibration = DEFAULT_RENDER_VIBRATION;
        [Tooltip("How you want to render the vibration during an interaction")]
        public VibrationEvaluationMode vibrationEvaluationMode = DEFAULT_VIBRATION_EVALUATION_MODE;

        [Header("FreePositionWithYRotation settings")]
        [Tooltip("Which Y axis will be took")]
        public WorldSpace ySpace = DEFAULT_WORLD_SPACE;

        [Header("Constraints settings")]
        [Tooltip("True to enable a maximal constraint, false otherwise")]
        public bool enableMaximalConstraint = DEFAULT_ENABLE_MAXIMAL_CONSTRAINT;
        [Tooltip("Your maximal constraint value")]
        public float maximalConstraint = DEFAULT_MAXIMAL_CONSTRAINT;
        [Space]
        [Tooltip("True to enable a minimal constraint, false otherwise")]
        public bool enableMinimalConstraint = DEFAULT_ENABLE_MINIMAL_CONSTRAINT;
        [Tooltip("Your minimal constraint value")]
        public float minimalConstraint = DEFAULT_MINIMAL_CONSTRAINT;
        #endregion


        #if UNITY_EDITOR
        #region Editor Life Cycles
        private void OnValidate()
        {
            if (maximalConstraint < 0)
                maximalConstraint = -maximalConstraint;

            if (minimalConstraint > 0)
                minimalConstraint = -minimalConstraint;
        }
        #endregion
        #endif

    }

}
