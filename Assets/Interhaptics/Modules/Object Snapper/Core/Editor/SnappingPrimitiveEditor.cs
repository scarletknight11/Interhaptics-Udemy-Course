using Interhaptics.ObjectSnapper.core;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.Editor
{
    [CustomEditor(typeof(SnappingPrimitive))]
    [CanEditMultipleObjects]
    public class SnappingPrimitiveEditor : UnityEditor.Editor
    {
        #region Constants
        private const string TITLE_General = "Shape";
        private const string TITLE_Snapping = "Snapping";
        private const string TITLE_Edition = "Edition";
        private const string BUTTON_Save = "Save";
        private const string BUTTON_Exit = "Exit";
        private const string BUTTON_And = " & ";

        //Shape
        private const string SERIALIZEDPROPERTY_PrimitiveShape = "primitiveShape";
        private const string SERIALIZEDPROPERTY_LocalPosition = "localPosition";
        private const string SERIALIZEDPROPERTY_LocalRotation = "localRotation";
        private const string SERIALIZEDPROPERTY_PrimaryColor = "primaryColor";
        private const string SERIALIZEDPROPERTY_PrimaryRadius = "primaryRadius";
        private const string SERIALIZEDPROPERTY_Length = "length";
        private const string SERIALIZEDPROPERTY_SecondaryColor = "secondaryColor";
        private const string SERIALIZEDPROPERTY_SecondaryRadius = "secondaryRadius";

        //Snapping
        private const string SERIALIZEDPROPERTY_SnappingData = "snappingData";
        private const string SERIALIZEDPROPERTY_MovementType = "movementType";
        private const string SERIALIZEDPROPERTY_ForwardAxis = "forwardAxis";
        private const string SERIALIZEDPROPERTY_UpwardAxis = "upwardAxis";
        private const string SERIALIZEDPROPERTY_ModelSnappableActor = "modelSnappableActor";
        private const string SERIALIZEDPROPERTY_TrackingColor = "trackingColor";
        private const string SERIALIZEDPROPERTY_TrackingDistance = "trackingDistance";
        private const string SERIALIZEDPROPERTY_TrackingRadius = "trackingRadius";
        private const string SERIALIZEDPROPERTY_TrackingAxisLength = "trackingAxisLength";
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
        private SerializedProperty primaryColorCE;
        private SerializedProperty primaryRadiusCE;
        private SerializedProperty lengthCE;
        private SerializedProperty secondaryColorCE;
        private SerializedProperty secondaryRadiusCE;

        /*
         * 
         * Snapping
         * 
         */
        private SerializedProperty snappingDataCE;
        private SerializedProperty movementTypeCE;
        private SerializedProperty forwardAxisCE;
        private SerializedProperty upwardAxisCE;
        private SerializedProperty modelSnappableActorCE;
        private SerializedProperty trackingColorCE;
        private SerializedProperty trackingDistanceCE;
        private SerializedProperty trackingRadiusCE;
        private SerializedProperty trackingAxisLengthCE;
        #endregion
        
        #region Life Cycle
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
            primaryColorCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_PrimaryColor);
            primaryRadiusCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_PrimaryRadius);
            lengthCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_Length);
            secondaryColorCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_SecondaryColor);
            secondaryRadiusCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_SecondaryRadius);

            /*
             * 
             * Snapping
             * 
             */
            snappingDataCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_SnappingData);
            movementTypeCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_MovementType);
            forwardAxisCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_ForwardAxis);
            upwardAxisCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_UpwardAxis);

            modelSnappableActorCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_ModelSnappableActor);
            trackingColorCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_TrackingColor);
            trackingDistanceCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_TrackingDistance);
            trackingRadiusCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_TrackingRadius);
            trackingAxisLengthCE = serializedObject.FindProperty(SERIALIZEDPROPERTY_TrackingAxisLength);
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
            EditorGUILayout.PropertyField(primaryColorCE);
            EditorGUILayout.PropertyField(primaryRadiusCE);

            PrimitiveShape currentRepresentation = (PrimitiveShape)primitiveShapeCE.enumValueIndex;

            if(currentRepresentation != PrimitiveShape.Sphere)
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
                            EditorGUILayout.PropertyField(secondaryColorCE);
                            EditorGUILayout.PropertyField(secondaryRadiusCE);
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
                EditorGUILayout.PropertyField(forwardAxisCE);
                EditorGUILayout.PropertyField(upwardAxisCE);

                SnappingPrimitive snappingPrimitive = (SnappingPrimitive)target;

                if(snappingPrimitive && snappingPrimitive.isActiveAndEnabled && !Application.isPlaying)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(TITLE_Edition, EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(modelSnappableActorCE);

                    if (modelSnappableActorCE.objectReferenceValue)
                    {
                        EditorGUILayout.PropertyField(trackingColorCE);
                        EditorGUILayout.PropertyField(trackingRadiusCE);
                        EditorGUILayout.PropertyField(trackingDistanceCE);
                        EditorGUILayout.PropertyField(trackingAxisLengthCE);

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
