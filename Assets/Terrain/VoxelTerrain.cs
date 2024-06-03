using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class VoxelTerrain : MonoBehaviour {
    public static VoxelTerrain Instance { get; private set; }

    [Range(1, 8)]
    public int meshJobsPerFrame = 1;
    public Material[] voxelMaterials;
    public bool debugGUI = false;
    public GameObject chunkPrefab;

    private Queue<IVoxelEdit> tempVoxelEdits = new Queue<IVoxelEdit>();
    private Queue<PendingMeshJob> pendingMeshJobs;
    private List<(JobHandle, VoxelChunk, VoxelMesh)> ongoingBakeJobs;
    private List<MeshJobHandler> handlers;
    private List<GameObject> pooledChunkGameObjects;
    private List<GameObject> totalChunks;

    // Initialize the required voxel behaviours
    void Start() {
        Instance = this;
        tempVoxelEdits = new Queue<IVoxelEdit>();
        ongoingBakeJobs = new List<(JobHandle, VoxelChunk, VoxelMesh)>();
        handlers = new List<MeshJobHandler>(meshJobsPerFrame);
        pendingMeshJobs = new Queue<PendingMeshJob>();
        pooledChunkGameObjects = new List<GameObject>();
        totalChunks = new List<GameObject>();

        for (int i = 0; i < meshJobsPerFrame; i++) {
            handlers.Add(new MeshJobHandler());
        }

        // TODO: Supposedly a mem leak here??
        for (int x = -3; x <= 3; x++) {
            for (int y = -1; y <= 1; y++) {
                for (int z = -3; z <= 3; z++) {
                    GameObject newChunk = FetchPooledChunk();
                    VoxelChunk voxelChunk = newChunk.GetComponent<VoxelChunk>();
                    newChunk.transform.position = new Vector3(x, y, z) * VoxelUtils.Size * VoxelUtils.VoxelSizeFactor;
                    voxelChunk.memoryTypeTemp = false;
                    voxelChunk.hasCollisions = true;
                    voxelChunk.voxels = new NativeArray<Voxel>(VoxelUtils.Volume, Allocator.Persistent);
                    TerrainTestJob job = new TerrainTestJob { voxels = voxelChunk.voxels, offset = newChunk.transform.position };
                    voxelChunk.pendingVoxelEditJob = job.Schedule(VoxelUtils.Volume, 2048 * 16);
                    voxelChunk.Remesh(this);
                    totalChunks.Add(newChunk);
                }
            }
        }
    }

    // Dispose of all the native memory that we allocated
    private void OnApplicationQuit() {
        foreach (MeshJobHandler handler in handlers) {
            handler.Complete(new Mesh());
            handler.Dispose();
        }

        foreach (var chunk in totalChunks) {
            var voxelChunk = chunk.GetComponent<VoxelChunk>();

            if (voxelChunk.voxels.IsCreated) {
                voxelChunk.voxels.Dispose();
            }

            if (voxelChunk.lastCounters.IsCreated) {
                voxelChunk.lastCounters.Dispose();
            }
        }
    }

    // Handle completing finished jobs and initiating new ones
    void Update() {
        // Complete the bake jobs that have completely finished
        foreach (var (handle, voxelChunk, mesh) in ongoingBakeJobs) {
            if (handle.IsCompleted) {
                handle.Complete();
                voxelChunk.GetComponent<MeshCollider>().sharedMesh = voxelChunk.sharedMesh;
            }
        }
        ongoingBakeJobs.RemoveAll(item => item.Item1.IsCompleted);

        // Complete the jobs that finished and create the meshes
        foreach (var handler in handlers) {
            if ((handler.finalJobHandle.IsCompleted || (Time.frameCount - handler.startingFrame) > handler.maxFrames) && !handler.Free) {
                VoxelChunk voxelChunk = handler.chunk;

                if (voxelChunk == null)
                    return;

                var voxelMesh = handler.Complete(voxelChunk.sharedMesh);
                
                // Update counters yea!!!!
                if (voxelChunk.voxelCountersHandle != null)
                    UpdateCounters(handler, voxelChunk);

                // Begin a new collision baking jobs
                if (voxelMesh.VertexCount > 0 && voxelMesh.TriangleCount > 0 && voxelMesh.ComputeCollisions) {
                    BakeJob bakeJob = new BakeJob {
                        meshId = voxelChunk.sharedMesh.GetInstanceID(),
                    };

                    var handle = bakeJob.Schedule();
                    ongoingBakeJobs.Add((handle, voxelChunk, voxelMesh));
                }

                // Set chunk renderer settings
                voxelChunk.pendingVoxelEditJob = default;
                var renderer = voxelChunk.GetComponent<MeshRenderer>();
                voxelChunk.GetComponent<MeshFilter>().sharedMesh = voxelChunk.sharedMesh;
                voxelChunk.voxelMaterialsLookup = voxelMesh.VoxelMaterialsLookup;
                renderer.materials = voxelMesh.VoxelMaterialsLookup.Select(x => voxelMaterials[x]).ToArray();

                // Set renderer bounds
                renderer.bounds = voxelChunk.GetBounds();
            }
        }

        // If we can handle it, start unqueueing old work to do editing
        if (pendingMeshJobs.Count < meshJobsPerFrame) {
            IVoxelEdit edit;
            if (tempVoxelEdits.TryDequeue(out edit)) {
                ApplyVoxelEdit(edit, true);
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
                handler.colissions = request.collisions;
                handler.maxFrames = request.maxFrames;
                handler.startingFrame = Time.frameCount;

                // Pass through the edit system for any chunks that should be modified
                handler.voxelCounters.Reset();
                handler.BeginJob(request.chunk.pendingVoxelEditJob, request.chunk.voxels);
            }
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
    public class VoxelEditCountersHandle {
        public int[] changed;
        public int pending;
        public VoxelEditCounterCallback callback;
    }

    // Callback that we can pass to the voxel edit functions to allow us to check how many voxels were added/removed
    public delegate void VoxelEditCounterCallback(int[] changed);

    // Apply a voxel edit to the terrain world
    // Could either be used in game (for destructible terrain) or in editor for creating the terrain map
    public void ApplyVoxelEdit(IVoxelEdit edit, bool neverForget = false, bool immediate = false, VoxelEditCounterCallback callback = null) {
        if (!handlers.All(x => x.Free)) {
            if (neverForget)
                tempVoxelEdits.Enqueue(edit);
            return;
        }

        Bounds editBounds = edit.GetBounds();
        editBounds.Expand(3.0f);

        VoxelEditCountersHandle countersHandle = new VoxelEditCountersHandle {
            changed = new int[voxelMaterials.Length],
            pending = 0,
            callback = callback,
        };

        foreach (var chunk in totalChunks) {
            var voxelChunk = chunk.GetComponent<VoxelChunk>();

            if (voxelChunk.GetBounds().Intersects(editBounds)) {
                voxelChunk.pendingVoxelEditJob.Complete();
                
                if (!voxelChunk.lastCounters.IsCreated) {
                    voxelChunk.lastCounters = new NativeMultiCounter(voxelMaterials.Length, Allocator.Persistent);
                }
                
                JobHandle dep = edit.Apply(chunk.transform.position, voxelChunk.voxels, voxelChunk.lastCounters);
                voxelChunk.pendingVoxelEditJob = dep;
                voxelChunk.voxelCountersHandle = countersHandle;
                countersHandle.pending++;
                voxelChunk.Remesh(this, immediate ? 0 : 5);
            }
        }
    }


    // Update the modified voxel counters of a chunk after finishing meshing
    internal void UpdateCounters(MeshJobHandler handler, VoxelChunk voxelChunk) {
        VoxelEditCountersHandle handle = voxelChunk.voxelCountersHandle;
        if (handle != null) {
            handle.pending--;

            // Check current values, update them
            NativeMultiCounter lastValues = voxelChunk.lastCounters;

            // Store the data back into the sparse voxel array
            for (int i = 0; i < voxelMaterials.Length; i++) {
                int newValue = handler.voxelCounters[i];
                int delta = newValue - lastValues[i];
                handle.changed[i] += delta;
                voxelChunk.lastCounters[i] = newValue;
            }

            // Call the callback when we finished modifiying all requested chunks
            if (handle.pending == 0)
                handle.callback?.Invoke(handle.changed);
        }
    }

    // Used for debugging the amount of jobs remaining
    void OnGUI() {
        var offset = 80;
        void Label(string text) {
            GUI.Label(new Rect(0, offset, 300, 30), text);
            offset += 15;
        }

        if (debugGUI) {
            GUI.Box(new Rect(0, 80, 300, 15*4), "");
            Label($"# of pending mesh jobs: {pendingMeshJobs.Count}");
            Label($"# of pending mesh baking jobs: {ongoingBakeJobs.Count}");
            Label($"# of pooled chunk game objects: {pooledChunkGameObjects.Count}");
            Label($"# of pending voxel edits: {tempVoxelEdits.Count}");
        }
    }
}
