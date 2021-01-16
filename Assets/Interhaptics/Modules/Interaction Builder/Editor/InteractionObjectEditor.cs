#if UNITY_EDITOR
using Interhaptics.Modules.Interaction_Builder.Core;

using BodyPartInteractionStrategy = Interhaptics.InteractionsEngine.Shared.Types.BodyPartInteractionStrategy;
using InteractionStrategy = Interhaptics.InteractionsEngine.Shared.Types.InteractionStrategy;
using InteractionTrigger = Interhaptics.InteractionsEngine.Shared.Types.InteractionTrigger;

using UnityEditor;
using UnityEngine;


namespace Interhaptics.Modules.Interaction_Builder.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(InteractionObject))]
    public class InteractionObjectEditor: UnityEditor.Editor
    {

        #region Nested Enums
        private enum InteractionEvent
        {
            OnInteractionStart,
            OnInteractionFinish
        }

        private enum ConstraintsEvent
        {
            OnNoConstraintsReached,
            OnMaximalConstraintsReached,
            OnMinimalConstraintsReached
        }
        #endregion


        #region CONSTS HEADERS
        private const string INTERACTION_EVENTS_HEADER = "Interaction Events";
        private const string CONSTRAINTS_EVENTS_HEADER = "Contraint Events";
        private const string NUMERICAL_TARGETS_EVENTS_HEADER = "Numerical Targets Events";
        private const string PHYSICAL_TARGETS_EVENTS_HEADER = "Physical Targets Events";
        #endregion


        #region Serialized Names
        // Interaction Settings
        private const string INTERACTION_PRIMITIVE_NAME = "Interaction Primitive";
        private const string STOP_DOUBLE_GRASPING_WHEN_ONE_HAND_RELEASE_NAME = "stopDoubleGraspingWhenOneHandRelease";
        private const string GRASPING_STRENGTH_THRESHOLD_NAME = "Grasping Strength Threshold";
        private const string PINCHING_STRENGTH_THRESHOLD_NAME = "Pinching Strength Threshold";
        private const string DELAY_TIMER_NAME = "Delay Timer";
        private const string BLOCK_OBJECT_ON_INTERACTION_START_NAME = "Block Object On Interaction Start";
        private const string BLOCK_BODY_PART_ON_INTERACTION_START_NAME = "Block Body Part On Interaction Start";
        private const string SET_AS_KINEMATIC_DURING_INTERACTION_NAME = "Set As Kinematic During Interaction";
        private const string COMPUTE_ON_LOCAL_BASIS_NAME = "Compute On Local Basis";

        // RotationAroundPivot Settings
        private const string SLIDING_TRANSFORM_NAME = "Sliding Transform";
        private const string LOCAL_SLIDING_AXIS_NAME = "Local Sliding Axis";

        // RotationAroundPivot Settings
        private const string PIVOT_TRANSFORM_NAME = "Pivot Transform";
        private const string LOCAL_ROTATION_AXIS_NAME = "Local Rotation Axis";

        // Interaction Events
        private const string ON_INTERACTION_START_NAME = "Interaction Start";
        private const string ON_INTERACTION_FINISH_NAME = "Interaction Finish";

        // Constraints Events
        private const string ACTIVATE_CONSTRAINTS_EVENTS_NAME = "Activate Constraints Events";
        private const string ON_NO_CONSTRAINTS_REACHED_NAME = "No Constraints";
        private const string ON_MAXIMAL_CONSTRAINT_REACHED_NAME = "Maximal Constraint";
        private const string ON_MINIMAL_CONSTRAINT_REACHED_NAME = "Minimal Constraint";

        // Numerical Targets Events
        private const string ACTIVATE_NUMERICAL_TARGETS_EVENTS_NAME = "Activate Numerical Targets Events";        
        private const string ON_NO_NUMERICAL_TARGET_REACHED_NAME = "On No Numerical Targets Reached";
        private const string NUMERICAL_TARGET_LIST_NAME = "Numerical Target List";

        // Physical Targets Events
        private const string ACTIVATE_PHYSICAL_TARGETS_EVENTS_NAME = "Activate Physical Targets Events";        
        private const string PHYSICAL_TARGET_LIST_NAME = "Physical Target List";
        #endregion


        #region Private Fields
        // Interaction Settings
        private SerializedProperty _myInteractionPrimitive;
        private SerializedProperty _myStopDoubleGraspingWhenOneHandRelease;
        private SerializedProperty _myGraspingStrengthThreshold;
        private SerializedProperty _myPinchingStrengthThreshold;
        private SerializedProperty _myDelayTimer;
        private SerializedProperty _myBlockObjectOnInteractionStart;
        private SerializedProperty _myBlockBodyPartsOnInteractionStart;
        private SerializedProperty _mySetAsKinematicDuringInteraction;
        private SerializedProperty _computeOnLocalBasis;

        // Sliding Settings
        private SerializedProperty _mySlidingTransform;
        private SerializedProperty _myLocalSlidingAxis;

        // RotationAroundPivot Settings
        private SerializedProperty _myPivotTransform;
        private SerializedProperty _myLocalRotationAxis;

        // Interaction Events
        private SerializedProperty _myOnInteractionStart;
        private SerializedProperty _myOnInteractionFinish;

        // Constraints Events
        private SerializedProperty _myActivateConstraintsEvents;
        private SerializedProperty _myOnNoConstraintsReached;
        private SerializedProperty _myOnMaximalConstraintReached;
        private SerializedProperty _myOnMinimalConstraintReached;

        // Numerical Targets Events
        private SerializedProperty _myActivateNumericalTargetsEvents;
        private SerializedProperty _myOnNoNumericalTargetReached;
        private SerializedProperty _myNumericalTargetList;

        // Physical Targets Events
        private SerializedProperty _myActivatePhysicalTargetsEvents;
        private SerializedProperty _myPhysicalTargetList;
        #endregion


        #region Static Drawing Fields
        private static InteractionEvent _interactionEventToDraw = InteractionEvent.OnInteractionStart;
        private static ConstraintsEvent _constraintsEventToDraw = ConstraintsEvent.OnNoConstraintsReached;
        #endregion


        #region Life Cycles
        private void OnEnable()
        {
            // Interaction Settings
            _myInteractionPrimitive = serializedObject.FindProperty("interactionPrimitive");
            _myStopDoubleGraspingWhenOneHandRelease = serializedObject.FindProperty("stopDoubleGraspingWhenOneHandRelease");
            _myGraspingStrengthThreshold = serializedObject.FindProperty("graspingStrengthThreshold");
            _myPinchingStrengthThreshold = serializedObject.FindProperty("pinchingStrengthThreshold");
            _myDelayTimer = serializedObject.FindProperty("delayTimer");
            _myBlockObjectOnInteractionStart = serializedObject.FindProperty("blockObjectOnInteractionStart");
            _myBlockBodyPartsOnInteractionStart = serializedObject.FindProperty("blockBodyPartsOnInteractionStart");
            _mySetAsKinematicDuringInteraction = serializedObject.FindProperty("setAsKinematicDuringInteraction");
            _computeOnLocalBasis = serializedObject.FindProperty("computeOnLocalBasis");

            // RotationAroundPivot Settings
            _myPivotTransform = serializedObject.FindProperty("pivotTransform");
            _myLocalRotationAxis = serializedObject.FindProperty("localRotationAxis");

            // RotationAroundPivot Settings
            _mySlidingTransform = serializedObject.FindProperty("slidingTransform");
            _myLocalSlidingAxis = serializedObject.FindProperty("localSlidingAxis");

            // Interaction Events
            _myOnInteractionStart = serializedObject.FindProperty("onInteractionStart");
            _myOnInteractionFinish = serializedObject.FindProperty("onInteractionFinish");

            // Constraints Events
            _myActivateConstraintsEvents = serializedObject.FindProperty("activateConstraintsEvents");
            _myOnNoConstraintsReached = serializedObject.FindProperty("onNoConstraintsReached");
            _myOnMaximalConstraintReached = serializedObject.FindProperty("onMaximalConstraintReached");
            _myOnMinimalConstraintReached = serializedObject.FindProperty("onMinimalConstraintReached");

            // Numerical Targets Events
            _myActivateNumericalTargetsEvents = serializedObject.FindProperty("activateNumericalTargetsEvents");
            _myOnNoNumericalTargetReached = serializedObject.FindProperty("onNoNumericalTargetReached");
            _myNumericalTargetList = serializedObject.FindProperty("numericalTargetList");

            // Physical Targets Events
            _myActivatePhysicalTargetsEvents = serializedObject.FindProperty("activatePhysicalTargetsEvents");
            _myPhysicalTargetList = serializedObject.FindProperty("physicalTargetList");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Interaction Settings
            EditorGUILayout.PropertyField(_myInteractionPrimitive, new GUIContent(INTERACTION_PRIMITIVE_NAME));
            if (_myInteractionPrimitive.objectReferenceValue == null)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            InteractionTrigger trigger = ((InteractionPrimitive) _myInteractionPrimitive.objectReferenceValue)
                .interactionTrigger;
            BodyPartInteractionStrategy bodyPartStrategy =
                ((InteractionPrimitive) _myInteractionPrimitive.objectReferenceValue).bodyPart;

            EditorGUI.indentLevel++;
            if (trigger == InteractionTrigger.Grasp || trigger == InteractionTrigger.PinchOrGrasp)
                EditorGUILayout.PropertyField(_myGraspingStrengthThreshold,
                    new GUIContent(GRASPING_STRENGTH_THRESHOLD_NAME));
            if (trigger == InteractionTrigger.Pinch || trigger == InteractionTrigger.PinchOrGrasp)
                EditorGUILayout.PropertyField(_myPinchingStrengthThreshold,
                    new GUIContent(PINCHING_STRENGTH_THRESHOLD_NAME));

            if (trigger != InteractionTrigger.None && trigger != InteractionTrigger.Contact)
                EditorGUILayout.PropertyField(_myDelayTimer,
                    new GUIContent(DELAY_TIMER_NAME));

            if (bodyPartStrategy == BodyPartInteractionStrategy.TwoHands ||
                bodyPartStrategy == BodyPartInteractionStrategy.TwoHandsWithHead)
                EditorGUILayout.PropertyField(_myStopDoubleGraspingWhenOneHandRelease,
                    new GUIContent(STOP_DOUBLE_GRASPING_WHEN_ONE_HAND_RELEASE_NAME));
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(_myBlockObjectOnInteractionStart,
                new GUIContent(BLOCK_OBJECT_ON_INTERACTION_START_NAME));
            EditorGUILayout.PropertyField(_myBlockBodyPartsOnInteractionStart,
                new GUIContent(BLOCK_BODY_PART_ON_INTERACTION_START_NAME));
            EditorGUILayout.PropertyField(_mySetAsKinematicDuringInteraction,
                new GUIContent(SET_AS_KINEMATIC_DURING_INTERACTION_NAME));
            EditorGUILayout.PropertyField(_computeOnLocalBasis, new GUIContent(COMPUTE_ON_LOCAL_BASIS_NAME));

            InteractionStrategy strategy = ((InteractionPrimitive) _myInteractionPrimitive.objectReferenceValue)
                .interactionStrategy;
            switch (strategy)
            {
                // Sliding Settings
                case InteractionStrategy.Button:
                case InteractionStrategy.Sliding:
                {
                    EditorGUILayout.PropertyField(_mySlidingTransform, new GUIContent(SLIDING_TRANSFORM_NAME));
                    EditorGUILayout.PropertyField(_myLocalSlidingAxis, new GUIContent(LOCAL_SLIDING_AXIS_NAME));
                    break;
                }
                // RotationAroundPivot Settings
                case InteractionStrategy.RotationAroundPivot:
                {
                    EditorGUILayout.PropertyField(_myPivotTransform, new GUIContent(PIVOT_TRANSFORM_NAME));
                    EditorGUILayout.PropertyField(_myLocalRotationAxis, new GUIContent(LOCAL_ROTATION_AXIS_NAME));
                    break;
                }
            }

            // Interaction Events
            EditorGUILayout.Space();
            _myOnInteractionStart.isExpanded = EditorGUILayout.Foldout(_myOnInteractionStart.isExpanded,
                INTERACTION_EVENTS_HEADER, true, CustomEditorUtilities.HEADER_STYLE);
            _myOnInteractionFinish.isExpanded = _myOnInteractionStart.isExpanded;
            if (_myOnInteractionStart.isExpanded)
            {
                CustomEditorUtilities.DrawEventGroup(
                    new[] {InteractionEvent.OnInteractionStart, InteractionEvent.OnInteractionFinish},
                    new[] {ON_INTERACTION_START_NAME, ON_INTERACTION_FINISH_NAME},
                    new[] {_myOnInteractionStart, _myOnInteractionFinish}, ref _interactionEventToDraw);
            }

            // Constraints Events
            EditorGUILayout.Space();
            _myOnNoConstraintsReached.isExpanded = EditorGUILayout.Foldout(_myOnNoConstraintsReached.isExpanded,
                CONSTRAINTS_EVENTS_HEADER, true, CustomEditorUtilities.HEADER_STYLE);
            _myOnMinimalConstraintReached.isExpanded = _myOnNoConstraintsReached.isExpanded;
            _myOnMaximalConstraintReached.isExpanded = _myOnNoConstraintsReached.isExpanded;
            if (_myOnMaximalConstraintReached.isExpanded)
            {
                EditorGUILayout.PropertyField(_myActivateConstraintsEvents,
                    new GUIContent(ACTIVATE_CONSTRAINTS_EVENTS_NAME));
                if (_myActivateConstraintsEvents.boolValue)
                {
                    CustomEditorUtilities.DrawEventGroup(
                        new[] {ConstraintsEvent.OnNoConstraintsReached, ConstraintsEvent.OnMinimalConstraintsReached, ConstraintsEvent.OnMaximalConstraintsReached},
                        new[] {ON_NO_CONSTRAINTS_REACHED_NAME, ON_MINIMAL_CONSTRAINT_REACHED_NAME, ON_MAXIMAL_CONSTRAINT_REACHED_NAME},
                        new[] {_myOnNoConstraintsReached, _myOnMinimalConstraintReached, _myOnMaximalConstraintReached}, ref _constraintsEventToDraw);
                }
            }

            // Numerical Targets Events
            EditorGUILayout.Space();
            _myActivateNumericalTargetsEvents.isExpanded = EditorGUILayout.Foldout(
                _myActivateNumericalTargetsEvents.isExpanded, NUMERICAL_TARGETS_EVENTS_HEADER, true,
                CustomEditorUtilities.HEADER_STYLE);
            _myOnNoNumericalTargetReached.isExpanded = _myActivateNumericalTargetsEvents.isExpanded;
            _myNumericalTargetList.isExpanded = _myActivateNumericalTargetsEvents.isExpanded;
            if (_myActivateNumericalTargetsEvents.isExpanded)
            {
                EditorGUILayout.PropertyField(_myActivateNumericalTargetsEvents,
                    new GUIContent(ACTIVATE_NUMERICAL_TARGETS_EVENTS_NAME));
                if (_myActivateNumericalTargetsEvents.boolValue)
                {
                    EditorGUILayout.PropertyField(_myOnNoNumericalTargetReached,
                        new GUIContent(ON_NO_NUMERICAL_TARGET_REACHED_NAME));
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_myNumericalTargetList,
                        new GUIContent(NUMERICAL_TARGET_LIST_NAME), true);
                    EditorGUI.indentLevel--;
                }
            }

            // Physical Targets Events
            EditorGUILayout.Space();
            _myActivatePhysicalTargetsEvents.isExpanded = EditorGUILayout.Foldout(
                _myActivatePhysicalTargetsEvents.isExpanded, PHYSICAL_TARGETS_EVENTS_HEADER, true,
                CustomEditorUtilities.HEADER_STYLE);
            _myPhysicalTargetList.isExpanded = _myActivatePhysicalTargetsEvents.isExpanded;
            if (_myActivatePhysicalTargetsEvents.isExpanded)
            {
                EditorGUILayout.PropertyField(_myActivatePhysicalTargetsEvents,
                    new GUIContent(ACTIVATE_PHYSICAL_TARGETS_EVENTS_NAME));
                if (_myActivatePhysicalTargetsEvents.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_myPhysicalTargetList,
                        new GUIContent(PHYSICAL_TARGET_LIST_NAME), true);
                    EditorGUI.indentLevel--;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

    }

}
#endif