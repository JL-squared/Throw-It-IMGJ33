using System.IO;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(VoxelTerrain))]
public class VoxelTerrainCustomEditor : Editor {
    private void OnEnable() {
        EditorSceneManager.sceneSaving += OnSavingScene;
    }

    private void OnSavingScene(UnityEngine.SceneManagement.Scene scene, string path) {
        /*
        VoxelTerrain terrain = (VoxelTerrain)target;
        VoxelEditor editor = terrain.GetComponent<VoxelEditor>();
        terrain.KillChildren();
        editor.dirtyEdits = false;
        SaveUwu(terrain);
        */
    }

    private void OnDisable() {
        if (target != null) {
            VoxelTerrain terrain = (VoxelTerrain)target;
            VoxelEditor editor = terrain.GetComponent<VoxelEditor>();

            if (editor.dirtyEdits && editor.allowedToEdit) {
                editor.dirtyEdits = false;
                if (EditorUtility.DisplayDialog("Unsaved Voxel Changes", "Do you wanna save the ohio?", "Save")) {
                    SaveUwu(terrain);
                }
            }

            terrain.Dispose();
            editor.allowedToEdit = false;
            terrain.KillChildren();
        }
    }

    private void Callback(VoxelChunk voxelChunk, int index) {
        voxelChunk.hasCollisions = true;
        voxelChunk.voxels = new NativeArray<Voxel>(VoxelUtils.Volume, Allocator.Persistent);
        FlatMapJob job = new FlatMapJob { };
        voxelChunk.pendingVoxelEdit = job;
        voxelChunk.Remesh();
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        GUI.enabled = !Application.isPlaying;
        VoxelTerrain terrain = (VoxelTerrain)target;
        VoxelEditor editor = terrain.GetComponent<VoxelEditor>();

        if (GUILayout.Button("Generate New Map")) {
            terrain.Dispose();
            terrain.KillChildren();
            terrain.Init(true);
            terrain.GenerateWith(Callback);
            editor.allowedToEdit = true;
        }

        if (GUILayout.Button("Clear Children")) {
            terrain.KillChildren();
            editor.allowedToEdit = false;
        }

        if (GUILayout.Button("Update Terrain Settings")) {
            terrain.Init(true);
        }


        if (GUILayout.Button("Deserialize (decompress)")) {
            terrain.Dispose();
            terrain.KillChildren();
            terrain.Init(true);
            editor.allowedToEdit = terrain.LoadMapAll();
        }

        GUI.enabled = editor.allowedToEdit;
        if (GUILayout.Button("Serialize (compress)")) {
            editor.dirtyEdits = false;
            SaveUwu(terrain);
        }
        GUI.enabled = true;
    }

    private static void SaveUwu(VoxelTerrain terrain) {
        if (terrain.savedMap != null) {
            terrain.SaveMapEditor();
        }
    }

    public override bool RequiresConstantRepaint() {
        return true;
    }

    private void OnSceneGUI() {
        if (!Application.isPlaying) {
            ((VoxelTerrain)target).UpdateHook(true);
        }
    }
}