#if UNITY_EDITOR
using Interhaptics.Modules.Interaction_Builder.Core;

using UnityEditor;
using UnityEngine;


namespace Interhaptics.Modules.Interaction_Builder.Editor
{

    [AddComponentMenu("Interhaptics/Interaction Builder", 600)]
    public class IbEditorIntegration : MonoBehaviour
    {

        #region Message strings
        private const string MSG_RIG_INSTANTIATION =
            "<b>[InterhapticsEngine.InteractionsEngine]</b> The <i>Interhaptics Rig</i> was successfully instantiated.";

        private const string WARNING_RIG_FOUND_AT_WRONG_PLACE =
            "<b>[InterhapticsEngine.InteractionsEngine]</b> Some rig was found but at the wrong location. Be sure you didn't moved, renamed or deleted the initial <i>Interhaptics Rig</i> given.";

        private const string ERROR_IMPOSSIBLE_TO_FIND_RIG =
            "<b>[InterhapticsEngine.InteractionsEngine]</b> Impossible to find the rig prefab. Be sure that you haven't deleted or named it into the Interhaptics folder.";
        #endregion


        #region Editor Integration Methods
        [MenuItem("Interhaptics/Interaction Builder/Create InteractionPrimitive", false, 0)]
        private static void CreateInteractionMaterial()
        {
            InteractionPrimitive primitive = ScriptableObject.CreateInstance<InteractionPrimitive>();
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
                path = "Assets";
            else if (!string.IsNullOrEmpty(System.IO.Path.GetExtension(path)))
                path = path.Replace(System.IO.Path.GetFileName(path), "");

            AssetDatabase.CreateAsset(primitive,
                AssetDatabase.GenerateUniqueAssetPath($"{path}/New InteractionPrimitive.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = primitive;
        }

        [MenuItem("Interhaptics/Interaction Builder/Add Rig to the scene", false, 1)]
        private static void InstantiateRig()
        {
            string[] assetsFound = AssetDatabase.FindAssets("IB_RIG Variant t:GameObject", new []{"Assets"});
            if (assetsFound.Length == 0)
            {
                Debug.LogError(ERROR_IMPOSSIBLE_TO_FIND_RIG);
                return;
            }

            string assetPath;
            foreach (var str in assetsFound)
            {                
                try
                {
                    assetPath = AssetDatabase.GUIDToAssetPath(str);
                    if (!assetPath.Contains("Interaction Builder") || !assetPath.Contains("Prefabs"))
                        continue;

                    Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(assetPath));
                    Debug.Log(MSG_RIG_INSTANTIATION);
                    return;
                }
                catch (System.Exception)
                {
                    // ignored
                }
            }

            Debug.LogWarning(WARNING_RIG_FOUND_AT_WRONG_PLACE);
        }
        #endregion

    }

}
#endif