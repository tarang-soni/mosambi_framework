using UnityEngine;
using System.Collections.Generic;

namespace Mosambi.Tools.Editor
{
    // Needs to be serializable to show up in the Inspector
    [System.Serializable]
    public struct MosambiGroup
    {
        public string Name;
        public Color HeaderColor;
    }

    [CreateAssetMenu(fileName = "MosambiHierarchySettings", menuName = "Mosambi/Settings/Hierarchy Config")]
    public class MosambiHierarchySettingsSO : ScriptableObject
    {
        [Header("Hierarchy Groups")]
        [Tooltip("The names and colors of the headers you want to use in your scenes.")]
        public List<MosambiGroup> groups = new List<MosambiGroup>()
        {
            new MosambiGroup { Name = "MANAGERS", HeaderColor = new Color(1f, 0.8f, 0.4f, 1f) },
            new MosambiGroup { Name = "UI", HeaderColor = new Color(0.4f, 0.8f, 1f, 1f) },
            new MosambiGroup { Name = "GAMEPLAY", HeaderColor = new Color(1f, 0.4f, 0.4f, 1f) },
            new MosambiGroup { Name = "ENVIRONMENT", HeaderColor = new Color(0.4f, 1f, 0.4f, 1f) },
            new MosambiGroup { Name = "LIGHTING", HeaderColor = new Color(1f, 1f, 0.6f, 1f) },
            new MosambiGroup { Name = "POOLS", HeaderColor = new Color(0.8f, 0.4f, 1f, 1f) },
            new MosambiGroup { Name = "SYSTEMS", HeaderColor = new Color(0.6f, 0.6f, 0.6f, 1f) }
        };
    }
}