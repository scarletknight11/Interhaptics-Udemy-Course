using System.Linq;


namespace Interhaptics.HandTracking.Types
{

    [System.Serializable]
    public sealed class HandTrackingPreferences
    {

        #region NESTED STRUSCTS
        /// <summary>
        ///     Information data to characterize a tracking provider information
        /// </summary>
        [System.Serializable]
        public struct SHandTrackingProviderInfo
        {

            #region DEFAULT VALUES
            public const string DEFAULT_TRACKING_NAME = "GENERIC";
            public const bool DEFAULT_CAN_HAVE_SKELETON = false;
            public const string DEFAULT_DESCRIPTION = "A generic tracking provider"; 
            public const string DEFAULT_PROVIDER_TYPE = "Interhaptics.HandTracking.TrackingProviders.GenericTrackingProvider";
            #endregion


            #region FIELDS
            /// <summary>
            ///     The tracking name
            /// </summary>
            public string trackingName;
            /// <summary>
            ///     True if the tracking support skeletal tracking
            /// </summary>
            public bool canHaveSkeleton;
            /// <summary>
            ///     The tracking description
            /// </summary>
            public string description;
            /// <summary>
            ///     The tracking provider type (with namespaces)
            /// </summary>
            public string providerType;
            #endregion


            #region CONSTRUCTORS
            /// <summary>
            ///     The tracking provider info constructor
            /// </summary>
            /// <param name="trackingName">The tracking name</param>
            /// <param name="canHaveSkeleton">If the tracking support skeletal tracking</param>
            /// <param name="description">The tracking description</param>
            /// <param name="providerType">The provider type (fullname)</param>
            public SHandTrackingProviderInfo(string trackingName = DEFAULT_TRACKING_NAME,
                bool canHaveSkeleton = DEFAULT_CAN_HAVE_SKELETON, string description = DEFAULT_DESCRIPTION,
                string providerType = DEFAULT_PROVIDER_TYPE)
            {
                this.trackingName = trackingName;
                this.canHaveSkeleton = canHaveSkeleton;
                this.description = description;
                this.providerType = providerType;
            }
            #endregion

        }
        #endregion


        #region CONSTS DEBUG MESSAGES
        // LOGS
        private const string LOG_IMPOSSIBLE_TO_LOAD_PROVIDER = "<b>[Interhaptics.HandTracking]</b> Impossible to load {0}";
        private const string LOG_PROVIDER_LOADED = "<b>[Interhaptics.HandTracking]</b> {0} was correctly loaded";

        // WARNINGS
        private const string DEFAULT_LOG_WARNING_PROVIDER_NOT_COMPATIBLE = "<b>[Interhaptics.HandTracking]</b> provider {0} is not compatible with your current setup so it was unload";
        private const string LOG_WARNING_PROVIDER_PLATFORM_NOT_COMPATIBLE = "<b>[Interhaptics.HandTracking]</b> provider {0} is not compatible with the current runtime platform so it was unloaded";
        private const string LOG_WARNING_PROVIDER_XR_NOT_COMPATIBLE = "<b>[Interhaptics.HandTracking]</b> provider {0} is not compatible with the current XR sdk so it was unloaded";
        #endregion


        #if UNITY_EDITOR
        #region STATIC PROVIDER TARGET PLATFORM COMPATIBILITY CHECKING
        /// <summary>
        ///     Define if a collection of player platform is compatible with the actual one
        /// </summary>
        /// <param name="providerRuntimePlatforms">A player platform</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithTarget(
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> providerRuntimePlatforms)
        {
            if (providerRuntimePlatforms == null)
                return false;

            UnityEngine.RuntimePlatform[] runtimePlatformsToCheck;
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.StandaloneWindows:
                    runtimePlatformsToCheck = new[]
                    {
                        UnityEngine.RuntimePlatform.WindowsEditor,
                        UnityEngine.RuntimePlatform.WindowsPlayer
                    };
                    break;
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    runtimePlatformsToCheck = new[]
                    {
                        UnityEngine.RuntimePlatform.WindowsEditor,
                        UnityEngine.RuntimePlatform.WindowsPlayer
                    };
                    break;
                case UnityEditor.BuildTarget.WSAPlayer:
                    runtimePlatformsToCheck = new[]
                    {
                        UnityEngine.RuntimePlatform.WSAPlayerX64,
                        UnityEngine.RuntimePlatform.WSAPlayerX86,
                        UnityEngine.RuntimePlatform.WSAPlayerARM
                    };
                    break;
                case UnityEditor.BuildTarget.Android:
                    runtimePlatformsToCheck = new[]
                    {
                        UnityEngine.RuntimePlatform.Android
                    };
                    break;
                case UnityEditor.BuildTarget.StandaloneLinux64:
                    runtimePlatformsToCheck = new[]
                    {
                        UnityEngine.RuntimePlatform.LinuxEditor,
                        UnityEngine.RuntimePlatform.LinuxPlayer
                    };
                    break;
                case UnityEditor.BuildTarget.StandaloneOSX:
                    runtimePlatformsToCheck = new[]
                    {
                        UnityEngine.RuntimePlatform.OSXEditor,
                        UnityEngine.RuntimePlatform.OSXPlayer
                    };
                    break;
                default:
                    return false;
            }

            return providerRuntimePlatforms.Any(runtimePlatformsToCheck.Contains);
        }

        /// <summary>
        ///     Define if a provider is compatible with the actual player platform
        /// </summary>
        /// <param name="provider">A tracking provider</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithTarget(Interfaces.ITrackingProvider provider)
        {
            return provider != null &&
                   ProviderIsCompatibleWithTarget(provider.PlatformCompatibilities());
        }

        /// <summary>
        ///     Define if a provider is compatible with the actual player platform
        /// </summary>
        /// <param name="providerType">A tracking provider type</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithTarget(System.Type providerType)
        {
            return providerType != null &&
                   ProviderIsCompatibleWithTarget(providerType, System.Activator.CreateInstance(providerType));
        }

        /// <summary>
        ///     Define if a provider is compatible with the actual player platform
        /// </summary>
        /// <param name="providerType">A tracking provider type</param>
        /// <param name="providerInstance">Its instance</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithTarget(System.Type providerType, object providerInstance)
        {
            if (providerType == null || providerInstance == null)
                return false;

            System.Reflection.MethodInfo platformCompatibilitiesMethod =
                providerType.GetMethod(Tools.ReflectionNames.PLATFORM_COMPATIBILITIES_PROVIDER_METHOD_NAME);
            return platformCompatibilitiesMethod != null && ProviderIsCompatibleWithTarget(
                       (UnityEngine.RuntimePlatform[]) platformCompatibilitiesMethod.Invoke(providerInstance, null));
        }
        #endregion
        #endif


        #region STATIC PROVIDER PLATFORM COMPATIBILITY CHECKING
        /// <summary>
        ///     Define if two player platform collections are compatible
        /// </summary>
        /// <param name="providerRuntimePlatforms">Player platform compatibility of a provider</param>
        /// <param name="runtimePlatformsToCheck">A player platform collection to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithPlatforms(
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> providerRuntimePlatforms,
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> runtimePlatformsToCheck)
        {
            return providerRuntimePlatforms != null && runtimePlatformsToCheck != null &&
                   providerRuntimePlatforms.Any(runtimePlatformsToCheck.Contains);
        }

        /// <summary>
        ///     Define if a tracking provider is compatible with a player platform collection
        /// </summary>
        /// <param name="provider">A tracking provider</param>
        /// <param name="runtimePlatformsToCheck">A player platform collection to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithPlatforms(Interfaces.ITrackingProvider provider,
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> runtimePlatformsToCheck)
        {
            return provider != null &&
                   ProviderIsCompatibleWithPlatforms(provider.PlatformCompatibilities(), runtimePlatformsToCheck);
        }

        /// <summary>
        ///     Define if a tracking provider is compatible with a player platform collection
        /// </summary>
        /// <param name="providerType">A tracking provider type</param>
        /// <param name="runtimePlatformsToCheck">A player platform collection to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithPlatforms(System.Type providerType,
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> runtimePlatformsToCheck)
        {
            return providerType != null && runtimePlatformsToCheck != null &&
                   ProviderIsCompatibleWithPlatforms(providerType, System.Activator.CreateInstance(providerType),
                       runtimePlatformsToCheck);
        }


        /// <summary>
        ///     Define if a tracking provider is compatible with a player platform collection
        /// </summary>
        /// <param name="providerType">A tracking provider type</param>
        /// <param name="providerInstance">Its instance</param>
        /// <param name="runtimePlatformsToCheck">A player platform collection to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithPlatforms(System.Type providerType, object providerInstance,
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> runtimePlatformsToCheck)
        {
            if (providerType == null || providerInstance == null || runtimePlatformsToCheck == null)
                return false;

            System.Reflection.MethodInfo platformCompatibilitiesMethod =
                providerType.GetMethod(Tools.ReflectionNames.PLATFORM_COMPATIBILITIES_PROVIDER_METHOD_NAME);
            return platformCompatibilitiesMethod != null &&
                   ProviderIsCompatibleWithPlatforms(
                       (UnityEngine.RuntimePlatform[]) platformCompatibilitiesMethod.Invoke(providerInstance, null),
                       runtimePlatformsToCheck);
        }
        #endregion


        #region STATIC PROVIDER XR COMPATIBILITY CHECKING
        /// <summary>
        ///     Define if a XR SDK collection is compatible with a specific SDK
        /// </summary>
        /// <param name="providerXrSdks">XR SDK compatibility collection</param>
        /// <param name="xrSdkToCheck">An SDK to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithXrSdk(System.Collections.Generic.IEnumerable<string> providerXrSdks,
            string xrSdkToCheck)
        {
            return providerXrSdks != null && xrSdkToCheck != null && providerXrSdks.Contains(xrSdkToCheck);
        }

        /// <summary>
        ///     Define if tracking provider is compatible with a specific SDK
        /// </summary>
        /// <param name="provider">A tracking provider</param>
        /// <param name="xrSdkToCheck">An SDK to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithXrSdk(Interfaces.ITrackingProvider provider, string xrSdkToCheck)
        {
            return provider != null && ProviderIsCompatibleWithXrSdk(provider.XrSdkCompatibilities(), xrSdkToCheck);
        }

        /// <summary>
        ///     Define if tracking provider is compatible with a specific SDK
        /// </summary>
        /// <param name="providerType">A tracking provider type</param>
        /// <param name="xrSdkToCheck">An SDK to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithXrSdk(System.Type providerType, string xrSdkToCheck)
        {
            return providerType != null && ProviderIsCompatibleWithXrSdk(providerType,
                       System.Activator.CreateInstance(providerType), xrSdkToCheck);
        }

        /// <summary>
        ///     Define if tracking provider is compatible with a specific SDK
        /// </summary>
        /// <param name="providerType">A tracking provider type</param>
        /// <param name="providerInstance">Its instance</param>
        /// <param name="xrSdkToCheck">An SDK to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWithXrSdk(System.Type providerType, object providerInstance,
            string xrSdkToCheck)
        {
            if (providerType == null || providerInstance == null || xrSdkToCheck == null)
                return false;

            System.Reflection.MethodInfo xrCompatibilitiesMethod =
                providerType.GetMethod(Tools.ReflectionNames.XR_SDK_COMPATIBILITIES_PROVIDER_METHOD_NAME);
            return xrCompatibilitiesMethod != null &&
                   ProviderIsCompatibleWithXrSdk((string[]) xrCompatibilitiesMethod.Invoke(providerInstance, null),
                       xrSdkToCheck);
        }
        #endregion


        #region STATIC PROVIDER COMPATIBILITY CHECKING
        /// <summary>
        ///     Define if collections of player runtime and XR SDKs are compatible together 
        /// </summary>
        /// <param name="providerRuntimePlatforms">A tracking provider runtime player compatibility collections</param>
        /// <param name="runtimePlatformsToCheck">A runtime player compatibility collections to check</param>
        /// <param name="providerXrSdks">A tracking provider XR SDK compatibility collections</param>
        /// <param name="xrSdkToCheck">An SDK to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWith(
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> providerRuntimePlatforms,
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> runtimePlatformsToCheck,
            System.Collections.Generic.IEnumerable<string> providerXrSdks, string xrSdkToCheck)
        {
            return ProviderIsCompatibleWithPlatforms(providerRuntimePlatforms, runtimePlatformsToCheck) &&
                   ProviderIsCompatibleWithXrSdk(providerXrSdks, xrSdkToCheck);
        }

        /// <summary>
        ///     Define if a tracking provider is compatible with collections of player runtime and an XR SDK 
        /// </summary>
        /// <param name="provider">A tracking provider</param>
        /// <param name="runtimePlatformsToCheck">A runtime player compatibility collections to check</param>
        /// <param name="xrSdkToCheck">An SDK to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWith(Interfaces.ITrackingProvider provider,
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> runtimePlatformsToCheck,
            string xrSdkToCheck)
        {
            return ProviderIsCompatibleWithPlatforms(provider, runtimePlatformsToCheck) &&
                   ProviderIsCompatibleWithXrSdk(provider, xrSdkToCheck);
        }

        /// <summary>
        ///     Define if a tracking provider is compatible with collections of player runtime and an XR SDK 
        /// </summary>
        /// <param name="providerType">A tracking provider type</param>
        /// <param name="runtimePlatformsToCheck">A runtime player compatibility collections to check</param>
        /// <param name="xrSdkToCheck">An SDK to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWith(System.Type providerType,
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> runtimePlatformsToCheck,
            string xrSdkToCheck)
        {
            return ProviderIsCompatibleWith(providerType, System.Activator.CreateInstance(providerType),
                runtimePlatformsToCheck, xrSdkToCheck);
        }

        /// <summary>
        ///     Define if a tracking provider is compatible with collections of player runtime and an XR SDK 
        /// </summary>
        /// <param name="providerType">A tracking provider type</param>
        /// <param name="providerInstance">Its instance</param>
        /// <param name="runtimePlatformsToCheck">A runtime player compatibility collections to check</param>
        /// <param name="xrSdkToCheck">An SDK to check</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool ProviderIsCompatibleWith(System.Type providerType, object providerInstance,
            System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform> runtimePlatformsToCheck,
            string xrSdkToCheck)
        {
            return ProviderIsCompatibleWithPlatforms(providerType, providerInstance, runtimePlatformsToCheck) &&
                   ProviderIsCompatibleWithXrSdk(providerType, providerInstance, xrSdkToCheck);
        }
        #endregion


        #region SERIALIZED FIELDS
        [UnityEngine.SerializeField]
        private System.Collections.Generic.List<SHandTrackingProviderInfo> data =
            new System.Collections.Generic.List<SHandTrackingProviderInfo>();
        #endregion


        #region GETTERS
        /// <summary>
        ///     Clone an instance
        /// </summary>
        /// <returns>An HandTrackingPreferences cloned</returns>
        public HandTrackingPreferences Clone()
        {
            return (HandTrackingPreferences)MemberwiseClone();
        }

        /// <summary>
        ///     Determines whether any element of a sequence exists or satisfies a condition.
        /// </summary>
        /// <param name="checker">The checking method</param>
        /// <returns>True if the instance contains any element. False otherwise</returns>
        public bool Any(System.Func<SHandTrackingProviderInfo, bool> checker)
        {
            return data.Any(checker);
        }

        /// <summary>
        ///     Get the provider information
        /// </summary>
        /// <returns>The data stored in the object</returns>
        public System.Collections.Generic.List<SHandTrackingProviderInfo> GetProviderInfo()
        {
            return data;
        }

        /// <summary>
        ///     Get every provider type matching to the stored data
        /// </summary>
        /// <param name="verbose">Define if the call will log messages</param>
        /// <returns>An array of tracking provider types</returns>
        public System.Type[] GetProviders(bool verbose = false)
        {
            return GetProvidersBehaviour(new System.Func<System.Type, bool>[0], new string[0], verbose);
        }

        /// <summary>
        ///     Get every provider type matching to the stored data and compatible with a platform
        /// </summary>
        /// <param name="platform">The platform to check</param>
        /// <param name="verbose">Define if the call will log messages</param>
        /// <returns>An array of tracking provider types</returns>
        public System.Type[] GetProviders(UnityEngine.RuntimePlatform platform, bool verbose = false)
        {
            return GetProvidersBehaviour(
                new[]
                {
                    (System.Func<System.Type, bool>) (providerType =>
                        ProviderIsCompatibleWithPlatforms(providerType, new[] {platform}))
                }, new[] {LOG_WARNING_PROVIDER_PLATFORM_NOT_COMPATIBLE}, verbose);
        }

        /// <summary>
        ///     Get every provider type matching to the stored data and compatible with a XR SDK
        /// </summary>
        /// <param name="xrSdk">The SDK to check</param>
        /// <param name="verbose">Define if the call will log messages</param>
        /// <returns>An array of tracking provider types</returns>
        public System.Type[] GetProviders(string xrSdk, bool verbose = false)
        {
            return GetProvidersBehaviour(
                new[]
                {
                    (System.Func<System.Type, bool>) (providerType =>
                        ProviderIsCompatibleWithXrSdk(providerType, xrSdk))
                }, new[] {LOG_WARNING_PROVIDER_XR_NOT_COMPATIBLE}, verbose);
        }

        /// <summary>
        ///     Get every provider type matching to the stored data and compatible with a platform and a XR SDK
        /// </summary>
        /// <param name="platform">The platform to check</param>
        /// <param name="xrSdk">The SDK to check</param>
        /// <param name="verbose">Define if the call will log messages</param>
        /// <returns>An array of tracking provider types</returns>
        public System.Type[] GetProviders(UnityEngine.RuntimePlatform platform, string xrSdk, bool verbose = false)
        {
            return GetProvidersBehaviour(
                new System.Func<System.Type, bool>[]
                {
                    providerType =>
                        ProviderIsCompatibleWithPlatforms(providerType, new[] {platform}),
                    providerType =>
                        ProviderIsCompatibleWithXrSdk(providerType, xrSdk)
                }, new[] {LOG_WARNING_PROVIDER_PLATFORM_NOT_COMPATIBLE, LOG_WARNING_PROVIDER_XR_NOT_COMPATIBLE}, verbose);
        }

        private System.Type[] GetProvidersBehaviour(System.Func<System.Type, bool>[] checkAfterLoading,
            string[] warningMessageIfCheckerFailed, bool verbose = false)
        {
            System.Type[] res = new System.Type[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                SHandTrackingProviderInfo providerInfo = data[i];

                System.Type providerType = null;
                foreach (System.Reflection.Assembly a in Tools.ReflectionNames.GetCompatibleAssemblies())
                {
                    try
                    {
                        providerType = a.GetType(providerInfo.providerType);                        
                    }
                    catch (System.Exception)
                    {
                        continue;
                    }

                    if (providerType != null)
                        break;
                }

                if (providerType == null)
                {
                    if (verbose)
                        UnityEngine.Debug.LogWarning(string.Format(LOG_IMPOSSIBLE_TO_LOAD_PROVIDER,
                            providerInfo.providerType));
                    res[i] = null;
                }
                else
                {
                    if (verbose)
                        UnityEngine.Debug.Log(string.Format(LOG_PROVIDER_LOADED, providerInfo.providerType));
                    System.Collections.IEnumerator checkerEnumerator = checkAfterLoading.GetEnumerator();
                    System.Collections.IEnumerator messageEnumerator = warningMessageIfCheckerFailed.GetEnumerator();

                    bool oneCheckerFailed = false;
                    while (checkerEnumerator.MoveNext() && messageEnumerator.MoveNext())
                    {
                        System.Func<System.Type, bool> checker = (System.Func<System.Type, bool>)checkerEnumerator.Current;
                        if (checker == null || checker.Invoke(providerType))
                            continue;

                        string warningMessage = (string)messageEnumerator.Current;
                        if (verbose)
                            UnityEngine.Debug.LogWarning(string.Format(
                                warningMessage ?? DEFAULT_LOG_WARNING_PROVIDER_NOT_COMPATIBLE, providerType.FullName));
                        oneCheckerFailed = true;
                    }
                    res[i] = oneCheckerFailed ? null : providerType;
                }
            }

            return res;
        }
        #endregion


        #region MODIFIERS
        /// <summary>
        ///     Empty the data stored
        /// </summary>
        public void Empty()
        {
            data.RemoveAll(e => true);
        }

        /// <summary>
        ///     Remove a specific tracking provider type
        /// </summary>
        /// <param name="type">The provider type to remove</param>
        public void Remove(System.Type type)
        {
            if (type == null)
                return;
            data.RemoveAll(e => e.providerType == type.FullName);
        }

        /// <summary>
        ///     Remove a specific tracking provider
        /// </summary>
        /// <param name="element">The information to remove</param>
        public void Remove(SHandTrackingProviderInfo element)
        {
            data.Remove(element);
        }
        #endregion

    }

}
