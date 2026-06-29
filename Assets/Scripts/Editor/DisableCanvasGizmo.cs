using UnityEditor;
using UnityEngine;

public static class DisableCanvasGizmo
{
    [MenuItem("Tools/Magic School/Disable Canvas Gizmo")]
    public static void Execute()
    {
        GizmoUtility.SetGizmoEnabled(typeof(Canvas), false);
        GizmoUtility.SetIconEnabled(typeof(Canvas), false);
        Debug.Log("[DisableCanvasGizmo] Canvas gizmo hidden.");
    }
}
