#if UNITY_EDITOR
namespace Interhaptics.Assets.Editor.Pointers
{

    [UnityEditor.CustomEditor(typeof(Interhaptics.Assets.Pointers.Pointer))]
    [UnityEditor.CanEditMultipleObjects]
    public class PointerEditor : UnityEditor.Editor
    {

        #region CONSTS FIELDS NAME
        private const string VISUALISATION_OBJECT_FIELD_NAME = "visualisationObject";
        private const string STARTING_OFFSET_FIELD_NAME = "startingOffset";
        private const string RAY_TYPE_FIELD_NAME = "rayType";

        private const string ON_SELECTION_FIELD_NAME = "onSelection";

        private const string GRAVITY_DIRECTION_FIELD_NAME = "gravityDirection";
        private const string GRAVITY_MULTIPLIER_FIELD_NAME = "gravityMultiplier";

        private const string LAYER_MASK_FIELD_NAME = "layerMask";
        private const string MAX_DISTANCE_FIELD_NAME = "maxDistance";
        private const string SMOOTHNESS_FIELD_NAME = "smoothness";

        private const string INTERPOLATION_TYPE_FIELD_NAME = "interpolationType";
        private const string INTERPOLATION_CURVE_FIELD_NAME = "interpolationCurve";
        #endregion


        #region PRIVATE FIELDS
        private UnityEditor.SerializedProperty _myVisualisationObject = null;
        private UnityEditor.SerializedProperty _myStartingOffset = null;
        private UnityEditor.SerializedProperty _myRayType = null;

        private UnityEditor.SerializedProperty _myOnSelection = null;

        private UnityEditor.SerializedProperty _myGravityDirection = null;
        private UnityEditor.SerializedProperty _myGravityMultiplier = null;

        private UnityEditor.SerializedProperty _myLayerMask = null;
        private UnityEditor.SerializedProperty _myMaxDistance = null;
        private UnityEditor.SerializedProperty _mySmoothness = null;

        private UnityEditor.SerializedProperty _myInterpolationType = null;
        private UnityEditor.SerializedProperty _myInterpolationCurve = null;
        #endregion


        #region LIFE CYCLES
        protected virtual void OnEnable()
        {
            _myVisualisationObject = serializedObject.FindProperty(VISUALISATION_OBJECT_FIELD_NAME);
            _myStartingOffset = serializedObject.FindProperty(STARTING_OFFSET_FIELD_NAME);
            _myRayType = serializedObject.FindProperty(RAY_TYPE_FIELD_NAME);

            _myOnSelection = serializedObject.FindProperty(ON_SELECTION_FIELD_NAME);

            _myGravityDirection = serializedObject.FindProperty(GRAVITY_DIRECTION_FIELD_NAME);
            _myGravityMultiplier = serializedObject.FindProperty(GRAVITY_MULTIPLIER_FIELD_NAME);

            _myLayerMask = serializedObject.FindProperty(LAYER_MASK_FIELD_NAME);
            _myMaxDistance = serializedObject.FindProperty(MAX_DISTANCE_FIELD_NAME);
            _mySmoothness = serializedObject.FindProperty(SMOOTHNESS_FIELD_NAME);

            _myInterpolationType = serializedObject.FindProperty(INTERPOLATION_TYPE_FIELD_NAME);
            _myInterpolationCurve = serializedObject.FindProperty(INTERPOLATION_CURVE_FIELD_NAME);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UnityEditor.EditorGUILayout.PropertyField(_myVisualisationObject);
            UnityEditor.EditorGUILayout.PropertyField(_myStartingOffset);
            UnityEditor.EditorGUILayout.PropertyField(_myRayType);

            UnityEditor.EditorGUILayout.PropertyField(_myOnSelection);

            if (_myRayType.intValue == (int) Interhaptics.Assets.Pointers.Pointer.ERayType.Curve)
            {
                UnityEditor.EditorGUILayout.PropertyField(_myGravityDirection);
                UnityEditor.EditorGUILayout.PropertyField(_myGravityMultiplier);
            }

            UnityEditor.EditorGUILayout.PropertyField(_myLayerMask);
            UnityEditor.EditorGUILayout.PropertyField(_myMaxDistance);
            UnityEditor.EditorGUILayout.PropertyField(_mySmoothness);

            UnityEditor.EditorGUILayout.PropertyField(_myInterpolationType);
            if (_myInterpolationType.intValue == (int) Interhaptics.Assets.Pointers.Pointer.EInterpolationType.Custom)
                UnityEditor.EditorGUILayout.PropertyField(_myInterpolationCurve);

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

    }

}
#endif
