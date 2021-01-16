namespace Interhaptics.Assets.Pointers
{

    public abstract class FloatableValuePointer : Pointer
    {

        #region SERIALIZED FIELDS
        [UnityEngine.Header("FloatableValuePointer Settings")]
        [UnityEngine.SerializeField]
        [UnityEngine.Range(0, 1)]
        [UnityEngine.Tooltip("Confidence interval for this value. The selection will be thrown when the floating value will be higher than its threshold. But the selection will be finished when this value will be lower than this percentage of the same threshold")]
        private float confidenceInterval = .95f;
        #endregion


        #region PUBLIC MEMBERS
        // ReSharper disable once UnusedMember.Global
        public float ConfidenceInterval
        {
            get => confidenceInterval;
            set => confidenceInterval = UnityEngine.Mathf.Clamp01(value);
        }
        #endregion


        #region ABSTRACT METHODS
        // ReSharper disable once MemberCanBeProtected.Global
        public abstract float GetSelectionValue();
        // ReSharper disable once MemberCanBeProtected.Global
        public abstract float GetSelectionThreshold();
        #endregion


        #region PUBLIC VIRTUAL METHODS
        // ReSharper disable once MemberCanBeProtected.Global
        public virtual bool GetSelectionFromFloat()
        {
            return IsSelecting
                ? GetSelectionValue() >= confidenceInterval * GetSelectionThreshold()
                : GetSelectionValue() >= GetSelectionThreshold();
        }
        #endregion


        #region OVERRIDED METHODS
        protected override void Update()
        {
            base.Update();
            IsSelecting = GetSelectionFromFloat();
        }
        #endregion

    }

}