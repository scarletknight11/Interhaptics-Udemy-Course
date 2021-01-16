using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.InteractionsEngine;

using UnityEngine.Events;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;


namespace Interhaptics.Modules.Interaction_Builder.Core {

    /// <summary>
    ///     An interactable object
    /// </summary>
    [AddComponentMenu("Interhaptics/Interaction Builder/InteractionObject")]
    [RequireComponent(typeof(Rigidbody))]
    public class InteractionObject : MonoBehaviour
    {

        #region Log Messages
        private const string ERROR_DLL_NOT_FOUND = "<b>[InteractionObject]</b> The HAR dll wasn't found, so no haptic will be applied to this interaction";
        #endregion


        #region Enums
        /// <summary>
        ///     Axis of the 3D engine
        /// </summary>
        [System.Serializable]
        public enum Axis
        {
            X = 0, Y = 1, Z = 2
        }
        #endregion


        #region Structs
        /// <summary>
        ///     A numerical target representation
        /// </summary>
        [System.Serializable]
        public struct NumericalTarget
        {

            /// <summary>
            ///     The targeted value
            /// </summary>
            [Tooltip("The targeted value")]
            public float targetValue;
            /// <summary>
            ///     Event linked to this target
            /// </summary>
            [Tooltip("Event which will be raise when the value is reached")]
            public UnityEvent onTargetReachedEvent;
            /// <summary>
            ///     Event linked to this target
            /// </summary>
            [Tooltip("Event which will be raise when the value isn't reached anymore")]
            public UnityEvent onTargetNotReachedAnymoreEvent;

            /// <summary>
            ///     A constructor
            /// </summary>
            /// <param name="targetValue">The target value</param>
            /// <param name="onTargetReachedEvent">A target event</param>
            /// <param name="onTargetNotReachedAnymoreEvent">A target event</param>
            public NumericalTarget(float targetValue, UnityEvent onTargetReachedEvent,
                UnityEvent onTargetNotReachedAnymoreEvent)
            {
                this.targetValue = targetValue;
                this.onTargetReachedEvent = onTargetReachedEvent;
                this.onTargetNotReachedAnymoreEvent = onTargetNotReachedAnymoreEvent;
            }

        }

        /// <summary>
        ///     A physical target representation
        /// </summary>
        [System.Serializable]
        public struct PhysicalTarget
        {

            #region Nested Enums
            public enum TargetType
            {
                TargetOnCollider = 0,
                TargetOnTag = 1,
                TargetOnLayer = 2
            }
            #endregion


            /// <summary>
            ///     Type of physical target. If set to TargetOnCollider, the collider list will be this target. If set to TargetOnTag, every collider with the targeted tag will be a target. If set to TargetOnLayer, every collider with the targeted layer mask will be a target
            /// </summary>
            [Tooltip("Type of physical target. If set to TargetOnCollider, the collider list will be this target. If set to TargetOnTag, every collider with the targeted tag will be a target. If set to TargetOnLayer, every collider with the targeted layer mask will be a target")]
            public TargetType targetType;
            /// <summary>
            ///     List of targeted collider
            /// </summary>
            [Tooltip("Colliders targeted")]
            public List<Collider> targetCollider;
            /// <summary>
            ///     A targeted collider tag
            /// </summary>
            [Tooltip("Tag targeted")]
            public string targetTag;
            /// <summary>
            ///     A targeted layer
            /// </summary>
            [Tooltip("Layer targeted")]
            public LayerMask targetLayer;
            /// <summary>
            ///     Event called the object collide with a target
            /// </summary>
            [Tooltip("Event which will be raise when the object will collide a target")]
            public UnityEvent onTargetEnterEvent;
            /// <summary>
            ///     Event called the object stay in collision with a target
            /// </summary>
            [Tooltip("Event which will be raise when the object will stay in collision with a target")]
            public UnityEvent onTargetStayEvent;
            /// <summary>
            ///     Event called the object exit the collision with a target
            /// </summary>
            [Tooltip("Event which will be raise when the object will exit a collision with a target")]
            public UnityEvent onTargetExitEvent;

            /// <summary>
            ///     A constructor
            /// </summary>
            /// <param name="targetType">TypeOfThisTarget</param>
            /// <param name="targetCollider">Targets</param>
            /// <param name="targetTag">Tags</param>
            /// <param name="targetLayer">Layer mask</param>
            /// <param name="onTargetEnterEvent">The entering event</param>
            /// <param name="onTargetStayEvent">The staying event</param>
            /// <param name="onTargetExitEvent">The exiting event</param>
            public PhysicalTarget(TargetType targetType, List<Collider> targetCollider, string targetTag,
                LayerMask targetLayer, UnityEvent onTargetEnterEvent, UnityEvent onTargetStayEvent,
                UnityEvent onTargetExitEvent)
            {
                this.targetType = targetType;
                this.targetCollider = targetCollider;
                this.targetTag = targetTag;
                this.targetLayer = targetLayer;
                this.onTargetEnterEvent = onTargetEnterEvent;
                this.onTargetStayEvent = onTargetStayEvent;
                this.onTargetExitEvent = onTargetExitEvent;
            }

        }
        #endregion


        #region Serialized Fields
        [Header("Interaction Settings")]
        [SerializeField]
        [Tooltip("The interaction to do with this object")]
        private InteractionPrimitive interactionPrimitive = null;
        [SerializeField]
        [Tooltip("Threshold value to consider a grab")]
        [Range(0, 1)]
        private float graspingStrengthThreshold =
            InteractionsEngine.Shared.DOs.InteractionThresholds.DefaultGraspingStrengthTriggering;
        [SerializeField]
        [Tooltip("Threshold value to consider a pinch")]
        [Range(0, 1)]
        private float pinchingStrengthThreshold =
            InteractionsEngine.Shared.DOs.InteractionThresholds.DefaultPinchingStrengthTriggering;
        [SerializeField]
        [Tooltip("Delay timer to wait before triggering interaction")]
        private float delayTimer = InteractionsEngine.Shared.DOs.InteractionThresholds.DefaultTriggeringTimer;
        [SerializeField]
        [Tooltip("if true an interaction with two hands will be finished when one hand will stop. Otherwise, the interaction will be finished when both hand finished")]
        private bool stopDoubleGraspingWhenOneHandRelease = true;
        [SerializeField]
        [Tooltip("True to block interactions on the object when an interaction will begin")]
        public bool blockObjectOnInteractionStart = false;
        [SerializeField]
        [Tooltip("True to block interactions on the body part when an interaction will finish")]
        public bool blockBodyPartsOnInteractionStart= false;
        [SerializeField]
        [Tooltip("True to set the object as kinematic during the interaction")]
        public bool setAsKinematicDuringInteraction = true;
        [SerializeField]
        [Tooltip("True to use local position/rotation, false to use global ones")]
        private bool computeOnLocalBasis = true;

        [Header("RotationAroundPivot Settings")]
        [SerializeField]
        [Tooltip("The pivot to use, if null the object itself will be take")]
        private Transform pivotTransform = null;
        [SerializeField]
        [Tooltip("Which pivot local axis use perform the rotation. None is the null vector")]
        private Axis localRotationAxis = Axis.Y;

        [Header("Sliding Settings")]
        [SerializeField]
        [Tooltip("The reference transform, if null the object itself will be take")]
        private Transform slidingTransform = null;
        [SerializeField]
        [Tooltip("Which reference transform local axis use perform the sliding. None is the null vector")]
        private Axis localSlidingAxis = Axis.Y;

        [SerializeField]
        [Tooltip("Event raised when an interaction will be start")]
        private UnityEvent onInteractionStart = new UnityEvent();
        [SerializeField]
        [Tooltip("Event raised when an interaction will be finish")]
        private UnityEvent onInteractionFinish = new UnityEvent();

        [SerializeField]
        [Tooltip("True to activate events on constraints")]
        public bool activateConstraintsEvents = false;
        [SerializeField]
        [Tooltip("Event raised when the object exit constraints")]
        private UnityEvent onNoConstraintsReached = new UnityEvent();
        [SerializeField]
        [Tooltip("Event raised when the object reached the maximal")]
        private UnityEvent onMaximalConstraintReached = new UnityEvent();
        [SerializeField]
        [Tooltip("Event raised when the object reached the minimal")]
        private UnityEvent onMinimalConstraintReached = new UnityEvent();

        [SerializeField]
        [Tooltip("True to activate events on numerical targets")]
        public bool activateNumericalTargetsEvents = false;
        [SerializeField]
        [Tooltip("Event raised when the object exit every numerical targets")]
        private UnityEvent onNoNumericalTargetReached = new UnityEvent();
        [SerializeField]
        [Tooltip("All numerical targets")]
        private List<NumericalTarget> numericalTargetList = new List<NumericalTarget>();

        [SerializeField]
        [Tooltip("True to activate events on Physical targets")]
        public bool activatePhysicalTargetsEvents = false;
        [SerializeField]
        [Tooltip("All physical targets")]
        private List<PhysicalTarget> physicalTargetList = new List<PhysicalTarget>();
        #endregion


        #region Fields Getters
        public InteractionPrimitive InteractionPrimitive
        {
            get => interactionPrimitive;
            set => interactionPrimitive = value;
        }
        public float GraspingStrengthThreshold
        {
            get => graspingStrengthThreshold;
            set
            {
                if (InteractionEngineApi.SetInteractionGrabbingThresholds(ObjectId, value))
                    graspingStrengthThreshold = value;
            }
        }

        public float PinchingStrengthThreshold
        {
            get => pinchingStrengthThreshold;
            set
            {
                if (InteractionEngineApi.SetInteractionPinchingThresholds(ObjectId, value))
                    pinchingStrengthThreshold = value;
            }
        }

        public float DelayTimer
        {
            get => delayTimer;
            set
            {
                if (InteractionEngineApi.SetInteractionDelayTimer(ObjectId, value))
                    delayTimer = value;
            }
        }

        public bool StopDoubleGraspingWhenOneHandRelease
        {
            get => stopDoubleGraspingWhenOneHandRelease;
            set => stopDoubleGraspingWhenOneHandRelease = value;
        }

        public int HapticMaterialId { get; private set; } = -1;
        public int ObjectId { get; set; } = -1;
        public float RealTimeSinceStartupAtInteractionStart { get; private set; }
        public Vector3 PositionAtStart { get; private set; }
        public bool IsInteracting { get; private set; }
        public BodyPartInteractionStrategy InteractWith { get; private set; }
        public bool ComputeOnLocalBasis => computeOnLocalBasis;

        public UnityEvent OnInteractionStart => onInteractionStart;
        public UnityEvent OnInteractionFinish => onInteractionFinish;

        public UnityEvent OnNoConstraintsReached => onNoConstraintsReached;
        public UnityEvent OnMaximalConstraintReached => onMaximalConstraintReached;
        public UnityEvent OnMinimalConstraintReached => onMinimalConstraintReached;

        public UnityEvent OnNoNumericalTargetReached => onNoNumericalTargetReached;
        public List<NumericalTarget> NumericalTargetList => numericalTargetList;
        
        public List<PhysicalTarget> PhysicalTargetList => physicalTargetList;
        #endregion


        #region Private Fields
        private Rigidbody _rigidbody;
        private bool _kinematicBuffer;
        private bool _noConstraintsCalled;
        private bool _maximalConstraintReached = true;
        private bool _minimalConstraintReached = true;
        private NumericalTarget? _lastNumericalTarget;
        #endregion


        #region Life Cycles
        private void Start ()
        {
            if (interactionPrimitive == null)
                Destroy(this);

            if (interactionPrimitive.hapticInteractionFeedback)
            {
                try
                {
                    HapticMaterialId =
                        HapticRenderer.Core.HARWrapper.AddHM(interactionPrimitive.hapticInteractionFeedback);
                    if (interactionPrimitive.vibrationEvaluationMode ==
                        InteractionPrimitive.VibrationEvaluationMode.OnlyOnTrigger ||
                        interactionPrimitive.vibrationEvaluationMode == InteractionPrimitive.VibrationEvaluationMode
                            .SinceTheInteractionBeginning)
                        HapticRenderer.Core.HARWrapper.StopVibration(HapticMaterialId);
                }
                catch (System.DllNotFoundException)
                {
                    Debug.LogError(ERROR_DLL_NOT_FOUND, gameObject);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString(), gameObject);
                }
            }

            _rigidbody = GetComponent<Rigidbody>();
            Transform t = transform;
            Vector3 position = computeOnLocalBasis ? t.localPosition : t.position;
            Quaternion rotation = computeOnLocalBasis ? t.localRotation : t.rotation;
            SpatialRepresentation spatialRepresentation = new SpatialRepresentation
            {
                Position = IbTools.Convert(position),
                Rotation = IbTools.Convert(rotation)
            };
            List<Target> targetList = numericalTargetList.Select(target => new Target {Value = target.targetValue}).ToList();
            Constraints constraints = new Constraints
            {
                Maximal = interactionPrimitive.enableMaximalConstraint ?
                    new  Target { Value = interactionPrimitive.maximalConstraint } :
                    (Target?) null,
                Minimal = interactionPrimitive.enableMinimalConstraint ?
                    new Target { Value = interactionPrimitive.minimalConstraint } :
                    (Target?) null
            };

            Vector3 axis;
            Transform dependentTransform;
            switch (interactionPrimitive.interactionStrategy)
            {
                case InteractionStrategy.FreeMovement:
                    ObjectId = InteractionEngineApi.CreateFreeObject(spatialRepresentation,
                        interactionPrimitive.bodyPart, graspingStrengthThreshold, pinchingStrengthThreshold,
                        delayTimer);
                    break;
                case InteractionStrategy.FreePositionWithFixRotation:
                    ObjectId = InteractionEngineApi.CreateFreePositionWithFixRotationObject(
                        spatialRepresentation, interactionPrimitive.bodyPart, graspingStrengthThreshold,
                        pinchingStrengthThreshold, delayTimer);
                    break;
                case InteractionStrategy.FreePositionWithYRotation:
                    ObjectId = InteractionEngineApi.CreateFreePositionWithYRotationObject(
                        spatialRepresentation, interactionPrimitive.bodyPart, interactionPrimitive.ySpace,
                        graspingStrengthThreshold, pinchingStrengthThreshold, delayTimer);
                    break;
                case InteractionStrategy.Sliding:
                    dependentTransform = slidingTransform ? slidingTransform : transform;
                    switch (localSlidingAxis)
                    {
                        case Axis.X:
                            axis = dependentTransform.right.normalized;
                            break;
                        case Axis.Y:
                            axis = dependentTransform.up.normalized;
                            break;
                        case Axis.Z:
                            axis = dependentTransform.forward.normalized;
                            break;
                        default:
                            axis = Vector3.zero;
                            break;
                    }
                    if (computeOnLocalBasis && transform.parent != null)
                        axis = transform.parent.InverseTransformDirection(axis);
                    ObjectId = InteractionEngineApi.CreateSlidingObject(spatialRepresentation,
                        IbTools.Convert(axis), interactionPrimitive.bodyPart, constraints, targetList,
                        graspingStrengthThreshold, pinchingStrengthThreshold, delayTimer);
                    break;
                case InteractionStrategy.RotationAroundPivot:
                    dependentTransform = pivotTransform ? pivotTransform : transform;
                    Vector3 pos = dependentTransform.position;
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    Vector3 pivotPosition = (computeOnLocalBasis && transform.parent != null)
                        // ReSharper disable once Unity.InefficientPropertyAccess
                        ? transform.parent.InverseTransformPoint(pos)
                        : pos;
                    SpatialRepresentation pivotRepresentation = new SpatialRepresentation
                    {
                        Position = IbTools.Convert(pivotPosition),
                        Rotation = System.Numerics.Quaternion.Identity,
                    };
                    switch (localRotationAxis)
                    {
                        case Axis.X:
                            axis = dependentTransform.right.normalized;
                            break;
                        case Axis.Y:
                            axis = dependentTransform.up.normalized;
                            break;
                        case Axis.Z:
                            axis = dependentTransform.forward.normalized;
                            break;
                        default:
                            axis = Vector3.zero;
                            break;
                    }
                    if (computeOnLocalBasis && transform.parent != null)
                        axis = transform.parent.InverseTransformDirection(axis).normalized;
                    ObjectId = InteractionEngineApi.CreateRotationAroundPivotObject(spatialRepresentation,
                        IbTools.Convert(axis), pivotRepresentation, interactionPrimitive.bodyPart, constraints,
                        targetList, graspingStrengthThreshold, pinchingStrengthThreshold, delayTimer);
                    break;
                case InteractionStrategy.Button:
                    dependentTransform = slidingTransform ? slidingTransform : transform;
                    switch (localSlidingAxis)
                    {
                        case Axis.X:
                            axis = dependentTransform.right.normalized;
                            break;
                        case Axis.Y:
                            axis = dependentTransform.up.normalized;
                            break;
                        case Axis.Z:
                            axis = dependentTransform.forward.normalized;
                            break;
                        default:
                            axis = Vector3.zero;
                            break;
                    }
                    if (computeOnLocalBasis && transform.parent != null)
                        axis = transform.parent.InverseTransformDirection(axis);
                    ObjectId = InteractionEngineApi.CreateButtonObject(spatialRepresentation,
                        IbTools.Convert(axis), interactionPrimitive.bodyPart, constraints, targetList,
                        graspingStrengthThreshold, pinchingStrengthThreshold, delayTimer);
                    break;
                default:
                    ObjectId = InteractionEngineApi.CreateNoneObject(spatialRepresentation,
                        interactionPrimitive.bodyPart, graspingStrengthThreshold, pinchingStrengthThreshold,
                        delayTimer);
                    break;
            }

            PositionAtStart = position;

            //Call virtual
            this.OnStart();
        }

        private void LateUpdate()
        {
            SpatialRepresentation? spatialRepresentation = InteractionEngineApi.ComputeNewObjectRepresentation(ObjectId);

            if (spatialRepresentation.HasValue)
            {
                Transform t = transform;
                if (computeOnLocalBasis)
                {
                    t.localRotation = IbTools.Convert(spatialRepresentation.Value.Rotation);
                    t.localPosition = IbTools.Convert(spatialRepresentation.Value.Position);
                }
                else
                {
                    t.rotation = IbTools.Convert(spatialRepresentation.Value.Rotation);
                    t.position = IbTools.Convert(spatialRepresentation.Value.Position);
                }
            }
            if (IsInteracting)
            {
                if (activateConstraintsEvents)
                    CheckConstraints();

                if (activateNumericalTargetsEvents)
                    CheckNumericalTargets();
            }

            //Call virtual
            this.OnLateUpdate();
        }

        private void OnDisable()
        {
            if (IsInteracting)
                ForceFinishInteraction();
        }
        #endregion


        #region Collisions System
        private void OnTriggerEnter(Collider other)
        {
            CollisionProcess(other, target => target.onTargetEnterEvent);
        }

        private void OnTriggerStay(Collider other)
        {
            CollisionProcess(other, target => target.onTargetStayEvent);
        }

        private void OnTriggerExit(Collider other)
        {
            CollisionProcess(other, target => target.onTargetExitEvent);
        }

        private void OnCollisionEnter(Collision other)
        {
            CollisionProcess(other.collider, target => target.onTargetEnterEvent);
        }

        private void OnCollisionStay(Collision other)
        {
            CollisionProcess(other.collider, target => target.onTargetStayEvent);
        }

        private void OnCollisionExit(Collision other)
        {
            CollisionProcess(other.collider, target => target.onTargetExitEvent);
        }

        private void CollisionProcess(Collider otherCollider, System.Func<PhysicalTarget, UnityEvent> eventGetter)
        {
            if (!activatePhysicalTargetsEvents || otherCollider == null)
                return;

            physicalTargetList.ForEach(physicalTarget =>
            {
                bool invokeEvent;
                switch (physicalTarget.targetType)
                {
                    case PhysicalTarget.TargetType.TargetOnCollider:
                        invokeEvent = physicalTarget.targetCollider.Any(targetCollider =>
                            targetCollider != null && targetCollider.GetInstanceID() == otherCollider.GetInstanceID());
                        break;
                    case PhysicalTarget.TargetType.TargetOnTag:
                        invokeEvent = otherCollider.CompareTag(physicalTarget.targetTag);
                        break;
                    case PhysicalTarget.TargetType.TargetOnLayer:
                        invokeEvent = physicalTarget.targetLayer ==
                                      (physicalTarget.targetLayer | (1 << otherCollider.gameObject.layer));
                        break;
                    default:
                        return;
                }
                if (invokeEvent)
                    eventGetter(physicalTarget)?.Invoke();
            });
        }
        #endregion


        #region Public Methods
        /// <summary>
        ///     Change the object blocking state
        /// </summary>
        /// <param name="b">True to block interactions on the object, false otherwise</param>
        /// <returns>True if it was possible to block/unblock the object, false otherwise</returns>
        public bool ChangeBlockingState(bool b)
        {
            return InteractionEngineApi.ChangeObjectBlockingState(ObjectId, b);
        }

        /// <summary>
        ///     Block interactions on an object
        /// </summary>
        /// <returns>True if the interaction was blocked, false otherwise</returns>
        public bool BlockObject()
        {
            return ChangeBlockingState(true);
        }

        /// <summary>
        ///     Unblock interactions on an object
        /// </summary>
        /// <returns>True if the interaction was unblocked, false otherwise</returns>
        public bool UnblockObject()
        {
            return ChangeBlockingState(false);
        }

        /// <summary>
        ///     Try to block interactions on an object
        /// </summary>
        public void TryToBlockObject()
        {
            BlockObject();
        }

        /// <summary>
        ///     Try to unblock interactions on an object
        /// </summary>
        public void TryToUnblockObject()
        {
            UnblockObject();
        }

        /// <summary>
        ///     Begin an interaction with a body part strategy
        /// </summary>
        /// <param name="bodyPart">The body part strategy with which interact</param>
        /// <returns>True if it was possible to interact, false otherwise</returns>
        public bool StartInteraction(BodyPartInteractionStrategy bodyPart)
        {
            Transform t = transform;
            Vector3 position = computeOnLocalBasis ? t.localPosition : t.position;
            Quaternion rotation = computeOnLocalBasis ? t.localRotation : t.rotation;
            SpatialRepresentation spatialRepresentation = new SpatialRepresentation
            {
                Position = IbTools.Convert(position),
                Rotation = IbTools.Convert(rotation)
            };
            if (!InteractionEngineApi.StartInteraction(ObjectId, bodyPart, spatialRepresentation))
                return false;

            IsInteracting = true;
            InteractWith = bodyPart;
            if (setAsKinematicDuringInteraction && _rigidbody)
            {
                _kinematicBuffer = _rigidbody.isKinematic; 
                _rigidbody.isKinematic = true;
            }

            if (blockObjectOnInteractionStart)
                InteractionEngineApi.ChangeObjectBlockingState(ObjectId, true);
            if (blockBodyPartsOnInteractionStart)
                InteractionEngineApi.ChangeBodyPartBlockingState(bodyPart, true);

            if (!(interactionPrimitive is null) && interactionPrimitive.vibrationEvaluationMode ==
                InteractionPrimitive.VibrationEvaluationMode.SinceTheInteractionBeginning)
                PlayVibration();

            onInteractionStart.Invoke();
            return true;
        }

        /// <summary>
        ///     Finish an interaction with a body part strategy
        /// </summary>
        /// <param name="bodyPart">The body part strategy with which interact</param>
        /// <returns>True if it was possible to finish the interaction, false otherwise</returns>
        public bool FinishInteraction(BodyPartInteractionStrategy bodyPart)
        {
            if (!InteractionEngineApi.FinishInteraction(ObjectId, bodyPart))
                return false;
            if (setAsKinematicDuringInteraction && _rigidbody)
                _rigidbody.isKinematic = _kinematicBuffer;

            IsInteracting = false;
            InteractWith = BodyPartInteractionStrategy.None;
            if (!(interactionPrimitive is null) && interactionPrimitive.vibrationEvaluationMode ==
                InteractionPrimitive.VibrationEvaluationMode.SinceTheInteractionBeginning)
                StopVibration();

            _lastNumericalTarget = null;
            onInteractionFinish.Invoke();

            return true;
        }

        /// <summary>
        ///     Begin an interaction with a body part
        /// </summary>
        /// <param name="bodyPart">The body part with which interact</param>
        /// <returns>True if it was possible to interact, false otherwise</returns>
        public bool StartInteraction(BodyPart bodyPart)
        {
            return StartInteraction(IbTools.Convert(bodyPart));
        }

        /// <summary>
        ///     Finish an interaction with a body part
        /// </summary>
        /// <param name="bodyPart">The body part with which interact</param>
        /// <returns>True if it was possible to finish the interaction, false otherwise</returns>
        public bool FinishInteraction(BodyPart bodyPart)
        {
            return FinishInteraction(IbTools.Convert(bodyPart));
        }

        /// <summary>
        ///     Force to finish the actual interaction
        /// </summary>
        public void ForceFinishInteraction()
        {
            InteractionEngineApi.ChangeObjectBlockingState(ObjectId, false);
            InteractionEngineApi.ChangeBodyPartBlockingState(InteractWith, false);
            FinishInteraction(InteractWith);
        }

        /// <summary>
        ///     Force to start an interaction
        /// </summary>
        public void ForceStartInteraction()
        {
            InteractionEngineApi.ChangeObjectBlockingState(ObjectId, false);
            InteractionEngineApi.ChangeBodyPartBlockingState(InteractWith, false);
            StartInteraction(InteractWith);
        }

        /// <summary>
        ///     Move the object to a specific place
        /// </summary>
        /// <param name="snappingPosition">The specific global position</param>
        /// <param name="snappingRotation">The specific global rotation</param>
        public void SnapTo(Vector3 snappingPosition, Quaternion snappingRotation)
        {
            var t = transform;
            t.position = snappingPosition;
            t.rotation = snappingRotation;
        }

        /// <summary>
        ///     Move the object to a specific place
        /// </summary>
        /// <param name="snappingTransform">The specific transform to copy</param>
        public void SnapTo(Transform snappingTransform)
        {
            SnapTo(snappingTransform.position, snappingTransform.rotation);
        }

        /// <summary>
        ///     Move the object to a specific place
        /// </summary>
        /// <param name="snappingPosition">The specific global position</param>
        public void SnapTo(Vector3 snappingPosition)
        {
            SnapTo(snappingPosition, transform.rotation);
        }

        /// <summary>
        ///     Move the object to a specific place
        /// </summary>
        /// <param name="snappingRotation">The specific global rotation</param>
        public void SnapTo(Quaternion snappingRotation)
        {
            SnapTo(transform.position, snappingRotation);
        }

        /// <summary>
        ///     Evaluate the haptic amplitude of this object 
        /// </summary>
        public void EvaluateHapticAmplitude()
        {
            if (!InteractionPrimitive || !IsInteracting || !interactionPrimitive.hapticInteractionFeedback)
                return;

            Vector3 movement;
            switch (InteractionPrimitive.interactionStrategy)
            {
                case InteractionStrategy.RotationAroundPivot:
                {
                    movement = Vector3.one * (Mathf.Abs(InteractionEngineApi.GetDistance(ObjectId)) /
                                                InteractionPrimitive.degreeValueCorresponding);
                    break;
                }
                case InteractionStrategy.Sliding:
                case InteractionStrategy.Button:
                    movement = Vector3.one * Mathf.Abs(InteractionEngineApi.GetDistance(ObjectId));
                    break;
                default:
                    movement = Vector3.one * (transform.position - PositionAtStart).magnitude;
                    break;
            }

            try
            {
                if (InteractWith == BodyPartInteractionStrategy.TwoHands ||
                    InteractWith == BodyPartInteractionStrategy.TwoHandsWithHead)
                {
                    HapticRenderer.Core.HARWrapper.ComputeHaptics(HapticMaterialId, (int) HumanBodyBones.LeftHand,
                        movement, interactionPrimitive.renderTexture, interactionPrimitive.renderStiffness,
                        interactionPrimitive.renderVibration);
                    HapticRenderer.Core.HARWrapper.ComputeHaptics(HapticMaterialId, (int) HumanBodyBones.RightHand,
                        movement, interactionPrimitive.renderTexture, interactionPrimitive.renderStiffness,
                        interactionPrimitive.renderVibration);
                    if (InteractWith == BodyPartInteractionStrategy.TwoHandsWithHead)
                    {
                        HapticRenderer.Core.HARWrapper.ComputeHaptics(HapticMaterialId, (int) HumanBodyBones.Head,
                            movement, interactionPrimitive.renderTexture, interactionPrimitive.renderStiffness,
                            interactionPrimitive.renderVibration);
                    }
                }
                else if (InteractWith.ToHumanBodyBonesValue() !=
                         BodyPartInteractionStrategy.None.ToHumanBodyBonesValue())
                {
                    HapticRenderer.Core.HARWrapper.ComputeHaptics(HapticMaterialId,
                        InteractWith.ToHumanBodyBonesValue(), movement, interactionPrimitive.renderTexture,
                        interactionPrimitive.renderStiffness, interactionPrimitive.renderVibration);
                }
            }
            catch (System.DllNotFoundException)
            {
                Debug.LogError(ERROR_DLL_NOT_FOUND, gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString(), gameObject);
            }
        }

        /// <summary>
        ///     Get the distance made by the object fom its original position
        /// </summary>
        public float GetDistanceFromObjectOriginPoint()
        {
            return InteractionEngineApi.GetDistance(ObjectId);
        }

        /// <summary>
        ///     Set thresholds to this object
        /// </summary>
        /// <param name="thresholds">The new thresholds to use</param>
        /// <returns>True if value is set. False otherwise</returns>
        public bool SetThresholds(InteractionsEngine.Shared.DOs.InteractionThresholds thresholds)
        {
            if (!InteractionEngineApi.SetInteractionThresholds(ObjectId, thresholds))
                return false;

            graspingStrengthThreshold = thresholds.GraspingStrength;
            pinchingStrengthThreshold = thresholds.PinchingStrength;
            delayTimer = thresholds.Timer;
            return true;
        }

        /// <summary>
        ///     Try to set thresholds to this object
        /// </summary>
        /// <param name="thresholds">The new thresholds to use</param>
        public void TryToSetThresholds(InteractionsEngine.Shared.DOs.InteractionThresholds thresholds)
        {
            SetThresholds(thresholds);
        }

        /// <summary>
        ///     Set grabbing thresholds to this object
        /// </summary>
        /// <param name="grabbingThreshold">The new grabbing threshold to use</param>
        /// <returns>True if value is set. False otherwise</returns>
        public bool SetGrabbingThresholds(float grabbingThreshold)
        {
            if (!InteractionEngineApi.SetInteractionGrabbingThresholds(ObjectId, grabbingThreshold))
                return false;

            graspingStrengthThreshold = grabbingThreshold;
            return true;
        }

        /// <summary>
        ///     Try to set grabbing thresholds to this object
        /// </summary>
        /// <param name="grabbingThreshold">The new grabbing threshold to use</param>
        public void TryToSetGrabbingThresholds(float grabbingThreshold)
        {
            SetGrabbingThresholds(grabbingThreshold);
        }

        /// <summary>
        ///     Set pinching thresholds to this object
        /// </summary>
        /// <param name="pinchingThreshold">The new pinching threshold to use</param>
        /// <returns>True if value is set. False otherwise</returns>
        public bool SetPinchingThresholds(float pinchingThreshold)
        {
            if (!InteractionEngineApi.SetInteractionPinchingThresholds(ObjectId, pinchingThreshold))
                return false;

            pinchingStrengthThreshold = pinchingThreshold;
            return true;
        }

        /// <summary>
        ///     Try to set pinching thresholds to this object
        /// </summary>
        /// <param name="pinchingThreshold">The new pinching threshold to use</param>
        public void TryToSetPinchingThresholds(float pinchingThreshold)
        {
            SetPinchingThresholds(pinchingThreshold);
        }

        /// <summary>
        ///     Set delay timer to this object
        /// </summary>
        /// <param name="timer">The new delay timer to use</param>
        /// <returns>True if value is set. False otherwise</returns>
        public bool SetDelayTimer(float timer)
        {
            if (!InteractionEngineApi.SetInteractionDelayTimer(ObjectId, timer))
                return false;

            delayTimer = timer;
            return true;
        }

        /// <summary>
        ///     Try to set delay timer to this object
        /// </summary>
        /// <param name="timer">The new delay timer to use</param>
        public void TryToSetDelayTimer(float timer)
        {
            SetDelayTimer(timer);
        }

        /// <summary>
        ///     Play the vibration corresponding to this object interaction
        /// </summary>
        public void PlayVibration()
        {
            HapticRenderer.Core.HARWrapper.ResetVibration(HapticMaterialId);
            HapticRenderer.Core.HARWrapper.PlayVibration(HapticMaterialId);
        }

        /// <summary>
        ///     Stop the vibration corresponding to this object interaction
        /// </summary>
        public void StopVibration()
        {
            HapticRenderer.Core.HARWrapper.StopVibration(HapticMaterialId);
        }

        /// <summary>
        ///     Reset the vibration corresponding to this object interaction
        /// </summary>
        public void ResetVibration()
        {
            HapticRenderer.Core.HARWrapper.StopVibration(HapticMaterialId);
        }
        #endregion


        #region Private Methods
        private void CheckConstraints()
        {
            Constraints? constraints = InteractionEngineApi.GetObjectConstraints(ObjectId);
            if (!constraints.HasValue)
                return;

            if (constraints.Value.Maximal.HasValue &&
                _maximalConstraintReached != constraints.Value.Maximal.Value.IsReached)
            {
                _maximalConstraintReached = constraints.Value.Maximal.Value.IsReached;
                if (_maximalConstraintReached)
                {
                    onMaximalConstraintReached.Invoke();
                    return;
                }
            }

            if (constraints.Value.Minimal.HasValue &&
                _minimalConstraintReached != constraints.Value.Minimal.Value.IsReached)
            {
                _minimalConstraintReached = constraints.Value.Minimal.Value.IsReached;
                if (_minimalConstraintReached)
                    onMinimalConstraintReached.Invoke();
            }

            if (_maximalConstraintReached || _minimalConstraintReached)
            {
                _noConstraintsCalled = false;
                return;
            }
            
            if (_noConstraintsCalled)
                return;

            onNoConstraintsReached.Invoke();
            _noConstraintsCalled = true;
        }

        private void CheckNumericalTargets()
        {
            Target? target = InteractionEngineApi.GetLastReachedTarget(ObjectId);
            if (!target.HasValue)
            {
                if (!_lastNumericalTarget.HasValue)
                    return;
                _lastNumericalTarget.Value.onTargetNotReachedAnymoreEvent.Invoke();
                _lastNumericalTarget = null;
                onNoNumericalTargetReached.Invoke();
                return;
            }

            if (_lastNumericalTarget.HasValue &&
                Mathf.Abs(_lastNumericalTarget.Value.targetValue - target.Value.Value) <= IbTools.FLOATING_PRECISION)
                return;

            if (InteractionEngineApi.GetNotReachedTargets(ObjectId).Any(targetNotReached =>
                _lastNumericalTarget != null &&
                Mathf.Abs(_lastNumericalTarget.Value.targetValue - targetNotReached.Value) <=
                IbTools.FLOATING_PRECISION))
                _lastNumericalTarget?.onTargetNotReachedAnymoreEvent.Invoke();

            foreach (NumericalTarget numericalTarget in numericalTargetList)
            {
                if (Mathf.Abs(numericalTarget.targetValue - target.Value.Value) > IbTools.FLOATING_PRECISION)
                    continue;

                _lastNumericalTarget = numericalTarget;
                numericalTarget.onTargetReachedEvent?.Invoke();
                return;
            }
        }
        #endregion

        #region Protected
        protected virtual void OnStart() { }
        protected virtual void OnLateUpdate() { }
        #endregion

    }

}
