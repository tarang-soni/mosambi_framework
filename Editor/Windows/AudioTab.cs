using UnityEngine;
using UnityEditor;
using Mosambi.Core.Audio;

namespace Mosambi.Tools.Editor
{
    public class AudioTab : GenericDatabaseTab<AudioEventSO>
    {
        public override string TabName => "Audio Events";
        
        private AudioSource _previewSource;
        private bool _loopPreview = false;

        public override void OnEnable()
        {
            base.OnEnable(); // Call the base class to load the list
            if (_previewSource == null)
            {
                GameObject previewObj = new GameObject("Mosambi_AudioPreviewer");
                previewObj.hideFlags = HideFlags.HideAndDontSave;
                _previewSource = previewObj.AddComponent<AudioSource>();
            }
        }
        protected override void CreateNewItem()
        {
            string gameRoot = EditorPrefs.GetString($"Mosambi_Root_{Application.productName}", "_Project");
            string targetFolder = $"Assets/{gameRoot}/Data/Audio";

            // --- DEFENSIVE FOLDER CREATION ---
            string absolutePath = System.IO.Path.Combine(Application.dataPath, $"{gameRoot}/Data/Audio");
            if (!System.IO.Directory.Exists(absolutePath))
            {
                System.IO.Directory.CreateDirectory(absolutePath);
                AssetDatabase.Refresh();
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Audio Asset",
                "NewAudioData",
                "asset",
                "Save your new audio asset",
                targetFolder
            );

            if (string.IsNullOrEmpty(path)) return;

            // Replace 'AudioDataSO' with whatever your actual Audio ScriptableObject class is named
            var newItem = ScriptableObject.CreateInstance<AudioEventSO>();
            AssetDatabase.CreateAsset(newItem, path);
            AssetDatabase.SaveAssets();

            RefreshDatabase();
            _selectedItem = newItem;
        }
        public override void OnDisable()
        {
            if (_previewSource != null) Object.DestroyImmediate(_previewSource.gameObject);
        }

        protected override void OnItemSelected()
        {
            if (_previewSource != null && _previewSource.isPlaying) _previewSource.Stop();
        }

        // This injects the preview buttons right above the Inspector!
        protected override void DrawCustomHeader()
        {
            EditorGUILayout.BeginVertical("helpbox");
            GUILayout.Label("Preview Audio", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button("► Play", GUILayout.Height(30))) PlayPreview();

            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("■ Stop", GUILayout.Height(30))) _previewSource?.Stop();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            _loopPreview = GUILayout.Toggle(_loopPreview, "Loop", GUILayout.Height(30));
            if (_previewSource != null) _previewSource.loop = _loopPreview;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void PlayPreview()
        {
            if (_selectedItem == null || _selectedItem.clips == null || _selectedItem.clips.Length == 0) return;
            AudioClip clipToPlay = _selectedItem.clips[Random.Range(0, _selectedItem.clips.Length)];
            if (clipToPlay == null) return;

            _previewSource.clip = clipToPlay;
            _previewSource.volume = _selectedItem.volume;
            _previewSource.pitch = Random.Range(_selectedItem.minPitch, _selectedItem.maxPitch);
            _previewSource.loop = _loopPreview;
            _previewSource.Play();
        }
    }
}