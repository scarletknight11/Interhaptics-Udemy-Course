namespace Interhaptics.HandTracking.TrackingProviders
{

    // ReSharper disable once UnusedType.Global
    public class KeyboardEmulatorProvider : Interfaces.ITrackingProvider
    {

        #region CONSTS
        // SPEEDS
        private const float MOVEMENT_SPEED = .005f;
        private const float ROTATION_SPEED = 2f;
        private const float SPEED_MULTIPLIER = 5f;

        // KEY CODES
        private const UnityEngine.KeyCode LEFT_HAND = UnityEngine.KeyCode.L;
        private const UnityEngine.KeyCode RIGHT_HAND = UnityEngine.KeyCode.R;
        private const UnityEngine.KeyCode VERTICAL_PLANE_MODE = UnityEngine.KeyCode.RightShift;
        private const UnityEngine.KeyCode INCREASE_SPEED = UnityEngine.KeyCode.LeftShift;
        // MOVES
        private const UnityEngine.KeyCode MOVE_FORWARDS = UnityEngine.KeyCode.W;
        private const UnityEngine.KeyCode MOVE_RIGHT = UnityEngine.KeyCode.D;
        private const UnityEngine.KeyCode MOVE_LEFT = UnityEngine.KeyCode.A;
        private const UnityEngine.KeyCode MOVE_BACKWARD = UnityEngine.KeyCode.S;
        // ROTATES
        private const UnityEngine.KeyCode ROTATE_RIGHT_YAW = UnityEngine.KeyCode.E;
        private const UnityEngine.KeyCode ROTATE_LEFT_YAW = UnityEngine.KeyCode.Q;
        // GESTURES
        private const UnityEngine.KeyCode GESTURE_GUN = UnityEngine.KeyCode.G;
        private const UnityEngine.KeyCode GESTURE_POINT = UnityEngine.KeyCode.P;
        private const UnityEngine.KeyCode GESTURE_FIST = UnityEngine.KeyCode.F;
        private const UnityEngine.KeyCode GESTURE_LIKE = UnityEngine.KeyCode.I;
        private const UnityEngine.KeyCode GESTURE_OK = UnityEngine.KeyCode.O;
        #endregion


        #region PRIVATE FIELDS
        private bool _isInVerticalPlanMode = false;
        private UnityEngine.Vector3 _leftPosition = .2f * UnityEngine.Vector3.forward;
        private UnityEngine.Quaternion _leftRotation = UnityEngine.Quaternion.Euler(-90, 0, -100);
        private UnityEngine.Vector3 _rightPosition = .2f * UnityEngine.Vector3.forward;
        private UnityEngine.Quaternion _rightRotation = UnityEngine.Quaternion.Euler(90, 0, 80);
        #endregion


        public bool CanHaveSkeleton()
        {
            return false;
        }

        public string Description()
        {
            return "Mock-up hand tracking by keyboard";
        }

        public string DisplayName()
        {
            return "Keyboard Emulator";
        }

        public Types.ETrackingDeviceClass DeviceClass()
        {
            return Types.ETrackingDeviceClass.Emulator;
        }

        public System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> PlatformCompatibilities()
        {
            return new [] {
                UnityEngine.RuntimePlatform.WindowsEditor
            };
        }

        public System.Collections.Generic.IEnumerable<string> XrSdkCompatibilities()
        {
            return new [] {
                "",
                "None",
                "MockHMD Display",
                "OpenVR",
                "Oculus",
                "OpenVR Display",
                "oculus display",
            };
        }

        public string Manufacturer()
        {
            return "Interhaptics";
        }

        public string Version()
        {
            return "20.10.01.01";
        }

        public bool Init()
        {
            return true;
        }

        public bool Cleanup()
        {
            return true;
        }

        public bool IsPresent()
        {
            return true;
        }

        public Types.SGestureTrackingData GetGesture(UnityEngine.Transform rigTransform, UnityEngine.Transform cameraTransform)
        {
            UnityEngine.Vector3 cameraPosition = cameraTransform.localPosition;
            UnityEngine.Quaternion cameraRotation = cameraTransform.localRotation;
            Types.SGestureTrackingData trackingData = new Types.SGestureTrackingData
            {
                isLeftTracked = UnityEngine.Input.GetKey(LEFT_HAND),
                isRightTracked = UnityEngine.Input.GetKey(RIGHT_HAND)
            };

            bool useSpeedMultiplier = UnityEngine.Input.GetKey(INCREASE_SPEED);
            if (UnityEngine.Input.GetKeyDown(VERTICAL_PLANE_MODE))
                _isInVerticalPlanMode = !_isInVerticalPlanMode;
            UnityEngine.Vector3 forwardVector =
                _isInVerticalPlanMode ? UnityEngine.Vector3.up : UnityEngine.Vector3.forward;

            if (trackingData.isLeftTracked)
            {
                if (UnityEngine.Input.GetKey(GESTURE_OK))
                {
                    trackingData.leftGesture = Types.EGestureType.OK;
                    trackingData.leftPinchStrength = 1;
                    trackingData.leftGrabStrength = 0;
                }
                else if (UnityEngine.Input.GetKey(GESTURE_GUN))
                {
                    trackingData.leftGesture = Types.EGestureType.Gun;
                    trackingData.leftPinchStrength = 0;
                    trackingData.leftGrabStrength = .75f;
                }
                else if (UnityEngine.Input.GetKey(GESTURE_FIST))
                {
                    trackingData.leftGesture = Types.EGestureType.Fist;
                    trackingData.leftPinchStrength = 0;
                    trackingData.leftGrabStrength = 1;
                }
                else if (UnityEngine.Input.GetKey(GESTURE_LIKE))
                {
                    trackingData.leftGesture = Types.EGestureType.Like;
                    trackingData.leftPinchStrength = 0;
                    trackingData.leftGrabStrength = .9f;
                }
                else if (UnityEngine.Input.GetKey(GESTURE_POINT))
                {
                    trackingData.leftGesture = Types.EGestureType.Pointing;
                    trackingData.leftPinchStrength = 0;
                    trackingData.leftGrabStrength = .75f;
                }
                else
                {
                    trackingData.leftGesture = Types.EGestureType.Five;
                    trackingData.leftPinchStrength = 0;
                    trackingData.leftGrabStrength = 0;
                }


                if (UnityEngine.Input.GetKey(MOVE_FORWARDS))
                    _leftPosition += forwardVector * (MOVEMENT_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1));
                if (UnityEngine.Input.GetKey(MOVE_BACKWARD))
                    _leftPosition -= forwardVector * (MOVEMENT_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1));
                if (UnityEngine.Input.GetKey(MOVE_RIGHT))
                    _leftPosition += UnityEngine.Vector3.right *
                                     (MOVEMENT_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1));
                if (UnityEngine.Input.GetKey(MOVE_LEFT))
                    _leftPosition += UnityEngine.Vector3.left *
                                     (MOVEMENT_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1));

                if (UnityEngine.Input.GetKey(ROTATE_RIGHT_YAW))
                    _leftRotation =
                        UnityEngine.Quaternion.AngleAxis(-ROTATION_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1),
                            UnityEngine.Vector3.forward) * _leftRotation;
                if (UnityEngine.Input.GetKey(ROTATE_LEFT_YAW))
                    _leftRotation =
                        UnityEngine.Quaternion.AngleAxis(ROTATION_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1),
                            UnityEngine.Vector3.forward) * _leftRotation;

                trackingData.leftPosition = cameraPosition + cameraRotation * _leftPosition;
                trackingData.leftRotation = cameraRotation * _leftRotation;
            }

            if (trackingData.isRightTracked)
            {
                if (UnityEngine.Input.GetKey(GESTURE_OK))
                {
                    trackingData.rightGesture = Types.EGestureType.OK;
                    trackingData.rightPinchStrength = 1;
                    trackingData.rightGrabStrength = 0;
                }
                else if (UnityEngine.Input.GetKey(GESTURE_GUN))
                {
                    trackingData.rightGesture = Types.EGestureType.Gun;
                    trackingData.rightPinchStrength = 0;
                    trackingData.rightGrabStrength = .75f;
                }
                else if (UnityEngine.Input.GetKey(GESTURE_FIST))
                {
                    trackingData.rightGesture = Types.EGestureType.Fist;
                    trackingData.rightPinchStrength = 0;
                    trackingData.rightGrabStrength = 1;
                }
                else if (UnityEngine.Input.GetKey(GESTURE_LIKE))
                {
                    trackingData.rightGesture = Types.EGestureType.Like;
                    trackingData.rightPinchStrength = 0;
                    trackingData.rightGrabStrength = .9f;
                }
                else if (UnityEngine.Input.GetKey(GESTURE_POINT))
                {
                    trackingData.rightGesture = Types.EGestureType.Pointing;
                    trackingData.rightPinchStrength = 0;
                    trackingData.rightGrabStrength = .75f;
                }
                else
                {
                    trackingData.rightGesture = Types.EGestureType.Five;
                    trackingData.rightPinchStrength = 0;
                    trackingData.rightGrabStrength = 0;
                }

                if (UnityEngine.Input.GetKey(MOVE_FORWARDS))
                    _rightPosition += forwardVector * (MOVEMENT_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1));
                if (UnityEngine.Input.GetKey(MOVE_BACKWARD))
                    _rightPosition -= forwardVector * (MOVEMENT_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1));
                if (UnityEngine.Input.GetKey(MOVE_RIGHT))
                    _rightPosition += UnityEngine.Vector3.right *
                                      (MOVEMENT_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1));
                if (UnityEngine.Input.GetKey(MOVE_LEFT))
                    _rightPosition += UnityEngine.Vector3.left *
                                      (MOVEMENT_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1));

                if (UnityEngine.Input.GetKey(ROTATE_RIGHT_YAW))
                    _rightRotation =
                        UnityEngine.Quaternion.AngleAxis(-ROTATION_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1),
                            UnityEngine.Vector3.forward) * _rightRotation;
                if (UnityEngine.Input.GetKey(ROTATE_LEFT_YAW))
                    _rightRotation =
                        UnityEngine.Quaternion.AngleAxis(ROTATION_SPEED * (useSpeedMultiplier ? SPEED_MULTIPLIER : 1),
                            UnityEngine.Vector3.forward) * _rightRotation;

                trackingData.rightPosition = cameraPosition + cameraRotation * _rightPosition;
                trackingData.rightRotation = cameraRotation * _rightRotation;
            }

            return trackingData;
        }

        public Types.SSkeletonTrackingData GetSkeleton(UnityEngine.Transform rigTransform, UnityEngine.Transform cameraTransform)
        {
            throw new System.NotSupportedException("Keyboard don't have any skeletal input");
        }

    }

}
