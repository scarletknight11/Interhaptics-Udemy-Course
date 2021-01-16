#if UNITY_EDITOR
namespace Interhaptics.Assets.Editor.Pointers
{

    [UnityEditor.CustomEditor(typeof(Interhaptics.Assets.Pointers.GrabSelectionPointer))]
    [UnityEditor.CanEditMultipleObjects]
    public class GrabSelectionPointerEditor : FloatableValuePointerEditor
    {

        #region CONSTS FIELDS NAME
        private const string IS_LEFT_FIELD_NAME = "isLeft";
        private const string GRABBING_THRESHOLD_FIELD_NAME = "grabbingThreshold";
        #endregion


        #region PRIVATE FIELDS
        private UnityEditor.SerializedProperty _myIsLeft = null;
        private UnityEditor.SerializedProperty _myGrabbingThreshold = null;
        #endregion


        #region LIFE CYCLES
        protected override void OnEnable()
        {
            base.OnEnable();
            _myIsLeft = serializedObject.FindProperty(IS_LEFT_FIELD_NAME);
            _myGrabbingThreshold = serializedObject.FindProperty(GRABBING_THRESHOLD_FIELD_NAME);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            UnityEditor.EditorGUILayout.PropertyField(_myIsLeft);
            UnityEditor.EditorGUILayout.PropertyField(_myGrabbingThreshold);

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

    }

}
#endif