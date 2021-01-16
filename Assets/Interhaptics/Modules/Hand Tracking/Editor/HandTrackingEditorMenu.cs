#if UNITY_EDITOR
namespace Interhaptics.HandTracking.Editor
{

    [UnityEngine.AddComponentMenu("Interhaptics/Hand Tracking", 600)]
    public class HandTrackingEditorMenu : UnityEngine.MonoBehaviour
    {

        [UnityEditor.MenuItem("Interhaptics/Hand Tracking/Go to tracking settings", false)]
        private static void CreateInteractionMaterial()
        {
            UnityEditor.SettingsService.OpenProjectSettings(HandTrackingSettingsEditor.SETTINGS_PATH);
        }

    }

}
#endif