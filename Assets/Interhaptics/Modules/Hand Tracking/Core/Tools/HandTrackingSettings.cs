// ReSharper disable RedundantNameQualifier
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedParameter.Global
#pragma warning disable 618
namespace Interhaptics.HandTracking.Tools
{

    /// <summary>
    ///     Hand Tracking module settings
    /// </summary>
    public class HandTrackingSettings : UnityEngine.ScriptableObject
    {

        #region INSTANCES
        private static HandTrackingSettings BufferedInstance { get; set; } = null;

        /// <summary>
        ///     Instance getter
        /// </summary>
        public static HandTrackingSettings Instance =>
            BufferedInstance == null ? GetOrCreateSettings() : BufferedInstance;
        #endregion


        #region STATIC CONST PATHS
        private const string MY_ASSET_SETTINGS_NAME = "HandTrackingSettings";
        public static readonly string MY_ASSET_SETTINGS_PATH = $"Assets/Interhaptics/Modules/Hand Tracking/Resources/{MY_ASSET_SETTINGS_NAME}.asset";
        #endregion


        #region CONSTS DEBUG MESSAGES
        // LOGS
        private const string LOG_PROVIDER_NOT_PRESENT = "<b>[Interhaptics.HandTracking]</b> {0} is not present";
        private const string LOG_PROVIDER_INITIALIZED = "<b>[Interhaptics.HandTracking]</b> {0} was correctly initialized";
        private const string LOG_PROVIDER_CLEANED = "<b>[Interhaptics.HandTracking]</b> {0} was correctly cleaned";

        // WARNINGS
        private const string LOG_WARNING_NO_SETTINGS_FOUND = "<b>[Interhaptics.HandTracking]</b> Hand Tracking settings was not found so an empty one was generated";
        private const string LOG_WARNING_NO_INITIALIZING_METHOD_FOUND = "<b>[Interhaptics.HandTracking]</b> No Initializing method found {0}";
        private const string LOG_WARNING_INITIALIZING_FAILED = "<b>[Interhaptics.HandTracking]</b> It was impossible to initialize {0} so it was cleaned";
        private const string LOG_WARNING_IMPOSSIBLE_TO_CLEAN = "<b>[Interhaptics.HandTracking]</b> Impossible clean {0}";
        #endregion


        #region SERIALIZED FIELDS
        /// <summary>
        ///     The hand tracking used sorted by the preference order
        /// </summary>
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Your preference order for the hand tracking used (gray ones are not compatible with your target built platform)")]
        public Types.HandTrackingPreferences handTrackingPreferencesOrder = new Types.HandTrackingPreferences();
        /// <summary>
        ///     Represents the size of physical space available for XR. If set to Stationary verify the eye-height matches the hand height position.
        /// </summary>
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Represents the size of physical space available for XR. If set to Stationary verify the eye-height matches the hand height position.")]
        public UnityEngine.XR.TrackingSpaceType trackingSpace = UnityEngine.XR.TrackingSpaceType.RoomScale;
        /// <summary>
        ///     In which life cycle the HandTrackingManager will get the tracking data
        /// </summary>
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("In which life cycle the HandTrackingManager will get the tracking data")]
        public Interhaptics.Core.EUpdateLifeCycle updateLifeCycle = Interhaptics.Core.EUpdateLifeCycle.FixedUpdate;
        /// <summary>
        ///     If true hands will use its Renderer. Otherwise it will be disabled
        /// </summary>
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("If true hands will use its Renderer. Otherwise it will be disabled")]
        public bool useMeshRenderer = true;
        /// <summary>
        ///     If true hands will be disabled when it will be not tracked. Otherwise it will stay at its place
        /// </summary>
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("If true hands will be disabled when it will be not tracked. Otherwise it will stay at its place")]
        public bool disableHandsWhenNotTracked = true;

        /// <summary>
        ///     If true, will log everything. Turn to false if you don't want the Interhaptics.HandTracking logs
        /// </summary>
        [UnityEngine.Header("Debugging tools")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("If true, will log everything. Turn to false if you don't want the Interhaptics.HandTracking logs")]
        public bool verbose = true;
        /// <summary>
        ///     If true, will hide warning logs
        /// </summary>
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("If true, will hide warning logs")]
        public bool hideWarnings = true;
        /// <summary>
        ///     If true, will hide error logs
        /// </summary>
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("If true, will hide error logs")]
        public bool hideErrors = true;
        /// <summary>
        ///     If true, will log tracking data. Turn to false if you don't want the Interhaptics.HandTracking logs
        /// </summary>
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("If true, will log tracking data. Turn to false if you don't want the Interhaptics.HandTracking logs")]
        public bool showTrackingData = false;
        #endregion


        #region PRIVATE FIELDS
        private System.Type[] _providerTypes = new System.Type[0];
        private object[] _providerInstances;
        #endregion


        #region PUBLIC MEMBERS
        /// <summary>
        ///     Return true if providers was already initialized
        /// </summary>
        public bool ProvidersAreInitialized { get; private set; } = false;
        #endregion


        #region LOADING METHODS
#if UNITY_EDITOR
        /// <summary>
        ///     Load the HandTrackingSettings by the asset database
        /// </summary>
        /// <returns>The HandTrackingSettings instance</returns>
        public static HandTrackingSettings EditorLoadSettings()
        {
            return BufferedInstance =
                UnityEditor.AssetDatabase.LoadAssetAtPath<HandTrackingSettings>(MY_ASSET_SETTINGS_PATH);
        }

        /// <summary>
        ///     Load the HandTrackingSettings and serialize them into an object
        /// </summary>
        /// <returns>A SerializedObject corresponding to the HandTrackingSettings</returns>
        public static UnityEditor.SerializedObject GetSerializedSettings()
        {
            return new UnityEditor.SerializedObject(GetOrCreateSettings());
        }
#endif

        /// <summary>
        ///     Load the HandTrackingSettings and create one if no one exists
        /// </summary>
        /// <returns>The HandTrackingSettings instance</returns>
        private static HandTrackingSettings GetOrCreateSettings()
        {

            if (BufferedInstance != null)
                return BufferedInstance;

#if UNITY_EDITOR
            BufferedInstance = UnityEditor.AssetDatabase.LoadAssetAtPath<HandTrackingSettings>(MY_ASSET_SETTINGS_PATH);
#else
            BufferedInstance = (HandTrackingSettings)UnityEngine.Resources.Load(MY_ASSET_SETTINGS_NAME);
#endif

            if (BufferedInstance != null)
                return BufferedInstance;

            UnityEngine.Debug.LogWarning(LOG_WARNING_NO_SETTINGS_FOUND);

#if UNITY_EDITOR
            BufferedInstance = CreateInstance<HandTrackingSettings>();
            UnityEditor.AssetDatabase.CreateAsset(BufferedInstance, MY_ASSET_SETTINGS_PATH);
            UnityEditor.AssetDatabase.SaveAssets();
#else
            BufferedInstance = CreateInstance<HandTrackingSettings>();
#endif

            return BufferedInstance;
        }

        
        /// <summary>
        ///     Remove every providers
        /// </summary>
        [UnityEngine.ContextMenu("Empty Tracking")]
        private void EmptyTracking()
        {
            handTrackingPreferencesOrder.Empty();
        }
        #endregion


        #region PROVIDER SETUP METHODS
        /// <summary>
        ///     Initialize every provider set
        /// </summary>
        public void InitializeProviders()
        {
            if (ProvidersAreInitialized)
                return;

            _providerTypes = handTrackingPreferencesOrder.GetProviders(UnityEngine.Application.platform,
                UnityEngine.XR.XRSettings.loadedDeviceName);

            int providerCount = _providerTypes.Length;
            _providerInstances = new object[providerCount];
            for (int i = 0; i < providerCount; i++)
            {
                System.Type provider = _providerTypes[i];
                if (provider == null)
                    continue;

                System.Reflection.MethodInfo initMethod = provider.GetMethod(ReflectionNames.INIT_PROVIDER_METHOD_NAME);
                if (initMethod == null)
                {
                    if (verbose && !hideWarnings)
                        UnityEngine.Debug.LogWarning(string.Format(LOG_WARNING_NO_INITIALIZING_METHOD_FOUND,
                            provider.FullName));
                    _providerInstances[i] = null;
                    continue;
                }

                object instance = System.Activator.CreateInstance(provider);
                if ((bool) initMethod.Invoke(instance, null))
                {
                    if (verbose)
                        UnityEngine.Debug.Log(string.Format(LOG_PROVIDER_INITIALIZED, provider.FullName));
                    _providerInstances[i] = instance;
                }
                else
                {
                    System.Reflection.MethodInfo cleanMethod =
                        provider.GetMethod(ReflectionNames.CLEANUP_PROVIDER_METHOD_NAME);
                    if (cleanMethod != null)
                    {
                        if (verbose && !hideWarnings)
                            UnityEngine.Debug.LogWarning(string.Format(LOG_WARNING_INITIALIZING_FAILED,
                                provider.FullName));
                        cleanMethod.Invoke(instance, null);
                    }
                    else if (verbose && !hideWarnings)
                        UnityEngine.Debug.LogWarning(string.Format(LOG_WARNING_IMPOSSIBLE_TO_CLEAN, provider.FullName));

                    _providerInstances[i] = null;
                }
            }

            ProvidersAreInitialized = true;
        }

        /// <summary>
        ///     Clean every provider set
        /// </summary>
        public void CleanProviders()
        {
            if (!ProvidersAreInitialized)
                return;

            for (int i = 0; i < _providerTypes.Length; i++)
            {
                System.Type provider = _providerTypes[i];
                object instance = _providerInstances[i];
                if (provider == null || instance == null)
                    continue;

                System.Reflection.MethodInfo cleanMethod =
                    provider.GetMethod(ReflectionNames.CLEANUP_PROVIDER_METHOD_NAME);
                if (cleanMethod == null || !(bool) cleanMethod.Invoke(instance, null))
                {
                    if (verbose && !hideWarnings)
                        UnityEngine.Debug.LogWarning(string.Format(LOG_WARNING_IMPOSSIBLE_TO_CLEAN, provider.FullName));
                }
                else if (verbose)
                    UnityEngine.Debug.Log(string.Format(LOG_PROVIDER_CLEANED, provider.FullName));
            }

            ProvidersAreInitialized = false;
        }

        /// <summary>
        ///     Search the first tracking provider available
        /// </summary>
        /// <param name="providerType">The provider type found</param>
        /// <param name="providerInstance">The provider instance found</param>
        /// <returns></returns>
        public bool GetFirstAvailableProviderWithInstance(out System.Type providerType, out object providerInstance)
        {
            for (int i = 0; i < _providerTypes.Length; i++)
            {
                providerType = _providerTypes[i];
                providerInstance = _providerInstances[i];
                if (providerType == null || providerInstance == null)
                    continue;

                System.Reflection.MethodInfo isPresentMethod =
                    providerType.GetMethod(ReflectionNames.IS_PRESENT_PROVIDER_METHOD_NAME);
                if (isPresentMethod != null && (bool) isPresentMethod.Invoke(providerInstance, null))
                    return true;

                if (verbose)
                    UnityEngine.Debug.Log(string.Format(LOG_PROVIDER_NOT_PRESENT, providerType.FullName));
            }

            providerType = null;
            providerInstance = null;
            return false;
        }
        #endregion

    }

}