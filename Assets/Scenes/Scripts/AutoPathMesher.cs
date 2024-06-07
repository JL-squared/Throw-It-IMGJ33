using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class AutoPathMesher : MonoBehaviour {
    public NavMeshSurface surface;
    public LayerMask mask;
    private AsyncOperation op;
    private bool hooked = false;

    private void Start() {
        surface.navMeshData = new NavMeshData();
    }

    void UpdateNavMesh() {
        NavMeshBuildSettings settings = surface.GetBuildSettings();
        settings.maxJobWorkers = 2;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2000);
        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(bounds, mask, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);

        for (int i = sources.Count - 1; i >= 0; i--) {
            if (sources[i].sourceObject is Mesh) {
                Mesh mesh = (Mesh)sources[i].sourceObject;

                if (mesh.vertexCount == 0) {
                    sources.RemoveAt(i);
                }
            }
        }

        surface.AddData();
        op = NavMeshBuilder.UpdateNavMeshDataAsync(surface.navMeshData, settings, sources, bounds);
    }

    void Update() {
        if (!hooked && VoxelTerrain.Instance != null) {
            hooked = true;
            VoxelTerrain.Instance.Finished += UpdateNavMesh;
        }

        if (hooked && op != null && op.isDone) {
            UpdateNavMesh();
        }
    }
}
