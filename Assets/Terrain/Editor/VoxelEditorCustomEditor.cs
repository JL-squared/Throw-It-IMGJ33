using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelEditor))]
public class VoxelEditorCustomEditor : Editor {
    private void OnEnable() {
        var t = ((VoxelEditor)target);
        t.heldCtrl = false;
        t.heldShift = false;
        t.allowedToEdit = false;
    }

    public override void OnInspectorGUI() {
        VoxelEditor editor = (VoxelEditor)target;

        editor.currentBrush = (VoxelEditor.BrushType)EditorGUILayout.EnumPopup("Current Brush", editor.currentBrush);
        editor.brushStrength = EditorGUILayout.Slider("Brush Strength", editor.brushStrength, 0.0f, 10.0f);
        editor.brushRadius = EditorGUILayout.Slider("Brush Radius", editor.brushRadius, 0.0f, 10.0f);
        editor.material = (byte)Mathf.Clamp(EditorGUILayout.IntField("Material", editor.material), 0, 255);

        switch (editor.currentBrush) {
            case VoxelEditor.BrushType.AddRemove:
                break;
            case VoxelEditor.BrushType.RaiseLower:
                break;
            case VoxelEditor.BrushType.Sphere:
                editor.paintOnly = EditorGUILayout.Toggle("Paint Only", editor.paintOnly);
                break;
            case VoxelEditor.BrushType.Cube:
                editor.paintOnly = EditorGUILayout.Toggle("Paint Only", editor.paintOnly);
                break;
            case VoxelEditor.BrushType.Flatten:
                break;
            case VoxelEditor.BrushType.Noise:
                editor.noiseScale = EditorGUILayout.FloatField("Noise Scale", editor.noiseScale);
                editor.noiseType = (NoiseVoxelEdit.NoiseType)EditorGUILayout.EnumPopup("Noise Type", editor.noiseType);
                editor.noiseDimensionality = (NoiseVoxelEdit.Dimensionality)EditorGUILayout.EnumPopup("Noise Dimensionality", editor.noiseDimensionality);
                break;
            case VoxelEditor.BrushType.SetDensity:
                editor.targetDensity = EditorGUILayout.FloatField("Target Density", editor.targetDensity);
                break;
            case VoxelEditor.BrushType.SetHeight:
                editor.targetHeight = EditorGUILayout.FloatField("Target Height", editor.targetHeight);
                break;
        }
    }

    public override bool RequiresConstantRepaint() {
        return true;
    }

    private void OnSceneGUI() {
        var t = ((VoxelEditor)target);
        if (!Application.isPlaying) {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            t.Paint(ray, Event.current);
            SceneView.RepaintAll();
        }
    }
}