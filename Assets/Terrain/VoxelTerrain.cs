using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VoxelTerrain : MonoBehaviour {
    public static VoxelTerrain Instance { get; private set; }

    public Vector3Int mapChunkSize;
    [Range(1, 8)]
    public int meshJobsPerFrame = 1;
    public Material[] voxelMaterials;
    public bool debugGUI = false;
    public GameObject chunkPrefab;
    public SavedVoxelMap savedMap;

    private Queue<IVoxelEdit> tempVoxelEdits = new Queue<IVoxelEdit>();
    private Queue<PendingMeshJob> pendingMeshJobs;
    private List<(JobHandle, VoxelChunk, VoxelMesh)> ongoingBakeJobs;
    private List<MeshJobHandler> handlers;
    private List<GameObject> pooledChunkGameObjects;
    private List<GameObject> totalChunks;

    private bool alrDisposed = false;

    // Initialize buffers and required memory
    public void Init() {
        Dispose();
        alrDisposed = false;
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
    }

    // Dispose of all allocated memory
    public void Dispose() {
        if (alrDisposed)
            return;
        alrDisposed = true;

        if (handlers != null) {
            foreach (MeshJobHandler handler in handlers) {
                handler.Complete(new Mesh());
                handler.Dispose();
            }
        }

        if (totalChunks != null) {
            foreach (var chunk in totalChunks) {
                if (chunk != null) {
                    var voxelChunk = chunk.GetComponent<VoxelChunk>();

                    if (voxelChunk.voxels.IsCreated) {
                        voxelChunk.voxels.Dispose();
                    }

                    if (voxelChunk.lastCounters.IsCreated) {
                        voxelChunk.lastCounters.Dispose();
                    }
                }
            }
        }
    }

    // Initialize the required voxel behaviours and load the map
    void Start() {
        KillChildren();
        Init();
        LoadMap();
    }

    // Generate the fixed size map with a specific callback to execute for each chunk
    public void GenerateWith(Action<VoxelChunk, int> callback) {
        int index = 0;
        for (int x = -mapChunkSize.x; x < mapChunkSize.x; x++) {
            for (int y = -mapChunkSize.y; y < mapChunkSize.y; y++) {
                for (int z = -mapChunkSize.z; z < mapChunkSize.z; z++) {
                    GameObject newChunk = FetchPooledChunk();
                    VoxelChunk voxelChunk = newChunk.GetComponent<VoxelChunk>();
                    newChunk.transform.position = new Vector3(x, y, z) * VoxelUtils.Size * VoxelUtils.VoxelSizeFactor;
                    callback.Invoke(voxelChunk, index);
                    totalChunks.Add(newChunk);
                    index++;
                }
            }
        }
    }

    // Load the map from the saved voxel map representation
    public bool LoadMap() {
        if (savedMap == null) {
            Debug.LogWarning("No map set!!");
            return false;
        }

        MemoryStream regionStream = null;

        if (savedMap.mapSize != mapChunkSize) {
            Debug.LogWarning("Current map size does not match up with saved map size");
            Debug.LogWarning("Saved: " + savedMap.mapSize);
            Debug.LogWarning("Current: " + mapChunkSize);
            return false;
        }

        int currentRegionIndex = 0;

        void DecompressionCallback(VoxelChunk voxelChunk, int index) {
            if (index % SavedVoxelMap.ChunksInRegion == 0) {
                regionStream = new MemoryStream(savedMap.textAssets[currentRegionIndex].bytes);
                currentRegionIndex++;
            }

            int test = VoxelUtils.Volume * Voxel.size;
            Span<byte> testByteSpan = new Span<byte>(new byte[test]);
            regionStream.Read(testByteSpan);

            ReadOnlySpan<Voxel> voxels = MemoryMarshal.Cast<byte, Voxel>(testByteSpan);

            voxelChunk.hasCollisions = true;
            voxelChunk.voxels = new NativeArray<Voxel>(voxels.ToArray(), Allocator.Persistent);
            voxelChunk.Remesh();
        }

        KillChildren();
        GenerateWith(DecompressionCallback);
        return true;
    }

    // CAN ONLY BE EXECUTED IN THE EDITOR
#if UNITY_EDITOR
    public void SaveMap() {
        savedMap.mapSize = mapChunkSize;
        int maxRegionFiles = Mathf.CeilToInt((float)((mapChunkSize.x * 2) * (mapChunkSize.y * 2) * (mapChunkSize.z * 2)) / (float)SavedVoxelMap.ChunksInRegion);

        string soPath = AssetDatabase.GetAssetPath(savedMap);
        if (!Directory.Exists(soPath + "-regions")) {
            Directory.CreateDirectory(soPath + "-regions");
        }

        List<FileStream> streamWriters = new List<FileStream>();
        for (int i = 0; i < maxRegionFiles; i++) {
            string p = soPath + "-regions/reg" + i + ".bytes";
            streamWriters.Add(File.Open(p, FileMode.Create));
        }

        for (int i = 0; i < totalChunks.Count; i++) {
            int currentRegionFile = i / SavedVoxelMap.ChunksInRegion;
            FileStream writer = streamWriters[currentRegionFile];

            VoxelChunk chunk = totalChunks[i].GetComponent<VoxelChunk>();
            NativeArray<Voxel> voxels = chunk.voxels;
            NativeSlice<byte> readOnly = voxels.Slice().SliceConvert<byte>();
            byte[] bytes = readOnly.ToArray();
            Debug.Log(bytes.Length);
            writer.Write(bytes);
        }

        for (int i = 0; i < maxRegionFiles; i++) {
            streamWriters[i].Dispose();
        }
        AssetDatabase.Refresh();

        savedMap.textAssets = new TextAsset[maxRegionFiles];
        for (int i = 0; i < maxRegionFiles; i++) { 
            savedMap.textAssets[i] = AssetDatabase.LoadAssetAtPath<TextAsset>(soPath + "-regions/reg" + i + ".bytes");

            if (savedMap.textAssets[i] == null) {
                Debug.LogError("NOT GOOD");
            }
        }

        EditorUtility.SetDirty(savedMap);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Successfully saved the map!!");
    }
#endif

    public void KillChildren() {
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    // Dispose of all the native memory that we allocated
    private void OnApplicationQuit() {
        Dispose();
    }

    // Handle completing finished jobs and initiating new ones
    void Update() {
        UpdateHook();
    }

    // Separate function since it needs to get called inside the editor
    public void UpdateHook() {
        if (ongoingBakeJobs == null) {
            Init();
        }

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
                voxelChunk.dependency = default;
                var renderer = voxelChunk.GetComponent<MeshRenderer>();
                voxelChunk.GetComponent<MeshFilter>().sharedMesh = voxelChunk.sharedMesh;
                voxelChunk.voxelMaterialsLookup = voxelMesh.VoxelMaterialsLookup;
                renderer.materials = voxelMesh.VoxelMaterialsLookup.Select(x => voxelMaterials[x]).ToArray();

                // Set mesh and renderer bounds
                voxelChunk.sharedMesh.bounds = new Bounds { min = Vector3.zero, max = Vector3.one * VoxelUtils.Size * VoxelUtils.VoxelSizeFactor };
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
                handler.BeginJob(request.chunk.dependency, request.chunk.voxels);
            }
        }
    }

    // Begin generating the mesh data using the given chunk and voxel container
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
                voxelChunk.dependency.Complete();
                
                if (!voxelChunk.lastCounters.IsCreated) {
                    voxelChunk.lastCounters = new NativeMultiCounter(voxelMaterials.Length, Allocator.Persistent);
                }
                
                JobHandle dep = edit.Apply(chunk.transform.position, voxelChunk.voxels, voxelChunk.lastCounters);
                voxelChunk.dependency = dep;
                voxelChunk.voxelCountersHandle = countersHandle;
                countersHandle.pending++;
                voxelChunk.Remesh(immediate ? 0 : 5);
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
