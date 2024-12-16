using UnityEngine;
using UnityEditor;

namespace DeveloperConsole
{
    public class ConfigGenerator
    {
        [MenuItem("Assets/Create/Developer Console/New console config")]
        public static void CreateNewConsoleConfig()
        {
            DeveloperConsoleConfig newConfig = ScriptableObject.CreateInstance<DeveloperConsoleConfig>();

            string filename = EditorUtility.SaveFilePanelInProject("Save new script", "NewConsoleConfig", "asset", "Create a new console config");
            AssetDatabase.CreateAsset(newConfig, filename);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
