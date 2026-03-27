using UnityEditor;
using UnityEngine;
using Mosambi.Core.Audio;
using System.Collections.Generic;

namespace Mosambi.Tools.Editor
{
    public class AudioDatabaseWindow : EditorWindow
    {
        private List<AudioEventSO> _audioEvents = new List<AudioEventSO>();
        private AudioEventSO _selectedEvent;
        private Vector2 _leftScrollPos;
        private Vector2 _rightScrollPos;

        private UnityEditor.Editor _cachedEditor;

        // --- PREVIEW VARIABLES ---
        private AudioSource _previewSource;
        private bool _loopPreview = false;

        [MenuItem("Mosambi/Tools/Audio Database")]
        public static void ShowWindow()
        {
            var window = GetWindow<AudioDatabaseWindow>("Audio Database");
            window.minSize = new Vector2(700, 450);
        }

        private void OnEnable()
        {
            RefreshDatabase();
            InitializePreviewer();
        }

        private void OnDisable()
        {
            // CRITICAL: We must destroy the ghost object when the window closes
            // Otherwise, every time you open the tool, you create a permanent memory leak.
            if (_previewSource != null)
            {
                DestroyImmediate(_previewSource.gameObject);
            }
        }

        private void InitializePreviewer()
        {
            if (_previewSource == null)
            {
                // Create a Ghost Object. 
                // HideAndDontSave means it won't show in the Hierarchy and won't be saved into the scene.
                GameObject previewObj = new GameObject("Mosambi_AudioPreviewer");
                previewObj.hideFlags = HideFlags.HideAndDontSave;
                _previewSource = previewObj.AddComponent<AudioSource>();
            }
        }

        private void RefreshDatabase()
        {
            _audioEvents.Clear();

            string[] guids = AssetDatabase.FindAssets("t:AudioEventSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                _audioEvents.Add(AssetDatabase.LoadAssetAtPath<AudioEventSO>(path));
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();

            // --- LEFT PANEL ---
            GUILayout.BeginVertical("box", GUILayout.Width(250));
            DrawLeftPanel();
            GUILayout.EndVertical();

            // --- RIGHT PANEL ---
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            DrawRightPanel();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            GUILayout.Label("Audio Events", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);
            if (GUILayout.Button("Create New Event", GUILayout.Height(30)))
            {
                CreateNewAudioEvent();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(5);

            _leftScrollPos = GUILayout.BeginScrollView(_leftScrollPos);
            foreach (var audioEvent in _audioEvents)
            {
                if (audioEvent == null) continue;

                GUI.backgroundColor = _selectedEvent == audioEvent ? Color.cyan : Color.white;

                if (GUILayout.Button(audioEvent.name, EditorStyles.miniButton))
                {
                    _selectedEvent = audioEvent;

                    // Stop playing the old sound if we click a new one
                    if (_previewSource != null && _previewSource.isPlaying) _previewSource.Stop();

                    GUI.FocusControl(null);
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndScrollView();

            if (GUILayout.Button("Refresh List"))
            {
                RefreshDatabase();
            }
        }

        private void DrawRightPanel()
        {
            if (_selectedEvent == null)
            {
                GUILayout.Label("Select an Audio Event from the list to edit it.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            GUILayout.Label($"Editing: {_selectedEvent.name}", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // --- THE NEW PREVIEW CONTROLS ---
            DrawPreviewControls();

            GUILayout.Space(10);

            _rightScrollPos = GUILayout.BeginScrollView(_rightScrollPos);

            UnityEditor.Editor.CreateCachedEditor(_selectedEvent, null, ref _cachedEditor);
            if (_cachedEditor != null)
            {
                _cachedEditor.OnInspectorGUI();
            }

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Ping in Project Window", GUILayout.Height(30)))
            {
                EditorGUIUtility.PingObject(_selectedEvent);
            }

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Delete Event", GUILayout.Height(30), GUILayout.Width(120)))
            {
                DeleteSelectedEvent();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
        }

        private void DrawPreviewControls()
        {
            EditorGUILayout.BeginVertical("helpbox");
            GUILayout.Label("Preview Audio", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            // PLAY BUTTON
            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button("? Play", GUILayout.Height(30)))
            {
                PlayPreview();
            }

            // STOP BUTTON
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("? Stop", GUILayout.Height(30)))
            {
                if (_previewSource != null) _previewSource.Stop();
            }
            GUI.backgroundColor = Color.white;

            // LOOP TOGGLE
            GUILayout.Space(10);
            _loopPreview = GUILayout.Toggle(_loopPreview, "Loop Preview", GUILayout.Height(30));

            // If they uncheck loop while it's playing, stop looping instantly
            if (_previewSource != null) _previewSource.loop = _loopPreview;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void PlayPreview()
        {
            if (_selectedEvent == null || _selectedEvent.clips == null || _selectedEvent.clips.Length == 0)
            {
                Debug.LogWarning("[AudioTool] No clips assigned to this event.");
                return;
            }

            // 1. Pick a random clip just like the runtime engine does
            AudioClip clipToPlay = _selectedEvent.clips[Random.Range(0, _selectedEvent.clips.Length)];

            if (clipToPlay == null) return;

            // 2. Configure the Ghost AudioSource
            _previewSource.clip = clipToPlay;
            _previewSource.volume = _selectedEvent.volume;

            // Test the exact pitch randomization you set in the inspector
            _previewSource.pitch = Random.Range(_selectedEvent.minPitch, _selectedEvent.maxPitch);

            _previewSource.loop = _loopPreview;

            // 3. Fire
            _previewSource.Play();
        }

        private void CreateNewAudioEvent()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Audio Event", "NewAudioEvent", "asset", "");
            if (string.IsNullOrEmpty(path)) return;

            AudioEventSO newEvent = CreateInstance<AudioEventSO>();
            AssetDatabase.CreateAsset(newEvent, path);
            AssetDatabase.SaveAssets();

            RefreshDatabase();
            _selectedEvent = newEvent;
        }

        private void DeleteSelectedEvent()
        {
            bool confirm = EditorUtility.DisplayDialog("Delete Audio Event", $"Permanently delete '{_selectedEvent.name}'?", "Yes", "Cancel");
            if (confirm)
            {
                string path = AssetDatabase.GetAssetPath(_selectedEvent);
                AssetDatabase.DeleteAsset(path);
                _selectedEvent = null;
                if (_previewSource != null) _previewSource.Stop();
                RefreshDatabase();
            }
        }
    }
}