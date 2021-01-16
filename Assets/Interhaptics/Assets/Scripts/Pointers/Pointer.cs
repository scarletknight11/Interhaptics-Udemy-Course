using static System.Linq.Enumerable;


namespace Interhaptics.Assets.Pointers
{

    public class Pointer : UnityEngine.MonoBehaviour
    {

        #region NESTED TYPES
        public enum EInterpolationType
        {
            // ReSharper disable once UnusedMember.Global
            None = -1,
            Linear = 0,
            Exponential = 1,
            Sine = 2,
            Custom = 3
        }

        public enum ERayType
        {
            Custom = -1,
            // ReSharper disable once UnusedMember.Global
            Linear = 0,
            Curve = 1
        }

        public sealed class PointedPose
        {
            // ReSharper disable once NotAccessedField.Global
            public bool isValid;
            // ReSharper disable once NotAccessedField.Global
            public UnityEngine.Vector3 position;
            // ReSharper disable once NotAccessedField.Global
            public UnityEngine.Quaternion rotation;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        [System.Serializable]
        public sealed class SelectionEvent : UnityEngine.Events.UnityEvent<PointedPose> {}
        #endregion


        #region CONSTS VALUE
        private const int MIN_DISTANCE_POSSIBLE = 0;
        private const int MAX_DISTANCE_POSSIBLE = 500;
        #endregion


        #region SERIALIZED FIELDS
        [UnityEngine.Header("Pointing Settings")]
        [UnityEngine.Tooltip("GameObject used to visualize the pointing curve")]
        [UnityEngine.SerializeField]
        private UnityEngine.GameObject visualisationObject = null;
        [UnityEngine.Tooltip("A position offset applied to start the Ray")]
        [UnityEngine.SerializeField]
        private UnityEngine.Vector3 startingOffset = UnityEngine.Vector3.zero;
        [UnityEngine.Tooltip("Ray type used for the pointing")]
        [UnityEngine.SerializeField]
        private ERayType rayType = ERayType.Curve;

        [UnityEngine.Header("Selection Settings")]
        [UnityEngine.Tooltip("Event called when is in selection")]
        [UnityEngine.SerializeField]
        private SelectionEvent onSelection = new SelectionEvent();

        [UnityEngine.Header("Curved Settings")]
        [UnityEngine.Tooltip("The gravity direction")]
        [UnityEngine.SerializeField]
        private UnityEngine.Vector3 gravityDirection = UnityEngine.Vector3.down;
        [UnityEngine.Tooltip("Gravity strength, in the algorithm the gravityDirection is normalized and multiplied by this fields to be applied as a strength on the curve")]
        [UnityEngine.SerializeField]
        private float gravityMultiplier = 2;

        [UnityEngine.Header("Physic Settings")]
        [UnityEngine.Tooltip("Every layer which can be seen by the ray")]
        [UnityEngine.SerializeField]
        private UnityEngine.LayerMask layerMask = ~0;
        [UnityEngine.Tooltip("Max distance in which the ray will be stopped")]
        [UnityEngine.SerializeField]
        [UnityEngine.Range(MIN_DISTANCE_POSSIBLE, MAX_DISTANCE_POSSIBLE)]
        private int maxDistance = 100;
        [UnityEngine.SerializeField]
        private int smoothness = 1;

        [UnityEngine.Header("Interpolation Settings")]
        [UnityEngine.Tooltip("Interpolation type used to smooth the pointing behaviour")]
        [UnityEngine.SerializeField]
        private EInterpolationType interpolationType = EInterpolationType.Linear;
        [UnityEngine.Tooltip("Curved used for a custom interpolation type")]
        [UnityEngine.SerializeField]
        private UnityEngine.AnimationCurve interpolationCurve = new UnityEngine.AnimationCurve();
        #endregion


        #region PUBLIC MEMBERS
        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public bool IsActivated
        {
            get => isActiveAndEnabled;
            // ReSharper disable once UnusedMember.Global
            set => enabled = value;
        }
        // ReSharper disable once UnusedMember.Global
        public int MaxDistance
        {
            get => maxDistance;
            set => maxDistance = UnityEngine.Mathf.Clamp(value, MIN_DISTANCE_POSSIBLE, MAX_DISTANCE_POSSIBLE);
        }
        // ReSharper disable once UnusedMember.Global
        public int Smoothness
        {
            get => smoothness;
            set => smoothness = value <= 0 ? 0 : value;
        }
        // ReSharper disable once UnusedMember.Global
        public EInterpolationType InterpolationType
        {
            get => interpolationType;
            set => interpolationType = value;
        }
        // ReSharper disable once UnusedMember.Global
        public ERayType RayType
        {
            get => rayType;
            set => rayType = value;
        }
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsSelecting
        {
            get => _isSelecting;
            set
            {
                if (!IsActivated)
                    return;

                bool tmp = _isSelecting;
                _isSelecting = value;
                if (value && !tmp)
                    OnSelection.Invoke(targetedPose);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public UnityEngine.GameObject VisualisationInstance => _visualisationInstance;
        // ReSharper disable once UnusedMember.Global
        public UnityEngine.LineRenderer VisualisationLine => _visualisationLine;
        // ReSharper disable once UnusedMember.Global
        public float GravityMultiplier => gravityMultiplier;
        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public UnityEngine.LayerMask LayerMask => layerMask;
        // ReSharper disable once UnusedMember.Global
        public UnityEngine.AnimationCurve InterpolationCurve => interpolationCurve;
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public SelectionEvent OnSelection => onSelection;
        #endregion


        #region PUBLIC FIELDS
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public readonly PointedPose targetedPose = new PointedPose
        {
            isValid = false
        };
        #endregion


        #region PROTECTED FIELDS
        // ReSharper disable once MemberCanBePrivate.Global
        protected System.Collections.Generic.List<UnityEngine.Vector3> previousPoints =
            new System.Collections.Generic.List<UnityEngine.Vector3>();
        #endregion


        #region PRIVATE FIELDS
        private bool _isSelecting = false;
        private UnityEngine.GameObject _visualisationInstance = null;
        private UnityEngine.LineRenderer _visualisationLine = null;
        #endregion


        #region LIFE CYCLES
        protected virtual void OnEnable()
        {
            if (GetRenderPoint() && _visualisationInstance)
                _visualisationInstance.SetActive(true);
            if (GetRenderLine() && _visualisationLine)
                _visualisationLine.enabled = true;
        }

        protected virtual void OnDisable()
        {
            if (_visualisationInstance)
                _visualisationInstance.SetActive(false);
            if (_visualisationLine)
                _visualisationLine.enabled = false;
        }

        protected virtual void Awake()
        {
            if (!visualisationObject)
                return;

            _visualisationInstance = visualisationObject.gameObject.scene.name == null
                ? Instantiate(visualisationObject)
                : visualisationObject;
            if ((_visualisationLine = _visualisationInstance.GetComponentInChildren<UnityEngine.LineRenderer>(includeInactive: true)) is null)
                _visualisationLine = _visualisationInstance.AddComponent<UnityEngine.LineRenderer>();
        }

        protected virtual void Update()
        {
            // ReSharper disable once Unity.InefficientPropertyAccess
            UnityEngine.Vector3 rayOrigin = GetOrigin() + transform.TransformPoint(startingOffset) - transform.position;
            if (!IsActivated || !Cast(rayOrigin
                , GetPointingDirection(rayOrigin),
                smoothness, maxDistance, out UnityEngine.RaycastHit hitInfo,
                out System.Collections.Generic.List<UnityEngine.Vector3> points))
            {
                targetedPose.isValid = false;
                if (_visualisationInstance)
                    _visualisationInstance.SetActive(false);
                if (_visualisationLine)
                    _visualisationLine.enabled = false;
            }
            else
            {
                Interpolate(points);
                targetedPose.isValid = true;
                targetedPose.position = new UnityEngine.Vector3(previousPoints[previousPoints.Count - 1].x, hitInfo.point.y,
                    previousPoints[previousPoints.Count - 1].z);
                targetedPose.rotation = UnityEngine.Quaternion.FromToRotation(UnityEngine.Vector3.up, hitInfo.normal);

                if (GetRenderLine() && _visualisationLine)
                {
                    _visualisationLine.enabled = true;
                    _visualisationLine.positionCount = previousPoints.Count;
                    _visualisationLine.SetPositions(previousPoints.ToArray());
                }

                if (GetRenderPoint() && _visualisationInstance)
                {
                    _visualisationInstance.SetActive(true);
                    _visualisationInstance.transform.position = targetedPose.position;
                    _visualisationInstance.transform.rotation = targetedPose.rotation;
                }
            }
        }
        #endregion


        #region CAST METHODS
        // ReSharper disable once MemberCanBePrivate.Global
        public bool Cast(UnityEngine.Vector3 origin, UnityEngine.Vector3 direction, int smooth,
            float maxDist, out UnityEngine.RaycastHit hitInfo,
            out System.Collections.Generic.List<UnityEngine.Vector3> points)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (rayType)
            {
                case ERayType.Custom:
                    return CustomCast(origin, direction, smooth, maxDist, out hitInfo, out points);
                case ERayType.Curve:
                    return CurvedCast(origin, direction, smooth, maxDist, out hitInfo, out points);
                default:
                    return LinearCast(origin, direction, smooth, maxDist, out hitInfo, out points);
            }
        }

        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        public virtual bool CustomCast(UnityEngine.Vector3 origin, UnityEngine.Vector3 direction, int smooth,
            float maxDist, out UnityEngine.RaycastHit hitInfo,
            out System.Collections.Generic.List<UnityEngine.Vector3> points)
        {
            return LinearCast(origin, direction, smooth, maxDist, out hitInfo, out points);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool LinearCast(UnityEngine.Vector3 origin, UnityEngine.Vector3 direction, int smooth,
            float maxDist, out UnityEngine.RaycastHit hitInfo,
            out System.Collections.Generic.List<UnityEngine.Vector3> points)
        {
            if (float.IsPositiveInfinity(maxDist) || maxDist < 0)
                maxDist = 500;

            if (smooth <= 0)
                smooth = 1;

            UnityEngine.Vector3 currPos, hypoPos = origin, hypoVel = direction.normalized / smooth;
            System.Collections.Generic.List<UnityEngine.Vector3> v =
                new System.Collections.Generic.List<UnityEngine.Vector3>();
            UnityEngine.RaycastHit hit;
            float castLength = 0;

            do
            {
                v.Add(hypoPos);
                currPos = hypoPos;
                hypoPos = currPos + hypoVel;
                hypoVel = hypoPos - currPos;
                castLength += hypoVel.magnitude;
            } while (UnityEngine.Physics.Raycast(currPos, hypoVel, out hit, hypoVel.magnitude,
                layerMask.value) == false && castLength < maxDist);

            hitInfo = hit;
            points = v;

            return UnityEngine.Physics.Raycast(currPos, hypoVel, hypoVel.magnitude, layerMask: layerMask.value);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool CurvedCast(UnityEngine.Vector3 origin, UnityEngine.Vector3 direction, int smooth,
            float maxDist, out UnityEngine.RaycastHit hitInfo,
            out System.Collections.Generic.List<UnityEngine.Vector3> points)
        {
            if (float.IsPositiveInfinity(maxDist))
                maxDist = 500;

            if (smooth == 0)
                smooth = 1;

            UnityEngine.Vector3 currPos,
                hypoPos = origin,
                hypoVel = direction.normalized / smooth,
                gravityDir = gravityDirection.normalized * gravityMultiplier;
            System.Collections.Generic.List<UnityEngine.Vector3> v =
                new System.Collections.Generic.List<UnityEngine.Vector3>();
            UnityEngine.RaycastHit hit;
            float curveCastLength = 0;

            do
            {
                v.Add(hypoPos);
                currPos = hypoPos;
                hypoPos = currPos + hypoVel + (gravityDir * UnityEngine.Time.fixedDeltaTime / (smooth * smooth));
                hypoVel = hypoPos - currPos;
                curveCastLength += hypoVel.magnitude;
            } while (!UnityEngine.Physics.Raycast(currPos, hypoVel, out hit, hypoVel.magnitude, LayerMask.value) &&
                     curveCastLength < maxDist);

            hitInfo = hit;
            points = v;

            return UnityEngine.Physics.Raycast(currPos, hypoVel, hypoVel.magnitude, LayerMask.value);
        }
        #endregion


        #region INTERPOLATION METHODS
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedMethodReturnValue.Global
        protected System.Collections.Generic.List<UnityEngine.Vector3> Interpolate(
            System.Collections.Generic.List<UnityEngine.Vector3> points)
        {
            if (previousPoints.Count == 0)
                previousPoints = points.Select(v =>
                    new UnityEngine.Vector3(v.x,
                        v.y + UnityEngine.Mathf.InverseLerp(0, points.Count, points.IndexOf(v)),
                        v.z)).ToList();
            float lengthI = points.Count;
            for (int i = 0; i < lengthI; i++)
            {
                float t;
                float interpolVal = i / lengthI;
                switch (interpolationType)
                {
                    case EInterpolationType.Custom:
                        t = interpolationCurve.Evaluate(interpolVal);
                        break;
                    case EInterpolationType.Linear:
                        t = interpolVal;
                        break;
                    case EInterpolationType.Exponential:
                        t = UnityEngine.Mathf.Pow(interpolVal, 2);
                        break;
                    case EInterpolationType.Sine:
                        t = UnityEngine.Mathf.Sin(interpolVal * UnityEngine.Mathf.PI) * .9f;
                        break;
                    default:
                        t = 0;
                        break;
                }

                UnityEngine.Vector3 tmp = UnityEngine.Vector3.Lerp(points[i],
                    previousPoints.Count >= 1
                        ? previousPoints[i > previousPoints.Count - 1 ? previousPoints.Count - 1 : i]
                        : points[i],
                    t * .9f);
                points[i] = new UnityEngine.Vector3(tmp.x, points[i].y, tmp.z);
            }

            previousPoints = points;
            return previousPoints;
        }
        #endregion


        #region PUBLIC VIRTUAL METHODS
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        public virtual bool GetRenderLine()
        {
            return true;
        }

        // ReSharper disable once VirtualMemberNeverOverridden.Global
        public virtual bool GetRenderPoint()
        {
            return true;
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public virtual UnityEngine.Vector3 GetOrigin()
        {
            return transform.position;
        }

        // ReSharper disable once UnusedParameter.Global
        // ReSharper disable once MemberCanBeProtected.Global
        public virtual UnityEngine.Vector3 GetPointingDirection(UnityEngine.Vector3 rayOrigin)
        {
            return transform.right;
        }
        #endregion

    }

}