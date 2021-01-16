namespace Interhaptics.Assets.UI
{

    public sealed class EnableOnFacing : UnityEngine.MonoBehaviour
    {

        #region SERIALIZED FIELDS
        [UnityEngine.Header("Settings")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Max angle between the camera forward and the object up vectors to enable child object")]
        [UnityEngine.Range(0, 180)]
        private float maxAngle = 30;

        [UnityEngine.Header("Events")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Event called when this object is facing the camera")]
        private UnityEngine.Events.UnityEvent onFacingEnter = new UnityEngine.Events.UnityEvent();
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Event called when this object is not facing the camera anymore")]
        private UnityEngine.Events.UnityEvent onFacingExit = new UnityEngine.Events.UnityEvent();
        #endregion


        #region PUBLIC MEMBERS
        public float MaxAngle
        {
            get => maxAngle;
            set => maxAngle = UnityEngine.Mathf.Clamp(value, 0, 180);
        }

        public UnityEngine.Events.UnityEvent OnFacingEnter => onFacingEnter;
        public UnityEngine.Events.UnityEvent OnFacingExit => onFacingExit;
        #endregion


        #region PRIVATE FIELDS
        private bool _lastChecking = true;
        #endregion


        #region LIFE CYCLES
        private void Update()
        {
            UnityEngine.Transform t = transform;
            // ReSharper disable once RedundantNameQualifier 
            bool needEnabling = UnityEngine.Vector3.Angle(t.up,
                -Interhaptics.HandTracking.HandTrackingManager.Instance.MainCamera.transform
                    .forward) < maxAngle;

            if (needEnabling == _lastChecking)
                return;

            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(needEnabling);

            _lastChecking = needEnabling;
            (needEnabling ? onFacingEnter : onFacingExit).Invoke();
        }
        #endregion

    }

}