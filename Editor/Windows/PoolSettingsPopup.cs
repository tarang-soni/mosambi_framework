using UnityEditor;
using UnityEngine;
using Mosambi.Core.Pooling;

namespace Mosambi.Tools.Editor
{
    public class PoolSettingsPopup : EditorWindow
    {
        private PoolDataSO _poolData;
        private UnityEditor.Editor _cachedEditor;

        public static void Open(PoolDataSO poolData)
        {
            PoolSettingsPopup window = GetWindow<PoolSettingsPopup>("Pool Settings");
            window._poolData = poolData;
            // Increased window size to fit the preview panel comfortably
            window.minSize = new Vector2(450, 250);
            window.ShowAuxWindow();
        }

        private void OnGUI()
        {
            if (_poolData == null) { Close(); return; }

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label($"Editing: {_poolData.name}", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            // --- LEFT COLUMN: Settings ---
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            UnityEditor.Editor.CreateCachedEditor(_poolData, null, ref _cachedEditor);
            if (_cachedEditor != null)
            {
                _cachedEditor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();

            // --- RIGHT COLUMN: Visual Preview ---
            EditorGUILayout.BeginVertical("helpbox", GUILayout.ExpandHeight(true));
            GUILayout.Label("Prefab Preview", EditorStyles.centeredGreyMiniLabel);

            GameObject prefab = GetPrefabFromSO();
            if (prefab != null)
            {
                // Request a 2D thumbnail from Unity's cache
                Texture2D previewTexture = AssetPreview.GetAssetPreview(prefab);

                if (previewTexture != null)
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    // Draw the image
                    GUILayout.Label(previewTexture, GUILayout.Width(128), GUILayout.Height(128));

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    // AssetPreview is asynchronous. If it's still generating, tell the UI to repaint.
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Generating Preview...", EditorStyles.centeredGreyMiniLabel);
                    GUILayout.FlexibleSpace();
                    Repaint();
                }
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("No Prefab Assigned", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close Window", GUILayout.Height(30))) Close();
            EditorGUILayout.EndVertical();
        }

        // --- THE MAGIC FINDER ---
        // Scans the ScriptableObject to find the actual assigned prefab dynamically
        private GameObject GetPrefabFromSO()
        {
            SerializedObject so = new SerializedObject(_poolData);
            SerializedProperty prop = so.GetIterator();

            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                    prop.objectReferenceValue is GameObject go)
                {
                    return go; // Found the prefab!
                }
            }
            return null; // No prefab assigned yet
        }
    }
}