using UnityEditor;
using UnityEngine;
using Mosambi.Core.Pooling;
using System.Collections.Generic;

namespace Mosambi.Tools.Editor
{
    public class PoolingTab : GenericDatabaseTab<PoolManifestSO>
    {
        public override string TabName => "Pool Manifests";

        private Vector2 _inManifestScroll;
        private Vector2 _availableScroll;

        protected override void DrawRightPanel()
        {
            if (_selectedItem == null)
            {
                GUILayout.Label("Select a Manifest from the list to edit it.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (_selectedItem.requiredPools == null) _selectedItem.requiredPools = new List<PoolDataSO>();

            // --- HEADER ---
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Editing Manifest: {_selectedItem.name}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button("+ Create & Assign New Pool", GUILayout.Width(180), GUILayout.Height(25)))
            {
                CreateAndAssignNewPool();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 1. SCAN PROJECT
            List<PoolDataSO> allPools = new List<PoolDataSO>();
            string[] guids = AssetDatabase.FindAssets("t:PoolDataSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                allPools.Add(AssetDatabase.LoadAssetAtPath<PoolDataSO>(path));
            }

            // 2. DUAL COLUMNS
            EditorGUILayout.BeginHorizontal();

            // --- COLUMN 1: ASSIGNED ---
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Assigned to Manifest", EditorStyles.miniBoldLabel);
            _inManifestScroll = EditorGUILayout.BeginScrollView(_inManifestScroll);
            for (int i = 0; i < _selectedItem.requiredPools.Count; i++)
            {
                var pool = _selectedItem.requiredPools[i];
                if (pool == null) continue;

                EditorGUILayout.BeginHorizontal("helpbox");
                GUILayout.Label(pool.name, GUILayout.ExpandWidth(true));

                // Edit (Gear)
                if (GUILayout.Button("⚙", GUILayout.Width(25))) { PoolSettingsPopup.Open(pool); }

                // Remove from Manifest (X)
                GUI.backgroundColor = new Color(1f, 0.7f, 0.4f); // Orange-ish
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    _selectedItem.requiredPools.RemoveAt(i);
                    EditorUtility.SetDirty(_selectedItem);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // --- COLUMN 2: AVAILABLE ---
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Available Pools", EditorStyles.miniBoldLabel);
            _availableScroll = GUILayout.BeginScrollView(_availableScroll);
            foreach (var pool in allPools)
            {
                if (pool == null || _selectedItem.requiredPools.Contains(pool)) continue;

                EditorGUILayout.BeginHorizontal("helpbox");
                GUILayout.Label(pool.name, GUILayout.ExpandWidth(true));

                // Edit (Gear)
                if (GUILayout.Button("⚙", GUILayout.Width(25))) { PoolSettingsPopup.Open(pool); }

                // Delete Asset (Trash Can)
                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f); // Red
                if (GUILayout.Button("🗑", GUILayout.Width(25))) { DeletePoolAsset(pool); }

                // Add to Manifest (+)
                GUI.backgroundColor = new Color(0.4f, 1f, 0.4f); // Green
                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    _selectedItem.requiredPools.Add(pool);
                    EditorUtility.SetDirty(_selectedItem);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // --- FOOTER ---
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping Manifest", GUILayout.Height(30))) EditorGUIUtility.PingObject(_selectedItem);

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Delete Manifest", GUILayout.Height(30), GUILayout.Width(120))) DeleteSelectedItem();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void CreateAndAssignNewPool()
        {
            string gameRoot = EditorPrefs.GetString($"Mosambi_Root_{Application.productName}", "_Project");
            string targetFolder = $"Assets/{gameRoot}/Data/Pools";

            // --- DEFENSIVE FOLDER CREATION ---
            string absolutePath = System.IO.Path.Combine(Application.dataPath, $"{gameRoot}/Data/Pools");
            if (!System.IO.Directory.Exists(absolutePath))
            {
                System.IO.Directory.CreateDirectory(absolutePath);
                AssetDatabase.Refresh();
            }

            string path = EditorUtility.SaveFilePanelInProject("Create New Pool Data", "NewPoolData", "asset", "", targetFolder);
            if (string.IsNullOrEmpty(path)) return;

            PoolDataSO newPool = ScriptableObject.CreateInstance<PoolDataSO>();
            AssetDatabase.CreateAsset(newPool, path);
            AssetDatabase.SaveAssets();

            _selectedItem.requiredPools.Add(newPool);
            EditorUtility.SetDirty(_selectedItem);
            PoolSettingsPopup.Open(newPool);
        }

        private void DeletePoolAsset(PoolDataSO pool)
        {
            if (EditorUtility.DisplayDialog("Delete Pool Asset",
                $"Are you sure you want to permanently delete the '{pool.name}' asset from the project?\n\nThis will remove it from ALL manifests.",
                "Yes, Delete", "Cancel"))
            {
                string path = AssetDatabase.GetAssetPath(pool);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();

                // Refresh is automatic because DrawRightPanel scans assets every frame
            }
        }
    }
}