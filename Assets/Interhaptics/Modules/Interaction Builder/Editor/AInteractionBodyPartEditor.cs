#if UNITY_EDITOR
using Interhaptics.Modules.Interaction_Builder.Core.Abstract;

using UnityEditor;
using UnityEngine;


namespace Interhaptics.Modules.Interaction_Builder.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(AInteractionBodyPart), true)]
    public class AInteractionBodyPartEditor: UnityEditor.Editor
    {

        #region Nested Enums
        private enum InteractionEvent
        {
            OnInteractionStart,
            OnInteractionFinish,
        }
        #endregion


        #region Private Fields
        private SerializedProperty _myBodyPart;
        private SerializedProperty _myCustomPointToFollow;

        private SerializedProperty _myOnInteractionStart;
        private SerializedProperty _myOnInteractionFinish;

        private SerializedProperty _myCastSphereRadius;
        private SerializedProperty _myCastSphereCenter;

        private SerializedProperty _myDrawCastSphereInEditor;
        private SerializedProperty _myCastSphereColor;
        #endregion


        #region Static Drawing Fields
        private static InteractionEvent _eventToDraw = InteractionEvent.OnInteractionStart;
        #endregion


        #region Life Cycles
        private void OnEnable()
        {
            _myBodyPart = serializedObject.FindProperty("bodyPart");
            _myCustomPointToFollow = serializedObject.FindProperty("customPointToFollow");

            _myOnInteractionStart = serializedObject.FindProperty("onInteractionStart");
            _myOnInteractionFinish = serializedObject.FindProperty("onInteractionFinish");

            _myCastSphereRadius = serializedObject.FindProperty("castSphereRadius");
            _myCastSphereCenter = serializedObject.FindProperty("castSphereCenter");

            _myDrawCastSphereInEditor = serializedObject.FindProperty("drawCastSphereInEditor");
            _myCastSphereColor = serializedObject.FindProperty("castSphereColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_myBodyPart, new GUIContent("Body Part"));
            EditorGUILayout.PropertyField(_myCustomPointToFollow, new GUIContent("Custom Point To Follow"));

            GUIStyle foldoutStyle = EditorStyles.foldout;
            foldoutStyle.fontStyle = FontStyle.Bold;

            EditorGUILayout.Space();
            _myOnInteractionStart.isExpanded = EditorGUILayout.Foldout(_myOnInteractionStart.isExpanded,
                "Interaction Events", true, foldoutStyle);
            _myOnInteractionFinish.isExpanded = _myOnInteractionStart.isExpanded;
            if (_myOnInteractionStart.isExpanded || _myOnInteractionFinish.isExpanded)
            {
                CustomEditorUtilities.DrawEventGroup(
                    new[] {InteractionEvent.OnInteractionStart, InteractionEvent.OnInteractionFinish},
                    new[] {"Interaction Start", "Interaction Finish"},
                    new[] {_myOnInteractionStart, _myOnInteractionFinish}, ref _eventToDraw);
            }

            EditorGUILayout.PropertyField(_myCastSphereRadius, new GUIContent("Cast Sphere Radius"));
            EditorGUILayout.PropertyField(_myCastSphereCenter, new GUIContent("Cast Sphere Center"));
            
            EditorGUILayout.PropertyField(_myDrawCastSphereInEditor, new GUIContent("Draw Cast Sphere In Editor"));
            if (_myDrawCastSphereInEditor.boolValue)
                EditorGUILayout.PropertyField(_myCastSphereColor, new GUIContent("Cast Sphere Color"));

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

    }

}
#endif