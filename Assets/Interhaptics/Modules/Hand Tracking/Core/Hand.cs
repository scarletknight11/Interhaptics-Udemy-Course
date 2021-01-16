// ReSharper disable Unity.InefficientPropertyAccess
namespace Interhaptics.HandTracking
{

    /// <summary>
    /// Hand Class represented by palm position/rotation and gesture
    /// </summary>
    [System.Serializable]
    public sealed class Hand
    {

        #region PRIVATE FIELDS
        private readonly UnityEngine.Vector2[] _myFingerNorms = {
            //THUMB
            new UnityEngine.Vector2(115, 145),
            //INDEX
            new UnityEngine.Vector2(41, 170),
            //MIDDLE
            new UnityEngine.Vector2(35, 140),
            //RING
            new UnityEngine.Vector2(35, 170),
            //PINKY
            new UnityEngine.Vector2(66, 130),
            //PINCH
            new UnityEngine.Vector2(0.04f, 0.16f)
        };

        private float _grabStrength = 0;
        private float _pinchStrength = 0;

        private Types.SSkeleton _myInitSSkeleton = new Types.SSkeleton
        {
            bonesPositions = new UnityEngine.Vector3[(int)Types.EHandBones.HandEnd],
            bonesRotations = new UnityEngine.Quaternion[(int)Types.EHandBones.HandEnd]
        };

        private Types.SSkeleton _mySSkeleton = new Types.SSkeleton
        {
            rootPosition = new UnityEngine.Vector3(),
            rootRotation = new UnityEngine.Quaternion(),
            bonesRotations = new UnityEngine.Quaternion[(int)Types.EHandBones.HandMaxBones]
        };
        #endregion


        #region PUBLIC MEMBERS
        /// <summary>
        ///     The current position
        /// </summary>
        public UnityEngine.Vector3 Position
        {
            get => _mySSkeleton.rootPosition;
            set => _mySSkeleton.rootPosition = value;
        }
        /// <summary>
        ///     The current rotation
        /// </summary>
        public UnityEngine.Quaternion Rotation
        {
            get => _mySSkeleton.rootRotation;
            set => _mySSkeleton.rootRotation = value;
        }

        /// <summary>
        ///     The current gesture
        /// </summary>
        public Types.EGestureType Gesture { get; set; }
        /// <summary>
        ///     The skeleton tracking confidence
        /// </summary>
        public float SkeletonConfidence { get; set; } = 0f;
        /// <summary>
        ///     The current skeleton
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public Types.SSkeleton SSkeleton  => _mySSkeleton;
        /// <summary>
        ///     True if it's needed to use gesture data. False to use skeletal data
        /// </summary>
        public bool UseGestures { get; set; }
        /// <summary>
        ///     True if its the left hand. False otherwise
        /// </summary>
        public bool IsLeft { get; private set; }
        /// <summary>
        ///     True if its the hand is tracked. False otherwise
        /// </summary>
        public bool IsActive { get; set; } = false;
        /// <summary>
        ///     The actual grabbing strength
        /// </summary>
        /// <remarks>The value is clamped between 0 and 1</remarks>
        public float GrabStrength
        {
            get => _grabStrength;
            set => _grabStrength = UnityEngine.Mathf.Clamp01(value);
        }
        /// <summary>
        ///     The actual pinching strength
        /// </summary>
        /// <remarks>The value is clamped between 0 and 1</remarks>
        public float PinchStrength {
            get => _pinchStrength;
            set => _pinchStrength = UnityEngine.Mathf.Clamp01(value);
        }
        #endregion


        #region CONSTRUCTORS
        /// <summary>
        ///     The hand constructor
        /// </summary>
        /// <param name="isLeft">True if you want a left hand. False otherwise</param>
        public Hand(bool isLeft)
        {
            IsLeft = isLeft;
            UseGestures = true;
            Gesture = Types.EGestureType.Five;
            Position = UnityEngine.Vector3.zero;
            Rotation = UnityEngine.Quaternion.identity;
            GrabStrength = 0;
            PinchStrength = 0;
        }
        #endregion


        #region SKELETON METHODS
        /// <summary>
        ///     Initialize the skeletal data
        /// </summary>
        /// <param name="hand">The root game object of the hand</param>
        public void InitSkeleton(UnityEngine.GameObject hand)
        {
            if(hand is null)
                return;

            UnityEngine.Transform skeletonRootTransform = hand.transform.GetChild(0);
            UnityEngine.Transform boneTransform = hand.transform;

            _myInitSSkeleton.bonesRotations[(int) Types.EHandBones.HandWrist] = boneTransform.localRotation;
            _myInitSSkeleton.bonesPositions[(int) Types.EHandBones.HandWrist] = boneTransform.localPosition;

            boneTransform = skeletonRootTransform.GetChild(0);
            _myInitSSkeleton.bonesRotations[(int) Types.EHandBones.HandPalm] = boneTransform.localRotation;
            _myInitSSkeleton.bonesPositions[(int) Types.EHandBones.HandPalm] = boneTransform.localPosition;

            boneTransform = skeletonRootTransform.GetChild(1);
            _myInitSSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbMetacarpal] = UnityEngine.Quaternion.identity;
            _myInitSSkeleton.bonesPositions[(int) Types.EHandBones.HandThumbMetacarpal] = UnityEngine.Vector3.zero;
            _myInitSSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbProximal] = boneTransform.localRotation;
            _myInitSSkeleton.bonesPositions[(int) Types.EHandBones.HandThumbProximal] = boneTransform.localPosition;
            boneTransform = boneTransform.GetChild(0);
            _myInitSSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbMiddle] = boneTransform.localRotation;
            _myInitSSkeleton.bonesPositions[(int) Types.EHandBones.HandThumbMiddle] = boneTransform.localPosition;
            boneTransform = boneTransform.GetChild(0);
            _myInitSSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbDistal] = boneTransform.localRotation;
            _myInitSSkeleton.bonesPositions[(int) Types.EHandBones.HandThumbDistal] = boneTransform.localPosition;
            boneTransform = boneTransform.GetChild(0);
            _myInitSSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbTip] = boneTransform.localRotation;
            _myInitSSkeleton.bonesPositions[(int) Types.EHandBones.HandThumbTip] = boneTransform.localPosition;

            for (int fingerIndex = (int) Types.EFingers.Index;
                fingerIndex < (int) Types.EFingers.FingerEnd;
                fingerIndex++)
            {
                boneTransform = skeletonRootTransform.GetChild(fingerIndex + 1);
                _myInitSSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbMetacarpal + 4*fingerIndex] =
                    boneTransform.localRotation;
                _myInitSSkeleton.bonesPositions[(int) Types.EHandBones.HandThumbMetacarpal + 4*fingerIndex] =
                    boneTransform.localPosition;

                for (int boneIndex = (int) Types.EHandBones.HandThumbProximal + 4*fingerIndex;
                    boneIndex <= (int) Types.EHandBones.HandThumbDistal + 4*fingerIndex;
                    boneIndex++)
                {
                    boneTransform = boneTransform.GetChild(0);
                    _myInitSSkeleton.bonesRotations[boneIndex] = boneTransform.localRotation;
                    _myInitSSkeleton.bonesPositions[boneIndex] = boneTransform.localPosition;
                }

                boneTransform = boneTransform.GetChild(0);
                _myInitSSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbTip + fingerIndex] = boneTransform.localRotation;
                _myInitSSkeleton.bonesPositions[(int) Types.EHandBones.HandThumbTip + fingerIndex] = boneTransform.localPosition;
            }
        }

        /// <summary>
        ///     Reset the skeletal data to its initial data
        /// </summary>
        public void ResetSkeleton()
        {
            //_mySkeleton.bones_rotations = m_init_skeleton;
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Stores a skeleton. Returns false if fails
        /// </summary>
        /// <param name="inputSkeleton">The input skeleton</param>
        /// <param name="rootPosition">Its root position</param>
        /// <param name="rootRotation">Its root rotation</param>
        /// <returns>Return true if data are stored, false otherwise</returns>
        public bool SetSkeleton(UnityEngine.Quaternion[] inputSkeleton, UnityEngine.Vector3 rootPosition,
            UnityEngine.Quaternion rootRotation)
        {
            if (inputSkeleton != null && _myInitSSkeleton.bonesRotations != null)
            {
                UnityEngine.Quaternion[] bonesRotations = new UnityEngine.Quaternion[inputSkeleton.Length];

                for (int i = (int) Types.EHandBones.HandStart; i < (int) Types.EHandBones.HandMaxBones; i++)
                {
                    if (i >= inputSkeleton.Length)
                        return false;

                    UnityEngine.Quaternion rot = inputSkeleton[i];
                    if (!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z) && !float.IsNaN(rot.w))
                        bonesRotations[i] = rot;
                    else if (i < _myInitSSkeleton.bonesRotations.Length &&
                             !float.IsNaN((rot = _myInitSSkeleton.bonesRotations[i]).x) && !float.IsNaN(rot.y) &&
                             !float.IsNaN(rot.z) && !float.IsNaN(rot.w))
                        bonesRotations[i] = rot;
                    else
                        bonesRotations[i] = UnityEngine.Quaternion.identity;
                }

                _mySSkeleton.bonesRotations = bonesRotations;
            }

            _mySSkeleton.rootPosition = rootPosition;
            _mySSkeleton.rootRotation = rootRotation;

            return true;
        }

        /// <summary>
        ///     Applies the skeleton to a game object hand
        /// </summary>
        /// <param name="hand">The GameObject which represent your hand</param>
        /// <param name="setPalm">true to set the palm, false otherwise</param>
        public void SetHandSkeleton(UnityEngine.GameObject hand, bool setPalm = false)
        {
            if(hand is null)
                return;

            if (setPalm)
            {
                hand.transform.localRotation = _mySSkeleton.rootRotation;
                hand.transform.localPosition = _mySSkeleton.rootPosition;
            }
            UnityEngine.Transform skeletonRootTransform = hand.transform.GetChild(0);
            UnityEngine.Transform boneTransform = skeletonRootTransform.GetChild(1);
            
            boneTransform.localRotation = _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbMetacarpal] *
                                          _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbProximal];
            boneTransform = boneTransform.GetChild(0);
            boneTransform.localRotation = _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbMiddle];
            boneTransform = boneTransform.GetChild(0);
            boneTransform.localRotation = _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbDistal];
            
            for (int fingerIndex = (int) Types.EFingers.Index;
                fingerIndex < (int) Types.EFingers.FingerEnd;
                fingerIndex++)
            {
                boneTransform = skeletonRootTransform.GetChild(fingerIndex + 1);
                boneTransform.localRotation =
                    _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbMetacarpal + 4 * fingerIndex];
            
                for (int boneIndex = (int) Types.EHandBones.HandThumbProximal + 4 * fingerIndex;
                    boneIndex <= (int) Types.EHandBones.HandThumbDistal + 4 * fingerIndex;
                    boneIndex++)
                {
                    boneTransform = boneTransform.GetChild(0);
                    boneTransform.localRotation = _mySSkeleton.bonesRotations[boneIndex];
                }
            }
        }

        /*
         * Next function is to avoid the following code pasted 20 times :
         * 
            Vector3 index = _mySkeleton.init_positions[(int)Bones.Hand_Index0] + _mySkeleton.bones_rotations[(int)Bones.Hand_Index0] *
                            (
                                _mySkeleton.init_positions[(int)Bones.Hand_Index1] + _mySkeleton.bones_rotations[(int)Bones.Hand_Index1] *
                                (
                                    _mySkeleton.init_positions[(int)Bones.Hand_Index2] + _mySkeleton.bones_rotations[(int)Bones.Hand_Index2] * 
                                    (
                                        _mySkeleton.init_positions[(int)Bones.Hand_Index3] + _mySkeleton.bones_rotations[(int)Bones.Hand_Index3] * 
                                        _mySkeleton.init_positions[(int)Bones.Hand_IndexTip]
                                    )
                                )
                            );
        */
        private UnityEngine.Vector3 GetFingerTipPosition(int fingerStartIndex, int fingerTipIndex, int bonesCount)
        {
            UnityEngine.Vector3 fingerTip = _myInitSSkeleton.bonesPositions[fingerTipIndex];
            for(int i = fingerStartIndex + bonesCount - 1; i >= fingerStartIndex; i--)
                fingerTip = _myInitSSkeleton.bonesPositions[i] + _mySSkeleton.bonesRotations[i] * fingerTip;

            return fingerTip;
        }

        public float GetFingerAngleState(Types.EFingers finger, bool normalize = true)
        {
            float angle=
                UnityEngine.Quaternion.Angle(
                    _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbMetacarpal + 4 * (int) finger],
                    _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbProximal + 4 * (int) finger]) +
                UnityEngine.Quaternion.Angle(
                    _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbProximal + 4 * (int) finger],
                    _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbMiddle + 4 * (int) finger]) +
                UnityEngine.Quaternion.Angle(
                    _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbMiddle + 4 * (int) finger],
                    _mySSkeleton.bonesRotations[(int) Types.EHandBones.HandThumbDistal + 4 * (int) finger]);

            if (normalize)
                angle = Normalize(_myFingerNorms[(int) finger].x, _myFingerNorms[(int) finger].y, angle);

            return angle;
        }
        #endregion


        #region SKELETAL GRABBING / PINCHING METHODS
        private float ThreeFingersGrabStrength()
        {
            float strength = 0;

            UnityEngine.Vector3 thumb = GetFingerTipPosition((int) Types.EHandBones.HandThumbMetacarpal,
                (int) Types.EHandBones.HandThumbTip, 4);
            UnityEngine.Vector3 index = GetFingerTipPosition((int) Types.EHandBones.HandIndexMetacarpal,
                (int) Types.EHandBones.HandIndexTip, 4);
            UnityEngine.Vector3 middle = GetFingerTipPosition((int) Types.EHandBones.HandMiddleMetacarpal,
                (int) Types.EHandBones.HandMiddleTip, 4);

            UnityEngine.Vector3 ti = index - thumb;
            UnityEngine.Vector3 im = middle - index;
            UnityEngine.Vector3 norm = UnityEngine.Vector3.Cross(ti, im);


            return strength;
        }

        private float AngularFingerGrabStrength()
        {
            float thumb = GetFingerAngleState(Types.EFingers.Thumb);
            float index = GetFingerAngleState(Types.EFingers.Index);
            float middle = GetFingerAngleState(Types.EFingers.Middle);
            float ring = GetFingerAngleState(Types.EFingers.Ring);
            float pinky = GetFingerAngleState(Types.EFingers.Pinky);

            return Normalize(0, 5, thumb + index + middle + ring + pinky);
        }

        public void GrabStrengthFromSkeleton()
        {
            GrabStrength = AngularFingerGrabStrength();
        }

        public void PinchStrengthFromSkeleton()
        {
            float dist = UnityEngine.Vector3.Distance(
                GetFingerTipPosition((int) Types.EHandBones.HandIndexMetacarpal, (int) Types.EHandBones.HandIndexTip,
                    4),
                GetFingerTipPosition((int) Types.EHandBones.HandThumbMetacarpal, (int) Types.EHandBones.HandThumbTip,
                    4));
            PinchStrength =
                UnityEngine.Mathf.Clamp(1 - Normalize(_myFingerNorms[5].x, _myFingerNorms[5].y, dist), 0, 1);
        }
        #endregion


        #region SKELETAL GESTURE METHODS
        /// <summary>
        ///     Convert a skeleton to a gesture
        /// </summary>
        public void GestureFromSkeleton()
        {
            Gesture = Types.GestureTypeHelper.GetGestureTypeFromFingers(
                GetFingerAngleState(Types.EFingers.Thumb),
                GetFingerAngleState(Types.EFingers.Index),
                GetFingerAngleState(Types.EFingers.Middle),
                GetFingerAngleState(Types.EFingers.Ring),
                GetFingerAngleState(Types.EFingers.Pinky));
        }
        #endregion


        #region PRIVATE METHODS
        private static float Normalize(float inMin, float inMax, float input)
        {
            return System.Math.Abs(inMin - inMax) > UnityEngine.Mathf.Epsilon ? (input - inMin) / (inMax - inMin) : 0;
        }
        #endregion

    }

}