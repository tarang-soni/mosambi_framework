using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Mosambi.Tools.Editor
{
    [InitializeOnLoad]
    public static class MosambiHierarchyPainter
    {
        private static MosambiHierarchySettingsSO _cachedSettings;

        static MosambiHierarchyPainter()
        {
            EditorApplication.hierarchyWindowItemOnGUI += PaintHeaders;
            EditorApplication.delayCall += LoadSettings;
        }

        public static void LoadSettings()
        {
            // 1. Try to load the explicitly assigned profile from the Settings Hub
            string activeGuid = EditorPrefs.GetString("Mosambi_ActiveHierarchySettings", "");
            if (!string.IsNullOrEmpty(activeGuid))
            {
                string path = AssetDatabase.GUIDToAssetPath(activeGuid);
                if (!string.IsNullOrEmpty(path))
                {
                    _cachedSettings = AssetDatabase.LoadAssetAtPath<MosambiHierarchySettingsSO>(path);
                    if (_cachedSettings != null) return; // Success!
                }
            }

            // 2. Fallback: If nothing is assigned, try to find ANY existing setting file
            string[] guids = AssetDatabase.FindAssets("t:MosambiHierarchySettingsSO");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedSettings = AssetDatabase.LoadAssetAtPath<MosambiHierarchySettingsSO>(path);
                EditorPrefs.SetString("Mosambi_ActiveHierarchySettings", guids[0]); // Auto-assign it
            }
            else
            {
                _cachedSettings = null;
            }
        }

        public static List<MosambiGroup> GetActiveGroups()
        {
            if (_cachedSettings != null) return _cachedSettings.groups;
            return new List<MosambiGroup>();
        }

        private static void PaintHeaders(int instanceID, Rect selectionRect)
        {
            if (_cachedSettings == null) return;

            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;

            if (obj.name.StartsWith("---") && obj.name.EndsWith("---"))
            {
                string cleanName = obj.name.Replace("-", "").Trim().ToUpper();

                foreach (var group in _cachedSettings.groups)
                {
                    if (group.Name.ToUpper() == cleanName)
                    {
                        Rect bgRect = new Rect(32, selectionRect.y, selectionRect.width + 50, selectionRect.height);

                        Color bgColor = new Color(group.HeaderColor.r, group.HeaderColor.g, group.HeaderColor.b, 0.3f);
                        EditorGUI.DrawRect(bgRect, bgColor);

                        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            normal = { textColor = group.HeaderColor }
                        };
                        EditorGUI.LabelField(bgRect, group.Name.ToUpper(), style);
                        break;
                    }
                }
            }
        }

        // --- QUICK GROUP HOTKEY (Ctrl+G) ---
        [MenuItem("GameObject/Group Selected %g", false, 0)]
        private static void OpenGroupPopup()
        {
            if (Selection.transforms.Length == 0) return;
            if (_cachedSettings == null) LoadSettings();

            if (_cachedSettings != null && _cachedSettings.groups.Count > 0)
            {
                MosambiGroupPopup.OpenWindow();
            }
            else
            {
                Debug.LogWarning("[Mosambi] Create a Hierarchy Config asset first (Create > Mosambi > Settings > Hierarchy Config) to use quick-grouping.");
            }
        }
    }

    // --- POPUP WINDOW ---
    public class MosambiGroupPopup : EditorWindow
    {
        public static void OpenWindow()
        {
            MosambiGroupPopup window = CreateInstance<MosambiGroupPopup>();
            window.titleContent = new GUIContent("Assign Group");

            var groups = MosambiHierarchyPainter.GetActiveGroups();
            int height = (groups.Count * 30) + 10;
            window.minSize = new Vector2(200, height);
            window.maxSize = new Vector2(200, height);

            // ShowUtility forces it to float above the editor, drawing immediate focus
            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.Space(5);
            var groups = MosambiHierarchyPainter.GetActiveGroups();

            foreach (var group in groups)
            {
                GUI.backgroundColor = group.HeaderColor;
                if (GUILayout.Button($"Group into {group.Name}", GUILayout.Height(25)))
                {
                    ExecuteGrouping(group.Name.ToUpper());
                    Close(); // Auto-close the popup after selection
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private void ExecuteGrouping(string groupName)
        {
            string targetName = $"--- {groupName} ---";
            GameObject headerObj = GameObject.Find(targetName);

            // 1. If the header doesn't exist, create it exactly where you need it
            if (headerObj == null)
            {
                headerObj = new GameObject(targetName);
                Undo.RegisterCreatedObjectUndo(headerObj, "Create Header");

                // Place it at the exact level of the first thing you selected
                Transform primary = Selection.transforms[0];
                headerObj.transform.SetParent(primary.parent, false);
                headerObj.transform.SetSiblingIndex(primary.GetSiblingIndex());
            }

            // 2. Move your selected items into the header
            foreach (Transform t in Selection.transforms)
            {
                if (t.gameObject == headerObj) continue;
                Undo.SetTransformParent(t, headerObj.transform, "Group Objects");
            }

            // 3. Highlight the new group folder so you can see where things went
            EditorGUIUtility.PingObject(headerObj);
            Selection.activeGameObject = headerObj;
        }
    }
}