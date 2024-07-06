using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshRebuilder : MonoBehaviour {
    // TODO: Implement fully 3D asynchronous pathfinding using a custom A* or floodfill pathfinder
    // The unity navmesh implementation doesn't allow us to generate off mesh links automatically and it kinda sucks anyways
    public bool useNavMesh = false;
    public NavMeshSurface testSurface;
    public NavMeshSurface terrainSurface;
    public LayerMask mask;
    private AsyncOperation op;


    private void Start() {
        terrainSurface.navMeshData = new NavMeshData();
        testSurface.navMeshData = new NavMeshData();
    }

    // https://forum.unity.com/threads/navmesh-mesh-collection-using-submesh-indexing.1607172/
    public void UpdateNavMesh() {
        NavMeshSurface surface = null;

        if (terrainSurface.gameObject.activeSelf) {
            surface = terrainSurface;
        } else {
            surface = testSurface;
        }

        NavMeshBuildSettings settings = surface.GetBuildSettings();
        settings.maxJobWorkers = 1;
        settings.ledgeDropHeight = 10;
        settings.maxJumpAcrossDistance = 10;
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
        if (op != null && op.isDone && useNavMesh) {
            UpdateNavMesh();
        }
    }
}
