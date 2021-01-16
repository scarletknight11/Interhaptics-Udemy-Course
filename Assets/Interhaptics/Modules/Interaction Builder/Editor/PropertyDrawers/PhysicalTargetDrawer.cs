#if UNITY_EDITOR
using PhysicalTarget = Interhaptics.Modules.Interaction_Builder.Core.InteractionObject.PhysicalTarget;

using UnityEditor;
using UnityEngine;


namespace Interhaptics.Modules.Interaction_Builder.Editor.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(PhysicalTarget))]
    public class PhysicalTargetDrawer : PropertyDrawer
    {

        #region Nested Enums
        private enum PhysicalTargetEvent
        {
            OnTargetEnterEvent,
            OnTargetStayEvent,
            OnTargetExitEvent,
        }
        #endregion


        #region Private Consts
        private const string ON_TARGET_ENTER_EVENT_NAME = "Target Enter";
        private const string ON_TARGET_STAY_EVENT_NAME = "Target Stay";
        private const string ON_TARGET_EXIT_EVENT_NAME = "Target Exit";
        #endregion


        #region Private Fields
        private PhysicalTargetEvent _eventToDraw;
        #endregion


        #region Private Members
        private static string OnTargetReachedEventToString(PhysicalTargetEvent eventTarget)
        {
            switch (eventTarget)
            {
                case PhysicalTargetEvent.OnTargetEnterEvent:
                    return "onTargetEnterEvent";
                case PhysicalTargetEvent.OnTargetStayEvent:
                    return "onTargetStayEvent";
                case PhysicalTargetEvent.OnTargetExitEvent:
                    return "onTargetExitEvent";
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
        private string OnTargetReachedEventToDrawName => OnTargetReachedEventToString(_eventToDraw);

        private static SerializedProperty
            OnTargetEventToDrawProperty(SerializedProperty property, string propertyName) =>
            property.FindPropertyRelative(propertyName);
        private SerializedProperty OnTargetEventToDrawProperty(SerializedProperty property) =>
            OnTargetEventToDrawProperty(property, OnTargetReachedEventToDrawName);

        private static SerializedProperty TargetTypeProperty(SerializedProperty property) =>
            property.FindPropertyRelative("targetType");
        private static SerializedProperty TargetColliderProperty(SerializedProperty property) =>
            property.FindPropertyRelative("targetCollider");
        private static SerializedProperty TargetTagProperty(SerializedProperty property) =>
            property.FindPropertyRelative("targetTag");
        private static SerializedProperty TargetLayerProperty(SerializedProperty property) =>
            property.FindPropertyRelative("targetLayer");

        private static string OnTargetEnterEventName(SerializedProperty property) =>
            ON_TARGET_ENTER_EVENT_NAME + $" ({property.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize})";
        private static string OnTargetStayEventName(SerializedProperty property) =>
            ON_TARGET_STAY_EVENT_NAME + $" ({property.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize})";
        private static string OnTargetExitEventName(SerializedProperty property) =>
            ON_TARGET_EXIT_EVENT_NAME + $" ({property.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize})";
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
                Rect targetTypeRect = new Rect(foldoutRect.x,
                    foldoutRect.y + foldoutRect.height + EditorGUIUtility.standardVerticalSpacing, position.width,
                    EditorGUIUtility.singleLineHeight);
                SerializedProperty targetProperty = TargetTypeProperty(property);
                EditorGUI.PropertyField(targetTypeRect, targetProperty);

                Rect targetValueRect = new Rect(targetTypeRect.x,
                    targetTypeRect.y + targetTypeRect.height + EditorGUIUtility.standardVerticalSpacing, position.width,
                    0);

                PhysicalTarget.TargetType type = (PhysicalTarget.TargetType) targetProperty.enumValueIndex;
                switch (type)
                {
                    case PhysicalTarget.TargetType.TargetOnCollider:
                        targetValueRect.height = EditorGUI.GetPropertyHeight(TargetColliderProperty(property));
                        EditorGUI.indentLevel++;
                        EditorGUI.PropertyField(targetValueRect, TargetColliderProperty(property), true);
                        EditorGUI.indentLevel--;
                        break;
                    case PhysicalTarget.TargetType.TargetOnTag:
                        targetValueRect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.PropertyField(targetValueRect, TargetTagProperty(property));
                        break;
                    case PhysicalTarget.TargetType.TargetOnLayer:
                        targetValueRect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.PropertyField(targetValueRect, TargetLayerProperty(property));
                        break;
                }

                Rect boxEventRect = new Rect(targetValueRect.x,
                    targetValueRect.y + targetValueRect.height + EditorGUIUtility.standardVerticalSpacing +
                    GUI.skin.box.margin.top, targetValueRect.width,
                    position.height - foldoutRect.height - targetTypeRect.height - targetValueRect.height -
                    3 * EditorGUIUtility.standardVerticalSpacing - GUI.skin.box.margin.top);
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
                                 2*GUI.skin.button.margin.right) / 3f;

            string enterEvent = OnTargetEnterEventName(OnTargetEventToDrawProperty(property,
                OnTargetReachedEventToString(PhysicalTargetEvent.OnTargetEnterEvent)));
            string stayEvent = OnTargetStayEventName(OnTargetEventToDrawProperty(property,
                OnTargetReachedEventToString(PhysicalTargetEvent.OnTargetStayEvent)));
            string exitEvent = OnTargetExitEventName(OnTargetEventToDrawProperty(property,
                OnTargetReachedEventToString(PhysicalTargetEvent.OnTargetExitEvent)));
            float buttonAreaHeight =
                Mathf.Max(CustomEditorUtilities.BUTTON_STYLE.CalcHeight(new GUIContent(enterEvent), buttonWidth),
                    Mathf.Max(CustomEditorUtilities.BUTTON_STYLE.CalcHeight(new GUIContent(stayEvent), buttonWidth),
                        CustomEditorUtilities.BUTTON_STYLE.CalcHeight(new GUIContent(exitEvent), buttonWidth)));

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + (property.isExpanded
                       ? EditorGUI.GetPropertyHeight(OnTargetEventToDrawProperty(property)) +
                         ((PhysicalTarget.TargetType) TargetTypeProperty(property).enumValueIndex ==
                          PhysicalTarget.TargetType.TargetOnCollider
                             ? EditorGUI.GetPropertyHeight(TargetColliderProperty(property))
                             : EditorGUIUtility.singleLineHeight) + 3 * EditorGUIUtility.standardVerticalSpacing +
                         2 * EditorGUIUtility.singleLineHeight + buttonAreaHeight + GUI.skin.box.padding.bottom +
                         GUI.skin.box.padding.top + GUI.skin.box.margin.bottom + GUI.skin.box.margin.top
                       : 0);
        }

        private void DrawBoxEvent(Rect position, SerializedProperty property)
        {
            GUI.Box(position, "", GUI.skin.box);
            float buttonWidth = (position.width - GUI.skin.box.padding.left - 2*GUI.skin.box.padding.right -
                                 GUI.skin.button.margin.right) / 3f;
            Rect areaRect = new Rect(position.x + GUI.skin.box.padding.left,
                position.y + GUI.skin.box.padding.top, buttonWidth,
                EditorGUIUtility.singleLineHeight);

            float maxButtonHeight = DrawButtons(areaRect, property, buttonWidth);

            Rect targetEventRect = new Rect(position.x + GUI.skin.box.padding.left,
                areaRect.y + maxButtonHeight + 3 * EditorGUIUtility.standardVerticalSpacing +
                2 * GUI.skin.button.margin.bottom,
                position.width - GUI.skin.box.padding.left - GUI.skin.box.padding.right,
                EditorGUI.GetPropertyHeight(OnTargetEventToDrawProperty(property)));

            EditorGUI.PropertyField(targetEventRect, OnTargetEventToDrawProperty(property));
        }

        private float DrawButtons(Rect areaRect, SerializedProperty property, float buttonWidth)
        {
            string label = OnTargetEnterEventName(OnTargetEventToDrawProperty(property,
                OnTargetReachedEventToString(PhysicalTargetEvent.OnTargetEnterEvent)));
            float maxButtonHeight = areaRect.height =
                CustomEditorUtilities.BUTTON_STYLE.CalcHeight(new GUIContent(label), buttonWidth);
            bool isSelected = _eventToDraw == PhysicalTargetEvent.OnTargetEnterEvent;
            if (isSelected)
                GUI.enabled = false;
            if (GUI.Toggle(areaRect, isSelected, label, CustomEditorUtilities.BUTTON_STYLE))
                _eventToDraw = PhysicalTargetEvent.OnTargetEnterEvent;
            if (isSelected)
                GUI.enabled = true;

            areaRect.x += areaRect.width + GUI.skin.button.margin.right;
            label = OnTargetStayEventName(OnTargetEventToDrawProperty(property,
                OnTargetReachedEventToString(PhysicalTargetEvent.OnTargetStayEvent)));
            if ((areaRect.height = CustomEditorUtilities.BUTTON_STYLE.CalcHeight(new GUIContent(label), buttonWidth)) >
                maxButtonHeight)
                maxButtonHeight = areaRect.height;
            isSelected = _eventToDraw == PhysicalTargetEvent.OnTargetStayEvent;
            if (isSelected)
                GUI.enabled = false;
            if (GUI.Toggle(areaRect, isSelected, label, CustomEditorUtilities.BUTTON_STYLE))
                _eventToDraw = PhysicalTargetEvent.OnTargetStayEvent;
            if (isSelected)
                GUI.enabled = true;

            areaRect.x += areaRect.width + GUI.skin.button.margin.right;
            label = OnTargetExitEventName(OnTargetEventToDrawProperty(property,
                OnTargetReachedEventToString(PhysicalTargetEvent.OnTargetExitEvent)));
            if ((areaRect.height = CustomEditorUtilities.BUTTON_STYLE.CalcHeight(new GUIContent(label), buttonWidth)) >
                maxButtonHeight)
                maxButtonHeight = areaRect.height;
            isSelected = _eventToDraw == PhysicalTargetEvent.OnTargetExitEvent;
            if (isSelected)
                GUI.enabled = false;
            if (GUI.Toggle(areaRect, isSelected, label, CustomEditorUtilities.BUTTON_STYLE))
                _eventToDraw = PhysicalTargetEvent.OnTargetExitEvent;
            if (isSelected)
                GUI.enabled = true;

            return maxButtonHeight;
        }
        #endregion

    }

}
#endif