// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PossibleNullReferenceException
// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Interhaptics.HandTracking
{

    /// <summary>
    ///     Manager of the HandTracking module
    /// </summary>
    [UnityEngine.HelpURL("https://www.interhaptics.com/resources/docs")]
    [UnityEngine.AddComponentMenu("Interhaptics/Hand Tracking/HandTrackingManager")]
    public sealed class HandTrackingManager : Utils.Singleton<HandTrackingManager>
    {

        #region GENERAL CONSTS
        private const string GHT_VERSION = "20.11.02.00";
        #endregion


        #region CONSTS DEBUG MESSAGES
        private const string LOG_WARNING_NO_PROVIDER_AVAILABLE = "<b>[Interhaptics.HandTracking]</b> no provider available";

        // ERRORS
        private const string LOG_ERROR_PROVIDER_OR_INSTANCE_NULL = "<b>[Interhaptics.HandTracking]</b> Your provider type and your provider instance can't be nullable";
        private const string LOG_ERROR_PROVIDER_METHOD_NOT_FOUND = "<b>[Interhaptics.HandTracking]</b> Your wanted method wasn't found in this provider type";
        private const string LOG_ERROR_IMPOSSIBLE_TO_GET_SKELETON = "<b>[Interhaptics.HandTracking]</b> Impossible to get the skeletal data with the provider {0}\n{1}";
        private const string LOG_ERROR_IMPOSSIBLE_TO_GET_GESTURE = "<b>[Interhaptics.HandTracking]</b> Impossible to get tracking data with the provider {0}, no tracking data will be apply in this life cycle\n{1}";
        #endregion


        #region SERIALIZED FIELDS
        [UnityEngine.Header("Hand Tracking Manager : " + GHT_VERSION)]
        #endregion


        #region PRIVATE FIELDS
        private System.Type _providerTypeUsed = null;
        private object _providerInstanceUsed;
        #endregion


        #region PUBLIC MEMBERS
        /// <summary>
        ///     If true, will log tracking data. Turn to false if you don't want the Interhaptics.HandTracking logs
        /// </summary>
        public static bool Verbose
        {
            get => Tools.HandTrackingSettings.Instance.verbose;
            set => Tools.HandTrackingSettings.Instance.verbose = value;
        }
        /// <summary>
        ///     If true, will hide every warnings logs
        /// </summary>
        public static bool HideWarnings
        {
            get => Tools.HandTrackingSettings.Instance.hideWarnings;
            set => Tools.HandTrackingSettings.Instance.hideWarnings = value;
        }
        /// <summary>
        ///     If true, will hide every error logs
        /// </summary>
        public static bool HideErrors
        {
            get => Tools.HandTrackingSettings.Instance.hideErrors;
            set => Tools.HandTrackingSettings.Instance.hideErrors = value;
        }
        /// <summary>
        ///     If true, will log tracking data. Turn to false if you don't want the Interhaptics.HandTracking logs
        /// </summary>
        public static bool ShowTrackingData
        {
            get => Tools.HandTrackingSettings.Instance.showTrackingData;
            set => Tools.HandTrackingSettings.Instance.showTrackingData = value;
        }

        /// <summary>
        ///     The TrackingProvider type used now to get tracking data
        /// </summary>
        public System.Type ProviderTypeUsed => _providerTypeUsed;
        /// <summary>
        ///     The TrackingProvider instance used now to get tracking data
        /// </summary>
        public object ProviderInstanceUsed => _providerInstanceUsed;
        /// <summary>
        /// Say if the current input is skeletal
        /// </summary>
        public bool IsSkeletalInput { get; set; } = false;
        /// <summary>
        /// Left Hand
        /// </summary>
        public Hand LeftHand { get; } = new Hand(true);
        /// <summary>
        /// Right Hand
        /// </summary>
        public Hand RightHand { get; } = new Hand(false);

        /// <summary>
        ///     Transform of the main camera
        /// </summary>
        public UnityEngine.Transform MainCamera { get; private set; } = null;
        /// <summary>
        ///     The tracking space preferences defined
        /// </summary>
#pragma warning disable 618
        public static UnityEngine.XR.TrackingSpaceType TrackingSpace => Tools.HandTrackingSettings.Instance.trackingSpace;
#pragma warning restore 618
        /// <summary>
        ///     Event call when a hand tracking data are available
        /// </summary>
        public static readonly Tools.TrackingReceivedEvent OnTrackingReceived = new Tools.TrackingReceivedEvent();
        #endregion


        #region CONSTRUCTORS
        private HandTrackingManager() { }
        #endregion


        #region LIFE CYCLES
        protected override void OnAwake()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            MainCamera = cam == null ? transform : cam.transform;
#pragma warning disable 618
            UnityEngine.XR.XRDevice.SetTrackingSpaceType(TrackingSpace); // Use this to set oculus height correctly - carefully
#pragma warning restore 618
            Tools.HandTrackingSettings.Instance.InitializeProviders();
        }

        protected override void OnFixedUpdate()
        {
            if (Tools.HandTrackingSettings.Instance.updateLifeCycle == Interhaptics.Core.EUpdateLifeCycle.FixedUpdate)
                GetTrackingData();
        }

        protected override void OnUpdate()
        {
            if (Tools.HandTrackingSettings.Instance.updateLifeCycle == Interhaptics.Core.EUpdateLifeCycle.Update)
                GetTrackingData();
        }

        protected override void OnLateUpdate()
        {
            if (Tools.HandTrackingSettings.Instance.updateLifeCycle == Interhaptics.Core.EUpdateLifeCycle.LateUpdate)
                GetTrackingData();
        }

        protected override void OnOnApplicationQuit()
        {
            Tools.HandTrackingSettings.Instance.CleanProviders();
            OnTrackingReceived.RemoveAllListeners();
        }
        #endregion


        #region APPLY TRACKING DATA METHODS
        private void GetTrackingData()
        {
            if (!Tools.HandTrackingSettings.Instance.GetFirstAvailableProviderWithInstance(out _providerTypeUsed,
                out _providerInstanceUsed))
            {
                RightHand.IsActive = false;
                LeftHand.IsActive = false;

                OnTrackingReceived.Invoke(LeftHand);
                OnTrackingReceived.Invoke(RightHand);

                if(Verbose && !HideWarnings)
                    UnityEngine.Debug.LogWarning(LOG_WARNING_NO_PROVIDER_AVAILABLE, gameObject);
                return;
            }

            System.Reflection.MethodInfo canHaveSkeletonMethod =
                _providerTypeUsed.GetMethod(Tools.ReflectionNames.CAN_HAVE_SKELETON_PROVIDER_METHOD_NAME);

            // SKELETAL INPUT
            if (canHaveSkeletonMethod != null && (bool) canHaveSkeletonMethod.Invoke(_providerInstanceUsed, null))
            {
                try
                {
                    System.Reflection.MethodInfo skeletonMethod =
                        _providerTypeUsed.GetMethod(Tools.ReflectionNames.GET_SKELETON_PROVIDER_METHOD_NAME);
                    Types.SSkeletonTrackingData trackingData =
                        (Types.SSkeletonTrackingData) skeletonMethod.Invoke(_providerInstanceUsed,
                            new object[] {transform, MainCamera});
                    
                    ApplySkeletonTrackingData(trackingData);
                    OnTrackingReceived.Invoke(LeftHand);
                    OnTrackingReceived.Invoke(RightHand);

                    if (ShowTrackingData)
                        UnityEngine.Debug.Log(trackingData.ToString(), gameObject);
                    return;
                }
                catch (System.Reflection.TargetInvocationException  e)
                {
                    if (Verbose && !HideErrors)
                        UnityEngine.Debug.LogError(
                            string.Format(LOG_ERROR_IMPOSSIBLE_TO_GET_SKELETON, _providerTypeUsed.FullName,
                                e.InnerException != null ? e.InnerException.Message : e.Message), gameObject);
                }
            }

            // GESTURE INPUT
            try
            {
                System.Reflection.MethodInfo gestureMethod =
                    _providerTypeUsed.GetMethod(Tools.ReflectionNames.GET_GESTURE_PROVIDER_METHOD_NAME);

                Types.SGestureTrackingData trackingData =
                    (Types.SGestureTrackingData) gestureMethod.Invoke(_providerInstanceUsed,
                        new object[] {transform, MainCamera});

                ApplyGestureTrackingData(trackingData);
                OnTrackingReceived.Invoke(LeftHand);
                OnTrackingReceived.Invoke(RightHand);

                if (ShowTrackingData)
                    UnityEngine.Debug.Log(trackingData.ToString(), gameObject);
            }
            catch (System.Reflection.TargetInvocationException  e)
            {
                if (Verbose && !HideErrors)
                    UnityEngine.Debug.LogError(
                        string.Format(LOG_ERROR_IMPOSSIBLE_TO_GET_GESTURE, _providerTypeUsed.FullName,
                            e.InnerException != null ? e.InnerException.Message : e.Message), gameObject);
            }
        }

        private void ApplyGestureTrackingData(Types.SGestureTrackingData trackingData)
        {
            // LEFT HAND
            LeftHand.IsActive = trackingData.isLeftTracked;
            LeftHand.UseGestures = true;
            if (LeftHand.IsActive)
            {
                LeftHand.Gesture = trackingData.leftGesture;
                LeftHand.Position = trackingData.leftPosition;
                LeftHand.Rotation = trackingData.leftRotation;
                LeftHand.GrabStrength = trackingData.leftGrabStrength;
                LeftHand.PinchStrength = trackingData.leftPinchStrength;
            }

            // RIGHT HAND
            RightHand.IsActive = trackingData.isRightTracked;
            RightHand.UseGestures = true;
            if (RightHand.IsActive)
            {
                RightHand.Gesture = trackingData.rightGesture;
                RightHand.Position = trackingData.rightPosition;
                RightHand.Rotation = trackingData.rightRotation;
                RightHand.GrabStrength = trackingData.rightGrabStrength;
                RightHand.PinchStrength = trackingData.rightPinchStrength;
            }

            IsSkeletalInput = false;
        }

        private void ApplySkeletonTrackingData(Types.SSkeletonTrackingData trackingData)
        {
            // LEFT HAND
            LeftHand.IsActive = trackingData.isLeftTracked;
            LeftHand.UseGestures = false;
            if (LeftHand.IsActive)
            {
                LeftHand.SkeletonConfidence = trackingData.leftSkeletonConfidence;
                LeftHand.SetSkeleton(trackingData.leftSkeleton.bonesRotations,
                    trackingData.leftSkeleton.rootPosition, trackingData.leftSkeleton.rootRotation);
                LeftHand.GrabStrengthFromSkeleton();
                LeftHand.PinchStrengthFromSkeleton();
                LeftHand.GestureFromSkeleton();
            }

            // RIGHT HAND
            RightHand.IsActive = trackingData.isRightTracked;
            RightHand.UseGestures = false;
            if (RightHand.IsActive)
            {
                LeftHand.SkeletonConfidence = trackingData.rightSkeletonConfidence;
                RightHand.SetSkeleton(trackingData.rightSkeleton.bonesRotations,
                    trackingData.rightSkeleton.rootPosition, trackingData.rightSkeleton.rootRotation);
                RightHand.GrabStrengthFromSkeleton();
                RightHand.PinchStrengthFromSkeleton();
                RightHand.GestureFromSkeleton();
            }

            IsSkeletalInput = true;
        }
        #endregion


        #region DECORATOR METHODS
        /// <summary>
        ///     Apply a provider method depending on its method name
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">A provider instance</param>
        /// <param name="methodName">The method name to call</param>
        /// <param name="parameters">An array which contain every method parameters</param>
        /// <returns>the returned value of the method called</returns>
        private object DecoratorCallProviderMethod(System.Type providerType, object providerInstance, string methodName, object[] parameters)
        {
            if (providerType == null || providerInstance == null)
            {
                if (Verbose && !HideErrors)
                    UnityEngine.Debug.LogError(LOG_ERROR_PROVIDER_OR_INSTANCE_NULL, gameObject);
                return null;
            }

            System.Reflection.MethodInfo method = providerType.GetMethod(methodName);
            if (method != null)
                return method.Invoke(providerInstance, parameters);

            if (Verbose && !HideErrors)
                UnityEngine.Debug.LogError(LOG_ERROR_PROVIDER_METHOD_NOT_FOUND, gameObject);
            return null;
        }

        /// <summary>
        ///     Call a specific method on the actual provider used for tracking 
        /// </summary>
        /// <param name="methodName">The method name to call</param>
        /// <param name="parameters">An array which contain every method parameters</param>
        /// <returns></returns>
        private object DecoratorCallProviderMethod(string methodName, object[] parameters)
        {
            return DecoratorCallProviderMethod(_providerTypeUsed, _providerInstanceUsed, methodName, parameters);
        }

        /// <summary>
        ///     Define if the current provider support the skeletal tracking
        /// </summary>
        /// <returns>True if the current provider support skeletal tracking. False otherwise</returns>
        public bool CanHaveSkeleton()
        {
            return (bool) DecoratorCallProviderMethod(Tools.ReflectionNames.CAN_HAVE_SKELETON_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Define if a provider support the skeletal tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">Its instance</param>
        /// <returns>True if a provider support skeletal tracking. False otherwise</returns>
        public bool CanHaveSkeleton(System.Type providerType, object providerInstance)
        {
            return (bool) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.CAN_HAVE_SKELETON_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get the current tracking description
        /// </summary>
        /// <returns>The current provider string description</returns>
        public string Description()
        {
            return (string) DecoratorCallProviderMethod(Tools.ReflectionNames.DESCRIPTION_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get a tracking description
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">Its instance</param>
        /// <returns>A provider string description</returns>
        public string Description(System.Type providerType, object providerInstance)
        {
            return (string) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.DESCRIPTION_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get the current tracking displaying name
        /// </summary>
        /// <returns>The current provider string name</returns>
        public string DisplayName()
        {
            return (string) DecoratorCallProviderMethod(Tools.ReflectionNames.DISPLAY_NAME_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get a tracking displaying name
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">Its instance</param>
        /// <returns>A provider string name</returns>
        public string DisplayName(System.Type providerType, object providerInstance)
        {
            return (string) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.DISPLAY_NAME_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get the current tracking device class
        /// </summary>
        /// <returns>The current provider device class</returns>
        public Types.ETrackingDeviceClass DeviceClass()
        {
            return (Types.ETrackingDeviceClass) DecoratorCallProviderMethod(Tools.ReflectionNames
                .DEVICE_CLASS_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get a tracking device class
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">Its instance</param>
        /// <returns>A provider device class</returns>
        public Types.ETrackingDeviceClass DeviceClass(System.Type providerType, object providerInstance)
        {
            return (Types.ETrackingDeviceClass) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.DEVICE_CLASS_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Define the player platforms compatibility with the current tracking
        /// </summary>
        /// <returns>A collection of player platform compatible with the current tracking</returns>
        public UnityEngine.RuntimePlatform[] PlatformCompatibilities()
        {
            return (UnityEngine.RuntimePlatform[]) DecoratorCallProviderMethod(Tools.ReflectionNames
                .PLATFORM_COMPATIBILITIES_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Define the player platforms compatibility with a tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <returns>A collection of player platform compatible with a tracking</returns>
        public UnityEngine.RuntimePlatform[] PlatformCompatibilities(System.Type providerType, object providerInstance)
        {
            return (UnityEngine.RuntimePlatform[]) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.PLATFORM_COMPATIBILITIES_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Define the XR SDKs compatibility with the current tracking
        /// </summary>
        /// <returns>A collection of XR SDK compatible with the current tracking</returns>
        public string[] XrSdkCompatibilities()
        {
            return (string[]) DecoratorCallProviderMethod(Tools.ReflectionNames
                .XR_SDK_COMPATIBILITIES_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Define the XR SDKs compatibility with a tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <returns>A collection of XR SDK compatible with a tracking</returns>
        public string[] XrSdkCompatibilities(System.Type providerType, object providerInstance)
        {
            return (string[]) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.XR_SDK_COMPATIBILITIES_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get the manufacturer name of the current tracking
        /// </summary>
        /// <returns>The manufacturer string name</returns>
        public string Manufacturer()
        {
            return (string) DecoratorCallProviderMethod(Tools.ReflectionNames.MANUFACTURER_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get the manufacturer name of a tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <returns>The manufacturer string name</returns>
        public string Manufacturer(System.Type providerType, object providerInstance)
        {
            return (string) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.MANUFACTURER_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get the version of the current tracking
        /// </summary>
        /// <returns>The version string</returns>
        public string Version()
        {
            return (string) DecoratorCallProviderMethod(Tools.ReflectionNames.VERSION_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get the version of a tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <returns>The version string</returns>
        public string Version(System.Type providerType, object providerInstance)
        {
            return (string) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.VERSION_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Initialize the actual tracking
        /// </summary>
        /// <returns>True if the provider was initialized. False otherwise</returns>
        public bool Init()
        {
            return (bool) DecoratorCallProviderMethod(Tools.ReflectionNames.INIT_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Initialize a tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <returns>True if the provider was initialized. False otherwise</returns>
        public bool Init(System.Type providerType, object providerInstance)
        {
            return (bool) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.INIT_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Clean the current tracking
        /// </summary>
        /// <returns>True if the provider was cleaned. False otherwise</returns>
        public bool Cleanup()
        {
            return (bool) DecoratorCallProviderMethod(Tools.ReflectionNames.CLEANUP_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Clean a tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <returns>True if the provider was cleaned. False otherwise</returns>
        public bool Cleanup(System.Type providerType, object providerInstance)
        {
            return (bool) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.CLEANUP_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get if the current tracking is available
        /// </summary>
        /// <returns>True if the provider is available. False otherwise</returns>
        public bool IsPresent()
        {
            return (bool) DecoratorCallProviderMethod(Tools.ReflectionNames.IS_PRESENT_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get if a tracking is available
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <returns>True if the provider is available. False otherwise</returns>
        public bool IsPresent(System.Type providerType, object providerInstance)
        {
            return (bool) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.IS_PRESENT_PROVIDER_METHOD_NAME, null);
        }

        /// <summary>
        ///     Get gesture tracking data of the current tracking
        /// </summary>
        /// <param name="rigTransform">Transform referring to the user rig</param>
        /// <param name="cameraTransform">Transform referring to the user camera</param>
        /// <returns>The provider gesture data</returns>
        public Types.SGestureTrackingData GetGesture(UnityEngine.Transform rigTransform, UnityEngine.Transform cameraTransform)
        {
            return (Types.SGestureTrackingData) DecoratorCallProviderMethod(Tools.ReflectionNames
                .GET_GESTURE_PROVIDER_METHOD_NAME, new object[] {rigTransform, cameraTransform});
        }

        /// <summary>
        ///     Get gesture tracking data of the current tracking
        /// </summary>
        /// <returns>The provider gesture data</returns>
        public Types.SGestureTrackingData GetGesture()
        {
            return GetGesture(transform, MainCamera);
        }

        /// <summary>
        ///     Get gesture tracking data of a tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <param name="rigTransform">Transform referring to the user rig</param>
        /// <param name="cameraTransform">Transform referring to the user camera</param>
        /// <returns>The provider gesture data</returns>
        public Types.SGestureTrackingData GetGesture(System.Type providerType, object providerInstance, UnityEngine.Transform rigTransform, UnityEngine.Transform cameraTransform)
        {
            return (Types.SGestureTrackingData) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.GET_GESTURE_PROVIDER_METHOD_NAME, new object[] {rigTransform, cameraTransform});
        }

        /// <summary>
        ///     Get gesture tracking data of a tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <returns>The provider gesture data</returns>
        public Types.SGestureTrackingData GetGesture(System.Type providerType, object providerInstance)
        {
            return GetGesture(providerType, providerInstance, transform, MainCamera);
        }

        /// <summary>
        ///     Get skeletal tracking data of the current tracking
        /// </summary>
        /// <param name="rigTransform">Transform referring to the user rig</param>
        /// <param name="cameraTransform">Transform referring to the user camera</param>
        /// <returns>The provider skeletal data</returns>
        public string GetSkeleton(UnityEngine.Transform rigTransform, UnityEngine.Transform cameraTransform)
        {
            return (string) DecoratorCallProviderMethod(Tools.ReflectionNames.GET_SKELETON_PROVIDER_METHOD_NAME, new object[] {rigTransform, cameraTransform});
        }

        /// <summary>
        ///     Get skeletal tracking data of the current tracking
        /// </summary>
        /// <returns>The provider skeletal data</returns>
        public string GetSkeleton()
        {
            return (string) DecoratorCallProviderMethod(Tools.ReflectionNames.GET_SKELETON_PROVIDER_METHOD_NAME, new object[] {transform, MainCamera});
        }

        /// <summary>
        ///     Get skeletal tracking data of a tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <param name="rigTransform">Transform referring to the user rig</param>
        /// <param name="cameraTransform">Transform referring to the user camera</param>
        /// <returns>The provider skeletal data</returns>
        public string GetSkeleton(System.Type providerType, object providerInstance, UnityEngine.Transform rigTransform, UnityEngine.Transform cameraTransform)
        {
            return (string) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.GET_SKELETON_PROVIDER_METHOD_NAME, new object[] {rigTransform, cameraTransform});
        }

        /// <summary>
        ///     Get skeletal tracking data of a tracking
        /// </summary>
        /// <param name="providerType">A provider type</param>
        /// <param name="providerInstance">ts instance</param>
        /// <returns>The provider skeletal data</returns>
        public string GetSkeleton(System.Type providerType, object providerInstance)
        {
            return (string) DecoratorCallProviderMethod(providerType, providerInstance,
                Tools.ReflectionNames.GET_SKELETON_PROVIDER_METHOD_NAME, new object[] {transform, MainCamera});
        }
        #endregion

    }

}