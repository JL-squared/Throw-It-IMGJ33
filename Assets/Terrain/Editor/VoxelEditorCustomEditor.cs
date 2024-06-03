using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelEditor))]
public class VoxelEditorCustomEditor : Editor {
    public override void OnInspectorGUI() {
        VoxelEditor editor = (VoxelEditor)target;

        editor.brushStrength = EditorGUILayout.Slider("Brush Strength", editor.brushStrength, 0.0f, 10.0f);
        editor.brushRadius = EditorGUILayout.Slider("Brush Radius", editor.brushRadius, 0.0f, 100.0f);
        editor.currentBrush = (VoxelEditor.BrushType)EditorGUILayout.EnumPopup("Current Brush", editor.currentBrush);
    }

    public override bool RequiresConstantRepaint() {
        return true;
    }

    private void OnSceneGUI() {
        if (!Application.isPlaying) {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            ((VoxelEditor)target).Paint(ray, Event.current);
            SceneView.RepaintAll();
        }
    }
}