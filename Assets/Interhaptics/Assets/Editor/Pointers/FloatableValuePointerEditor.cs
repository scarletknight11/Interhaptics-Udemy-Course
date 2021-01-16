#if UNITY_EDITOR
namespace Interhaptics.Assets.Editor.Pointers
{

    [UnityEditor.CustomEditor(typeof(Interhaptics.Assets.Pointers.FloatableValuePointer))]
    [UnityEditor.CanEditMultipleObjects]
    public class FloatableValuePointerEditor : PointerEditor
    {

        #region CONSTS FIELDS NAME
        private const string CONFIDENCE_INTERVAL_FIELD_NAME = "confidenceInterval";
        #endregion


        #region PRIVATE FIELDS
        private UnityEditor.SerializedProperty _myConfidenceInterval = null;
        #endregion


        #region LIFE CYCLES
        protected override void OnEnable()
        {
            base.OnEnable();
            _myConfidenceInterval = serializedObject.FindProperty(CONFIDENCE_INTERVAL_FIELD_NAME);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            UnityEditor.EditorGUILayout.PropertyField(_myConfidenceInterval);

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

    }

}
#endif