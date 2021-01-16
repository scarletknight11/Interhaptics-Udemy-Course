using Interhaptics.InteractionsEngine.Shared.Types;
using Interhaptics.InteractionsEngine;

using Interhaptics.Modules.Interaction_Builder.Core.Events;

using UnityEngine.Events;
using UnityEngine;

using System.Linq;


namespace Interhaptics.Modules.Interaction_Builder.Core.Abstract
{

    /// <summary>
    ///     A body part which will can interact with objects
    /// </summary>
    public abstract class AInteractionBodyPart : MonoBehaviour
    {

        #region Log Messages
        private const string ERROR_BODY_PART_NOT_CREATED =
            "<b>[InteractionBodyPart]</b> it's impossible to create your body part";
        #endregion


        #region Serialized Fields
        [Header("Interaction Configuration")]
        [SerializeField]
        [Tooltip("The body part")]
        public BodyPart bodyPart = BodyPart.RightHand;
        [SerializeField]
        [Tooltip("a custom transform to follow during interactions with this body part")]
        private Transform customPointToFollow = null;

        [SerializeField]
        [Tooltip("When an interaction start")]
        private BodyPartInteractionStartEvent onInteractionStart = new BodyPartInteractionStartEvent();
        [SerializeField]
        [Tooltip("When an interaction finish")]
        private BodyPartInteractionFinishEvent onInteractionFinish = new BodyPartInteractionFinishEvent();

        [Header("BodyPart Physics")]
        [SerializeField]
        [Tooltip("The radius of the cast sphere. It's used to know when an object will exit the body part action space")]
        private float castSphereRadius = .15f;
        [SerializeField]
        [Tooltip("The cast sphere local position")]
        private Vector3 castSphereCenter = Vector3.zero;

        #if UNITY_EDITOR
        [Header("Editor Integration")]
        [SerializeField]
        [Tooltip("True to draw your cast sphere in the Gizmos editor")]
        private bool drawCastSphereInEditor = true;
        [SerializeField]
        [Tooltip("Color of your drawn sphere")]
        private Color castSphereColor = new Color(56/256f, 147/256f, 192/256f);
        #endif
        #endregion


        #region Private Fields
        private InteractionTrigger _interaction = InteractionTrigger.None;
        private bool _isInInteraction = false;
        #endregion


        #region Protected Fields
        [Range(0, 1)] private float _grabbingStrength;
        [Range(0, 1)] private float _pinchingStrength;
        #endregion


        #region Public Getter
        /// <summary>
        ///     Events called when your body part change its wanted interaction
        /// </summary>
        /// <remarks>Don't empty this event, the InteractionBuilderManager is subscribed to it to trigger interactions</remarks>
        public readonly UnityEvent OnInteractionStateChanged = new UnityEvent();

        /// <summary>
        ///     Events called when your body part is updated
        /// </summary>
        public readonly UnityEvent OnDataUpdated = new UnityEvent();

        /// <summary>
        ///     Events called when your body part start an interaction
        /// </summary>
        public BodyPartInteractionStartEvent OnInteractionStartEvent => onInteractionStart;
        /// <summary>
        ///     Events called when your body part finish an interaction wanted interaction
        /// </summary>
        public BodyPartInteractionFinishEvent OnInteractionFinishEvent => onInteractionFinish;

        /// <summary>
        ///     The body part
        /// </summary>
        public BodyPart BodyPart => bodyPart;
        /// <summary>
        ///     The body part's cast sphere radius
        /// </summary>
        public float CastSphereRadius => castSphereRadius;
        /// <summary>
        ///     The body part cast sphere center
        /// </summary>
        public Vector3 CastSphereCenter => castSphereCenter;

        /// <summary>
        ///     The actual grabbing strength
        /// </summary>
        public float GrabStrength => _grabbingStrength;
        /// <summary>
        ///     The actual pinching strength
        /// </summary>
        public float PinchStrength => _pinchingStrength;

        /// <summary>
        ///     The object collided by the body part
        /// </summary>
        public InteractionObject InteractionObject { get; private set; }

        /// <summary>
        ///     The object ID collided by the body part
        /// </summary>
        public int InteractionObjectId => InteractionObject ? InteractionObject.ObjectId : InteractionsEngine.Shared.DOs.BodyPartDo.NoInteraction;

        /// <summary>
        ///     True if the body part interact, false otherwise
        /// </summary>
        public bool IsInInteraction
        {
            get => _isInInteraction;
            private set
            {
                bool temp = _isInInteraction;
                _isInInteraction = value;
                if (value != temp)
                    ((value ? (BodyPartInteractionEvent)OnInteractionStartEvent : OnInteractionFinishEvent)).Invoke(this, InteractionObject);
            }
        }

        /// <summary>
        ///     Which kind of interaction wanted by the body part
        /// </summary>
        public InteractionTrigger Interaction
        {
            get => _interaction;
            private set
            {
                bool test = value != Interaction;
                _interaction = value;
                if (test)
                    OnInteractionStateChanged.Invoke();
            }
        }
        /// <summary>
        ///     The body part transform used for for interaction
        /// </summary>
        public Transform BodyPartTransform => customPointToFollow ? customPointToFollow : transform;
        #endregion


        #region Life Cycles
        protected virtual void Start ()
        {
            try
            {
                InteractionEngineApi.CreateBodyPart(bodyPart);
            }
            catch (System.Exception)
            {
                Debug.LogError(ERROR_BODY_PART_NOT_CREATED);
                enabled = false;
            }
        }

        protected virtual void Update()
        {
            this.BodypartUpdate();
        }
        #endregion

#if UNITY_EDITOR
        #region Editor Life Cycles
        protected virtual void OnDrawGizmos()
        {
            if (!drawCastSphereInEditor)
                return;

            Gizmos.color = castSphereColor;
            Gizmos.DrawSphere(BodyPartTransform.TransformPoint(castSphereCenter), castSphereRadius);
        }

        protected virtual void OnValidate()
        {
            if (castSphereRadius < 0)
                castSphereRadius = 0;
        }
        #endregion
#endif


        #region Collisions
        private void OnTriggerEnter(Collider other)
        {
            ProcessCollisionEnter(other);
        }

        private void OnCollisionEnter(Collision other)
        {
            ProcessCollisionEnter(other.collider);
        }

        private void ProcessCollisionEnter(Collider other)
        {
            if (IsInInteraction || other == null ||
                (InteractionObject != null &&
                 !(Vector3.Distance(BodyPartTransform.position, InteractionObject.transform.position) >
                   Vector3.Distance(BodyPartTransform.position, other.transform.position))) ||
                Interaction != InteractionTrigger.None)
                return;

            InteractionObject o = other.gameObject.GetComponent<InteractionObject>();
            if (o && o.enabled && o.gameObject.activeInHierarchy)
                InteractionObject = o;
            else
                return;

            if (!InteractionObject || !InteractionObject.InteractionPrimitive ||
                InteractionObject.InteractionPrimitive.interactionTrigger != InteractionTrigger.Contact)
                return;

            UpdateSpatialRepresentation();
            Interaction = InteractionTrigger.Contact;
        }

        private void OnTriggerStay(Collider other)
        {
            ProcessCollisionStay(other);
        }

        private void OnCollisionStay(Collision other)
        {
            ProcessCollisionEnter(other.collider);
        }

        private void ProcessCollisionStay(Collider other)
        {
            if (IsInInteraction || other == null ||
                (InteractionObject != null &&
                 !(Vector3.Distance(BodyPartTransform.position, InteractionObject.transform.position) >
                   Vector3.Distance(BodyPartTransform.position, other.transform.position))) ||
                Interaction != InteractionTrigger.None)
                return;

            InteractionObject o = other.gameObject.GetComponent<InteractionObject>();
            if (o && o.enabled && o.gameObject.activeInHierarchy)
                InteractionObject = o;
        }

        private void OnTriggerExit(Collider other)
        {
            ProcessCollisionExit(other);
        }

        private void OnCollisionExit(Collision other)
        {
            ProcessCollisionExit(other.collider);
        }

        private void ProcessCollisionExit(Collider other)
        {
            if (IsInInteraction && other != null && InteractionObject != null &&
                InteractionObject.InteractionPrimitive != null &&
                InteractionObject.InteractionPrimitive.interactionTrigger == InteractionTrigger.Contact &&
                InteractionObject.gameObject.GetInstanceID() == other.gameObject.GetInstanceID() &&
                InteractionObject.FinishInteraction(bodyPart))
                Interaction = InteractionTrigger.None;
        }
        #endregion


        #region Private Methods
        private void UpdateSpatialRepresentation()
        {
            Vector3 position;
            Quaternion rotation;
            if (BodyPartTransform == null)
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
            }
            else
            {
                position = BodyPartTransform.position;
                rotation = BodyPartTransform.rotation;
            }

            if (InteractionObject && InteractionObject.ComputeOnLocalBasis && InteractionObject.transform.parent)
            {
                Transform t = InteractionObject.transform.parent;
                position = t.InverseTransformPoint(position);
                rotation = Quaternion.Inverse(t.rotation).normalized * rotation;
            }

            InteractionEngineApi.UpdateBodyPartRepresentation(bodyPart, new SpatialRepresentation
            {
                Position = IbTools.Convert(position),
                Rotation = IbTools.Convert(rotation)
            });
        }

        private void UpdateStrengthValues()
        {
            switch (bodyPart)
            {
                case BodyPart.RightHand:
                    _grabbingStrength = Mathf.Clamp01(UpdateRightGrabbingStrength());
                    _pinchingStrength = Mathf.Clamp01(UpdateRightPinchingStrength());
                    break;
                case BodyPart.LeftHand:
                    _grabbingStrength = Mathf.Clamp01(UpdateLeftGrabbingStrength());
                    _pinchingStrength = Mathf.Clamp01(UpdateLeftPinchingStrength());
                    break;
                default:
                    _grabbingStrength = 0;
                    _pinchingStrength = 0;
                    break;
            }
        }
        #endregion


        #region Abstract Methods
        protected abstract float UpdateRightGrabbingStrength();
        protected abstract float UpdateLeftGrabbingStrength();

        protected abstract float UpdateRightPinchingStrength();
        protected abstract float UpdateLeftPinchingStrength();
        #endregion

        #region Virtuals
        protected virtual void BodypartUpdate()
        {
            UpdateSpatialRepresentation();
            if (Interaction != InteractionTrigger.Contact)
            {
                UpdateStrengthValues();
                float grabStrength = GrabStrength;
                float pinchStrength = PinchStrength;
                if (InteractionObject && InteractionObject.InteractionPrimitive)
                {
                    if (InteractionObject.InteractionPrimitive.interactionTrigger != InteractionTrigger.Grasp &&
                        InteractionObject.InteractionPrimitive.interactionTrigger != InteractionTrigger.PinchOrGrasp)
                        grabStrength = -1;
                    if (InteractionObject.InteractionPrimitive.interactionTrigger != InteractionTrigger.Pinch &&
                        InteractionObject.InteractionPrimitive.interactionTrigger != InteractionTrigger.PinchOrGrasp)
                        pinchStrength = -1;
                }
                InteractionTrigger interaction =
                    InteractionEngineApi.UpdateInteractionStrengths(bodyPart, InteractionObjectId, grabStrength,
                        pinchStrength);
                if (InteractionObject && InteractionObject.InteractionPrimitive &&
                    InteractionObject.InteractionPrimitive.interactionTrigger == InteractionTrigger.PinchOrGrasp &&
                    interaction != InteractionTrigger.None)
                    Interaction = InteractionTrigger.PinchOrGrasp;
                else
                    Interaction = interaction;
            }


            if ((IsInInteraction = InteractionEngineApi.IsInteracting(bodyPart)) || !InteractionObject)
                return;

            // ReSharper disable once Unity.PreferNonAllocApi
            Collider[] hits = Physics.OverlapSphere(BodyPartTransform.TransformPoint(castSphereCenter), castSphereRadius);
            if (hits.Any(hit => hit.gameObject.GetInstanceID() == InteractionObject.gameObject.GetInstanceID()))
                return;

            if (Interaction == InteractionTrigger.Contact)
                Interaction = InteractionTrigger.None;
            InteractionObject = null;

            //Hook
            this.OnDataUpdated.Invoke();
        }
        #endregion

    }

}
