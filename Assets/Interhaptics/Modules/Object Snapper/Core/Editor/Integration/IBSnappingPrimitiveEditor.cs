using Interhaptics.ObjectSnapper.core;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.Editor
{
    [CustomEditor(typeof(IBSnappingPrimitive))]
    [CanEditMultipleObjects]
    public class IBSnappingPrimitiveEditor : UnityEditor.Editor
    {
        #region Constants
        private const string PATH_LeftHand = "Prefabs/CustomLeft";
        private const string PATH_RightHand = "Prefabs/CustomRight";

        private const string TITLE_General = "Shape";
        private const string TITLE_Snapping = "Snapping";
        private const string TITLE_Edition = "Edition";

        private const string NAME_TorusTubeRadius = "Tube radius";

        private const string BUTTON_LeftHand = "Left Hand";
        private const string BUTTON_RightHand = "Right Hand";
        private const string BUTTON_Save = "Save";
        private const string BUTTON_Exit = "Exit";
        private const string BUTTON_And = " & ";

        private const string SERIALIZEDPROPERTY_PrimitiveShape = "primitiveShape";
        private const string SERIALIZEDPROPERTY_LocalPosition = "localPosition";
        private const string SERIALIZEDPROPERTY_LocalRotation = "localRotation";
        private const string SERIALIZEDPROPERTY_PrimaryRadius = "primaryRadius";
        private const string SERIALIZEDPROPERTY_Length = "length";
        private const string SERIALIZEDPROPERTY_SnappingData = "snappingData";
        private const string SERIALIZEDPROPERTY_MovementType = "movementType";
        private const string SERIALIZEDPROPERTY_ModelSnappableActor = "modelSnappableActor";
        #endregion

        #region Variables
        /*
         * 
         * Shape
         * 
         */
        private SerializedProperty primitiveShapeCE;
        private SerializedProperty localPositionOffsetCE;
        private SerializedProperty localRotationOffsetCE;
        private SerializedProperty primaryRadiusCE;
        private SerializedProperty lengthCE;
        private SerializedProperty secondaryRadiusCE;

        /*
         * 
         * Snapping
         * 
         */
        private SerializedProperty modelSnappableActorCE;
        private SerializedProperty movementTypeCE;
        private SerializedProperty snappingDataCE;

        //Privates
        private GameObject _leftHandRessource = null;
        private GameObject _rightHandRessource = null;
        #endregion

        #region Life Cycle
        private void Awake()
        {
            _leftHandRessource = Resources.Load<GameObject>(PATH_LeftHand);
            _rightHandRessource = Resources.Load<GameObject>(PATH_RightHand);
        }

        private void OnEnable()
        {
            /*
             * 
             * Shape
             * 
             */
            primitiveShapeCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_PrimitiveShape);
            localPositionOffsetCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_LocalPosition);
            localRotationOffsetCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_LocalRotation);
            primaryRadiusCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_PrimaryRadius);
            lengthCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_Length);
            secondaryRadiusCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_PrimaryRadius);

            /*
             * 
             * Snapping
             * 
             */
            modelSnappableActorCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_ModelSnappableActor);
            movementTypeCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_MovementType);
            snappingDataCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_SnappingData);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            /*
             * 
             * Shape
             * 
             */
            EditorGUILayout.LabelField(TITLE_General, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(primitiveShapeCE);
            EditorGUILayout.PropertyField(localPositionOffsetCE);
            EditorGUILayout.PropertyField(primaryRadiusCE);

            PrimitiveShape currentRepresentation = (PrimitiveShape)primitiveShapeCE.enumValueIndex;

            if (currentRepresentation != PrimitiveShape.Sphere)
            {
                EditorGUILayout.Space();
                primitiveShapeCE.isExpanded = EditorGUILayout.Foldout(primitiveShapeCE.isExpanded, currentRepresentation.ToString(), EditorStyles.foldout);
                if (primitiveShapeCE.isExpanded)
                {
                    EditorGUILayout.PropertyField(localRotationOffsetCE);

                    switch (currentRepresentation)
                    {
                        case PrimitiveShape.Cylinder:
                        case PrimitiveShape.Capsule:
                            EditorGUILayout.PropertyField(lengthCE);
                            break;
                        case PrimitiveShape.Torus:

                            //Uncomment to get the option in the inspector
                            EditorGUILayout.PropertyField(secondaryRadiusCE, new GUIContent(NAME_TorusTubeRadius));
                            break;
                    }
                }
            }

            /*
             * 
             * Snapping
             * 
             */
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(TITLE_Snapping, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(snappingDataCE);
            if (snappingDataCE.objectReferenceValue)
            {
                EditorGUILayout.PropertyField(movementTypeCE);

                EditorGUILayout.Space();

                SnappingPrimitive snappingPrimitive = (SnappingPrimitive)target;
                if (snappingPrimitive && snappingPrimitive.isActiveAndEnabled && !Application.isPlaying)
                {

                    EditorGUILayout.LabelField(TITLE_Edition, EditorStyles.boldLabel);

                    if (!modelSnappableActorCE.objectReferenceValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(BUTTON_LeftHand) && _leftHandRessource)
                            modelSnappableActorCE.objectReferenceValue = _leftHandRessource.GetComponent<ASnappableActor>();
                        if (GUILayout.Button(BUTTON_RightHand) && _rightHandRessource)
                            modelSnappableActorCE.objectReferenceValue = _rightHandRessource.GetComponent<ASnappableActor>();
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {

                        if (GUILayout.Button(BUTTON_Save + BUTTON_And + BUTTON_Exit))
                        {
                            if (snappingPrimitive.QuickSave())
                                snappingPrimitive.ResetSnapEdition();
                        }

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(BUTTON_Save))
                            snappingPrimitive.QuickSave();
                        if (GUILayout.Button(BUTTON_Exit))
                            snappingPrimitive.ResetSnapEdition();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}
