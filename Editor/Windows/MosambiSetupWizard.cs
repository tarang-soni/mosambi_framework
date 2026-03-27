using UnityEditor;
using UnityEngine;
using System.IO;

namespace Mosambi.Tools.Editor
{
    public class MosambiSetupWizard : EditorWindow
    {
        private string _gameFolderName = "_Project";
        private bool _includePooling = true;
        private bool _includeAudio = true;

        [MenuItem("Mosambi/Framework Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<MosambiSetupWizard>("Mosambi Setup");
            window.minSize = new Vector2(400, 450);
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
            // Root Assets folder for all raw/imported media
            CreateFolder($"{root}/Assets/2D/Sprites");
            CreateFolder($"{root}/Assets/2D/UI");
            CreateFolder($"{root}/Assets/3D/Models");

            // Centralized Materials: Accessible by 2D, 3D, and VFX
            CreateFolder($"{root}/Assets/Materials");
            CreateFolder($"{root}/Assets/Textures"); // Shared textures (Noise, Gradients)

            CreateFolder($"{root}/Assets/Sounds/SFX");
            CreateFolder($"{root}/Assets/Sounds/Music");
            CreateFolder($"{root}/Assets/Fonts");
            CreateFolder($"{root}/Assets/VFX"); // Particles and Shaders

            // --- 2. DATA (SCRIPTABLE OBJECTS) ---
            CreateFolder($"{root}/Data/Manifests");
            CreateFolder($"{root}/Data/Levels");
            CreateFolder($"{root}/Data/Configs");

            // --- 3. SCRIPTS (ARCHITECTURE) ---
            CreateFolder($"{root}/Scripts/Core");     // Framework extensions
            CreateFolder($"{root}/Scripts/Managers"); // Singletons/LifetimeScopes
            CreateFolder($"{root}/Scripts/Utils");    // Static helpers
            CreateFolder($"{root}/Scripts/Gameplay"); // Game-specific logic
            CreateFolder($"{root}/Scripts/UI");       // Menu & HUD logic

            // --- 4. ENGINE DEFAULTS ---
            CreateFolder($"{root}/Prefabs");
            CreateFolder($"{root}/Scenes");
            CreateFolder($"{root}/Settings/URP"); // Renderers, Post-Processing, Lighting

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
    }
}