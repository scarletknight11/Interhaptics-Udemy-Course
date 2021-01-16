// ReSharper disable Unity.InefficientPropertyAccess
// ReSharper disable All
namespace Interhaptics.HandTracking
{

    /// <summary>
    ///     Hand Implementation. This component will consider the actual game object as a hand so it'll get the tracking value acquired and apply them
    /// </summary>
    [UnityEngine.HelpURL("https://www.interhaptics.com/resources/docs")]
    [UnityEngine.AddComponentMenu("Interhaptics/Hand Tracking/TrackedHand")]
    public class TrackedHand : UnityEngine.MonoBehaviour
    {

        #region PRIVATE CONSTS
        private const string GESTURE_NAME = "Gesture";
        #endregion


        #region PRIVATE STATIC READONLY
        private static readonly int GESTURE_ID = UnityEngine.Animator.StringToHash(GESTURE_NAME);
        #endregion


        #region SERIALIZED FIELDS
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("true to set the hand as left, false to set it as right ")]
        private bool isLeft = true;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Animator to be used for hand gesture")]
        private UnityEngine.Animator myHandAnimator = null;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Hand reference - Transform to be influenced by gesture data")]
        private UnityEngine.GameObject myHandObject = null;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Hand's MeshRenderer")]
        private UnityEngine.Renderer myRenderer = null;
        #endregion


        #region PUBLIC MEMBERS
        /// <summary>
        /// When hand is updated
        /// </summary>
        public readonly UnityEngine.Events.UnityEvent OnHandComputed = new UnityEngine.Events.UnityEvent();

        /// <summary>
        ///     If true the actual hand is left. Otherwise it's the right one
        /// </summary>
        public bool IsLeft => isLeft;
        /// <summary>
        ///     Animator used to display gestures
        /// </summary>
        public UnityEngine.Animator HandAnimator => myHandAnimator;
        /// <summary>
        ///     The hand's mesh used
        /// </summary>
        public UnityEngine.Renderer HandRenderer => myRenderer;
        /// <summary>
        ///     The rigged hand used
        /// </summary>
        public UnityEngine.GameObject HandObject => myHandObject == null ? gameObject : myHandObject;

        /// <summary>
        ///     The actual gesture of the tracked hand
        /// </summary>
        public Types.EGestureType Gesture { get; private set; } = Types.EGestureType.Fist;
        /// <summary>
        ///     The actual grabbing strength of the tracked hand. This value will always between 0 (when the user will open his hand) and 1 (when the hand will be in fist gesture)
        /// </summary>
        /// <remarks>This value can be considered as the probability of wishing to grab</remarks>
        public float GrabStrength { get; private set; } = 0;
        /// <summary>
        ///     The actual pinching strength of the tracked hand. This value will always between 0 (when the user will open his hand) and 1 (when the hand will be in OK gesture)
        /// </summary>
        /// <remarks>This value can be considered as the probability of wishing to pinch</remarks>
        public float PinchStrength { get; private set; } = 0;
        /// <summary>
        /// Let the TrackedHand refreshed is own position and rotation
        /// </summary>
        public bool RefreshSpatialRepresentation { get; set; } = true;
        /// <summary>
        /// Let the TrackedHand refreshed is own pos
        /// </summary>
        public bool RefreshPose { get; set; } = true;

        /// <summary>
        ///     If this hand should use its mesh renderer or not
        /// </summary>
        public bool UseMeshRenderer
        {
            get => Tools.HandTrackingSettings.Instance.useMeshRenderer && _useMeshRenderer;
            set => _useMeshRenderer = value;
        }
        #endregion


        #region PRIVATE FIELDS
        private bool _useMeshRenderer = true;
        #endregion


        #region LIFE CYCLES
        private void Start()
        {
            //Initialize skeleton positions
            if (HandTrackingManager.Instance != null)
            {
                if (isLeft && HandTrackingManager.Instance.LeftHand != null)
                    HandTrackingManager.Instance.LeftHand.InitSkeleton(HandObject);
                else if (!isLeft && HandTrackingManager.Instance.RightHand != null)
                    HandTrackingManager.Instance.RightHand.InitSkeleton(HandObject);
            }

            HandTrackingManager.OnTrackingReceived.AddListener(UpdateTrackingData);
            if (Tools.HandTrackingSettings.Instance.disableHandsWhenNotTracked)
                HandObject.SetActive(false);
            if (HandRenderer)
                HandRenderer.enabled = UseMeshRenderer;

            OnStart();
        }

        private void OnDestroy()
        {
            HandTrackingManager.OnTrackingReceived.RemoveListener(UpdateTrackingData);
            OnOnDestroy();
        }
        #endregion


        #region PRIVATE METHODS
        private void UpdateTrackingData(Hand handData)
        {
            if (handData == null || handData.IsLeft != isLeft)
                return;
        
            bool useGesture = handData.UseGestures;
            if (Tools.HandTrackingSettings.Instance.disableHandsWhenNotTracked)
                HandObject.SetActive(handData.IsActive);
            else if (!HandObject.activeSelf)
                HandObject.SetActive(true);

            if (HandRenderer && HandRenderer.enabled != UseMeshRenderer)
                HandRenderer.enabled = UseMeshRenderer;

            if (!handData.IsActive)
                return;

            UnityEngine.Transform t = transform;
            PinchStrength = handData.PinchStrength;
            GrabStrength = handData.GrabStrength;

            //Refresh Spatial Representation
            if (RefreshSpatialRepresentation)
            {
                t.localPosition = handData.Position;
                t.localRotation = handData.Rotation;
            }

            //RefreshPos
            if (RefreshPose)
            {
                Gesture = handData.Gesture;

                HandAnimator.enabled = useGesture;
                if (!useGesture)
                    handData.SetHandSkeleton(HandObject);
                else if (HandAnimator)
                    HandAnimator.SetInteger(GESTURE_ID, (int) Gesture);
            }
            else if (HandAnimator.enabled)
                HandAnimator.enabled = false;

            //End of the hand life cycle
            OnHandComputed.Invoke();
        }
        #endregion


        #region VIRTUAL METHODS
        /// <summary>
        ///     Method to implement if a child want to have access to the Start life cycle
        /// </summary>
        protected virtual void OnStart() {}
        /// <summary>
        ///     Method to implement if a child want to have access to the OnDestroy life cycle
        /// </summary>
        protected virtual void OnOnDestroy() {}
        #endregion

    }

}