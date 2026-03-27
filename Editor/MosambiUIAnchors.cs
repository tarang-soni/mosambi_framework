using UnityEditor;
using UnityEngine;

namespace Mosambi.Tools.Editor
{
    public static class MosambiUIAnchors
    {
        // % represents Ctrl on Windows and Cmd on Mac
        [MenuItem("Mosambi/UI/Anchors to Corners %SPACE")]
        private static void AnchorsToCorners()
        {
            foreach (Transform transform in Selection.transforms)
            {
                RectTransform t = transform as RectTransform;
                RectTransform pt = transform.parent as RectTransform;

                if (t == null || pt == null) continue;

                // Register the action so you can Ctrl+Z to undo it
                Undo.RecordObject(t, "Anchor to Corners");

                // Calculate the new anchor positions relative to the parent's rect
                Vector2 newAnchorsMin = new Vector2(
                    t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                    t.anchorMin.y + t.offsetMin.y / pt.rect.height
                );
                
                Vector2 newAnchorsMax = new Vector2(
                    t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                    t.anchorMax.y + t.offsetMax.y / pt.rect.height
                );

                // Apply the anchors
                t.anchorMin = newAnchorsMin;
                t.anchorMax = newAnchorsMax;

                // Zero out the offsets so the corners snap perfectly to the anchors
                t.offsetMin = t.offsetMax = Vector2.zero;
            }
        }

        // The validation hotkey string MUST match the execution string exactly
        [MenuItem("Mosambi/UI/Anchors to Corners %SPACE", true)]
        private static bool ValidateAnchorsToCorners()
        {
            return Selection.activeTransform != null && Selection.activeTransform is RectTransform;
        }
    }
}