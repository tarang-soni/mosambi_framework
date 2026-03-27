using UnityEditor;
using UnityEngine;
using System.IO;

namespace Mosambi.Tools.Editor
{
    public class MosambiProjectSetup : EditorWindow
    {
        private string _gameFolderName = "_Project";
        private bool _includePooling = true;
        private bool _includeAudio = true;
        private bool _copyExampleScenes = true; // New toggle!

        [MenuItem("Mosambi/Framework Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<MosambiProjectSetup>("Mosambi Setup");
            window.minSize = new Vector2(400, 480);
        }

        private void OnGUI()
        {
            DrawHeader();

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("General Settings", EditorStyles.boldLabel);
            _gameFolderName = EditorGUILayout.TextField("Game Root Name", _gameFolderName);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Modules to Initialize", EditorStyles.boldLabel);
            _includePooling = EditorGUILayout.Toggle("Pooling System", _includePooling);
            _includeAudio = EditorGUILayout.Toggle("Audio System", _includeAudio);
            _copyExampleScenes = EditorGUILayout.Toggle("Copy Example Scenes", _copyExampleScenes); // UI for the new feature
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("INITIALIZE PROJECT STRUCTURE", GUILayout.Height(40)))
            {
                ExecuteSetup();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(10);
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("helpbox");
            GUILayout.Label("MOSAMBI SETUP", EditorStyles.whiteLargeLabel);
            GUILayout.Label("Industry Standard Architecture", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void ExecuteSetup()
        {
            string root = _gameFolderName;

            // --- 1. ASSETS (ART & SOUND) ---
            CreateFolder($"{root}/Assets/2D/Sprites");
            CreateFolder($"{root}/Assets/2D/UI");
            CreateFolder($"{root}/Assets/3D/Models");
            CreateFolder($"{root}/Assets/Materials");
            CreateFolder($"{root}/Assets/Textures");
            CreateFolder($"{root}/Assets/Sounds/SFX");
            CreateFolder($"{root}/Assets/Sounds/Music");
            CreateFolder($"{root}/Assets/Fonts");
            CreateFolder($"{root}/Assets/VFX");

            // --- 2. DATA (SCRIPTABLE OBJECTS) ---
            CreateFolder($"{root}/Data/Manifests");
            CreateFolder($"{root}/Data/Levels");
            CreateFolder($"{root}/Data/Configs");

            // --- 3. SCRIPTS (ARCHITECTURE) ---
            CreateFolder($"{root}/Scripts/Core");
            CreateFolder($"{root}/Scripts/Managers");
            CreateFolder($"{root}/Scripts/Utils");
            CreateFolder($"{root}/Scripts/Gameplay");
            CreateFolder($"{root}/Scripts/UI");

            // --- 4. ENGINE DEFAULTS ---
            CreateFolder($"{root}/Prefabs");
            CreateFolder($"{root}/Scenes");
            CreateFolder($"{root}/Settings/URP");

            // --- 5. COPY SCENES ---
            if (_copyExampleScenes)
            {
                CopyExampleScenes(_gameFolderName);
            }

            EditorPrefs.SetString($"Mosambi_Root_{Application.productName}", _gameFolderName);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "Project Structure Initialized!", "Let's Build!");
            Close();
        }

        private void CreateFolder(string path)
        {
            string fullPath = Path.Combine(Application.dataPath, path);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        private void CopyExampleScenes(string targetRootName)
        {
            string targetScenesFolder = $"Assets/{targetRootName}/Scenes";

            // Check both possible locations (UPM Package vs Local Assets)
            string packageSource = "Packages/com.mosambi.framework/ExampleProject/Scenes";
            string assetSource = "Assets/Mosambi/ExampleProject/Scenes";

            string sourceFolder = AssetDatabase.IsValidFolder(packageSource) ? packageSource : assetSource;

            if (!AssetDatabase.IsValidFolder(sourceFolder))
            {
                Debug.LogWarning($"[Mosambi] Could not find example scenes at {packageSource} or {assetSource}. Skipping scene copy.");
                return;
            }

            // Find all scenes in the source folder
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { sourceFolder });

            foreach (string guid in sceneGuids)
            {
                string sourcePath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileName(sourcePath);
                string targetPath = $"{targetScenesFolder}/{fileName}";

                // Only copy if it doesn't already exist to prevent overwriting user work
                if (!File.Exists(Path.Combine(Application.dataPath, $"{targetRootName}/Scenes/{fileName}")))
                {
                    AssetDatabase.CopyAsset(sourcePath, targetPath);
                    Debug.Log($"<color=cyan>[Mosambi]</color> Copied {fileName} into your new project!");
                }
            }
        }
    }
}