using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.Modules.Interaction_Builder.Core;

namespace Interhaptics.ObjectSnapper.core
{
    [UnityEngine.AddComponentMenu("Interhaptics/Object Snapper/SnappingObject")]
    [RequireComponent(typeof(InteractionObject))]
    public class SnappingObject : MonoBehaviour
    {
        #region Constants
        private const string TOOLTIP_FixedSnapping = "If true, the SnappableActor will be fixed to the SnappingPrimitive";
        private const string TOOLTIP_AutomaticallyFindNearest = "If true,  the SnappableActor will automatically switch to the nearest SnappingPrimitive";
        #endregion

        #region Structures
        private class InteractionData
        {
            public SnappingPrimitive snappingPrimitive;

            public SpatialRepresentation? localSP;

            public Pose originalPose;
        }
        #endregion

        #region Variables
        [Tooltip(TOOLTIP_FixedSnapping)] [SerializeField] private bool fixedASnappableActorInteraction = true;
        [Tooltip(TOOLTIP_AutomaticallyFindNearest)] [SerializeField] private bool automaticallyFindNearest = false;

        private List<SnappingPrimitive> _subscribedSnappingPrimitives = new List<SnappingPrimitive>();
        private Dictionary<ASnappableActor, InteractionData> _subscribedASnappableActors = new Dictionary<ASnappableActor, InteractionData>();
        #endregion

        #region Getters/Setters
        /// <summary>
        /// Return true if the snapping is fixed.
        /// </summary>
        public bool FixedASnappableActorInteraction { get { return fixedASnappableActorInteraction; } set { fixedASnappableActorInteraction = value; } }

        /// <summary>
        /// Return true if the ASnappableActor is automatically snapped to the nearest SnappingPrimitive.
        /// </summary>
        public bool AutomaticallyFindNearest { get { return automaticallyFindNearest; } set { automaticallyFindNearest = value; } }

        /// <summary>
        /// Return all the subscribed SnappingPrimitive.
        /// </summary>
        public SnappingPrimitive[] SubscribedSnappingPrimitives { get { return _subscribedSnappingPrimitives.ToArray(); } }

        /// <summary>
        /// Return all the subscribed ASnappableActor.
        /// </summary>
        public ASnappableActor[] SubscribedASnappableActors { get { return _subscribedASnappableActors.Keys.ToArray(); } }
        #endregion

        #region Publics
        /// <summary>
        /// Subscribe an ASnappableActor to the SnappingObject. Called by the ASnappableActor on interaction start.
        /// </summary>
        /// <param name="aSnappableActor">ASnappableActor to subscribe</param>
        public void SubscribeASnappableActor(ASnappableActor aSnappableActor)
        {
            if (!aSnappableActor || _subscribedASnappableActors.ContainsKey(aSnappableActor))
                return;

            InteractionData objectMasterInteractionData = new InteractionData();
            SnappingData.GenerateTransformPose(aSnappableActor.transform, out objectMasterInteractionData.originalPose);

            _subscribedASnappableActors.Add(aSnappableActor, objectMasterInteractionData);
        }

        /// <summary>
        /// Unsubscribe an ASnappableActor from the SnappingObject. Called by the ASnappableActor on interaction finish.
        /// </summary>
        /// <param name="aSnappableActor">ASnappableActor to unsubscribe</param>
        public void UnsubscribeASnappableActor(ASnappableActor aSnappableActor)
        {
            if (!aSnappableActor || !_subscribedASnappableActors.ContainsKey(aSnappableActor))
                return;

            InteractionData InteractionData;
            if (_subscribedASnappableActors.TryGetValue(aSnappableActor, out InteractionData))
            {
                //Apply the original local position and rotation of each ASnappableActor GameObject's children
                SnappingData.SetTransformPose(aSnappableActor.transform, InteractionData.originalPose);

                _subscribedASnappableActors.Remove(aSnappableActor);
            }

        }

        /// <summary>
        /// Subscribe a SnappingPrimitive to the SnappingObject. Called by the SnappingPrimitive during OnEnable.
        /// </summary>
        /// <param name="snappingPrimitive">SnappingPrimitive to subscribe</param>
        public void SubscribeSnappingPrimitive(SnappingPrimitive snappingPrimitive)
        {
            if (!snappingPrimitive || _subscribedSnappingPrimitives.Contains(snappingPrimitive))
                return;

            _subscribedSnappingPrimitives.Add(snappingPrimitive);
        }

        /// <summary>
        /// Unsubscribe a SnappingPrimitive from the SnappingObject. Called by the SnappingPrimitive during OnDisable.
        /// </summary>
        /// <param name="snappingPrimitive">SnappingPrimitive to unsubscribe</param>
        public void UnsubscribeSnappingPrimitive(SnappingPrimitive snappingPrimitive)
        {
            if (!snappingPrimitive || !_subscribedSnappingPrimitives.Contains(snappingPrimitive))
                return;

            _subscribedSnappingPrimitives.Remove(snappingPrimitive);
        }

        /// <summary>
        /// If the ASnappableActor has already subscribed to the SnappingObject, this function will handle its spatial representation depending of the InteractionData.
        /// </summary>
        /// <param name="aSnappableActor">ASnappableActor to handle</param>
        public void SetObjectMasterSpatialRepresentation(ASnappableActor aSnappableActor)
        {
            if (!aSnappableActor || _subscribedASnappableActors == null || _subscribedASnappableActors.Count == 0)
                return;

            InteractionData interactionData;
            if (!_subscribedASnappableActors.TryGetValue(aSnappableActor, out interactionData))
                return;

            //If the InteractionData, data are considered corrupted and are deleted
            if (interactionData == null)
                return;

            SpatialRepresentation spatialRepresentation = new SpatialRepresentation()
            {
                Position = IbTools.Convert(aSnappableActor.transform.position),
                Rotation = IbTools.Convert(aSnappableActor.transform.rotation)
            };

            if (interactionData.snappingPrimitive == null || automaticallyFindNearest)
            {
                SnappingPrimitive snappingPrimitive = this.GetNearestSnappingPrimitive(aSnappableActor, spatialRepresentation);
                if (interactionData.snappingPrimitive != snappingPrimitive)
                {
                    SnappingData lastSnappingData = interactionData.snappingPrimitive != null ? interactionData.snappingPrimitive.SnappingData : null;
                    SnappingData newSnappingData = snappingPrimitive != null ? snappingPrimitive.SnappingData : null;

                    if (lastSnappingData != newSnappingData)
                    {
                        Pose posdata;
                        SnappableActorData snappableActorData;

                        if (newSnappingData == null || !newSnappingData.GetASnappableActorData(aSnappableActor, out snappableActorData))
                            posdata = interactionData.originalPose;
                        else
                            posdata = snappableActorData.rootPose;

                        SnappingData.SetTransformPose(aSnappableActor.transform, posdata);
                    }

                    interactionData.snappingPrimitive = snappingPrimitive;
                }
            }

            //If the new SnappingPrimitive isn't null, the snappingPrimitive SpatialRepresentation is computed by the Object Snapper
            if (interactionData.snappingPrimitive)
            {
                //Check if the ASnappableActor needs to be refresh
                bool objectIsFixed = fixedASnappableActorInteraction && _subscribedASnappableActors.Count <= 1;

                //If the object is already initialized and don't have to move, we apply the last position and rotation local to the ObjectGizo Transform, else, we store it in the ObjectMasterInteractionData
                if (interactionData.localSP != null && objectIsFixed)
                {
                    aSnappableActor.transform.position = interactionData.snappingPrimitive.transform.TransformPoint(IbTools.Convert(interactionData.localSP.Value.Position));
                    aSnappableActor.transform.rotation = interactionData.snappingPrimitive.transform.rotation * IbTools.Convert(interactionData.localSP.Value.Rotation);
                }
                else
                {
                    spatialRepresentation = interactionData.snappingPrimitive.GetComputedSpatialRepresentation(spatialRepresentation, aSnappableActor);
                    aSnappableActor.transform.SetPositionAndRotation(IbTools.Convert(spatialRepresentation.Position), IbTools.Convert(spatialRepresentation.Rotation));

                    if (interactionData.snappingPrimitive.SnappingData && interactionData.snappingPrimitive.SnappingData.ApplyRootOffset(aSnappableActor))
                    {
                        interactionData.localSP = new SpatialRepresentation()
                        {
                            Position = IbTools.Convert(interactionData.snappingPrimitive.transform.InverseTransformPoint(aSnappableActor.transform.position)),
                            Rotation = IbTools.Convert(Quaternion.Inverse(interactionData.snappingPrimitive.transform.rotation) * aSnappableActor.transform.rotation)
                        };
                    }
                }
            }
            else
                _subscribedASnappableActors.Remove(aSnappableActor);
        }
        #endregion

        #region Privates
        private SnappingPrimitive GetNearestSnappingPrimitive(ASnappableActor aSnappableActor, SpatialRepresentation spatialRepresentation)
        {
            SnappingPrimitive snappingPrimitive = null;

            if (aSnappableActor != null && _subscribedSnappingPrimitives != null && _subscribedSnappingPrimitives.Count != 0)
            {
                float? lastDistance = null;
                for (int i = _subscribedSnappingPrimitives.Count - 1; i >= 0; i--)
                {
                    if (_subscribedSnappingPrimitives[i] == null)
                    {
                        _subscribedSnappingPrimitives.RemoveAt(i);
                        i++;
                    }
                    else
                    {
                        float newDistance = System.Numerics.Vector3.Distance(spatialRepresentation.Position, _subscribedSnappingPrimitives[i].GetComputedSpatialRepresentation(spatialRepresentation, aSnappableActor).Position);
                    
                        if ( (lastDistance == null || newDistance < lastDistance.Value) && _subscribedSnappingPrimitives[i] != null && _subscribedSnappingPrimitives[i].SnappingData != null && _subscribedSnappingPrimitives[i].SnappingData.ContainData(aSnappableActor))
                        {
                            lastDistance = newDistance;
                            snappingPrimitive = _subscribedSnappingPrimitives[i];
                        }
                    }
                }
            }

            return snappingPrimitive;
        }
        #endregion
    }
}