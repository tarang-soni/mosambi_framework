using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class CanvasToggleOverlay
{
    static CanvasToggleOverlay()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyGUI;
    }

    private static void HandleHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        // Only show if the object has a Canvas component
        Canvas canvas = obj.GetComponent<Canvas>();
        if (canvas == null) return;

        // CALCULATING THE RED CIRCLE POSITION:
        // selectionRect starts where the text/icon of the object begins.
        // To hit your red circle, we go to absolute 0 and move right by 28 pixels.
        Rect buttonRect = new Rect(selectionRect);
        buttonRect.x = 30;
        buttonRect.width = 14;
        buttonRect.height = 14;
        buttonRect.y += 1;

        // Simple Toggle Visuals
        Color defaultColor = GUI.color;

        // Green for ON, Red/Dark for OFF
        GUI.color = canvas.enabled ? new Color(0.2f, 1f, 0.2f) : new Color(1f, 0.2f, 0.2f, 0.5f);

        // We use a small 'box' or 'circle' character
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 12;

        // Show a solid dot if enabled, an 'x' or empty circle if disabled
        string label = canvas.enabled ? "●" : "○";

        if (GUI.Button(buttonRect, label, style))
        {
            Undo.RecordObject(canvas, "Toggle Canvas");
            canvas.enabled = !canvas.enabled;
            EditorUtility.SetDirty(canvas);

            // This ensures the UI refreshes immediately
            EditorApplication.RepaintHierarchyWindow();
        }

        GUI.color = defaultColor;
    }
}