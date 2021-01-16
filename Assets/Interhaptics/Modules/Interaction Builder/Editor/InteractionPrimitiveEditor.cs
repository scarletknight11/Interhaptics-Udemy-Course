#if UNITY_EDITOR
using Interhaptics.Modules.Interaction_Builder.Core;

using InteractionStrategy = Interhaptics.InteractionsEngine.Shared.Types.InteractionStrategy;

using UnityEditor;
using UnityEngine;


namespace Interhaptics.Modules.Interaction_Builder.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(InteractionPrimitive))]
    public class InteractionPrimitiveEditor : UnityEditor.Editor
    {

        #region Private Fields
        private SerializedProperty _myInteractionStrategy;
        private SerializedProperty _myInteractionTrigger;
        private SerializedProperty _myBodyPartInteractionStrategy;

        private SerializedProperty _myHapticInteractionFeedback;
        private SerializedProperty _myRenderStiffness;
        private SerializedProperty _myRenderVibration;
        private SerializedProperty _myRenderTexture;
        private SerializedProperty _myDegreeValueCorresponding;
        private SerializedProperty _myVibrationEvaluationMode;

        private SerializedProperty _myYSpace;

        private SerializedProperty _myMaximalConstraint;
        private SerializedProperty _myEnableMaximalConstraint;

        private SerializedProperty _myMinimalConstraint;
        private SerializedProperty _myEnableMinimalConstraint;
        #endregion


        #region Life Cycles
        private void OnEnable()
        {
            _myInteractionStrategy = serializedObject.FindProperty("interactionStrategy");
            _myInteractionTrigger = serializedObject.FindProperty("interactionTrigger");
            _myBodyPartInteractionStrategy = serializedObject.FindProperty("bodyPart");

            _myHapticInteractionFeedback = serializedObject.FindProperty("hapticInteractionFeedback");
            _myRenderStiffness = serializedObject.FindProperty("renderStiffness");
            _myRenderVibration = serializedObject.FindProperty("renderVibration");
            _myRenderTexture = serializedObject.FindProperty("renderTexture");
            _myDegreeValueCorresponding = serializedObject.FindProperty("degreeValueCorresponding");
            _myVibrationEvaluationMode = serializedObject.FindProperty("vibrationEvaluationMode");

            _myYSpace = serializedObject.FindProperty("ySpace");

            _myMaximalConstraint = serializedObject.FindProperty("maximalConstraint");
            _myEnableMaximalConstraint = serializedObject.FindProperty("enableMaximalConstraint");

            _myMinimalConstraint = serializedObject.FindProperty("minimalConstraint");
            _myEnableMinimalConstraint = serializedObject.FindProperty("enableMinimalConstraint");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            bool interactionStrategyHaveConstraints = false;
            bool isYRotation = false;
            bool isRotationAroundPivot = false;
            switch ((InteractionStrategy)_myInteractionStrategy.intValue)
            {
                case InteractionStrategy.FreePositionWithYRotation:
                    isYRotation = true;
                    break;
                case InteractionStrategy.RotationAroundPivot:
                    isRotationAroundPivot = true;
                    interactionStrategyHaveConstraints = true;
                    break;
                case InteractionStrategy.Sliding:
                case InteractionStrategy.Button:
                    interactionStrategyHaveConstraints = true;
                    break;
            }

            EditorGUILayout.PropertyField(_myInteractionStrategy, new GUIContent("Interaction Strategy"));
            EditorGUILayout.PropertyField(_myInteractionTrigger, new GUIContent("Interaction Trigger"));
            EditorGUILayout.PropertyField(_myBodyPartInteractionStrategy, new GUIContent("Body Part"));

            EditorGUILayout.PropertyField(_myHapticInteractionFeedback, new GUIContent("Haptic Interaction Feedback"));
            if (_myHapticInteractionFeedback.objectReferenceValue)
            {
                EditorGUILayout.PropertyField(_myRenderStiffness, new GUIContent("Render Stiffness"));
                EditorGUILayout.PropertyField(_myRenderTexture, new GUIContent("Render Texture"));
                EditorGUILayout.PropertyField(_myRenderVibration, new GUIContent("Render Vibration"));
                if (isRotationAroundPivot && (_myRenderTexture.boolValue || (_myRenderStiffness.boolValue)))
                    EditorGUILayout.PropertyField(_myDegreeValueCorresponding,
                        new GUIContent("Degree Value Corresponding"));
                if (_myRenderVibration.boolValue)
                    EditorGUILayout.PropertyField(_myVibrationEvaluationMode,
                        new GUIContent("Vibration Evaluation Mode"));
            }

            if (isYRotation)
                EditorGUILayout.PropertyField(_myYSpace, new GUIContent("Y Space"));

            if (interactionStrategyHaveConstraints)
            {
                EditorGUILayout.PropertyField(_myEnableMaximalConstraint, new GUIContent("Enable Maximal Constraint"));
                if (_myEnableMaximalConstraint.boolValue)
                    EditorGUILayout.PropertyField(_myMaximalConstraint, new GUIContent("Maximal Constraint"));

                EditorGUILayout.PropertyField(_myEnableMinimalConstraint, new GUIContent("Enable Minimal Constraint"));
                if (_myEnableMinimalConstraint.boolValue)
                    EditorGUILayout.PropertyField(_myMinimalConstraint, new GUIContent("Minimal Constraint"));
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

    }

}
#endif