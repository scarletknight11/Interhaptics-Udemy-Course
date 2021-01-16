#if UNITY_EDITOR
namespace Interhaptics.Assets.Editor.UI
{

    [UnityEditor.CustomEditor(typeof(Interhaptics.Assets.UI.SwitchButton))]
    [UnityEditor.CanEditMultipleObjects]
    public class SwitchButtonEditor : UnityEditor.Editor
    {

        #region CONSTS FIELDS NAME
        private const string BUTTON_RENDERER_FIELD_NAME = "buttonRenderer";

        private const string INITIAL_STATE_FIELD_NAME = "initialState";
        private const string SWITCH_STATES_FIELD_NAME = "switchStates";
        #endregion


        #region PRIVATE FIELDS
        private UnityEditor.SerializedProperty _myButtonRenderer = null;

        private UnityEditor.SerializedProperty _myInitialState = null;
        private UnityEditor.SerializedProperty _mySwitchStates = null;
        #endregion


        #region LIFE CYCLES
        private void OnEnable()
        {
            _myButtonRenderer = serializedObject.FindProperty(BUTTON_RENDERER_FIELD_NAME);

            _myInitialState = serializedObject.FindProperty(INITIAL_STATE_FIELD_NAME);
            _mySwitchStates = serializedObject.FindProperty(SWITCH_STATES_FIELD_NAME);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UnityEditor.EditorGUILayout.PropertyField(_myButtonRenderer);

            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.LabelField("Switch Settings", UnityEditor.EditorStyles.boldLabel);
            UnityEditor.EditorGUILayout.IntSlider(_myInitialState, 0, _mySwitchStates.arraySize);
            UnityEditor.EditorGUILayout.PropertyField(_mySwitchStates);

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

    }

}
#endif