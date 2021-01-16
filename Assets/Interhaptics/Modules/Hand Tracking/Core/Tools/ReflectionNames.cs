using System.Linq;


namespace Interhaptics.HandTracking.Tools
{

    public static class ReflectionNames
    {

        #region ASSEMBLY NAMES
        public const string DEFAULT_ASSEMBLY_NAME = "Assembly-CSharp";
        // ReSharper disable once MemberCanBePrivate.Global
        public const string ASSEMBLY_PREFIX_NAME_FOR_PROVIDERS = "Interhaptics.TrackingProvider.";
#if GHT_ENABLED
        public const string ASSEMBLY_PREFIX_NAME_FOR_PROVIDERS = "Interhaptics.TrackingProvider.";
#endif
        #endregion


        #region HAND TRACKING PROVIDER INFO FIELDS NAMES
        public const string TRACKING_NAME_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME = "trackingName";
        public const string CAN_HAVE_SKELETON_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME = "canHaveSkeleton";
        public const string DESCRIPTION_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME = "description";
        public const string PROVIDER_TYPE_HAND_TRACKING_PROVIDER_INFO_FIELD_NAME = "providerType";
        #endregion


        #region PROVIDERS METHODS NAMES
        // TRACKING CHARACTERISTICS
        public const string CAN_HAVE_SKELETON_PROVIDER_METHOD_NAME = "CanHaveSkeleton";
        public const string DESCRIPTION_PROVIDER_METHOD_NAME = "Description";
        public const string DISPLAY_NAME_PROVIDER_METHOD_NAME = "DisplayName";
        public const string DEVICE_CLASS_PROVIDER_METHOD_NAME = "DeviceClass";
        public const string POSITION_IS_RELATIVE_PROVIDER_METHOD_NAME = "PositionIsRelative";
        public const string ROTATION_IS_RELATIVE_PROVIDER_METHOD_NAME = "RotationIsRelative";
        public const string PLATFORM_COMPATIBILITIES_PROVIDER_METHOD_NAME = "PlatformCompatibilities";
        public const string XR_SDK_COMPATIBILITIES_PROVIDER_METHOD_NAME = "XrSdkCompatibilities";
        public const string MANUFACTURER_PROVIDER_METHOD_NAME = "Manufacturer";
        public const string VERSION_PROVIDER_METHOD_NAME = "Version";

        // PROVIDER SETUP
        public const string INIT_PROVIDER_METHOD_NAME = "Init";
        public const string CLEANUP_PROVIDER_METHOD_NAME = "Cleanup";
        public const string CAN_EXPORT_PROVIDER_METHOD_NAME = "CanExport";

        // PROVIDER TRACKING
        public const string IS_PRESENT_PROVIDER_METHOD_NAME = "IsPresent";
        public const string GET_GESTURE_PROVIDER_METHOD_NAME = "GetGesture";
        public const string GET_SKELETON_PROVIDER_METHOD_NAME = "GetSkeleton";
        #endregion


        #region PUBLIC METHODS
        /// <summary>
        ///     Get interhaptics assemblies in which a tracking provider can be
        /// </summary>
        /// <returns>An assembly collection</returns>
        public static System.Collections.Generic.IEnumerable<System.Reflection.Assembly>
            GetInterhapticsTrackingProviderAssemblies()
        {
            return GetAssemblies(assembly => assembly.FullName.StartsWith(ASSEMBLY_PREFIX_NAME_FOR_PROVIDERS));
        }

        /// <summary>
        ///     Get assemblies in which a tracking provider can be
        /// </summary>
        /// <returns>An assembly collection</returns>
        public static System.Collections.Generic.IEnumerable<System.Reflection.Assembly> GetCompatibleAssemblies()
        {
            return GetAssemblies(assembly =>
                assembly.FullName.StartsWith(ASSEMBLY_PREFIX_NAME_FOR_PROVIDERS) ||
                assembly.GetName().Name == DEFAULT_ASSEMBLY_NAME);
        }

        /// <summary>
        ///     Get assemblies in which a tracking provider can be depending on a parametrized checking method
        /// </summary>
        /// <param name="checker">A checking method</param>
        /// <returns>An assembly collection</returns>
        private static System.Collections.Generic.IEnumerable<System.Reflection.Assembly> GetAssemblies(
            System.Func<System.Reflection.Assembly, bool> checker)
        {
            return System.AppDomain.CurrentDomain.GetAssemblies().Where(checker);
        }
        #endregion

    }

}