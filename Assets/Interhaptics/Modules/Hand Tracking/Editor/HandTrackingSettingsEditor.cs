#if UNITY_EDITOR
namespace Interhaptics.HandTracking.Editor
{

    [UnityEditor.CustomEditor(typeof(Tools.HandTrackingSettings))]
    public class HandTrackingSettingsEditor : UnityEditor.Editor
    {

        #region LABEL CONSTS
        public const string SETTINGS_PATH = "Project/InterhapticsHandTracking";
        private const string SETTINGS_LABEL = "Interhaptics : Hand Tracking";

        private const string NONE_LABEL = "None";
        private const string XR_SDK_LABEL = "XR SDK : {0}";
        private const string TRACKING_SYSTEM_LABEL = "Tracking System : {0}";
        private const string PROVIDER_TYPE_LABEL = "Provider Type : {0}";
        #endregion


        #region GENERAL GUI SETTINGS
        private const string HELP_BOX_NAME = "HelpBox";

        // TEXTURE SETTINGS
        private const string GHT_TEXTURE_ASSET_PATH = "Assets/Interhaptics/Modules/Hand Tracking/Resources/ght.png";
        private const int GHT_TEXTURE_SIZE = 60;

        // SPACING SETTINGS
        private const int HORIZONTAL_SPACING_SIZE = 10;
        private const int VERTICAL_SPACING_SIZE = 10;

        // STYLE SETTINGS
        private static UnityEngine.Texture _image;
        #endregion


        #region LIFE CYCLES
        public void OnEnable()
        {
            _image =
                UnityEditor.AssetDatabase.LoadAssetAtPath(GHT_TEXTURE_ASSET_PATH, typeof(UnityEngine.Texture)) as
                    UnityEngine.Texture;
        }

        public override void OnInspectorGUI()
        {
            if (UnityEngine.Application.isPlaying && HandTrackingManager.Instance != null)
                DrawHelpBox();

            base.OnInspectorGUI();
        }
        #endregion


        #region PRIVATE STATIC METHODS
        private static void DrawHelpBox()
        {
            
            UnityEditor.EditorGUILayout.BeginHorizontal(HELP_BOX_NAME);
            if (_image != null)
                UnityEngine.GUILayout.Label(_image, UnityEngine.GUILayout.Height(GHT_TEXTURE_SIZE),
                    UnityEngine.GUILayout.Width(GHT_TEXTURE_SIZE));

            UnityEngine.GUILayout.Space(HORIZONTAL_SPACING_SIZE);

            UnityEditor.EditorGUILayout.BeginVertical();

            UnityEngine.GUIStyle labelStyle = UnityEngine.GUI.skin.label;
            labelStyle.fontStyle = UnityEngine.FontStyle.Bold;
            labelStyle.clipping = UnityEngine.TextClipping.Clip;
            UnityEngine.GUILayout.Label(
                string.Format(XR_SDK_LABEL,
                    string.IsNullOrEmpty(UnityEngine.XR.XRSettings.loadedDeviceName)
                        ? NONE_LABEL
                        : UnityEngine.XR.XRSettings.loadedDeviceName), labelStyle);
                
            bool tempV = Tools.HandTrackingSettings.Instance.verbose;
            Tools.HandTrackingSettings.Instance.verbose = false;
            string displayName =
                HandTrackingManager.Instance.ProviderTypeUsed == null ||
                HandTrackingManager.Instance.ProviderInstanceUsed == null
                    ? NONE_LABEL
                    : HandTrackingManager.Instance.DisplayName();
            string providerTypeName = HandTrackingManager.Instance.ProviderTypeUsed != null
                ? HandTrackingManager.Instance.ProviderTypeUsed.FullName
                : NONE_LABEL;
            Tools.HandTrackingSettings.Instance.verbose = tempV;

            UnityEngine.GUILayout.Space(VERTICAL_SPACING_SIZE);
            UnityEngine.GUILayout.Label(string.Format(TRACKING_SYSTEM_LABEL, displayName), labelStyle);
            UnityEngine.GUILayout.Label(string.Format(PROVIDER_TYPE_LABEL, providerTypeName), labelStyle);
            UnityEditor.EditorGUILayout.EndVertical();
            UnityEditor.EditorGUILayout.EndHorizontal();

            UnityEditor.EditorGUILayout.Space();
        }
        #endregion


        #region NESTED STATIC CLASS
        // ReSharper disable once InconsistentNaming
        internal static class HandTrackingSettingsIMGUIRegister
        {

            private const string HAND_TRACKING_PREFERENCES_ORDER_FIELD_NAME = "handTrackingPreferencesOrder";
            private const string TRACKING_SPACE_FIELD_NAME = "trackingSpace";
            private const string UPDATE_LIFE_CYCLE_FIELD_NAME = "updateLifeCycle";
            private const string USE_MESH_RENDERER_FIELD_NAME = "useMeshRenderer";
            private const string DISABLE_HANDS_WHEN_NOT_TRACKED_FIELD_NAME = "disableHandsWhenNotTracked";
            private const string VERBOSE_FIELD_NAME = "verbose";
            private const string HIDE_WARNINGS_FIELD_NAME = "hideWarnings";
            private const string HIDE_ERRORS_FIELD_NAME = "hideErrors";
            private const string SHOW_TRACKING_DATA_FIELD_NAME = "showTrackingData";


            private static UnityEditor.SerializedObject _settings = null;

            [UnityEditor.SettingsProvider]
            public static UnityEditor.SettingsProvider CreateMyCustomSettingsProvider()
            {
                _settings = Tools.HandTrackingSettings.GetSerializedSettings();
                _image =
                    UnityEditor.AssetDatabase.LoadAssetAtPath(GHT_TEXTURE_ASSET_PATH, typeof(UnityEngine.Texture)) as
                        UnityEngine.Texture;
                // First parameter is the path in the Settings window.
                // Second parameter is the scope of this setting: it only appears in the Project Settings window.
                UnityEditor.SettingsProvider provider =
                    new UnityEditor.SettingsProvider(SETTINGS_PATH,
                        UnityEditor.SettingsScope.Project)
                    {
                        label = SETTINGS_LABEL,
                        guiHandler = Draw,
                        keywords = UnityEditor.SettingsProvider.GetSearchKeywordsFromPath(Tools.HandTrackingSettings.MY_ASSET_SETTINGS_PATH)
                    };

                return provider;
            }


            private static void Draw(string searchContext)
            {
                UnityEditor.EditorGUILayout.Space();
                if (UnityEngine.Application.isPlaying && HandTrackingManager.Instance != null)
                    DrawHelpBox();

                UnityEditor.EditorGUILayout.PropertyField(_settings.FindProperty(HAND_TRACKING_PREFERENCES_ORDER_FIELD_NAME));
                UnityEditor.EditorGUILayout.PropertyField(_settings.FindProperty(TRACKING_SPACE_FIELD_NAME));
                UnityEditor.EditorGUILayout.PropertyField(_settings.FindProperty(UPDATE_LIFE_CYCLE_FIELD_NAME));
                UnityEditor.EditorGUILayout.PropertyField(_settings.FindProperty(USE_MESH_RENDERER_FIELD_NAME));
                UnityEditor.EditorGUILayout.PropertyField(_settings.FindProperty(DISABLE_HANDS_WHEN_NOT_TRACKED_FIELD_NAME));
                UnityEditor.SerializedProperty myVerbose = _settings.FindProperty(VERBOSE_FIELD_NAME);
                UnityEditor.EditorGUILayout.PropertyField(myVerbose);
                if (myVerbose.boolValue)
                {
                    UnityEditor.EditorGUI.indentLevel++;
                    UnityEditor.EditorGUILayout.PropertyField(_settings.FindProperty(HIDE_WARNINGS_FIELD_NAME));
                    UnityEditor.EditorGUILayout.PropertyField(_settings.FindProperty(HIDE_ERRORS_FIELD_NAME));
                    UnityEditor.EditorGUI.indentLevel--;
                }
                UnityEditor.EditorGUILayout.PropertyField(_settings.FindProperty(SHOW_TRACKING_DATA_FIELD_NAME));

                _settings.ApplyModifiedProperties();
            }

        }
        #endregion

    }

}
#endif