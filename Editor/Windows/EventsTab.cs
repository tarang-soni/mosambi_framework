using UnityEngine;
using UnityEditor;
using Mosambi.Events; // Where your event channels live

namespace Mosambi.Tools.Editor
{
    // We use ScriptableObject as the generic T because AudioEvents and GameEvents 
    // might not share a custom base class yet.
    public class EventsTab : GenericDatabaseTab<ScriptableObject>
    {
        public override string TabName => "Event Channels";

        // 1. Define the Sub-Tabs!
        protected override string[] SubTabNames => new string[] { "Game Events", "Audio Events" };

        // 2. Override the search to ONLY look for your specific Event Channel scripts
        // (Otherwise it would load every single ScriptableObject in your whole game!)
        public override void RefreshDatabase()
        {
            _items.Clear();
            
            // Find Game State Channels
            string[] gameEventGuids = AssetDatabase.FindAssets("t:GameStateChannelSO");
            foreach (string guid in gameEventGuids)
            {
                _items.Add(AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            // Find Audio Channels
            string[] audioEventGuids = AssetDatabase.FindAssets("t:AudioEventChannelSO");
            foreach (string guid in audioEventGuids)
            {
                _items.Add(AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)));
            }
        }

        // 3. The Sorting Hat: Tell the UI which items belong in which tab
        protected override bool ItemMatchesSubTab(ScriptableObject item, int subTabIndex)
        {
            if (subTabIndex == 0) // "Game Events" Tab
            {
                return item is GameStateChannelSO; 
            }
            else if (subTabIndex == 1) // "Audio Events" Tab
            {
                return item is AudioEventChannelSO;
            }

            return false;
        }
        protected override void CreateNewItem()
        {
            string gameRoot = EditorPrefs.GetString($"Mosambi_Root_{Application.productName}", "_Project");

            // 1. Determine the specific sub-folder based on the active tab
            string subFolder = _selectedSubTab == 0 ? "Game" : "Audio";
            string targetFolder = $"Assets/{gameRoot}/Data/Events/{subFolder}";

            // --- DEFENSIVE FOLDER CREATION ---
            string absolutePath = System.IO.Path.Combine(Application.dataPath, $"{gameRoot}/Data/Events/{subFolder}");
            if (!System.IO.Directory.Exists(absolutePath))
            {
                System.IO.Directory.CreateDirectory(absolutePath);
                AssetDatabase.Refresh();
            }

            // 2. Determine the default name and title
            string defaultName = _selectedSubTab == 0 ? "NewGameStateEvent" : "NewAudioEvent";
            string title = _selectedSubTab == 0 ? "Create Game Event" : "Create Audio Event";

            string path = EditorUtility.SaveFilePanelInProject(
                title,
                defaultName,
                "asset",
                "Save your new event asset",
                targetFolder
            );

            if (string.IsNullOrEmpty(path)) return;

            // 3. Create the correct ScriptableObject type
            ScriptableObject newItem = null;
            if (_selectedSubTab == 0)
            {
                newItem = ScriptableObject.CreateInstance<GameStateChannelSO>();
            }
            else if (_selectedSubTab == 1)
            {
                newItem = ScriptableObject.CreateInstance<AudioEventChannelSO>();
            }

            if (newItem != null)
            {
                AssetDatabase.CreateAsset(newItem, path);
                AssetDatabase.SaveAssets();

                RefreshDatabase();
                _selectedItem = newItem;
            }
        }
    }
}