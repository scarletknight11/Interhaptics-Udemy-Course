#if UNITY_EDITOR
namespace Interhaptics.Assets.Editor.Pointers
{

    [UnityEditor.CustomEditor(typeof(Interhaptics.Assets.Pointers.PinchViewfinderPointer), true)]
    [UnityEditor.CanEditMultipleObjects]
    public class PinchViewfinderPointerEditor : FloatableValuePointerEditor
    {

        #region CONSTS FIELDS NAME
        private const string FIRST_FINGER_TIP_FIELD_NAME = "firstFingerTip";
        private const string FIRST_FINGER_TIP_WEIGHT_FIELD_NAME = "firstFingerTipWeight";
        private const string SECOND_FINGER_TIP_FIELD_NAME = "secondFingerTip";
        private const string FINGER_TIP_DISTANCE_TO_TRIGGER_SELECTION_FIELD_NAME = "fingerTipDistanceToTriggerSelection";
        #endregion


        #region PRIVATE FIELDS
        private UnityEditor.SerializedProperty _myFirstFingerTip = null;
        private UnityEditor.SerializedProperty _myFirstFingerTipWeight = null;
        private UnityEditor.SerializedProperty _mySecondFingerTip = null;
        private UnityEditor.SerializedProperty _myFingerTipDistanceToTriggerSelection = null;
        #endregion


        #region LIFE CYCLES
        protected override void OnEnable()
        {
            base.OnEnable();
            _myFirstFingerTip = serializedObject.FindProperty(FIRST_FINGER_TIP_FIELD_NAME);
            _myFirstFingerTipWeight = serializedObject.FindProperty(FIRST_FINGER_TIP_WEIGHT_FIELD_NAME);
            _mySecondFingerTip = serializedObject.FindProperty(SECOND_FINGER_TIP_FIELD_NAME);
            _myFingerTipDistanceToTriggerSelection = serializedObject.FindProperty(FINGER_TIP_DISTANCE_TO_TRIGGER_SELECTION_FIELD_NAME);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            UnityEditor.EditorGUILayout.PropertyField(_myFirstFingerTip);
            UnityEditor.EditorGUILayout.PropertyField(_myFirstFingerTipWeight);
            UnityEditor.EditorGUILayout.PropertyField(_mySecondFingerTip);
            UnityEditor.EditorGUILayout.PropertyField(_myFingerTipDistanceToTriggerSelection);

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

    }

}
#endif