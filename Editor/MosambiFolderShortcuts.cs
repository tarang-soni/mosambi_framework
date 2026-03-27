using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace Mosambi.Tools.Editor
{
    public static class MosambiFolderTools
    {
        [MenuItem("Mosambi/Create Folder &n")]
        public static void CreateAndRenameFolder()
        {
            // 1. Get the current active folder path using Unity's internal method
            string path = GetActiveFolderPath();

            // 2. Create the internal Unity action to handle the "Enter" key rename
            var action = ScriptableObject.CreateInstance<DoCreateFolder>();

            // 3. Trigger the native rename UI
            // Using "ProjectWindowUtil" ensures it handles the folder icon and focus correctly
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                action,
                Path.Combine(path, "New Folder"),
                EditorGUIUtility.FindTexture("Folder Icon"),
                null);
        }

        private static string GetActiveFolderPath()
        {
            var projectWindowUtilType = typeof(ProjectWindowUtil);
            var getActiveFolderPathMethod = projectWindowUtilType.GetMethod("GetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic);

            if (getActiveFolderPathMethod != null)
            {
                return (string)getActiveFolderPathMethod.Invoke(null, null);
            }

            return "Assets";
        }

        // --- INTERNAL ACTION HANDLER ---
        private class DoCreateFolder : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                // Create the physical directory
                if (!Directory.Exists(pathName))
                {
                    Directory.CreateDirectory(pathName);
                    AssetDatabase.ImportAsset(pathName);
                }

                // Select it so you can see it immediately
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(pathName);
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }
        }
    }
}