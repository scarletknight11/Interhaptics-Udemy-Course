using Interhaptics.InteractionsEngine.Shared.Types;
using System.Collections.Generic;
using Interhaptics.InteractionsEngine.Shared.Extensions;
using Interhaptics.Modules.Interaction_Builder.Core;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    [CreateAssetMenu(fileName = "Snapping Data", menuName = "Interhaptics/Snapping Data")]
    public class SnappingData : ScriptableObject
    {
        #region Constants
        private const string ERROR_NULL_ASnappableActor = "The ASnappableActor is null";
        private const string ERROR_NULL_Animator = "ASnappableActor's Animator is null";
        private const string ERROR_NULL_AnimatorController = "Unable to save an Animator pose which misses AnimatorController!";

        private const string ERROR_UnableSaveData = "Unable to save a null object data!";
        private const string ERROR_PoseCancelled = "Pose saving cancelled";

        private const string LOG_PoseSaved = "Pose saved";

        private const string TEXT_Separator = " ";

        private const string POPUP_Title = "Override pose ?";
        private const string POPUP_Content = "A pose already exists with this ASnappableActor Animator's AnimatorController. Do you want to overwrite it ?";
        private const string POPUP_Save = "Save";
        private const string POPUP_Cancel = "Cancel";
        #endregion

        #region Variables
        [SerializeField] [HideInInspector] public SnappingDataDictionary snappingDataDictionary = new SnappingDataDictionary();
        #endregion

        #region Publics
        /// <summary>
        /// Check if the SnappableActorData exists for a ASnappableActor in the Dictionary.
        /// </summary>
        /// <param name="aSnappableActor">Key</param>
        /// <returns>True if contains</returns>
        public bool ContainData(ASnappableActor aSnappableActor)
        {
            return aSnappableActor && aSnappableActor.Animator && aSnappableActor.Animator.runtimeAnimatorController && snappingDataDictionary.Contains(aSnappableActor.Animator.runtimeAnimatorController.name);
        }

        /// <summary>
        /// Return the SnappableActorData from the Dictionary (if previously generated).
        /// </summary>
        /// <param name="aSnappableActor">Key</param>
        /// <param name="aSnappableActorData">Returned SnappableActorData</param>
        /// <returns>True if the SnappableActorData exists for the ASnappableActor in the Dictionary</returns>
        public bool GetASnappableActorData(ASnappableActor aSnappableActor, out SnappableActorData aSnappableActorData)
        {
            aSnappableActorData = default;
            bool success = false;

            if (aSnappableActor && aSnappableActor.Animator && aSnappableActor.Animator.runtimeAnimatorController)
                success = snappingDataDictionary.TryGetSnappableActorData(aSnappableActor.Animator.runtimeAnimatorController.name, out aSnappableActorData);

            return success;
        }

        /// <summary>
        /// Apply the offset (position and rotation) of the hand with the tracking (depending on the SnappableActorData).
        /// </summary>
        /// <param name="aSnappableActor">Key</param>
        /// <returns>True if the offset was correctly applied to the ASnappableActor's GameObject</returns>
        public bool ApplyRootOffset(ASnappableActor aSnappableActor)
        {
            bool success = false;

            SnappableActorData snappableActorData;
            if (aSnappableActor && aSnappableActor.Animator && aSnappableActor.Animator.runtimeAnimatorController && (snappingDataDictionary.TryGetSnappableActorData(aSnappableActor.Animator.runtimeAnimatorController.name, out snappableActorData)))
            {
                SpatialRepresentation aSnappableActorSP = snappableActorData.rootPose.spatialRepresentation.ToSpatialRepresentation();
                aSnappableActor.transform.position += aSnappableActor.transform.rotation * IbTools.Convert(aSnappableActorSP.Position);
                aSnappableActor.transform.rotation *= IbTools.Convert(aSnappableActorSP.Rotation);
                success = true;
            }

            return success;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Add a SnappableActorData to the Dictionary.
        /// </summary>
        /// <param name="aSnappableActor">Key</param>
        /// <param name="trackingSR">Tracking spatial representation (position and rotation)</param>
        /// <param name="aSnappableActorForward">ASnappableActor's Gameobject custom forward direction</param>
        /// <param name="aSnappableActorUpward">ASnappableActor's Gameobject custom upward direction</param>
        /// <returns>True if the SnappableActorData was correctly added to the Dictionary</returns>
        public bool AddSnappableActorData(ASnappableActor aSnappableActor, SpatialRepresentation trackingSR, Vector3 aSnappableActorForward, Vector3 aSnappableActorUpward)
        {
            string error = null;

            if (aSnappableActor == null)
                error = ERROR_NULL_ASnappableActor;
            else if (aSnappableActor.Animator == null)
                error = ERROR_NULL_Animator;
            else if (aSnappableActor.Animator.runtimeAnimatorController == null)
                error = ERROR_NULL_AnimatorController;
            else
            {
                //Create the ASnappableActor SnappableActorData
                SnappableActorData snappableActor = new SnappableActorData();
                snappableActor.forward = new SerializableVector(IbTools.Convert(aSnappableActorForward));
                snappableActor.upward = new SerializableVector(IbTools.Convert(aSnappableActorUpward));

                //Save the offset
                SpatialRepresentation spatialRepresentation = new SpatialRepresentation();
                spatialRepresentation.Position = IbTools.Convert(Quaternion.Inverse(IbTools.Convert(trackingSR.Rotation)) * (aSnappableActor.transform.position - IbTools.Convert(trackingSR.Position)));
                spatialRepresentation.Rotation = IbTools.Convert(Quaternion.Inverse(IbTools.Convert(trackingSR.Rotation)) * aSnappableActor.transform.rotation);

                //Recursively save the ASnappableActor children SpatialRepresentation
                snappableActor.rootPose = new Pose()
                {
                    spatialRepresentation = new SerializableSpatialRepresentation(spatialRepresentation),
                    childrenPose = new List<Pose>()
                };


                string avatarControllerName = aSnappableActor.Animator.runtimeAnimatorController.name;
                bool containKey = false;
                if (!(containKey = snappingDataDictionary.Contains(avatarControllerName)) || (containKey && EditorUtility.DisplayDialog(POPUP_Title, POPUP_Content, POPUP_Save, POPUP_Cancel)))
                {
                    Pose avatarControllerChildrenDatas;
                    if (SnappingData.GenerateTransformPose(aSnappableActor.transform, out avatarControllerChildrenDatas))
                    {
                        foreach (Pose child in avatarControllerChildrenDatas.childrenPose)
                            snappableActor.rootPose.childrenPose.Add(child);
                    }

                    if (containKey)
                        snappingDataDictionary.Remove(avatarControllerName);

                    snappingDataDictionary.Add(avatarControllerName, snappableActor);
                }
                else
                    error = ERROR_PoseCancelled;
            }

            if(error == null)
                Debug.Log(aSnappableActor.name + TEXT_Separator + LOG_PoseSaved);
            else
            {
                if(error != ERROR_PoseCancelled)
                    Debug.LogError(error);
            }

            bool success = (error == null);

            if (success)
                EditorUtility.SetDirty(this);

            return success;
        }

        /// <summary>
        /// Remove a SnappableActorData from the Dictionary.
        /// </summary>
        /// <param name="aSnappableActor">Key</param>
        /// <returns>True if the SnappableActorData was correctly removed from the Dictionary</returns>
        public bool RemoveSnappableActorData(ASnappableActor aSnappableActor)
        {
            bool success = false;

            if (aSnappableActor && aSnappableActor.Animator && aSnappableActor.Animator.runtimeAnimatorController)
            {

                string key = aSnappableActor.Animator.runtimeAnimatorController.name;
                if (snappingDataDictionary.Contains(key))
                {
                    snappingDataDictionary.Remove(key);
                    success = true;
                }

            }

            EditorUtility.SetDirty(this);
            return success;
        }
#endif
        #endregion

        #region Public Statics
        /// <summary>
        /// Apply a Pose to a Transform (if its structure fits with the Pose).
        /// </summary>
        /// <param name="transformToSet">Transform on which the Pose needs to be applied</param>
        /// <param name="finalPose">Pose which needs to be applied to the Transform</param>
        public static void SetTransformPose(Transform transformToSet, Pose finalPose)
        {
            if (transformToSet)
            {
                Dictionary<string, Pose> finalchildrenPose = new Dictionary<string, Pose>();
                if (finalPose.childrenPose != null && finalPose.childrenPose.Count > 0)
                {
                    foreach (Pose childPose in finalPose.childrenPose)
                    {
                        if (!finalchildrenPose.ContainsKey(childPose.transformName))
                            finalchildrenPose.Add(childPose.transformName, childPose);
                    }
                }
                
                for (int i = 0; i < transformToSet.childCount; i++)
                {
                    Transform child = transformToSet.GetChild(i);
                    if (child && finalchildrenPose.ContainsKey(child.name))
                        SetTransformPose(child, finalchildrenPose[child.name]);
                }
                
                SpatialRepresentation finalTransformSP = finalPose.spatialRepresentation.ToSpatialRepresentation();
                if (!finalTransformSP.IsNan())
                {
                    transformToSet.localPosition = IbTools.Convert(finalTransformSP.Position);
                    transformToSet.localRotation = IbTools.Convert(finalTransformSP.Rotation);   
                }
            }
        }

        /// <summary>
        /// Apply an Interpolated Pose between two Poses to a Transform (if its structure fits with them).
        /// </summary>
        /// <param name="transformToSet">Transform on which the Pose needs to be applied</param>
        /// <param name="initialTransformPose">Current Pose of the Transform</param>
        /// <param name="finalTransformPose">Pose which needs to be applied to the Transform</param>
        /// <param name="posLerp">Lerping value [0-1]</param>
        public static void LerpTransformPose(Transform transformToSet, Pose initialTransformPose, Pose finalTransformPose, float posLerp = 1)
        {

            posLerp = Mathf.Clamp01(posLerp);

            if (transformToSet)
            {
                Dictionary<string, Pose> initialChildrenPose = new Dictionary<string, Pose>();
                if (initialTransformPose.childrenPose != null && initialTransformPose.childrenPose.Count > 0)
                {
                    foreach (Pose childpose in initialTransformPose.childrenPose)
                    {
                        if (!initialChildrenPose.ContainsKey(childpose.transformName))
                            initialChildrenPose.Add(childpose.transformName, childpose);
                    }
                }
                
                Dictionary<string, Pose> finalChildrenPose = new Dictionary<string, Pose>();
                if (finalTransformPose.childrenPose != null && finalTransformPose.childrenPose.Count > 0)
                {
                    foreach (Pose childPose in finalTransformPose.childrenPose)
                    {
                        if (!finalChildrenPose.ContainsKey(childPose.transformName))
                            finalChildrenPose.Add(childPose.transformName, childPose);
                    }
                }
                
                for (int i = 0; i < transformToSet.childCount; i++)
                {
                    Transform child = transformToSet.GetChild(i);
                    if (child && initialChildrenPose.ContainsKey(child.name) && finalChildrenPose.ContainsKey(child.name))
                        LerpTransformPose(child, initialChildrenPose[child.name], finalChildrenPose[child.name], posLerp);
                }
                
                SpatialRepresentation initialTransformSP = initialTransformPose.spatialRepresentation.ToSpatialRepresentation();
                SpatialRepresentation finalTransformSP = finalTransformPose.spatialRepresentation.ToSpatialRepresentation();
                transformToSet.localPosition = Vector3.Lerp(IbTools.Convert(initialTransformSP.Position), IbTools.Convert(finalTransformSP.Position), posLerp);
                transformToSet.localRotation = Quaternion.Slerp(IbTools.Convert(initialTransformSP.Rotation), IbTools.Convert(finalTransformSP.Rotation), posLerp);
            }
        }

        /// <summary>
        /// Generate a Pose of a Transform.
        /// </summary>
        /// <param name="transformToGet">Transform which will be used to generate the Pose</param>
        /// <param name="transformPose">Generated Transform Pose</param>
        /// <returns>True if the Pose was correctly generated</returns>
        public static bool GenerateTransformPose(Transform transformToGet, out Pose transformPose)
        {
            string error = null;
            transformPose = default;

            if (transformToGet)
            {
                List<Pose> childrenPose = new List<Pose>();
                
                for (int i = 0; i < transformToGet.childCount; i++)
                {
                    Transform childTransform = transformToGet.GetChild(i);
                    if(childTransform)
                    {
                        Pose childPose;
                        if (GenerateTransformPose(childTransform, out childPose))
                            childrenPose.Add(childPose);
                    }
                }
                
                transformPose.transformName = transformToGet.name;

                SpatialRepresentation transformSP = new SpatialRepresentation() { Position = IbTools.Convert(transformToGet.transform.localPosition), Rotation = IbTools.Convert(transformToGet.transform.localRotation) };
                transformPose.spatialRepresentation = new SerializableSpatialRepresentation(transformSP);
                transformPose.childrenPose = childrenPose;
            }
            else
                error = ERROR_UnableSaveData;

            if (error != null)
                Debug.Log(error);
            
            return error == null;
        }
        #endregion
    }
}
