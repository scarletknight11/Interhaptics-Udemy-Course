#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace Interhaptics.Modules.Interaction_Builder.Editor
{

    internal static class CustomEditorUtilities
    {
        #region Public Members
        public static readonly GUIStyle HEADER_STYLE;
        public static readonly GUIStyle BUTTON_STYLE;
        #endregion


        #region STATIC CONSTRUCTORS
        static CustomEditorUtilities()
        {
            BUTTON_STYLE = GUI.skin.button;
            BUTTON_STYLE.wordWrap = true;

            HEADER_STYLE = EditorStyles.foldout;
            HEADER_STYLE.fontStyle = FontStyle.Bold;
        }
        #endregion


        #region GUI Methods
        public static void DrawEventGroup<T>(T[] listToDraw, string[] eventNames, SerializedProperty[] eventProperties,
            ref T eventDrawn) where T : System.Enum
        {
            Color tempColor = GUI.backgroundColor;
            GUI.backgroundColor = tempColor / 1f;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = tempColor;

            EditorGUILayout.BeginHorizontal();
            int index = -1;
            for (int i = 0; i < listToDraw.Length; i++)
            {
                T tempEvent = listToDraw[i];
                SerializedProperty temp = eventProperties[i];
                int eventNum = 0;
                if (temp != null && (temp = temp.FindPropertyRelative("m_PersistentCalls.m_Calls")) != null)
                    eventNum = temp.arraySize;
                bool isSelected = eventDrawn.CompareTo(tempEvent) == 0;
                if (tempEvent == null)
                    continue;

                if (isSelected)
                    GUI.enabled = false;
                bool b = GUILayout.Toggle(isSelected, $"{eventNames[i]} ({eventNum})", BUTTON_STYLE);
                if (isSelected)
                    GUI.enabled = true;

                if (!b)
                    continue;

                eventDrawn = tempEvent;
                index = i;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(eventProperties[index], new GUIContent(eventDrawn.ToString()));

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }
        #endregion

    }

}
#endif