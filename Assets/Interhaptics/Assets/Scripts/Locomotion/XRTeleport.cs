// ReSharper disable RedundantNameQualifier
// ReSharper disable MemberCanBeProtected.Global
using static System.Linq.Enumerable;


namespace Interhaptics.Assets.Locomotion
{

    // ReSharper disable once InconsistentNaming
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class XRTeleport : UnityEngine.MonoBehaviour
    {

        #region NESTED TYPES
        public enum ELocomotionType
        {
            // ReSharper disable once UnusedMember.Global
            Custom = -1,
            Teleport = 0,
            Dash = 1
        }
        #endregion


        #region CONSTS
        private const float MS_2_S = .001f;
        #endregion


        #region SERIALIZED FIELDS
        [UnityEngine.Header("Teleportation Settings")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Type of locomotion to use")]
        protected ELocomotionType locomotionType = ELocomotionType.Teleport;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Max ground tilt accepted for the teleportation. 0 means a strict flat ground and 180 means no constraints")]
        [UnityEngine.Range(0, 180)]
        protected float maxAngleToTeleport = 30;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Material used when it's possible to teleport")]
        protected UnityEngine.Material possibleToTeleportMaterial = null;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Material used when it's impossible to teleport")]
        protected UnityEngine.Material impossibleToTeleportMaterial = null;

        [UnityEngine.Header("Selection Settings")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("List of pointer which can be used to select the teleport place")]
        protected System.Collections.Generic.List<Interhaptics.Assets.Pointers.Pointer> pointersList =
            new System.Collections.Generic.List<Interhaptics.Assets.Pointers.Pointer>();

        [UnityEngine.Header("Dash Settings")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("The dashing time in millisecond to reach the target")]
        private float dashingTime = 150f;
        #endregion


        #region PUBLIC MEMBERS
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public ELocomotionType LocomotionType
        {
            get => locomotionType;
            set => locomotionType = value;
        }
        // ReSharper disable once UnusedMember.Global
        public float MaxAngleToTeleport
        {
            get => maxAngleToTeleport;
            set => maxAngleToTeleport = UnityEngine.Mathf.Clamp(value, 0f, 180f);
        }
        // ReSharper disable once UnusedMember.Global
        public float DashingTime
        {
            get => dashingTime;
            set => dashingTime = value < 0 ? 0 : value;
        }
        #endregion


        #region PROTECTED FIELDS
        // ReSharper disable once MemberCanBePrivate.Global
        protected bool isBusy = false;
        #endregion


        #region PRIVATE FIELDS
        private System.Collections.IEnumerator _dashingCoroutine = null;
        #endregion


        #region LIFE CYCLES
        protected virtual void OnEnable()
        {
            foreach (Interhaptics.Assets.Pointers.Pointer pointer in pointersList)
            {
                pointer.OnSelection.AddListener(pose =>
                {
                    if (pose.isValid)
                        Go(pose.position, pose.rotation);
                });
            }
        }

        protected virtual void OnDisable()
        {
            foreach (Interhaptics.Assets.Pointers.Pointer pointer in pointersList)
            {
                pointer.OnSelection.RemoveListener(pose =>
                {
                    if (pose.isValid)
                        Go(pose.position, pose.rotation);
                });
            }
        }

        protected virtual void Update()
        {
            bool MatchingPredicate(Interhaptics.Assets.Pointers.Pointer p) =>
                p.isActiveAndEnabled && p.IsActivated && p.targetedPose.isValid;

            if (pointersList == null || !pointersList.Any(MatchingPredicate))
                return;

            Interhaptics.Assets.Pointers.Pointer pointer = pointersList.Find(MatchingPredicate);
            UnityEngine.Material materialToApply =
                UnityEngine.Vector3.Angle(pointer.targetedPose.rotation * UnityEngine.Vector3.up,
                    UnityEngine.Vector3.up) <= maxAngleToTeleport
                    ? possibleToTeleportMaterial
                    : impossibleToTeleportMaterial;

            UnityEngine.Renderer[] renderers;
            if (materialToApply && pointer.GetRenderPoint() && pointer.VisualisationInstance &&
                (renderers = pointer.VisualisationInstance.GetComponentsInChildren<UnityEngine.Renderer>()) != null)
                foreach (UnityEngine.Renderer r in renderers)
                    if (r != pointer.VisualisationLine)
                        r.material = materialToApply;

            if (materialToApply && pointer.GetRenderLine() && pointer.VisualisationLine)
                pointer.VisualisationLine.material = materialToApply;
        }

        protected virtual void OnDestroy()
        {
            if (_dashingCoroutine != null)
                StopCoroutine(_dashingCoroutine);
        }
        #endregion


        #region PUBLIC VIRTUAL TRY TO GO METHODS
        public virtual bool TryToGo(UnityEngine.Vector3 desiredPosition)
        {
            if(!isActiveAndEnabled || isBusy)
                return false;

            UnityEngine.Transform rigTransform = Interhaptics.HandTracking.HandTrackingManager.Instance.transform;
            UnityEngine.Transform cameraTransform = Interhaptics.HandTracking.HandTrackingManager.Instance.MainCamera;
            UnityEngine.Vector3 desiredHeadPosition =
                desiredPosition + UnityEngine.Vector3.up * cameraTransform.localPosition.y;
            UnityEngine.Vector3 targetedPosition = desiredHeadPosition - (cameraTransform.position - rigTransform.position);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (LocomotionType)
            {
                case ELocomotionType.Teleport:
                    TeleportTo(rigTransform, targetedPosition);
                    break;
                case ELocomotionType.Dash:
                    DashTo(rigTransform, targetedPosition);
                    break;
                default:
                    CustomLocomotionTo(rigTransform, targetedPosition);
                    break;
            }

            return true;
        }

        public virtual bool TryToGo(UnityEngine.Vector3 goToPosition, UnityEngine.Quaternion goToRotation)
        {
            return UnityEngine.Vector3.Angle(goToRotation * UnityEngine.Vector3.up, UnityEngine.Vector3.up) <=
                maxAngleToTeleport && TryToGo(goToPosition);
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public virtual bool TryToGo(UnityEngine.Transform goTo)
        {
            return TryToGo(goTo.position, goTo.rotation);
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public virtual bool TryToGo()
        {
            bool MatchingPredicate(Interhaptics.Assets.Pointers.Pointer p) => p.IsActivated;
            if (pointersList == null || !pointersList.Any(MatchingPredicate))
                return false;

            Interhaptics.Assets.Pointers.Pointer pointer = pointersList.First(MatchingPredicate);
            return TryToGo(pointer.targetedPose.position, pointer.targetedPose.rotation);
        }
        #endregion


        #region PUBLIC GO METHODS
        // ReSharper disable once UnusedMember.Global
        public void Go(UnityEngine.Vector3 desiredPosition)
        {
            TryToGo(desiredPosition);
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public void Go(UnityEngine.Vector3 goToPosition, UnityEngine.Quaternion goToRotation)
        {
            TryToGo(goToPosition, goToRotation);
        }
        
        // ReSharper disable once UnusedMember.Global
        public void Go(UnityEngine.Transform goTo)
        {
            TryToGo(goTo);
        }

        // ReSharper disable once UnusedMember.Global
        public void Go()
        {
            TryToGo();
        }
        #endregion


        #region LOCOMOTION METHODS
        // ReSharper disable once MemberCanBePrivate.Global
        public void TeleportTo(UnityEngine.Transform rigTransform, UnityEngine.Vector3 targetedPosition)
        {
            isBusy = true;
            rigTransform.position = targetedPosition;
            isBusy = false;
        }

        private void DashTo(UnityEngine.Transform rigTransform, UnityEngine.Vector3 targetedPosition)
        {
            _dashingCoroutine = DashingCoroutine(rigTransform, targetedPosition);
            StartCoroutine(_dashingCoroutine);
        }

        public virtual void CustomLocomotionTo(UnityEngine.Transform rigTransform, UnityEngine.Vector3 targetedPosition)
        {
            TeleportTo(rigTransform, targetedPosition);
        }
        #endregion


        #region COROUTINE METHODS
        private System.Collections.IEnumerator DashingCoroutine(UnityEngine.Transform rigTransform,
            UnityEngine.Vector3 targetedPosition)
        {
            isBusy = true;

            UnityEngine.Vector3 startingPosition = rigTransform.position;
            float remainingTime = dashingTime * MS_2_S;
            yield return new UnityEngine.WaitForFixedUpdate();
            while (UnityEngine.Time.fixedDeltaTime < remainingTime)
            {
                rigTransform.position += (targetedPosition - startingPosition) /
                                         (dashingTime * MS_2_S / UnityEngine.Time.fixedDeltaTime);
                remainingTime -= UnityEngine.Time.fixedDeltaTime;
                yield return new UnityEngine.WaitForFixedUpdate();
            }

            rigTransform.position = targetedPosition;
            _dashingCoroutine = null;
            isBusy = false;
        }
        #endregion


        #region PUBLIC METHODS
        public void ChangeToTeleportLocomotionType()
        {
            LocomotionType = ELocomotionType.Teleport;
        }

        public void ChangeToDashLocomotionType()
        {
            LocomotionType = ELocomotionType.Dash;
        }

        // ReSharper disable once UnusedMember.Global
        public void ChangeToCustomLocomotionType()
        {
            LocomotionType = ELocomotionType.Custom;
        }
        #endregion

    }

}
