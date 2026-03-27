using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace Mosambi.Tools.Editor
{
    public class MosambiQuickbar : EditorWindow
    {
        private SceneAsset _bootScene;
        private bool _alwaysPlayFromBoot = true;
        private Vector2 _scrollPosition;

        [MenuItem("Mosambi/Quickbar (Scenes & Play)")]
        public static void ShowWindow()
        {
            var window = GetWindow<MosambiQuickbar>("Mosambi");
            window.minSize = new Vector2(150, 300);
            window.Show();
        }

        private void OnEnable()
        {
            FindBootScene();
            UpdatePlayModeStartScene();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            GUILayout.Space(5);

            DrawPlayControls();

            GUILayout.Space(10);
            DrawHorizontalLine();
            GUILayout.Space(10);

            DrawSceneSwitcher();

            GUILayout.EndVertical();
        }

        private void DrawPlayControls()
        {
            GUILayout.Label("Play Controls", EditorStyles.boldLabel);

            // --- THE HIGHLIGHT LOGIC ---
            EditorGUI.BeginChangeCheck();

            // 1. Change color based on state
            if (_alwaysPlayFromBoot)
            {
                GUI.backgroundColor = new Color(0.4f, 1f, 0.4f); // Bright Green
            }
            else
            {
                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f); // Dim Gray
            }

            // 2. Draw the toggle using the "Button" style so it looks like a thick block
            _alwaysPlayFromBoot = GUILayout.Toggle(_alwaysPlayFromBoot, "Force Boot Scene", "Button", GUILayout.Height(24));

            // 3. Reset the color back to normal so we don't tint the rest of the editor
            GUI.backgroundColor = Color.white;

            if (EditorGUI.EndChangeCheck())
            {
                UpdatePlayModeStartScene();
            }
            // ---------------------------

            GUILayout.Space(5);

            // The Direct "Play Now" Button
            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("▶ PLAY BOOT", GUILayout.Height(30)))
            {
                ForcePlayFromBoot();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawSceneSwitcher()
        {
            GUILayout.Label("Quick Switch", EditorStyles.boldLabel);
            GUILayout.Space(5);

            string gameRoot = EditorPrefs.GetString($"Mosambi_Root_{Application.productName}", "Mosambi/ExampleProject");
            string scenesFolder = $"Assets/{gameRoot}/Scenes";

            if (Directory.Exists(System.IO.Path.Combine(Application.dataPath, $"{gameRoot}/Scenes")))
            {
                string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { scenesFolder });

                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

                foreach (string guid in sceneGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string sceneName = Path.GetFileNameWithoutExtension(path);

                    if (GUILayout.Button(sceneName, EditorStyles.miniButton, GUILayout.Height(24)))
                    {
                        SwitchScene(path);
                    }
                }

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("(No Scenes Folder Found)", EditorStyles.miniLabel);
            }
        }

        private void DrawHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        // --- CORE LOGIC ---

        private void FindBootScene()
        {
            string[] bootGuids = AssetDatabase.FindAssets("0_Boot t:Scene");
            if (bootGuids.Length > 0)
            {
                string bootPath = AssetDatabase.GUIDToAssetPath(bootGuids[0]);
                _bootScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootPath);
            }
        }

        private void UpdatePlayModeStartScene()
        {
            if (_alwaysPlayFromBoot && _bootScene != null)
            {
                EditorSceneManager.playModeStartScene = _bootScene;
            }
            else
            {
                EditorSceneManager.playModeStartScene = null;
            }
        }

        private void ForcePlayFromBoot()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            if (_bootScene == null) FindBootScene();

            if (_bootScene == null)
            {
                Debug.LogError("[Mosambi] Cannot find a scene named '0_Boot'. Please ensure it exists.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.playModeStartScene = _bootScene;
                EditorApplication.isPlaying = true;
            }
        }

        private void SwitchScene(string path)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Mosambi] Cannot switch scenes while in Play Mode.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
            }
        }
    }
}