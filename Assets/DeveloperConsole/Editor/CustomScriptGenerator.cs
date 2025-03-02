using System.IO;
using UnityEngine;
using UnityEditor;

namespace DeveloperConsole
{
    public class CustomScriptGenerator
    {
        private const string TEMPLATE_FILENAME = "NewCommandTemplate.txt";

        [MenuItem("Assets/Create/Developer Console/New Command")]
        public static void CreateCustomScript()
        {
            string templatePath = FindTemplatePath();
            if (string.IsNullOrEmpty(templatePath))
            {
                Debug.LogError("Could not find command script template");
                return;
            }

            string filename = EditorUtility.SaveFilePanelInProject("Save new script", "NewCommand", "cs", "Create a new command");
            if (string.IsNullOrEmpty(filename)) return;

            string templateContent = File.ReadAllText(templatePath);
            templateContent = templateContent.Replace("#SCRIPTNAME#", Path.GetFileNameWithoutExtension(filename));

            File.WriteAllText(filename, templateContent);
            AssetDatabase.Refresh();

            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(filename));
        }

        private static string FindTemplatePath()
        {
            string[] editorDirectories = Directory.GetDirectories("Assets", "Editor", SearchOption.AllDirectories);

            foreach (var directory in editorDirectories)
            {
                string templatePath = Path.Combine(directory, TEMPLATE_FILENAME);

                if (File.Exists(templatePath)) return templatePath;
            }

            return null;
        }
    }

}
