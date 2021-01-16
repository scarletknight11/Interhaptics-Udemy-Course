#if UNITY_EDITOR
namespace Interhaptics.Assets.Editor.UI
{

    [UnityEditor.CustomEditor(typeof(Interhaptics.Assets.UI.SelectButtonGroup))]
    [UnityEditor.CanEditMultipleObjects]
    public class SelectButtonGroupEditor : UnityEditor.Editor
    {

        #region CONSTS FIELDS NAME
        private const string INITIAL_SELECTED_ITEM_FIELD_NAME = "initialSelectedItem";
        private const string SELECT_BUTTON_ITEMS_FIELD_NAME = "selectButtonItems";
        #endregion


        #region PRIVATE FIELDS
        private UnityEditor.SerializedProperty _myInitialSelectedItem = null;
        private UnityEditor.SerializedProperty _mySelectButtonItems = null;
        #endregion


        #region LIFE CYCLES
        private void OnEnable()
        {
            _myInitialSelectedItem = serializedObject.FindProperty(INITIAL_SELECTED_ITEM_FIELD_NAME);
            _mySelectButtonItems = serializedObject.FindProperty(SELECT_BUTTON_ITEMS_FIELD_NAME);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UnityEditor.EditorGUILayout.IntSlider(_myInitialSelectedItem, -1, _mySelectButtonItems.arraySize);
            UnityEditor.EditorGUILayout.PropertyField(_mySelectButtonItems);

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

    }

}
#endif