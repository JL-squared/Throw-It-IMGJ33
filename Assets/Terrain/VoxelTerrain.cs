using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(VoxelMesher))]
[RequireComponent(typeof(VoxelCollisions))]
[RequireComponent(typeof(VoxelEdits))]
public class VoxelTerrain : MonoBehaviour {
    public VoxelMesher voxelMesher;
    public VoxelCollisions voxelCollisions;
    public VoxelEdits voxelEdits;


    void Start() {
    }

    // Dispose of all the voxel behaviours
    private void OnApplicationQuit() {
        voxelMesher.Dispose();
        voxelEdits.Dispose();
        voxelCollisions.Dispose();
    }

    private void Update() {
    }

    // Give the chunk's resources back to the main pool
    private void PoolChunkBack(VoxelChunk voxelChunk) {
        /*
        voxelChunk.gameObject.SetActive(false);
        pooledChunkGameObjects.Add(voxelChunk.gameObject);

        if (voxelChunk.container != null && voxelChunk.container is UniqueVoxelChunkContainer) {
            pooledVoxelChunkContainers.Add((UniqueVoxelChunkContainer)voxelChunk.container);
        }

        if (voxelChunk.container is VoxelReadbackRequest) {
            voxelChunk.container.TempDispose();
        }

        voxelChunk.container = null;
        */

        /*
        GameObject gameObject = FetchPooledChunk();

            float size = item.scalingFactor;
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.transform.position = item.position;
            gameObject.transform.localScale = new Vector3(size, size, size);
            
            // RESETS ALL THE OLD CACHED PROPERTIES OF THE CHUNK
            VoxelChunk chunk = gameObject.GetComponent<VoxelChunk>();
            chunk.node = item;
            chunk.voxelCountersHandle = null;
            chunk.voxelMaterialsLookup = null;

            // Only generate chunk voxel data for chunks at lowest depth
            chunk.container = null;
            if (item.depth == VoxelUtils.MaxDepth) {
                chunk.container = FetchVoxelChunkContainer();
                chunk.container.chunk = chunk;
            }

            // Begin the voxel pipeline by generating the voxels for this chunk
            VoxelGenerator.GenerateVoxels(chunk);
            Chunks.TryAdd(item, chunk);
            toMakeVisible.Add(chunk);
            onChunkAdded?.Invoke(chunk);
         */
    }


    // Fetches a pooled chunk, or creates a new one from scratch
    private GameObject FetchPooledChunk() {
        return null;
        /*
        GameObject chunk;

        if (pooledChunkGameObjects.Count == 0) {
            GameObject obj = Instantiate(chunkPrefab, this.transform);
            Mesh mesh = new Mesh();
            obj.GetComponent<VoxelChunk>().sharedMesh = mesh;
            obj.name = $"Voxel Chunk";
            chunk = obj;
        } else {
            chunk = pooledChunkGameObjects[0];
            pooledChunkGameObjects.RemoveAt(0);
            chunk.GetComponent<MeshCollider>().sharedMesh = null;
            chunk.GetComponent<MeshFilter>().sharedMesh = null;
        }

        chunk.SetActive(true);
        return chunk;
        */
    }

    // Fetches a voxel native array, or allocates one from scratch
    private UniqueVoxelChunkContainer FetchVoxelChunkContainer() {
        return null;
    }

    // Update the mesh of the given chunk when we generate it
    private void OnVoxelMeshingComplete(VoxelChunk chunk, VoxelMesh mesh) {
        var renderer = chunk.GetComponent<MeshRenderer>();
        chunk.GetComponent<MeshFilter>().sharedMesh = chunk.sharedMesh;

        // Set mesh and renderer settings
        chunk.voxelMaterialsLookup = mesh.VoxelMaterialsLookup; 
        renderer.materials = mesh.VoxelMaterialsLookup.Select(x => voxelMesher.voxelMaterials[x]).ToArray();

        /*
        // Set renderer bounds
        renderer.bounds = new Bounds {
            min = chunk.node.position,
            max = chunk.node.position + chunk.node.size,
        };
        */
    }

    // Update the mesh collider when we finish collision baking
    private void OnCollisionBakingComplete(VoxelChunk chunk, VoxelMesh mesh) {
        if (mesh.VertexCount > 0 & mesh.TriangleCount > 0) {
            chunk.GetComponent<MeshCollider>().sharedMesh = chunk.sharedMesh;
        }
    }
}
