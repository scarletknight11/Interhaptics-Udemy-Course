#if UNITY_EDITOR
namespace Interhaptics.Assets.Editor.Locomotion
{

    [UnityEditor.CustomEditor(typeof(Interhaptics.Assets.Locomotion.XRTeleport))]
    [UnityEditor.CanEditMultipleObjects]
    // ReSharper disable once InconsistentNaming
    public class XRTeleportEditor : UnityEditor.Editor
    {

        #region CONSTS FIELDS NAME
        private const string LOCOMOTION_TYPE_FIELD_NAME = "locomotionType";
        private const string MAX_ANGLE_TO_TELEPORT_FIELD_NAME = "maxAngleToTeleport";
        private const string POSSIBLE_TO_TELEPORT_MATERIAL_FIELD_NAME = "possibleToTeleportMaterial";
        private const string IMPOSSIBLE_TO_TELEPORT_MATERIAL_FIELD_NAME= "impossibleToTeleportMaterial";

        private const string POINTERS_LIST_FIELD_NAME = "pointersList";

        private const string DASHING_TIME_FIELD_NAME = "dashingTime";
        #endregion


        #region PRIVATE FIELDS
        private UnityEditor.SerializedProperty _myLocomotionType = null;
        private UnityEditor.SerializedProperty _myMaxAngleToTeleport = null;
        private UnityEditor.SerializedProperty _myPossibleToTeleportMaterial = null;
        private UnityEditor.SerializedProperty _myImpossibleToTeleportMaterial = null;

        private UnityEditor.SerializedProperty _myPointersList = null;

        private UnityEditor.SerializedProperty _myDashingTime = null;
        #endregion


        #region LIFE CYCLES
        protected void OnEnable()
        {
            _myLocomotionType = serializedObject.FindProperty(LOCOMOTION_TYPE_FIELD_NAME);
            _myMaxAngleToTeleport = serializedObject.FindProperty(MAX_ANGLE_TO_TELEPORT_FIELD_NAME);
            _myPossibleToTeleportMaterial = serializedObject.FindProperty(POSSIBLE_TO_TELEPORT_MATERIAL_FIELD_NAME);
            _myImpossibleToTeleportMaterial = serializedObject.FindProperty(IMPOSSIBLE_TO_TELEPORT_MATERIAL_FIELD_NAME);

            _myPointersList = serializedObject.FindProperty(POINTERS_LIST_FIELD_NAME);

            _myDashingTime = serializedObject.FindProperty(DASHING_TIME_FIELD_NAME);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UnityEditor.EditorGUILayout.PropertyField(_myLocomotionType);
            UnityEditor.EditorGUILayout.PropertyField(_myMaxAngleToTeleport);
            UnityEditor.EditorGUILayout.PropertyField(_myPossibleToTeleportMaterial);
            UnityEditor.EditorGUILayout.PropertyField(_myImpossibleToTeleportMaterial);

            UnityEditor.EditorGUILayout.PropertyField(_myPointersList);

            UnityEditor.EditorGUILayout.PropertyField(_myDashingTime);

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

    }

}
#endif