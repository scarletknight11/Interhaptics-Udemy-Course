using Interhaptics.ObjectSnapper.core;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.Editor
{
    [CustomEditor(typeof(SnappingData))]
    public class SnappingDataEditor : UnityEditor.Editor
    {
        #region Constants
        private const string LABEL_NoDatas = "You havn't any datas saved for this scene for now. You can create datas by editing a pose with an the SnappingPrimitive script";
        private const string LABEL_ObjectID = "ObjectID";
        private const string LABEL_Actions = "Actions";

        private const string BUTTON_Delete = "Delete";
        #endregion

        #region Variables
        private GUIStyle _titleEditorStyle = null;
        #endregion
        
        #region Life Cycle
        private void Awake()
        {
            _titleEditorStyle = new GUIStyle(EditorStyles.boldLabel);
            _titleEditorStyle.alignment = TextAnchor.MiddleCenter;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            string[] dataKeys = ((SnappingData)target).snappingDataDictionary.GetDatabaseKeys();
            if (dataKeys == null || dataKeys.Length == 0)
                GUILayout.Label(LABEL_NoDatas, EditorStyles.wordWrappedLabel);
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(LABEL_ObjectID, EditorStyles.boldLabel);
                GUILayout.Label(LABEL_Actions, _titleEditorStyle);
                GUILayout.EndHorizontal();
                
                EditorGUILayout.Space();

                foreach (string key in dataKeys)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(key, EditorStyles.label);
                    if (GUILayout.Button(BUTTON_Delete, EditorStyles.miniButtonMid))
                        ((SnappingData)target).snappingDataDictionary.Remove(key);

                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}
