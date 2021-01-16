using Interhaptics.ObjectSnapper.core;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.Editor
{
    [CustomEditor(typeof(SnappingObject))]
    [CanEditMultipleObjects]
    public class SnappingObjectEditor : UnityEditor.Editor
    {
        #region Constants
        private const string SERIALIZEDPROPERTY_FixedASnappableActorInteraction = "fixedASnappableActorInteraction";
        private const string SERIALIZEDPROPERTY_AutomaticallyFindNearest = "automaticallyFindNearest";

        private const string TITLE_Settings = "Settings";
        private const string TITLE_DebugTracker = "Debug Tracker";
        private const string TITLE_SubscribedSnappingPrimitives = "Subscribed SnappingPrimitive";
        private const string TITLE_SubscribedAnimators = "Subscribed Animators";

        private const int VALUE_FoldoutLeftMargin = 16;
        private const int VALUE_FoldoutRightMargin = 6;
        #endregion

        #region Variables
        private SerializedProperty fixedASnappableActorInteractionCE;
        private SerializedProperty automaticallyFindNearestCE;

        private bool _snappingPrimitiveFoldout = true;
        private bool _animatorsFoldout = true;

        private GUIStyle _foldoutStyle;
        #endregion

        #region Life Cycle
        private void Awake()
        {
            _foldoutStyle = new GUIStyle(EditorStyles.foldout);
            _foldoutStyle.alignment = TextAnchor.MiddleCenter;
            _foldoutStyle.margin = new RectOffset(VALUE_FoldoutLeftMargin, VALUE_FoldoutRightMargin, 0,0);
        }

        private void OnEnable()
        {
            fixedASnappableActorInteractionCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_FixedASnappableActorInteraction);
            automaticallyFindNearestCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_AutomaticallyFindNearest);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField(TITLE_Settings, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fixedASnappableActorInteractionCE);
            EditorGUILayout.PropertyField(automaticallyFindNearestCE);

            if (Application.isPlaying)
            {
                SnappingObject snappingObject = (SnappingObject) target;

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField(TITLE_DebugTracker, EditorStyles.boldLabel);
                
                if (snappingObject)
                {
                    _snappingPrimitiveFoldout = EditorGUILayout.Foldout(_snappingPrimitiveFoldout, TITLE_SubscribedSnappingPrimitives, _foldoutStyle);
                    
                    if (_snappingPrimitiveFoldout)
                    {
                        SnappingPrimitive[] subscribedSnappingPrimitives = snappingObject.SubscribedSnappingPrimitives;

                        if (subscribedSnappingPrimitives != null && subscribedSnappingPrimitives.Length > 0)
                        {
                            foreach (SnappingPrimitive snappingPrimitive in subscribedSnappingPrimitives)
                            {
                                if (snappingPrimitive == null)
                                    continue;
                                
                                EditorGUILayout.ObjectField(snappingPrimitive, typeof(SnappingPrimitive), true);
                            }
                        }
                    }

                    EditorGUILayout.Space();
                    _animatorsFoldout = EditorGUILayout.Foldout(_animatorsFoldout, TITLE_SubscribedAnimators, _foldoutStyle);
                    if (_animatorsFoldout)
                    {
                        EditorGUILayout.Space();
                        ASnappableActor[] subscribedASnappableActor = snappingObject.SubscribedASnappableActors;
                    
                        if (subscribedASnappableActor != null && subscribedASnappableActor.Length > 0)
                        {
                            foreach (ASnappableActor aSnappableActors in subscribedASnappableActor)
                            {
                                if (aSnappableActors == null)
                                    continue;
                                
                                EditorGUILayout.ObjectField(aSnappableActors, typeof(Animator), true);
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}
