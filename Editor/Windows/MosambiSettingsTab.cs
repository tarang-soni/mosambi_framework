using UnityEditor;
using UnityEngine;
using System.IO;

namespace Mosambi.Tools.Editor
{
    public class MosambiSettingsTab : IMosambiTab
    {
        public string TabName => "Settings";

        private UnityEditor.Editor _cachedEditor;
        private MosambiHierarchySettingsSO _selectedHierarchySettings;
        private Vector2 _scrollPos;

        public void OnEnable()
        {
            LoadActiveSettings();
        }

        public void OnDisable()
        {
            // Cleanup if needed
        }

        private void LoadActiveSettings()
        {
            string activeGuid = EditorPrefs.GetString("Mosambi_ActiveHierarchySettings", "");
            if (!string.IsNullOrEmpty(activeGuid))
            {
                string path = AssetDatabase.GUIDToAssetPath(activeGuid);
                _selectedHierarchySettings = AssetDatabase.LoadAssetAtPath<MosambiHierarchySettingsSO>(path);
            }
        }

        // Implementation of your interface method
        public void DrawGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("MOSAMBI FRAMEWORK SETTINGS", EditorStyles.boldLabel);
            GUILayout.Label("Manage your global framework configurations here.", EditorStyles.miniLabel);
            GUILayout.EndVertical();

            EditorGUILayout.Space(15);

            DrawHierarchySettingsSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHierarchySettingsSection()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Hierarchy Grouping Theme", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            _selectedHierarchySettings = (MosambiHierarchySettingsSO)EditorGUILayout.ObjectField(
                "Active Theme", _selectedHierarchySettings, typeof(MosambiHierarchySettingsSO), false);

            if (EditorGUI.EndChangeCheck() && _selectedHierarchySettings != null)
            {
                string path = AssetDatabase.GetAssetPath(_selectedHierarchySettings);
                string guid = AssetDatabase.AssetPathToGUID(path);
                EditorPrefs.SetString("Mosambi_ActiveHierarchySettings", guid);

                MosambiHierarchyPainter.LoadSettings();
                EditorApplication.RepaintHierarchyWindow();
            }

            if (GUILayout.Button("+ New Theme", GUILayout.Width(100)))
            {
                CreateNewSettings();
            }
            EditorGUILayout.EndHorizontal();

            if (_selectedHierarchySettings != null)
            {
                DrawHorizontalLine();
                GUILayout.Label($"Editing: {_selectedHierarchySettings.name}", EditorStyles.miniLabel);

                UnityEditor.Editor.CreateCachedEditor(_selectedHierarchySettings, null, ref _cachedEditor);
                _cachedEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("No Hierarchy Theme assigned. Create a new one to enable color-coded groups and the Ctrl+G hotkey.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateNewSettings()
        {
            string gameRoot = EditorPrefs.GetString($"Mosambi_Root_{Application.productName}", "_Project");
            string targetFolder = $"Assets/{gameRoot}/Settings";

            if (!Directory.Exists(Path.Combine(Application.dataPath, $"{gameRoot}/Settings")))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, $"{gameRoot}/Settings"));
                AssetDatabase.Refresh();
            }

            string path = EditorUtility.SaveFilePanelInProject("Create Hierarchy Theme", "NewHierarchyTheme", "asset", "", targetFolder);
            if (string.IsNullOrEmpty(path)) return;

            var newAsset = ScriptableObject.CreateInstance<MosambiHierarchySettingsSO>();
            AssetDatabase.CreateAsset(newAsset, path);
            AssetDatabase.SaveAssets();

            _selectedHierarchySettings = newAsset;
            string guid = AssetDatabase.AssetPathToGUID(path);
            EditorPrefs.SetString("Mosambi_ActiveHierarchySettings", guid);

            MosambiHierarchyPainter.LoadSettings();
            EditorApplication.RepaintHierarchyWindow();
        }

        private void DrawHorizontalLine()
        {
            EditorGUILayout.Space(5);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(5);
        }
    }
}   