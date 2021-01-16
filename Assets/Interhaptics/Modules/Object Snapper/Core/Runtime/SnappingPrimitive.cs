using Interhaptics.InteractionsEngine.Shared.Types;
using System;
using System.Collections.Generic;
using Interhaptics.InteractionsEngine;
using Interhaptics.Modules.Interaction_Builder.Core;
using UnityEditor;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    [ExecuteInEditMode]
    public class SnappingPrimitive : MonoBehaviour
    {
        #region Enums
        protected enum Axis
        {
            X = 1,
            Y = 2,
            Z = 3,
            minusX = -1,
            minusY = -2,
            minusZ = -3
        }
        #endregion

        #region Constants
        /*
         * 
         * General
         * 
         */
        private const string TEXT_Separator = " ";

        /*
         * 
         * Ressources
         * 
         */
        private const string RESSOURCE_ModelsPath = "3DModels/";
        private const string RESSOURCE_IBCircle = "IBCircle";
        private const string RESSOURCE_IBCylinder = "IBCylinder";
        private const string RESSOURCE_IBTunnel = "IBTunnel";
        private const string RESSOURCE_IBSphere = "IBSphere";
        private const string RESSOURCE_IBCapsuleRound = "IBCapsuleRound";

        /*
         * 
         * Shape 
         * 
         */
        private const float VALUE_PrimaryRadius = 0.25f;
        private const float VALUE_SecondaryRadius = 0.2f;
        private const float VALUE_Length = 1f;

        private const string TOOLTIP_MovementType = "Determine if the spatial representation of the ASnappableAction is computed by the tracking position or rotation";
        private const string TOOLTIP_PoseData = "The file which will contain the snappingData";

        private const string ERROR_ResourcesNotFound = "Some SDK's resources were not found";

        /*
         * 
         * Snapping
         * 
         */
        private const float VALUE_TrackingRadius = 0.05f;
        private const float VALUE_TrackingDistance = 0.05f;
        private const int VALUE_AxisLength = 2;

        private const string TOOLTIP_ForwardVector = "The custom forward of the SnappableActor. For exemple, a hand forward should be the direction from the palm to the index distal";
        private const string TOOLTIP_UpwardVector = "The custom upward of the SnappableActor. For exemple, a hand upward should be the direction from the palm to the back of the hand";

        private const string NAME_Tracking = "Tracking";
        private const string NAME_Reset = "Reset";
        private const string NAME_ResetSnappingPrimitiveValues = NAME_Reset + " SnappingPrimive values";

        private const string WARNING_NoAnimator = "ASnappableActor's Animator is null";
        private const string WARNING_NoAnimatorController = "No AnimatorController found on this ASnappableActor's Animator";
        private const string WARNING_NoSnappingPrimitive = "No SnappingPrimitive found on it or one of its parents";
        private const string ERROR_UnableSave = "Failed to save the current pose";
        #endregion

        #region Statics
#if UNITY_EDITOR
        private static SnappingPrimitive s_editModSigleton = null;
#endif
        #endregion

        #region Variables
        /*
         * 
         * Shape 
         * 
         */
        //SerializeField
        [SerializeField] protected PrimitiveShape primitiveShape = PrimitiveShape.Sphere;
        [SerializeField] [Tooltip(TOOLTIP_MovementType)] protected MovementType movementType = MovementType.Rotation;
        [SerializeField] protected Vector3 localPosition = Vector3.zero;
        [SerializeField] protected Vector3 localRotation = Vector3.zero;
        [SerializeField] protected float primaryRadius = VALUE_PrimaryRadius;
        [SerializeField] protected Color primaryColor = Color.blue;
        [SerializeField] protected float length = VALUE_Length;
        [SerializeField] protected float secondaryRadius = VALUE_SecondaryRadius;
        [SerializeField] protected Color secondaryColor = Color.red;

        //Privates
        private PrimitiveShape _lastPrimitiveForm = (PrimitiveShape)(-1);
        private Vector3 _gizmoPosition = Vector3.zero, _scale = Vector3.one;
        private Quaternion _gizmoRotation = Quaternion.identity;
        private Mesh _primaryMesh = null, _secondaryMesh = null;

        /*
         * 
         * Snapping
         * 
         */
        //SerializeField
        [SerializeField] protected ModelSnappableActor modelSnappableActor = null;
        [Tooltip(TOOLTIP_ForwardVector)] [SerializeField] protected Axis forwardAxis = Axis.Z;
        [Tooltip(TOOLTIP_UpwardVector)] [SerializeField] protected Axis upwardAxis = Axis.Y;
        [SerializeField] protected float trackingRadius = VALUE_TrackingRadius;
        [SerializeField] protected Color trackingColor = Color.black;
        [SerializeField] protected float trackingDistance = VALUE_TrackingDistance;
        [SerializeField] protected float trackingAxisLength = VALUE_AxisLength;
        [Tooltip(TOOLTIP_PoseData)] [SerializeField] protected SnappingData snappingData = null;

        //Inspector
        [SerializeField] [HideInInspector] protected SphereCollider _simulationTracking = null;
        [SerializeField] [HideInInspector] protected ASnappableActor _simulationASnappableActor = null;

        //Unity Editor
#if UNITY_EDITOR
        private Action<SceneView> _inputListener = null;
        private bool _trackingInitialized = false;
        private ASnappableActor _lastModelSnappableActor = null;
#endif

        //Privates
        protected SpatialRepresentation _currentTrackingSP = default;
        private Vector3 _forwardVector = Vector3.forward, _upwardVector = Vector3.up;
        private SnappingObject _snappingObject = null;
        #endregion

        #region Life Cycle
        protected virtual void OnValidate()
        {
            //Shape
            this.ShapeOnValidate();

            //Snapping
            this.SnapEditionOnValidate();
        }

        protected virtual void OnDrawGizmos()
        {
            if (!this.isActiveAndEnabled)
                return;

            //Shape
            this.ShapeOnDrawGizmo();

            //Snapping
            this.SnapEditionOnDrawGizmo();
        }

        protected virtual void Awake()
        {
            //Snapping
            this.SnapEditionAwake();
        }

        protected virtual void OnEnable()
        {
            //Snapping
            this.SnapEditionOnEnable();
        }

        protected virtual void Update()
        {
            //Shape
            this.ShapeUpdate();

            //Dependencies
            this.SnapEditionUpdate();
        }

        protected virtual void OnDisable()
        {
            //Snapping
            this.SnapEditionOnDisable();
        }
        #endregion

        #region General
        //Privates
        [ContextMenu(NAME_Reset, false, 0)]
        private void Reset()
        {
            //Shape
            this.ResetShapeValues();

            primitiveShape = PrimitiveShape.Sphere;
            movementType = MovementType.Rotation;
            primaryColor = Color.blue;
            secondaryColor = Color.red;
            _lastPrimitiveForm = (PrimitiveShape)(-1);
            _scale = Vector3.one;
            _gizmoPosition = Vector3.zero;
            _gizmoRotation = Quaternion.identity;
            _primaryMesh = null;
            _secondaryMesh = null;

            //Snapping
            this.ResetSnapEdition();

            modelSnappableActor = null;
            forwardAxis = Axis.Z;
            upwardAxis = Axis.Y;
            trackingRadius = VALUE_TrackingRadius;
            trackingColor = Color.black;
            trackingDistance = VALUE_TrackingRadius;
            trackingAxisLength = VALUE_AxisLength;
            snappingData = null;
            _simulationTracking = null;
            _simulationASnappableActor = null;
            _currentTrackingSP = default;
            _forwardVector = Vector3.forward;
            _upwardVector = Vector3.up;

#if UNITY_EDITOR
            _inputListener = null;
            _lastModelSnappableActor = null;
            _trackingInitialized = false;
#endif

            this.OnReset();
        }

        [ContextMenu(NAME_ResetSnappingPrimitiveValues, false, 0)]
        private void ResetShapeValues()
        {
            //Shape
            localPosition = Vector3.zero;
            localRotation = Vector3.zero;
            primaryRadius = 0.25f;
            length = 1f;
            secondaryRadius = 0.2f;

            this.OnResetShapeValues();
        }

        //Publics
        /// <summary>
        /// Return the SnappingData linked with this Snapping Primitive.
        /// </summary>
        public SnappingData SnappingData { get { return snappingData; } }

        /// <summary>
        /// Return the snapping computed SpatialRepresentation of an aSnappableActor (depending on its SnappableActorData)
        /// </summary>
        /// <param name="currentSP">Current SpatialRepresentation of the aSnappableActor (Tracking)</param>
        /// <param name="aSnappableActor">ASnappableActor which needs to be snapped</param>
        /// <returns>Snapping computed SpatialRepresentation</returns>
        public SpatialRepresentation GetComputedSpatialRepresentation(SpatialRepresentation currentSP, ASnappableActor aSnappableActor)
        {
            System.Numerics.Vector3 forward = System.Numerics.Vector3.UnitZ;
            System.Numerics.Vector3 upward = System.Numerics.Vector3.UnitY;

            SnappableActorData snappableActorData;
            if (snappingData && snappingData.GetASnappableActorData(aSnappableActor, out snappableActorData))
            {
                forward = snappableActorData.forward.ToNumericsVector3();
                upward = snappableActorData.upward.ToNumericsVector3();
            }

            return this.GetSnappedSpatialRepresentation(currentSP, forward, upward);
        }
        #endregion

        #region Shape
        //Life Cycle
        protected virtual void ShapeOnValidate()
        {
            if (primaryRadius < 0)
                primaryRadius = 0;

            if (length < 0)
                length = 0;

            secondaryRadius = (primaryRadius > 0) ? Mathf.Clamp(secondaryRadius, 0, primaryRadius / 2) : 0;
        }

        protected virtual void ShapeOnDrawGizmo()
        {
            if (_primaryMesh)
            {
                Gizmos.color = primaryColor;
                Gizmos.DrawMesh(_primaryMesh, _gizmoPosition, _gizmoRotation, _scale);

                //Torus tube
                if (_secondaryMesh)
                {
                    if (primitiveShape == PrimitiveShape.Torus)
                    {
                        Gizmos.color = secondaryColor;
                        Gizmos.DrawMesh(_secondaryMesh, _gizmoPosition + (_gizmoRotation * Vector3.up) * (primaryRadius), _gizmoRotation * Quaternion.Euler(0, 0, 90), Vector3.one * secondaryRadius);
                    }
                    else if (primitiveShape == PrimitiveShape.Capsule)
                    {
                        Vector3 scale = Vector3.one * primaryRadius;
                        Gizmos.DrawMesh(_secondaryMesh, _gizmoPosition + (_gizmoRotation * Vector3.up) * length, _gizmoRotation, scale);
                        Gizmos.DrawMesh(_secondaryMesh, _gizmoPosition + (_gizmoRotation * Vector3.down) * length, _gizmoRotation * Quaternion.Euler(180, 0, 0) * Quaternion.Inverse(_gizmoRotation) * _gizmoRotation, scale);
                    }
                }
            }
        }

        private void ShapeUpdate()
        {
            if (primitiveShape != _lastPrimitiveForm)
            {
                //Load gizmos' mesh from resources
                switch (primitiveShape)
                {
                    case PrimitiveShape.Sphere:

                        if (_primaryMesh == null || _primaryMesh.name != RESSOURCE_IBSphere)
                            _primaryMesh = this.GetResourcesMeshByName(RESSOURCE_IBSphere);

                        break;
                    case PrimitiveShape.Cylinder:

                        if (_primaryMesh == null || _primaryMesh.name != RESSOURCE_IBCylinder)
                            _primaryMesh = this.GetResourcesMeshByName(RESSOURCE_IBCylinder);

                        break;
                    case PrimitiveShape.Capsule:

                        if (_primaryMesh == null || _primaryMesh.name != RESSOURCE_IBTunnel)
                            _primaryMesh = this.GetResourcesMeshByName(RESSOURCE_IBTunnel);

                        if (primitiveShape == PrimitiveShape.Capsule)
                        {
                            if (_secondaryMesh == null || _secondaryMesh.name != RESSOURCE_IBCapsuleRound)
                                _secondaryMesh = this.GetResourcesMeshByName(RESSOURCE_IBCapsuleRound);
                        }

                        break;
                    case PrimitiveShape.Torus:

                        if (_primaryMesh == null || _primaryMesh.name != RESSOURCE_IBCircle)
                            _primaryMesh = this.GetResourcesMeshByName(RESSOURCE_IBCircle);

                        if (_secondaryMesh == null || _secondaryMesh.name != RESSOURCE_IBCylinder)
                            _secondaryMesh = this.GetResourcesMeshByName(RESSOURCE_IBCylinder);

                        break;
                    default:
                        break;
                }

                _lastPrimitiveForm = primitiveShape;
            }

            _gizmoPosition = transform.position + (transform.rotation * localPosition);
            _gizmoRotation = transform.rotation * Quaternion.Euler(localRotation);

            switch (primitiveShape)
            {
                case PrimitiveShape.Sphere:

                    _scale = Vector3.one * primaryRadius * 2;

                    break;
                case PrimitiveShape.Cylinder:
                case PrimitiveShape.Capsule:

                    _scale = new Vector3(primaryRadius, length, primaryRadius);

                    break;

                case PrimitiveShape.Torus:

                    _scale = Vector3.one * primaryRadius * 2;

                    break;

                default:
                    break;
            }
        }

        //Privates
        private Mesh GetResourcesMeshByName(string name)
        {
            Mesh foundedMesh = Resources.Load<Mesh>(RESSOURCE_ModelsPath + name);

            if (foundedMesh == null)
                Debug.LogError(ERROR_ResourcesNotFound + TEXT_Separator + name);

            return foundedMesh;
        }
        #endregion

        #region Snap Edition
        //Life Cycle
        protected virtual void SnapEditionOnValidate()
        {
            //Set Axis
            int value = (int)forwardAxis;
            _forwardVector = Vector3.zero;
            _forwardVector[Mathf.Abs(value) - 1] = Math.Sign(value);

            value = (int)upwardAxis;
            _upwardVector = Vector3.zero;
            _upwardVector[Mathf.Abs(value) - 1] = Math.Sign(value);

            //Clamp variables
            if (trackingRadius < 0)
                trackingRadius = 0;

            if (trackingDistance < 0)
                trackingDistance = 0;

            if (trackingAxisLength < 0)
                trackingAxisLength = 0;

            if (_simulationTracking)
                this.AlignTracking();

            //Check Animator
            if ((!snappingData || Application.isPlaying) && modelSnappableActor)
                modelSnappableActor = null;

            if (modelSnappableActor)
            {
                if (!modelSnappableActor.Animator || !modelSnappableActor.Animator.runtimeAnimatorController)
                {
                    if (!modelSnappableActor.Animator)
                        Debug.LogWarning(WARNING_NoAnimator);
                    else
                        Debug.LogWarning(WARNING_NoAnimatorController, modelSnappableActor);

                    modelSnappableActor = null;
                }
            }
        }

        protected virtual void SnapEditionOnDrawGizmo()
        {
            if (!_simulationTracking)
                return;

            Gizmos.color = trackingColor;
            Gizmos.DrawSphere(_simulationTracking.transform.position, trackingRadius);

            Vector3 simulationTrackingPosition = _simulationTracking.transform.position;
            Quaternion simulationTrackingRotation = _simulationTracking.transform.rotation;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(simulationTrackingPosition, simulationTrackingPosition + simulationTrackingRotation * _forwardVector * trackingRadius * trackingAxisLength);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(simulationTrackingPosition, simulationTrackingPosition + simulationTrackingRotation * _upwardVector * trackingRadius * trackingAxisLength);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(simulationTrackingPosition, simulationTrackingPosition + Vector3.Cross(simulationTrackingRotation * _forwardVector, simulationTrackingRotation * _upwardVector) * trackingRadius * trackingAxisLength);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(simulationTrackingPosition, IbTools.Convert(this.GetSnappedSpatialRepresentation(_currentTrackingSP, IbTools.Convert(_forwardVector), IbTools.Convert(_upwardVector)).Position));
        }

        protected virtual void SnapEditionOnEnable()
        {
            if (_snappingObject)
                _snappingObject.SubscribeSnappingPrimitive(this);

#if UNITY_EDITOR
    #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += _inputListener;
    #else
            SceneView.onSceneGUIDelegate = view => _inputListener(view);
    #endif
#endif
        }

        protected virtual void SnapEditionAwake()
        {
            //Dependencies
            _snappingObject = gameObject.GetComponentInParent<SnappingObject>();
            if (!_snappingObject)
                Debug.LogWarning(WARNING_NoSnappingPrimitive, this);

#if UNITY_EDITOR
            _inputListener = (_) =>
            {
                Event currentEvent = Event.current;

                if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                    this.AlignTracking();
            };

            if (Application.isPlaying)
            {
                this.QuickSave();
                this.ResetSnapEdition();
            }
#endif

        }

        protected virtual void SnapEditionUpdate()
        {
#if UNITY_EDITOR
            //Check if there is a new ASnappableActor
            if (modelSnappableActor != _lastModelSnappableActor)
            {
                if (modelSnappableActor && s_editModSigleton != this)
                {
                    if (s_editModSigleton)
                    {
                        s_editModSigleton.QuickSave();
                        s_editModSigleton.ResetSnapEdition();
                    }

                    s_editModSigleton = this;

#if UNITY_2019_2_OR_NEWER
                    if (SceneVisibilityManager.instance)
                    {
                        List<GameObject> isolatedGameObject = new List<GameObject>();
                        isolatedGameObject.Add(gameObject);

                        if (_simulationASnappableActor)
                            isolatedGameObject.Add(_simulationASnappableActor.gameObject);

                        if (_simulationTracking)
                            isolatedGameObject.Add(_simulationTracking.gameObject);

                        SceneVisibilityManager.instance.Isolate(isolatedGameObject.ToArray(), true);
                    }
#endif
                }

                _lastModelSnappableActor = modelSnappableActor;
            }

            //Set the modelSnappableActor and tracking simulation
            if (modelSnappableActor)
            {
                if (!_simulationTracking)
                {
                    _simulationTracking = new GameObject(NAME_Tracking).AddComponent<SphereCollider>(); ;
                    _simulationTracking.radius = trackingRadius;
                }

                if (!_simulationASnappableActor)
                {
                    _simulationASnappableActor = GameObject.Instantiate<ASnappableActor>(modelSnappableActor);

                    if (_simulationASnappableActor)
                    {
                        SnappableActorData snappableActorData;
                        if (snappingData && snappingData.GetASnappableActorData(_simulationASnappableActor, out snappableActorData))
                            SnappingData.SetTransformPose(_simulationASnappableActor.transform, snappableActorData.rootPose);
                    }
                }
            }
            else
                this.ResetSnapEdition();

            if (_simulationTracking)
            {
                if (!_trackingInitialized)
                {

                    Camera camera = SceneView.lastActiveSceneView.camera;
                    _simulationTracking.transform.position = camera ? (camera.transform.position) : Vector3.zero;
                    _simulationTracking.transform.rotation = camera ? Quaternion.FromToRotation(camera.transform.forward, _simulationTracking.transform.rotation * _upwardVector) * (camera.transform.rotation) : Quaternion.identity;
                }

                this.RefreshTrackingPosition();

                if (!_trackingInitialized)
                {
                    this.AlignTracking();
                    _trackingInitialized = true;
                }
            }

            if (_simulationASnappableActor)
            {
                SpatialRepresentation spatialRepresentation = this.GetSnappedSpatialRepresentation(_currentTrackingSP, IbTools.Convert(_forwardVector), IbTools.Convert(_upwardVector));
                _simulationASnappableActor.transform.SetPositionAndRotation(IbTools.Convert(spatialRepresentation.Position), IbTools.Convert(spatialRepresentation.Rotation));
            }
#endif
        }

        protected virtual void SnapEditionOnDisable()
        {
            this.ResetSnapEdition();

            if (_snappingObject)
                _snappingObject.UnsubscribeSnappingPrimitive(this);

#if UNITY_EDITOR
    #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= _inputListener;
    #else
            SceneView.onSceneGUIDelegate = null;
    #endif
#endif
        }

        //Privates
        private void RefreshTrackingPosition()
        {
            _currentTrackingSP = new SpatialRepresentation() { Position = IbTools.Convert(_simulationTracking.transform.position), Rotation = IbTools.Convert(_simulationTracking.transform.rotation) };
            _currentTrackingSP = this.GetSnappedSpatialRepresentation(_currentTrackingSP, IbTools.Convert(_forwardVector), IbTools.Convert(_upwardVector));
            _currentTrackingSP.Position = IbTools.Convert(IbTools.Convert(_currentTrackingSP.Position) + IbTools.Convert(_currentTrackingSP.Rotation) * _upwardVector * trackingDistance);
        }

        private void AlignTracking()
        {
            if (!_simulationTracking)
                return;

            _simulationTracking.transform.SetPositionAndRotation(IbTools.Convert(_currentTrackingSP.Position), IbTools.Convert(_currentTrackingSP.Rotation));
        }

        private SpatialRepresentation GetSnappedSpatialRepresentation(SpatialRepresentation trackingSR, System.Numerics.Vector3 forward, System.Numerics.Vector3 upward)
        {
            if (forward != System.Numerics.Vector3.Zero && upward != System.Numerics.Vector3.Zero)
            {
                SpatialRepresentation snappingPrimitiveSR = new SpatialRepresentation() { Position = IbTools.Convert(_gizmoPosition), Rotation = IbTools.Convert(_gizmoRotation) };

                switch (primitiveShape)
                {
                    case PrimitiveShape.Sphere:
                        trackingSR = InteractionEngineApi.SphereComputing(forward, upward, trackingSR, snappingPrimitiveSR, primaryRadius);
                        break;
                    case PrimitiveShape.Cylinder:
                    case PrimitiveShape.Capsule:
                        trackingSR = InteractionEngineApi.CylinderComputing(forward, upward, trackingSR, snappingPrimitiveSR, length, primaryRadius, movementType, primitiveShape == PrimitiveShape.Cylinder);
                        break;
                    case PrimitiveShape.Torus:
                        trackingSR = InteractionEngineApi.TorusComputing(forward, upward, trackingSR, snappingPrimitiveSR, primaryRadius, secondaryRadius, movementType);
                        break;
                    default:
                        break;
                }
            }
            return trackingSR;
        }

        //Publics
        /// <summary>
        /// Close the editing mod.
        /// </summary>
        public void ResetSnapEdition()
        {
#if UNITY_EDITOR
            //Simulation
            if (_simulationTracking)
                GameObject.DestroyImmediate(_simulationTracking.gameObject);

            if (_simulationASnappableActor)
                GameObject.DestroyImmediate(_simulationASnappableActor.gameObject);

            //Variables
            modelSnappableActor = null;
            _lastModelSnappableActor = null;

            _trackingInitialized = false;

            if (s_editModSigleton == this)
            {
                s_editModSigleton = null;

#if UNITY_2019_2_OR_NEWER
                if (SceneVisibilityManager.instance && SceneVisibilityManager.instance.IsCurrentStageIsolated())
                    SceneVisibilityManager.instance.ExitIsolation();
#endif
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Save the SnappableActorData of an ASnappableActor into a SnappingData.
        /// </summary>
        /// <param name="aSnappableActor">ASnappableActor model</param>
        /// <param name="tracking">Tracking model</param>
        /// <param name="snappingData">Snapping which will contain the SnappableActorData</param>
        /// <param name="forward">Custom forward of the ASnappableActor</param>
        /// <param name="upward">Custom upward of the ASnappableActor</param>
        /// <returns>True if True if the SnappableActorData was correctly saved</returns>
        public bool SaveASnappableActorData(ASnappableActor aSnappableActor, Transform tracking, SnappingData snappingData, Vector3 forward, Vector3 upward)
        {
            bool success = false;

            if (aSnappableActor && tracking && snappingData)
            {
                SpatialRepresentation trackingSR = new SpatialRepresentation();
                trackingSR.Position = IbTools.Convert(tracking.position);
                trackingSR.Rotation = IbTools.Convert(tracking.rotation);


                SpatialRepresentation snappedSR = this.GetSnappedSpatialRepresentation(trackingSR, IbTools.Convert(forward), IbTools.Convert(upward));

                if (snappingData.AddSnappableActorData(aSnappableActor, snappedSR, forward, upward))
                    success = true;
            }
            else
                Debug.LogError(ERROR_UnableSave);

            return success;
        }

        /// <summary>
        /// Save a new SnappableActorData with the _simulationASnappableActor as ASnappableActor, _simulationTracking as Tracking and snappingData as SnappingData.
        /// </summary>
        /// <returns>True if True if the SnappableActorData was correctly saved</returns>
        public bool QuickSave()
        {
            bool success = false;

            if (_simulationASnappableActor && _simulationTracking && snappingData)
                success = this.SaveASnappableActorData(_simulationASnappableActor, _simulationTracking.transform, snappingData, _forwardVector, _upwardVector);

            return success;
        }
#endif
        #endregion

        #region Virtuals
        protected virtual void OnReset() { }

        protected virtual void OnResetShapeValues() { }
        #endregion

    }
}
