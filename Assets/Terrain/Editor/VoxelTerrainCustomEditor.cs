using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelTerrain))]
public class VoxelTerrainCustomEditor : Editor {
    void OnEnable() {
        if (target != null && !Application.isPlaying) {
            var terrain = ((VoxelTerrain)target);
            terrain.Init();
            terrain.GenerateMapChunksBase();
            Debug.Log("Enabled");
        }
    }

    void OnDisable() {
        if (target != null && !Application.isPlaying) {
            var terrain = ((VoxelTerrain)target);
            terrain.Dispose();
            terrain.KillChildren();
            Debug.Log("Disabled");
        }
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        GUI.enabled = !Application.isPlaying;
        VoxelTerrain terrain = (VoxelTerrain)target;

        if (GUILayout.Button("Load Map")) {
            EditorUtility.OpenFilePanel("Open saved Voxel map", "Assets", ".vxmap");
        }

        if (GUILayout.Button("Save Map")) {
            EditorUtility.SaveFilePanel("Open saved Voxel map", "Assets", "default-map", ".vxmap");
        }

        if (GUILayout.Button("Update Terrain Settings")) {
            terrain.Init();
        }
    }

    public override bool RequiresConstantRepaint() {
        return true;
    }

    private void OnSceneGUI() {
        if (!Application.isPlaying) {
            ((VoxelTerrain)target).UpdateHook();
        }
    }
}