using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace Mosambi.EditorTools
{
    public class MosambiScriptImporter : EditorWindow
    {
        private string codeString = "";
        private string relativeSavePath = "Mosambi/Scripts/"; // Default subfolder
        private Vector2 scrollPos;

        [MenuItem("Mosambi/Tools/Script Importer")]
        public static void ShowWindow()
        {
            GetWindow<MosambiScriptImporter>("Script Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Mosambi Quick Script Importer", EditorStyles.boldLabel);
            GUILayout.Space(5);

            GUILayout.Label("Paste Code Here:");
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
            codeString = EditorGUILayout.TextArea(codeString, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            GUILayout.Space(10);
            GUILayout.Label("Save Location (Inside Assets/):");
            relativeSavePath = EditorGUILayout.TextField(relativeSavePath);

            GUILayout.Space(10);
            if (GUILayout.Button("Generate Script", GUILayout.Height(40)))
            {
                GenerateScript();
            }
        }

        private void GenerateScript()
        {
            if (string.IsNullOrWhiteSpace(codeString))
            {
                Debug.LogWarning("[Script Importer] Code string is empty.");
                return;
            }

            string fileName = ExtractFileName(codeString);
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("[Script Importer] Could not detect a class, interface, or enum name in the code.");
                return;
            }

            // Ensure the path exists
            string fullDirectoryPath = Path.Combine(Application.dataPath, relativeSavePath);
            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            string fullFilePath = Path.Combine(fullDirectoryPath, fileName + ".cs");

            // Write the file
            File.WriteAllText(fullFilePath, codeString);
            Debug.Log($"[Script Importer] Successfully created: {fullFilePath}");

            // Tell Unity to recompile and show the new file
            AssetDatabase.Refresh();

            // Clear the tool for the next script
            codeString = "";
        }

        private string ExtractFileName(string code)
        {
            // Matches: public class Name, internal interface Name, enum Name, abstract class Name, etc.
            string pattern = @"(?:class|interface|enum)\s+([A-Za-z0-9_]+)";
            Match match = Regex.Match(code, pattern);

            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }
    }
}