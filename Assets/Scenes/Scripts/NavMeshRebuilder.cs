using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshRebuilder : MonoBehaviour {
    public NavMeshSurface testSurface;
    public NavMeshSurface terrainSurface;
    public LayerMask mask;
    private AsyncOperation op;
    private bool hooked = false;

    private void Start() {
        terrainSurface.navMeshData = new NavMeshData();
        testSurface.navMeshData = new NavMeshData();
    }

    // https://forum.unity.com/threads/navmesh-mesh-collection-using-submesh-indexing.1607172/

    void UpdateNavMesh() {
        NavMeshSurface surface = null;

        if (terrainSurface.gameObject.activeSelf) {
            surface = terrainSurface;
        } else {
            surface = testSurface;
        }

        NavMeshBuildSettings settings = surface.GetBuildSettings();
        settings.maxJobWorkers = 1;
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

        Debug.Log(sources.Count);
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
