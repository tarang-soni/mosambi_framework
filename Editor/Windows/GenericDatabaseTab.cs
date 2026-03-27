using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Mosambi.Tools.Editor
{
    public interface IMosambiTab
    {
        string TabName { get; }
        void OnEnable();
        void OnDisable();
        void DrawGUI();
    }
    public abstract class GenericDatabaseTab<T> : IMosambiTab where T : ScriptableObject
    {
        protected List<T> _items = new List<T>();
        protected T _selectedItem;
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;
        private UnityEditor.Editor _cachedEditor;

        public abstract string TabName { get; }

        // --- THE NEW SUB-TAB HOOKS ---
        protected virtual string[] SubTabNames => null; // Returns null by default (no sub-tabs)
        protected int _selectedSubTab = 0;

        // Child classes override this to decide what shows up in which sub-tab
        protected virtual bool ItemMatchesSubTab(T item, int subTabIndex) { return true; }

        public virtual void OnEnable()
        {
            RefreshDatabase();
        }

        public virtual void OnDisable() { }

        // Made virtual so complex tabs can override how they find files
        public virtual void RefreshDatabase()
        {
            _items.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                _items.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }
        }

        public void DrawGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("box", GUILayout.Width(250));
            DrawLeftPanel();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            DrawRightPanel();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            GUILayout.Label($"{TabName} Database", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);
            if (GUILayout.Button("Create New", GUILayout.Height(30))) CreateNewItem();
            GUI.backgroundColor = Color.white;
            GUILayout.Space(5);

            // --- DRAW THE SUB-FILTER DROPDOWN IF IT EXISTS ---
            if (SubTabNames != null && SubTabNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();

                // Swapped from Toolbar to a clean, infinitely scalable Dropdown
                _selectedSubTab = EditorGUILayout.Popup(_selectedSubTab, SubTabNames, EditorStyles.popup);

                if (EditorGUI.EndChangeCheck())
                {
                    _selectedItem = null;
                    GUI.FocusControl(null);
                }
                GUILayout.Space(5);
            }

            _leftScroll = GUILayout.BeginScrollView(_leftScroll);
            foreach (var item in _items)
            {
                if (item == null) continue;

                // --- FILTER CHECK ---
                if (!ItemMatchesSubTab(item, _selectedSubTab)) continue;

                GUI.backgroundColor = _selectedItem == item ? Color.cyan : Color.white;
                if (GUILayout.Button(item.name, EditorStyles.miniButton))
                {
                    _selectedItem = item;
                    OnItemSelected();
                    GUI.FocusControl(null);
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndScrollView();

            if (GUILayout.Button("Refresh List")) RefreshDatabase();
        }

        protected virtual void DrawRightPanel()
        {
            if (_selectedItem == null)
            {
                GUILayout.Label("Select an item from the list to edit it.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            GUILayout.Label($"Editing: {_selectedItem.name}", EditorStyles.boldLabel);
            GUILayout.Space(5);

            DrawCustomHeader();

            _rightScroll = GUILayout.BeginScrollView(_rightScroll);
            UnityEditor.Editor.CreateCachedEditor(_selectedItem, null, ref _cachedEditor);
            if (_cachedEditor != null) _cachedEditor.OnInspectorGUI();
            GUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping in Project Window", GUILayout.Height(30))) EditorGUIUtility.PingObject(_selectedItem);

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Delete", GUILayout.Height(30), GUILayout.Width(120))) DeleteSelectedItem();
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
        }

        protected virtual void CreateNewItem()
        {
            string gameRoot = EditorPrefs.GetString($"Mosambi_Root_{Application.productName}", "_Project");
            string targetFolder = $"Assets/{gameRoot}/Data/Manifests";

            // --- DEFENSIVE FOLDER CREATION ---
            // Application.dataPath already points to the "Assets" folder
            string absolutePath = System.IO.Path.Combine(Application.dataPath, $"{gameRoot}/Data/Manifests");
            if (!System.IO.Directory.Exists(absolutePath))
            {
                System.IO.Directory.CreateDirectory(absolutePath);
                AssetDatabase.Refresh(); // Tell Unity we made a new folder
            }

            string path = EditorUtility.SaveFilePanelInProject(
                $"Create New {typeof(T).Name}",
                $"New{typeof(T).Name}",
                "asset",
                "Save your new data asset",
                targetFolder
            );

            if (string.IsNullOrEmpty(path)) return;

            T newItem = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(newItem, path);
            AssetDatabase.SaveAssets();

            RefreshDatabase();
            _selectedItem = newItem;
        }

        protected void DeleteSelectedItem()
        {
            if (EditorUtility.DisplayDialog("Delete Asset", $"Permanently delete '{_selectedItem.name}'?", "Yes", "Cancel"))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_selectedItem));
                _selectedItem = null;
                RefreshDatabase();
            }
        }

        protected virtual void OnItemSelected() { }
        protected virtual void DrawCustomHeader() { }
    }
}