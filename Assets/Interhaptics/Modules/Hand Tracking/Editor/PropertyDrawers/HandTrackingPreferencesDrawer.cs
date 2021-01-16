#if UNITY_EDITOR
using System.Linq;


namespace Interhaptics.HandTracking.Editor.PropertyDrawers
{

    [UnityEditor.CustomPropertyDrawer(typeof(Types.HandTrackingPreferences))]
    public class HandTrackingPreferencesDrawer : UnityEditor.PropertyDrawer
    {

        #region FIELDS NAME
        // HandTrackingPreferences fields
        private const string HAND_TRACKING_PROVIDER_DATA_FIELD_NAME = "data";
        #endregion


        #region CONSTS
        private const string HEADER_LABEL = "My tracking preferences";

        private const string SKELETON_BLACK_ICON_RESOURCE_NAME = "boneIcon-black";
        private const string SKELETON_GRAY_ICON_RESOURCE_NAME = "boneIcon-gray";
        private const string SKELETON_WHITE_ICON_RESOURCE_NAME = "boneIcon-white";
        private const string SKELETON_ICON_TOOLTIP = "This tracking support the skeletal input";
        #endregion


        #region PRIVATE FIELDS
        private bool _showList = true;
        private UnityEditorInternal.ReorderableList _list = null;
        #endregion


        #region PRIVATE METHODS
        private void Init(UnityEditor.SerializedProperty property)
        {
            _list = new UnityEditorInternal.ReorderableList(property.serializedObject,
                property.FindPropertyRelative(HAND_TRACKING_PROVIDER_DATA_FIELD_NAME), true, true, true, true)
            {
                // Drawing
                drawElementCallback = DrawListItems,
                drawHeaderCallback = DrawHeader,

                // Adding
                onCanAddCallback = CanAddCallback,
                onAddDropdownCallback = AddDropdownCallback,

                // Removing
                onCanRemoveCallback = CanRemoveCallback,
            };
        }

        private static bool CanAddCallback(UnityEditorInternal.ReorderableList list)
        {
            return !UnityEngine.Application.isPlaying && list.count < Tools.ReflectionNames.GetCompatibleAssemblies()
                       .Sum(assembly =>
                           assembly.GetTypes().Count(t =>
                               t.GetInterfaces().Contains(typeof(Interfaces.ITrackingProvider))));
        }

        private void AddDropdownCallback(UnityEngine.Rect rect, UnityEditorInternal.ReorderableList list)
        {
            UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
            int arraySize = _list.serializedProperty.arraySize;

            foreach (System.Reflection.Assembly assembly in Tools.ReflectionNames.GetCompatibleAssemblies())
            {
                foreach (System.Type trackingProviderType in assembly.GetTypes().Where(t =>
                    t.GetInterfaces().Contains(typeof(Interfaces.ITrackingProvider))))
                {
                    bool found = false;
                    object instance = System.Activator.CreateInstance(trackingProviderType);
                    if (assembly.GetName().Name != Tools.ReflectionNames.DEFAULT_ASSEMBLY_NAME &&
                        !Interhaptics.Editor.Utils.BuildProcessor.GHTCanExport(trackingProviderType, instance))
                        continue;

                    string displayName = trackingProviderType.GetMethod(Tools.ReflectionNames.DISPLAY_NAME_PROVIDER_METHOD_NAME)
                        ?.Invoke(instance, null).ToString();
                    for (int i = 0; i < arraySize; i++)
                    {
                        if (_list.serializedProperty.GetArrayElementAtIndex(i)
                                .FindPropertyRelative(Tools.ReflectionNames.TRACKING_NAME_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME)
                                .stringValue != displayName)
                            continue;
                        found = true;
                        break;
                    }

                    if (found)
                        menu.AddDisabledItem(new UnityEngine.GUIContent(displayName));
                    else
                    {
                        string description = trackingProviderType.GetMethod(Tools.ReflectionNames.DESCRIPTION_PROVIDER_METHOD_NAME)
                            ?.Invoke(instance, null).ToString();
                        object canHaveSkeleton = trackingProviderType
                            .GetMethod(Tools.ReflectionNames.CAN_HAVE_SKELETON_PROVIDER_METHOD_NAME)
                            ?.Invoke(instance, null);

                        menu.AddItem(new UnityEngine.GUIContent(displayName, description), false, ClickHandler,
                            new Types.HandTrackingPreferences.SHandTrackingProviderInfo
                            {
                                trackingName = displayName,
                                canHaveSkeleton = (bool?) canHaveSkeleton ?? false,
                                description = description,
                                providerType = trackingProviderType.ToString(),
                            });
                    }
                }
            }

            menu.ShowAsContext();
        }

        private static bool CanRemoveCallback(UnityEditorInternal.ReorderableList list)
        {
            return !UnityEngine.Application.isPlaying && list.count > 0;
        }

        private void ClickHandler(object target)
        {
            int arraySize = _list.serializedProperty.arraySize;
            _list.serializedProperty.arraySize++;
            _list.index = arraySize;
            Types.HandTrackingPreferences.SHandTrackingProviderInfo castedTarget =
                (Types.HandTrackingPreferences.SHandTrackingProviderInfo) target;
            UnityEditor.SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(arraySize);

            element.FindPropertyRelative(Tools.ReflectionNames.TRACKING_NAME_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME).stringValue
                = castedTarget.trackingName;
            element.FindPropertyRelative(Tools.ReflectionNames.CAN_HAVE_SKELETON_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME).boolValue
                = castedTarget.canHaveSkeleton;
            element.FindPropertyRelative(Tools.ReflectionNames.DESCRIPTION_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME).stringValue
                = castedTarget.description;
            element.FindPropertyRelative(Tools.ReflectionNames.PROVIDER_TYPE_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME).stringValue
                = castedTarget.providerType;

            _list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private static bool IsCompatibleWitchActualConfig(string providerType)
        {
            System.Type t = null;
            return !string.IsNullOrEmpty(providerType) &&
                   Tools.ReflectionNames.GetCompatibleAssemblies()
                       .Any(assembly => (t = assembly.GetType(providerType)) != null) &&
                   IsCompatibleWitchActualConfig(t);
        }
        
        private static bool IsCompatibleWitchActualConfig(System.Type providerType)
        {
            return providerType != null && Types.HandTrackingPreferences.ProviderIsCompatibleWithTarget(providerType);
        }
        #endregion


        #region GUI METHODS
        public override void OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property,
            UnityEngine.GUIContent label)
        {
            if (_list == null)
                Init(property);

            position.y += 2 * UnityEditor.EditorGUIUtility.standardVerticalSpacing;
            UnityEditor.EditorGUI.BeginProperty(position, label, property);
            position = UnityEditor.EditorGUI.PrefixLabel(position,
                UnityEngine.GUIUtility.GetControlID(UnityEngine.FocusType.Passive), UnityEngine.GUIContent.none);

            // Don't make child fields be indented
            var indent = UnityEditor.EditorGUI.indentLevel;
            UnityEditor.EditorGUI.indentLevel = 0;

            _list.draggable = !UnityEngine.Application.isPlaying;
            _list.DoList(position);

            // Set indent back to what it was
            UnityEditor.EditorGUI.indentLevel = indent;
            UnityEditor.EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(UnityEditor.SerializedProperty property,
            UnityEngine.GUIContent label)
        {
            int arraySize = property.FindPropertyRelative(HAND_TRACKING_PROVIDER_DATA_FIELD_NAME).arraySize;
            return UnityEditor.EditorGUIUtility.singleLineHeight *
                   (2 + (_showList
                        ? (arraySize > 0 ? arraySize : 1) + 1
                        : 0));
        }

        private void DrawListItems(UnityEngine.Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!_showList)
                return;

            UnityEditor.SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
            string displayName = element
                .FindPropertyRelative(Tools.ReflectionNames.TRACKING_NAME_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME).stringValue;
            string description = element
                .FindPropertyRelative(Tools.ReflectionNames.DESCRIPTION_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME).stringValue;
            bool canHaveSkeleton = element
                .FindPropertyRelative(Tools.ReflectionNames.CAN_HAVE_SKELETON_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME).boolValue;
            string providerType = element
                .FindPropertyRelative(Tools.ReflectionNames.PROVIDER_TYPE_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME).stringValue;

            bool isCompatible = !UnityEngine.Application.isPlaying && IsCompatibleWitchActualConfig(providerType);
            if (canHaveSkeleton)
                UnityEditor.EditorGUI.LabelField(
                    new UnityEngine.Rect(rect.x, rect.y, UnityEditor.EditorGUIUtility.singleLineHeight,
                        UnityEditor.EditorGUIUtility.singleLineHeight),
                    new UnityEngine.GUIContent(
                        (UnityEngine.Texture) UnityEngine.Resources.Load(isCompatible
                            ? (UnityEditor.EditorGUIUtility.isProSkin
                                ? SKELETON_WHITE_ICON_RESOURCE_NAME
                                : SKELETON_BLACK_ICON_RESOURCE_NAME)
                            : SKELETON_GRAY_ICON_RESOURCE_NAME), SKELETON_ICON_TOOLTIP));

            UnityEngine.GUIStyle style = UnityEditor.EditorStyles.label;
            UnityEngine.Color c = style.normal.textColor;
            if (!isCompatible)
            {
                style.normal.textColor = UnityEngine.Color.gray;
            }

            UnityEditor.EditorGUI.LabelField(
                new UnityEngine.Rect(new UnityEngine.Rect(
                    rect.x + 1.25f * UnityEditor.EditorGUIUtility.singleLineHeight, rect.y, 3 * rect.width / 4,
                    UnityEditor.EditorGUIUtility.singleLineHeight)),
                new UnityEngine.GUIContent(displayName, description), style);
            style.normal.textColor = c;
        }

        private void DrawHeader(UnityEngine.Rect rect)
        {
            UnityEngine.GUIStyle style = UnityEditor.EditorStyles.foldout;
            style.fontStyle = UnityEngine.FontStyle.Bold;
            UnityEditor.EditorGUI.indentLevel = 1;
            _showList = UnityEditor.EditorGUI.Foldout(rect, _showList, HEADER_LABEL, true,
                UnityEditor.EditorStyles.foldout);
            UnityEditor.EditorGUI.indentLevel = 0;
            _list.displayAdd = _showList;
            _list.displayRemove = _showList;
            _list.elementHeight = _showList ? UnityEditor.EditorGUIUtility.singleLineHeight : 0;
            _list.draggable = _showList;
        }
        #endregion

    }

}
#endif