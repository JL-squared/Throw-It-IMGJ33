using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class VoxelTerrain : MonoBehaviour {
    [Range(1, 8)]
    public int meshJobsPerFrame = 1;
    public Material[] voxelMaterials;
    public bool debugGizmos = false;
    public GameObject chunkPrefab;

    private Queue<PendingMeshJob> pendingMeshJobs;
    private List<(JobHandle, VoxelChunk, VoxelMesh)> ongoingBakeJobs;
    private List<MeshJobHandler> handlers;
    private List<GameObject> pooledChunkGameObjects;

    // Initialize the required voxel behaviours
    void Start() {
        ongoingBakeJobs = new List<(JobHandle, VoxelChunk, VoxelMesh)>();
        handlers = new List<MeshJobHandler>(meshJobsPerFrame);
        pendingMeshJobs = new Queue<PendingMeshJob>();
        pooledChunkGameObjects = new List<GameObject>();

        for (int i = 0; i < meshJobsPerFrame; i++) {
            handlers.Add(new MeshJobHandler());
        }

        for (int x = -3; x <= 3; x++) {
            for (int y = -3; y <= 3; y++) {
                GameObject newChunk = FetchPooledChunk();
                VoxelChunk voxelChunk = newChunk.GetComponent<VoxelChunk>();
                newChunk.transform.position = new Vector3(x * VoxelUtils.Size, -VoxelUtils.Size, y * VoxelUtils.Size);
                voxelChunk.memoryTypeTemp = false;
                voxelChunk.hasCollisions = true;
                voxelChunk.voxels = new NativeArray<Voxel>(VoxelUtils.Volume, Allocator.Persistent);
                TerrainTestJob job = new TerrainTestJob { voxels = voxelChunk.voxels.Value };
                job.Run(VoxelUtils.Volume);
                voxelChunk.Remesh(this);
            }
        }
    }

    void Update() {
        // Complete the jobs that finished and create the meshes
        foreach (var handler in handlers) {
            if ((handler.finalJobHandle.IsCompleted || (Time.frameCount - handler.startingFrame) > handler.maxFrames) && !handler.Free) {
                VoxelChunk voxelChunk = handler.chunk;

                if (voxelChunk == null)
                    return;

                var voxelMesh = handler.Complete(voxelChunk.sharedMesh);

                /*
                if (voxelChunk.voxelCountersHandle != null)
                    terrain.voxelEdits.UpdateCounters(handler, voxelChunk);
                */

                // Begin a new collision baking jobs
                if (voxelMesh.VertexCount > 0 && voxelMesh.TriangleCount > 0 && voxelMesh.ComputeCollisions) {
                    BakeJob bakeJob = new BakeJob {
                        meshId = voxelChunk.sharedMesh.GetInstanceID(),
                    };

                    var handle = bakeJob.Schedule();
                    ongoingBakeJobs.Add((handle, voxelChunk, voxelMesh));
                }

                // Set chunk renderer settings
                var renderer = voxelChunk.GetComponent<MeshRenderer>();
                voxelChunk.GetComponent<MeshFilter>().sharedMesh = voxelChunk.sharedMesh;
                voxelChunk.voxelMaterialsLookup = voxelMesh.VoxelMaterialsLookup;
                renderer.materials = voxelMesh.VoxelMaterialsLookup.Select(x => voxelMaterials[x]).ToArray();

                // Set renderer bounds
                renderer.bounds = voxelChunk.GetBounds();
            }
        }

        // Begin the jobs for the meshes
        for (int i = 0; i < meshJobsPerFrame; i++) {
            PendingMeshJob request = PendingMeshJob.Empty;
            if (pendingMeshJobs.TryDequeue(out request)) {
                if (!handlers[i].Free) {
                    pendingMeshJobs.Enqueue(request);
                    continue;
                }

                MeshJobHandler handler = handlers[i];
                handler.chunk = request.chunk;
                handler.voxels.CopyFrom(request.chunk.voxels.Value);
                handler.colissions = request.collisions;
                handler.maxFrames = request.maxFrames;
                handler.startingFrame = Time.frameCount;

                // Pass through the edit system for any chunks that should be modified
                handler.voxelCounters.Reset();
                handler.BeginJob(default);
            }
        }

        // Complete the bake jobs that have completely finished
        foreach (var (handle, voxelChunk, mesh) in ongoingBakeJobs) {
            if (handle.IsCompleted) {
                handle.Complete();
                voxelChunk.GetComponent<MeshCollider>().sharedMesh = voxelChunk.sharedMesh;
            }
        }
        ongoingBakeJobs.RemoveAll(item => item.Item1.IsCompleted);
    }

    // Dispose of all the voxel behaviours
    private void OnApplicationQuit() {
        foreach (MeshJobHandler handler in handlers) {
            handler.Complete(new Mesh());
            handler.Dispose();
        }
    }

    // Begin generating the mesh data using the given chunk and voxel container
    // Automatically creates a dependency from the editing system if it is editing modified chunks
    public void GenerateMesh(VoxelChunk chunk, bool collisions, int maxFrames = 5) {
        if (chunk.voxels == null)
            return;

        var job = new PendingMeshJob {
            chunk = chunk,
            collisions = collisions,
            maxFrames = maxFrames,
        };

        if (pendingMeshJobs.Contains(job))
            return;

        pendingMeshJobs.Enqueue(job);
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
    }

    // Reference that we can use to fetch modified data of a voxel edit
    internal class VoxelEditCountersHandle {
        internal int[] changed;
        internal int pending;
        internal VoxelEditCounterCallback callback;
    }

    // Callback that we can pass to the voxel edit functions to allow us to check how many voxels were added/removed
    public delegate void VoxelEditCounterCallback(int[] changed);

    // Apply a voxel edit to the terrain world
    // Could either be used in game (for destructible terrain) or in editor for creating the terrain map
    public void ApplyVoxelEdit(IVoxelEdit edit, bool neverForget = false, bool immediate = false, VoxelEditCounterCallback callback = null) {
        /*
        // Custom job to find all the octree nodes that touch the bounds
        NativeList<OctreeNode>? temp;
        terrain.VoxelOctree.TryCheckAABBIntersection(bounds, out temp);

        // Re-mesh the chunks
        foreach (var node in temp) {
            VoxelChunk chunk = terrain.Chunks[node];
            chunk.voxelCountersHandle = countersHandle;
            countersHandle.pending++;
            chunk.Remesh(terrain, immediate ? 0 : 5);
        }
        */
    }
}
