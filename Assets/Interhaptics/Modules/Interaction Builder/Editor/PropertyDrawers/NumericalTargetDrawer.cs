#if UNITY_EDITOR
using NumericalTarget = Interhaptics.Modules.Interaction_Builder.Core.InteractionObject.NumericalTarget;

using UnityEditor;
using UnityEngine;


namespace Interhaptics.Modules.Interaction_Builder.Editor.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(NumericalTarget))]
    public class NumericalTargetDrawer : PropertyDrawer
    {

        #region Private Consts
        private const string ON_TARGET_REACHED_EVENT_NAME = "Target Reached";
        private const string ON_TARGET_NOT_REACHED_ANYMORE_EVENT_NAME = "Target Not Reached Anymore";
        #endregion


        #region Private Fields
        private bool _showOnTargetReachedEvent;
        #endregion


        #region Private Members
        private string OnTargetReachedEventToDrawName =>
            _showOnTargetReachedEvent ? "onTargetReachedEvent" : "onTargetNotReachedAnymoreEvent";

        private static SerializedProperty OnTargetReachedEventProperty(SerializedProperty property, string propertyName) =>
            property.FindPropertyRelative(propertyName);
        private SerializedProperty OnTargetReachedEventToDrawProperty(SerializedProperty property) =>
            OnTargetReachedEventProperty(property, OnTargetReachedEventToDrawName);
        private static SerializedProperty OnTargetReachedEventProperty(SerializedProperty property) =>
            OnTargetReachedEventProperty(property, "onTargetReachedEvent");
        private static SerializedProperty OnTargetNotReachedAnymoreEventProperty(SerializedProperty property) =>
            OnTargetReachedEventProperty(property, "onTargetNotReachedAnymoreEvent");

        private static string OnTargetReachedEventName(SerializedProperty property) =>
            ON_TARGET_REACHED_EVENT_NAME + $" ({OnTargetReachedEventProperty(property).FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize})";
        private static string OnTargetNotReachedAnymoreEventName(SerializedProperty property) =>
            ON_TARGET_NOT_REACHED_ANYMORE_EVENT_NAME + $" ({OnTargetNotReachedAnymoreEventProperty(property).FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize})";
        #endregion


        #region GUI Methods
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(position.x, position.y,
                position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            if (property.isExpanded)
            {
                Rect targetValueRect = new Rect(foldoutRect.x,
                    foldoutRect.y + foldoutRect.height + EditorGUIUtility.standardVerticalSpacing, position.width,
                    EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(targetValueRect, property.FindPropertyRelative("targetValue"));
                Rect boxEventRect = new Rect(targetValueRect.x,
                    targetValueRect.y + targetValueRect.height + EditorGUIUtility.standardVerticalSpacing +
                    GUI.skin.box.margin.top, targetValueRect.width,
                    position.height - foldoutRect.height - targetValueRect.height -
                    2 * EditorGUIUtility.standardVerticalSpacing - GUI.skin.box.margin.top);
                DrawBoxEvent(boxEventRect, property);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float buttonWidth = (EditorGUIUtility.currentViewWidth -
                                 EditorStyles.inspectorDefaultMargins.padding.horizontal -
                                 EditorStyles.inspectorDefaultMargins.margin.horizontal -
                                 GUI.skin.box.padding.horizontal - GUI.skin.box.margin.horizontal -
                                 CustomEditorUtilities.BUTTON_STYLE.padding.horizontal -
                                 GUI.skin.button.margin.right) / 2f;
            string reachLabel = OnTargetReachedEventName(property);
            string notReachLabel = OnTargetNotReachedAnymoreEventName(property);
            float buttonAreaHeight =
                Mathf.Max(
                    CustomEditorUtilities.BUTTON_STYLE.CalcHeight(new GUIContent(reachLabel),
                        buttonWidth),
                    CustomEditorUtilities.BUTTON_STYLE.CalcHeight(new GUIContent(notReachLabel),
                        buttonWidth));

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + (property.isExpanded
                       ? EditorGUI.GetPropertyHeight(OnTargetReachedEventToDrawProperty(property)) +
                         3 * EditorGUIUtility.standardVerticalSpacing +
                         buttonAreaHeight + 
                         2 * EditorGUIUtility.singleLineHeight +
                         GUI.skin.box.padding.bottom + GUI.skin.box.padding.top +
                         GUI.skin.box.margin.bottom + GUI.skin.box.margin.top
                       : 0);
        }

        private void DrawBoxEvent(Rect position, SerializedProperty property)
        {
            GUI.Box(position, "", GUI.skin.box);
            float buttonWidth = (position.width - GUI.skin.box.padding.left - GUI.skin.box.padding.right -
                                 GUI.skin.button.margin.right) / 2f; 
            Rect areaRect = new Rect(position.x + GUI.skin.box.padding.left,
                position.y + GUI.skin.box.padding.top, buttonWidth,
                EditorGUIUtility.singleLineHeight);

            string reachLabel = OnTargetReachedEventName(property);
            string notReachLabel = OnTargetNotReachedAnymoreEventName(property);

            float maxButtonHeight = areaRect.height =
                CustomEditorUtilities.BUTTON_STYLE.CalcHeight(new GUIContent(reachLabel), buttonWidth);
            if (_showOnTargetReachedEvent)
                GUI.enabled = false;
            _showOnTargetReachedEvent = GUI.Toggle(areaRect, _showOnTargetReachedEvent, reachLabel,
                CustomEditorUtilities.BUTTON_STYLE);
            if (_showOnTargetReachedEvent)
                GUI.enabled = true;

            areaRect.x += areaRect.width + GUI.skin.button.margin.right;
            if ((areaRect.height =
                    CustomEditorUtilities.BUTTON_STYLE.CalcHeight(new GUIContent(notReachLabel), buttonWidth)) >
                maxButtonHeight)
                maxButtonHeight = areaRect.height;
            if (!_showOnTargetReachedEvent)
                GUI.enabled = false;
            _showOnTargetReachedEvent = !GUI.Toggle(areaRect, !_showOnTargetReachedEvent, notReachLabel,
                CustomEditorUtilities.BUTTON_STYLE);
            if (!_showOnTargetReachedEvent)
                GUI.enabled = true;

            Rect targetEventRect = new Rect(position.x + GUI.skin.box.padding.left,
                areaRect.y + maxButtonHeight + 3 * EditorGUIUtility.standardVerticalSpacing +
                2 * GUI.skin.button.margin.bottom,
                position.width - GUI.skin.box.padding.left - GUI.skin.box.padding.right,
                EditorGUI.GetPropertyHeight(OnTargetReachedEventToDrawProperty(property)));

            EditorGUI.PropertyField(targetEventRect, OnTargetReachedEventToDrawProperty(property));
        }
        #endregion

    }

}
#endif