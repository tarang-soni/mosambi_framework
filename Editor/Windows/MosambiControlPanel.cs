using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mosambi.Tools.Editor
{
    public class MosambiControlPanel : EditorWindow
    {
        private int _selectedTabIndex = 0;
        private string[] _tabNames;
        private Vector2 _sidebarScroll;

        private List<IMosambiTab> _tabs = new List<IMosambiTab>();

        [MenuItem("Mosambi/Mosambi Hub")]
        public static void ShowWindow()
        {
            var window = GetWindow<MosambiControlPanel>("Mosambi Hub");
            window.minSize = new Vector2(850, 450);
        }

        private void OnEnable()
        {
            _tabs.Clear();

            // Find every class, push "MosambiSettingsTab" to the bottom, then sort the rest alphabetically
            var tabTypes = TypeCache.GetTypesDerivedFrom<IMosambiTab>()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.Name == "MosambiSettingsTab" ? 1 : 0)
                .ThenBy(t => t.Name);

            foreach (var type in tabTypes)
            {
                var tabInstance = (IMosambiTab)Activator.CreateInstance(type);
                tabInstance.OnEnable();
                _tabs.Add(tabInstance);
            }

            _tabNames = _tabs.Select(t => t.TabName).ToArray();
        }

        private void OnDisable()
        {
            foreach (var tab in _tabs)
            {
                tab.OnDisable();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();

            // --- PANE 1: THE DYNAMIC SIDEBAR ---
            GUILayout.BeginVertical("box", GUILayout.Width(150));
            GUILayout.Label("Mosambi Engine", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _sidebarScroll = GUILayout.BeginScrollView(_sidebarScroll);

            if (_tabs == null || _tabs.Count == 0)
            {
                GUI.color = Color.yellow;
                GUILayout.Label("0 Tabs Found.\nDid you inherit\nIMosambiTab?", EditorStyles.helpBox);
                GUI.color = Color.white;
            }
            else
            {
                for (int i = 0; i < _tabNames.Length; i++)
                {
                    // PIN SETTINGS TO THE BOTTOM:
                    // If this is the very last tab, and it's our Settings tab, push it down
                    if (i == _tabNames.Length - 1 && _tabNames[i].Contains("Settings"))
                    {
                        GUILayout.FlexibleSpace();
                        DrawHorizontalLine(); // Add a nice separator above it
                    }

                    GUI.backgroundColor = _selectedTabIndex == i ? new Color(0.2f, 0.6f, 1f) : Color.white;
                    if (GUILayout.Button(_tabNames[i], GUILayout.Height(30)))
                    {
                        _selectedTabIndex = i;
                        GUI.FocusControl(null);
                    }
                    GUI.backgroundColor = Color.white;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // --- PANES 2 & 3: DRAW THE ACTIVE TAB ---
            GUILayout.BeginVertical();

            if (_tabs != null && _tabs.Count > 0 && _selectedTabIndex < _tabs.Count)
            {
                _tabs[_selectedTabIndex].DrawGUI();
            }
            else
            {
                GUILayout.Label("No active tab to display.", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawHorizontalLine()
        {
            GUILayout.Space(5);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            GUILayout.Space(5);
        }
    }
}