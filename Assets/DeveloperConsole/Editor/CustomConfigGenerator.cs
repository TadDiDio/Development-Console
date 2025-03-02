using UnityEngine;
using UnityEditor;

namespace DeveloperConsole
{
    public class CustomConfigGenerator
    {
        [MenuItem("Assets/Create/Developer Console/New Console Config")]
        public static void CreateNewConsoleConfig()
        {
            DeveloperConsoleConfig newConfig = ScriptableObject.CreateInstance<DeveloperConsoleConfig>();

            string filename = EditorUtility.SaveFilePanelInProject("Save New Script", "NewConsoleConfig", "asset", "Create a new console config");
            AssetDatabase.CreateAsset(newConfig, filename);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
