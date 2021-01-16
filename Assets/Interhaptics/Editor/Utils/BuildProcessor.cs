#if UNITY_EDITOR
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
using System.Linq;


namespace Interhaptics.Editor.Utils
{

    [UnityEditor.InitializeOnLoad]
    public static class BuildProcessor
    {

        #region CONSTS DEBUG MESSAGES
        // WARNINGS
#if INTERHAPTICS_GHT_ENABLED
        private const string GHT_LOG_WARNING_IMPOSSIBLE_TO_EXPORT_PROVIDER = "<b>[Interhaptics.HandTracking]</b> Impossible to export provider {0} to the targeted platform";
#endif
#if INTERHAPTICS_HAR_ENABLED
        private const string HAR_LOG_WARNING_IMPOSSIBLE_TO_EXPORT_PROVIDER = "<b>[Interhaptics.HapticRenderer]</b> Impossible to export provider {0} to the targeted platform";
#endif
        #endregion


        #region CONSTRUCTORS
        static BuildProcessor()
        {
            UnityEditor.BuildPlayerWindow.RegisterBuildPlayerHandler(null);
            UnityEditor.BuildPlayerWindow.RegisterBuildPlayerHandler(OnBuild);
        }
        #endregion


        #region BUILDING METHODS
        private static void OnBuild(UnityEditor.BuildPlayerOptions options)
        {
#if INTERHAPTICS_GHT_ENABLED
            HandTracking.Tools.HandTrackingSettings mySettings = HandTracking.Tools.HandTrackingSettings.EditorLoadSettings();
            foreach (System.Reflection.Assembly assembly in HandTracking.Tools.ReflectionNames
                .GetInterhapticsTrackingProviderAssemblies())
            {
                System.Type type;
                try
                {
                    type = assembly.GetTypes()
                        .First(t => t.GetInterfaces().Contains(typeof(HandTracking.Interfaces.ITrackingProvider)));
                }
                catch (System.Exception)
                {
                    continue;
                }

                bool b;
                if (mySettings)
                    b = mySettings.handTrackingPreferencesOrder.Any(e => e.providerType == type.FullName) &&
                        GHTCompatibleAndCanExport(type);
                else
                    b = GHTCompatibleAndCanExport(type);

                string pathSeparator = new string(new[] {System.IO.Path.DirectorySeparatorChar});
                string dataPath =
                    System.Text.RegularExpressions.Regex.Replace(UnityEngine.Application.dataPath, @"\/",
                        pathSeparator);
                string path =
                    assembly.Location.Remove(0, dataPath.Length - 6 - (dataPath.EndsWith(pathSeparator) ? 1 : 0));

                UnityEditor.PluginImporter providerPlugin =
                    (UnityEditor.PluginImporter) UnityEditor.AssetImporter.GetAtPath(path);
                if (providerPlugin)
                    providerPlugin.SetIncludeInBuildDelegate(s => b);
                if (!b && mySettings && mySettings.handTrackingPreferencesOrder.Any(info => true))
                    mySettings.handTrackingPreferencesOrder.Remove(type);
            }
#endif

#if INTERHAPTICS_HAR_ENABLED
            HapticRenderer.Devices.HapticDevicesPreferences HDP = UnityEngine.ScriptableObject.CreateInstance<HapticRenderer.Devices.HapticDevicesPreferences>();

            foreach (System.Reflection.Assembly assembly in HapticRenderer.Tools.ReflectionNames
                .GetInterhapticsTrackingProviderAssemblies())
            {
                System.Type type;
                try
                {
                    type = assembly.GetTypes()
                        .First(t => t.GetInterfaces().Contains(typeof(HapticRenderer.Interfaces.IHapticProvider)));
                }
                catch (System.Exception)
                {
                    continue;
                }

                bool b;

                if (b = HARCompatibleAndCanExport(type))
                {
                    string s = type.FullName;
                    HDP.AddDevice(s);
                }

                string pathSeparator = new string(new[] { System.IO.Path.DirectorySeparatorChar });
                string dataPath =
                    System.Text.RegularExpressions.Regex.Replace(UnityEngine.Application.dataPath, @"\/",
                        pathSeparator);
                string path =
                    assembly.Location.Remove(0, dataPath.Length - 6 - (dataPath.EndsWith(pathSeparator) ? 1 : 0));

                UnityEditor.PluginImporter providerPlugin =
                    (UnityEditor.PluginImporter)UnityEditor.AssetImporter.GetAtPath(path);
                if (providerPlugin)
                    providerPlugin.SetIncludeInBuildDelegate(s => b);
            }

            UnityEditor.AssetDatabase.CreateAsset(HDP, HapticRenderer.Devices.HapticDevicesPreferences.PATH_TO_PREFERENCES);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            UnityEditor.BuildPipeline.BuildPlayer(options);
        }
        #endregion


        #region PUBLIC STATIC METHOD
#if INTERHAPTICS_GHT_ENABLED
        public static System.Reflection.MethodInfo GHTCanExportMethod(string providerType)
        {
            System.Type t = null;
            return HandTracking.Tools.ReflectionNames.GetInterhapticsTrackingProviderAssemblies()
                       .Any(assembly => (t = assembly.GetType(providerType)) != null) ? GHTCanExportMethod(t) : null;
        }

        public static System.Reflection.MethodInfo GHTCanExportMethod(System.Type providerType)
        {
            return providerType.GetMethod(HandTracking.Tools.ReflectionNames.CAN_EXPORT_PROVIDER_METHOD_NAME);
        }

        public static bool GHTCanExport(string providerType)
        {
            System.Type t = null;
            return HandTracking.Tools.ReflectionNames.GetInterhapticsTrackingProviderAssemblies()
                       .Any(assembly => (t = assembly.GetType(providerType)) != null) && GHTCanExport(t);
        }

        public static bool GHTCanExport(System.Type providerType)
        {
            return GHTCanExport(providerType, System.Activator.CreateInstance(providerType));
        }

        public static bool GHTCanExport(System.Type providerType, object providerInstance)
        {
            System.Reflection.MethodInfo method =
                providerType.GetMethod(HandTracking.Tools.ReflectionNames.CAN_EXPORT_PROVIDER_METHOD_NAME);
            return (method == null) || (bool) method.Invoke(providerInstance, null);
        }

        public static bool GHTCompatibleAndCanExport(string providerType)
        {
            System.Type t = null;
            return HandTracking.Tools.ReflectionNames.GetInterhapticsTrackingProviderAssemblies()
                       .Any(assembly => (t = assembly.GetType(providerType)) != null) && GHTCompatibleAndCanExport(t);
        }

        public static bool GHTCompatibleAndCanExport(System.Type providerType)
        {
            return GHTCompatibleAndCanExport(providerType, System.Activator.CreateInstance(providerType));
        }

        public static bool GHTCompatibleAndCanExport(System.Type providerType, object providerInstance)
        {
            System.Reflection.MethodInfo method =
                providerType.GetMethod(HandTracking.Tools.ReflectionNames.CAN_EXPORT_PROVIDER_METHOD_NAME);

            if (!HandTracking.Types.HandTrackingPreferences.ProviderIsCompatibleWithTarget(providerType, providerInstance))
            {
                UnityEngine.Debug.LogWarning(GHT_LOG_WARNING_IMPOSSIBLE_TO_EXPORT_PROVIDER);
                return false;
            }

            if (method != null)
                return (bool) method.Invoke(providerInstance, null);

            return true;
        }
#endif
        #endregion


        #region INTERNAL STATIC METHOD
#if INTERHAPTICS_HAR_ENABLED
        internal static System.Reflection.MethodInfo HARCanExportMethod(string providerType)
        {
            System.Type t = null;
            return HapticRenderer.Tools.ReflectionNames.GetInterhapticsTrackingProviderAssemblies()
                       .Any(assembly => (t = assembly.GetType(providerType)) != null) ? HARCanExportMethod(t) : null;
        }

        internal static System.Reflection.MethodInfo HARCanExportMethod(System.Type providerType)
        {
            return providerType.GetMethod(HapticRenderer.Tools.ReflectionNames.CAN_EXPORT_PROVIDER_METHOD_NAME);
        }

        internal static bool HARCanExport(string providerType)
        {
            System.Type t = null;
            return HapticRenderer.Tools.ReflectionNames.GetInterhapticsTrackingProviderAssemblies()
                       .Any(assembly => (t = assembly.GetType(providerType)) != null) && HARCanExport(t);
        }

        internal static bool HARCanExport(System.Type providerType)
        {
            return HARCanExport(providerType, System.Activator.CreateInstance(providerType));
        }

        internal static bool HARCanExport(System.Type providerType, object providerInstance)
        {
            System.Reflection.MethodInfo method =
                providerType.GetMethod(HapticRenderer.Tools.ReflectionNames.CAN_EXPORT_PROVIDER_METHOD_NAME);
            return (method == null) || (bool)method.Invoke(providerInstance, null);
        }

        internal static bool HARCompatibleAndCanExport(string providerType)
        {
            System.Type t = null;
            return HapticRenderer.Tools.ReflectionNames.GetInterhapticsTrackingProviderAssemblies()
                       .Any(assembly => (t = assembly.GetType(providerType)) != null) && HARCompatibleAndCanExport(t);
        }

        internal static bool HARCompatibleAndCanExport(System.Type providerType)
        {
            return HARCanExport(providerType) && HARProviderIsCompatibleWithTarget(providerType);
        }

        internal static bool HARCompatibleAndCanExport(System.Type providerType, object providerInstance)
        {
            System.Reflection.MethodInfo method_can_export =
                providerType.GetMethod(HapticRenderer.Tools.ReflectionNames.CAN_EXPORT_PROVIDER_METHOD_NAME);
            System.Reflection.MethodInfo method_platform_compatibility =
                providerType.GetMethod(HapticRenderer.Tools.ReflectionNames.PLATFORM_COMPATIBILITIES_PROVIDER_METHOD_NAME);

            bool compatible = true;

            if (method_platform_compatibility != null)
                compatible = ((System.Collections.Generic.IEnumerable<UnityEngine.RuntimePlatform>)method_platform_compatibility.Invoke(providerInstance, null))
                        .Contains(UnityEngine.Application.platform);

            if (method_can_export != null)
                return (bool)method_can_export.Invoke(providerInstance, null) && compatible;

            return compatible;
        }

        /// <summary>
        ///     Define if a collection of player platform is compatible with the actual one
        /// </summary>
        /// <param name="providerRuntimePlatforms">A player platform</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool HARProviderIsCompatibleWithTarget(
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
        /// <param name="provider">A haptic provider</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool HARProviderIsCompatibleWithTarget(HapticRenderer.Interfaces.IHapticProvider provider)
        {
            return provider != null &&
                   HARProviderIsCompatibleWithTarget(provider.PlatformCompatibilities());
        }

        /// <summary>
        ///     Define if a provider is compatible with the actual player platform
        /// </summary>
        /// <param name="providerType">A haptic provider type</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool HARProviderIsCompatibleWithTarget(System.Type providerType)
        {
            return providerType != null &&
                   HARProviderIsCompatibleWithTarget(providerType, System.Activator.CreateInstance(providerType));
        }

        /// <summary>
        ///     Define if a provider is compatible with the actual player platform
        /// </summary>
        /// <param name="providerType">A haptic provider type</param>
        /// <param name="providerInstance">Its instance</param>
        /// <returns>True if it's compatible. False otherwise</returns>
        public static bool HARProviderIsCompatibleWithTarget(System.Type providerType, object providerInstance)
        {
            if (providerType == null || providerInstance == null)
                return false;

            System.Reflection.MethodInfo platformCompatibilitiesMethod =
                providerType.GetMethod(HapticRenderer.Tools.ReflectionNames.PLATFORM_COMPATIBILITIES_PROVIDER_METHOD_NAME);
            return platformCompatibilitiesMethod != null && HARProviderIsCompatibleWithTarget(
                       (UnityEngine.RuntimePlatform[])platformCompatibilitiesMethod.Invoke(providerInstance, null));
        }
#endif
        #endregion

    }

}
#endif