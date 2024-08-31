using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.IO.Compression;
using Task = System.Threading.Tasks.Task;
using FileMode = System.IO.FileMode;
using Unity.Mathematics;
using UnityEngine.Rendering;




#if UNITY_EDITOR
using UnityEditor;
#endif

public class VoxelTerrain : MonoBehaviour {
    public static VoxelTerrain Instance { get; private set; }

    public Vector3Int mapChunkSize;
    [Range(1, 8)]
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
    private int pendingChunks;
    private bool firstGen;

    public delegate void InitGen();
    public event InitGen Finished;
    private VoxelTerrainSettings settings;

    // Initialize buffers and required memory
    public void Init(bool inEditor) {
        Dispose();
        alrDisposed = false;
        Instance = this;
        tempVoxelEdits = new Queue<IVoxelEdit>();
        ongoingBakeJobs = new List<(JobHandle, VoxelChunk, VoxelMesh)>();
        handlers = new List<MeshJobHandler>(VoxelUtils.JobsPerFrame);
        pendingMeshJobs = new Queue<PendingMeshJob>();
        pooledChunkGameObjects = new List<GameObject>();
        totalChunks = new List<GameObject>();
        gameObject.hideFlags = HideFlags.None;

        if (inEditor) {
            settings = new VoxelTerrainSettings();
        } else {
            settings = Utils.Load<VoxelTerrainSettings>("terrain.json");
        }

        VoxelUtils.JobsPerFrame = Mathf.Max(settings.jobsPerFrame, 1);
        VoxelUtils.SchedulingInnerloopBatchCount = Mathf.Max(settings.schedulingInnerloopBatchCount, 1);
        settings.maxFramesForDeferredEdits = Mathf.Max(1, settings.maxFramesForDeferredEdits);

        for (int i = 0; i < VoxelUtils.JobsPerFrame; i++) {
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
        Init(false);

        if (PersistentSaveManager.Instance == null || !PersistentSaveManager.Instance.load) {
            LoadMapAll();
            firstGen = true;
        }
    }

    // Generate the fixed size map with a specific callback to execute for each chunk
    public void GenerateWith(Action<VoxelChunk, int> callback) {
        int index = 0;
        totalChunks.Clear();
        for (int x = -mapChunkSize.x; x < mapChunkSize.x; x++) {
            for (int y = -mapChunkSize.y; y < mapChunkSize.y; y++) {
                for (int z = -mapChunkSize.z; z < mapChunkSize.z; z++) {
                    GameObject newChunk = FetchPooledChunk();
                    VoxelChunk voxelChunk = newChunk.GetComponent<VoxelChunk>();
                    voxelChunk.hideFlags = HideFlags.DontSave;
                    newChunk.transform.position = new Vector3(x, y, z) * VoxelUtils.Size * VoxelUtils.VoxelSizeFactor;
                    callback.Invoke(voxelChunk, index);
                    totalChunks.Add(newChunk);
                    index++;
                }
            }
        }
    }

    public bool LoadMapAll() {
        /*
        if (savedMap == null) {
            Debug.LogWarning("No map set!!");
            return false;
        }

        if (savedMap.mapSize != mapChunkSize) {
            Debug.LogWarning("Current map size does not match up with saved map size");
            Debug.LogWarning("Saved: " + savedMap.mapSize);
            Debug.LogWarning("Current: " + mapChunkSize);
            return false;
        }
        */

        List<MemoryStream> streams = new List<MemoryStream>();

        foreach (var item in savedMap.textAssets) {
            streams.Add(new MemoryStream(item.bytes));
        }

        LoadMap2(streams);
        Debug.Log("Loading terrain from editor map");
        return true;
    }

    public void LoadMap2<T>(List<T> streams, bool sigmoidate=false) where T: Stream {
        firstGen = true;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        int maxRegionFiles = Mathf.CeilToInt((float)((mapChunkSize.x * 2) * (mapChunkSize.y * 2) * (mapChunkSize.z * 2)) / (float)SavedVoxelMap.ChunksInRegion);


        void DecompressionCallback(VoxelChunk voxelChunk, int index) {
            voxelChunk.hasCollisions = true;
            voxelChunk.voxels = new NativeArray<Voxel>(VoxelUtils.Volume, Allocator.Persistent);
            voxelChunk.overwrite = sigmoidate;
        }

        KillChildren();
        GenerateWith(DecompressionCallback);

        List<BufferedStream> cachedStreams = new List<BufferedStream>();

        for (int i = 0; i < streams.Count; i++) {
            if (streams[i] != null) {
                var regionStream = new GZipStream(streams[i], CompressionMode.Decompress);
                cachedStreams.Add(new BufferedStream(regionStream, 1024 * 1024 * 32));
            } else {
                cachedStreams.Add(null);
            }
        }

        VoxelSerialization.DeserializeFromRegions(cachedStreams, totalChunks, mapChunkSize);

        for (int i = 0; i < streams.Count; i++) {
            if (cachedStreams[i] != null)
                cachedStreams[i].Dispose();
        }

        for (int i = 0; i < totalChunks.Count; i++) {
            VoxelChunk voxelChunk = totalChunks[i].GetComponent<VoxelChunk>();
            voxelChunk.Remesh();
            pendingChunks++;
        }
        Debug.Log("Took " + sw.ElapsedMilliseconds + "ms");
    }

    public void LoadMapSkibi(string directory) {
        int maxRegionFiles = Mathf.CeilToInt((float)((mapChunkSize.x * 2) * (mapChunkSize.y * 2) * (mapChunkSize.z * 2)) / (float)SavedVoxelMap.ChunksInRegion);
        List<Stream> streams = new List<Stream>();

        for (int i = 0; i < maxRegionFiles; i++) {
            streams.Add(new MemoryStream(savedMap.textAssets[i].bytes));
        }

        foreach (var item in Directory.EnumerateFiles(directory + "-regions", "*.bytes")) {
            if (item.EndsWith(".bytes")) {
                string name = Path.GetFileNameWithoutExtension(item);
                string[] godThisIsHorrible = name.Split("reg");
                string worse = godThisIsHorrible[1].Split(".bytes")[0];
                int wtf = int.Parse(worse);

                FileStream bruh = File.Open(item, FileMode.Open);
                streams[wtf] = bruh;
            }
        }

        LoadMap2(streams, sigmoidate: true);
        Debug.Log("Loading terrain from user region files");
    }

    // CAN ONLY BE EXECUTED IN THE EDITOR
#if UNITY_EDITOR
    public void SaveMapEditor() {
        savedMap.mapSize = mapChunkSize;
        string soPath = AssetDatabase.GetAssetPath(savedMap);
        int maxRegionFiles = SaveMap(soPath, false);

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

    // Returns the number of region files used
    public int SaveMap(string directory, bool onlySaveTagged) {
        if (!Directory.Exists(directory + "-regions")) {
            Directory.CreateDirectory(directory + "-regions");
        }

        int maxRegionFiles = Mathf.CeilToInt((float)((mapChunkSize.x * 2) * (mapChunkSize.y * 2) * (mapChunkSize.z * 2)) / (float)SavedVoxelMap.ChunksInRegion);

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        List<BufferedStream> streamWriters = new List<BufferedStream>();
        for (int i = 0; i < maxRegionFiles; i++) {
            bool modified = false;
            for (int k = 0; k < SavedVoxelMap.ChunksInRegion; k++) {
                int offset = SavedVoxelMap.ChunksInRegion * i + k;
                VoxelChunk chunk = totalChunks[offset].GetComponent<VoxelChunk>();
                modified |= chunk.overwrite;
            }
            Debug.Log(sw.ElapsedMilliseconds);

            BufferedStream bufferedStream = null;
            if (modified | !onlySaveTagged) {
                string p = directory + "-regions/reg" + i + ".bytes";
                FileStream fileStream = File.Open(p, FileMode.Create);
                GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
                bufferedStream = new BufferedStream(gzipStream, 1024 * 1024 * 32);
            }
            Debug.Log(sw.ElapsedMilliseconds);

            streamWriters.Add(bufferedStream);
        }

        Debug.Log(sw.ElapsedMilliseconds);
        VoxelSerialization.SerializeIntoRegions(streamWriters, totalChunks, savedMap.mapSize);

        List<Task> tasks = new List<Task>();
        for (int i = 0; i < maxRegionFiles; i++) {
            if (streamWriters[i] != null) {
                streamWriters[i].FlushAsync();
                Task t = streamWriters[i].DisposeAsync().AsTask();
                tasks.Add(t);
            }
        }
        Debug.Log(sw.ElapsedMilliseconds);

        Task.WhenAll(tasks).Wait();
        Debug.Log("Successfully saved the map!! Took " + sw.ElapsedMilliseconds + "ms");
        return maxRegionFiles;
    }

    public void KillChildren() {
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    private void OnDestroy() {
        Dispose();
    }

    // Handle completing finished jobs and initiating new ones
    void Update() {
        UpdateHook(false);

        if (pendingChunks == 0 && firstGen) {
            firstGen = false;
            Finished?.Invoke();
        }
    }

    // Separate function since it needs to get called inside the editor
    public void UpdateHook(bool inEditor) {
        if (ongoingBakeJobs == null) {
            Init(inEditor);
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

                var voxelMesh = voxelChunk.UnhookHandler(handler, voxelChunk.sharedMesh);

                // Update counters yea!!!!
                if (voxelChunk.voxelCountersHandle != null) {
                    UpdateCounters(handler, voxelChunk);
                }

                // Begin a new collision baking jobs
                if (voxelMesh.VertexCount > 0 && voxelMesh.TriangleCount > 0 && voxelMesh.ComputeCollisions) {
                    BakeJob bakeJob = new BakeJob {
                        meshId = voxelChunk.sharedMesh.GetInstanceID(),
                    };

                    var handle = bakeJob.Schedule();
                    ongoingBakeJobs.Add((handle, voxelChunk, voxelMesh));
                }

                // Set chunk renderer settings
                voxelChunk.pendingVoxelEdit = default;
                var renderer = voxelChunk.GetComponent<MeshRenderer>();
                voxelChunk.GetComponent<MeshFilter>().sharedMesh = voxelChunk.sharedMesh;
                voxelChunk.voxelMaterialsLookup = voxelMesh.VoxelMaterialsLookup;
                voxelChunk.triangleOffsetLocalMaterials = voxelMesh.TriangleOffsetLocalMaterials;
                renderer.materials = voxelMesh.VoxelMaterialsLookup.Where(x => x < voxelMaterials.Length).Select(x => voxelMaterials[x]).ToArray();

                // Set mesh and renderer bounds
                voxelChunk.sharedMesh.bounds = new Bounds { min = Vector3.zero, max = Vector3.one * VoxelUtils.Size * VoxelUtils.VoxelSizeFactor };
                renderer.bounds = voxelChunk.GetBounds();
                
                if (firstGen)
                    pendingChunks -= 1;
            }
        }

        // If we can handle it, start unqueueing old work to do editing
        if (pendingMeshJobs.Count < VoxelUtils.JobsPerFrame) {
            IVoxelEdit edit;
            if (tempVoxelEdits.TryDequeue(out edit)) {
                ApplyVoxelEdit(edit, neverForget: true, symmetric: false);
            }
        }

        // Begin the jobs for the meshes
        for (int i = 0; i < VoxelUtils.JobsPerFrame; i++) {
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
                request.chunk.HookHandler(handler);
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
    public void ApplyVoxelEdit(IVoxelEdit edit, bool neverForget = false, bool symmetric = false, bool immediate = false, VoxelEditCounterCallback callback = null) {
        if ((!handlers.All(x => x.Free) || pendingMeshJobs.Count > 0) && !symmetric) {
            if (neverForget)
                tempVoxelEdits.Enqueue(edit);
            return;
        }

        if (settings.neverDeferEdits) {
            immediate = true;
        }

        Bounds editBounds = edit.GetBounds();
        editBounds.Expand(3.0f);

        VoxelEditCountersHandle countersHandle = new VoxelEditCountersHandle {
            changed = new int[voxelMaterials.Length],
            pending = 0,
            callback = callback,
        };

        bool shouldRetryNextFrame = false;

        foreach (var chunk in totalChunks) {
            var voxelChunk = chunk.GetComponent<VoxelChunk>();

            if (voxelChunk.GetBounds().Intersects(editBounds)) {
                if (voxelChunk.pendingVoxelEdit != default && symmetric) {
                    shouldRetryNextFrame = true;
                    continue;
                }

                if (!voxelChunk.lastCounters.IsCreated) {
                    //voxelChunk.lastCounters = new NativeMultiCounter(voxelMaterials.Length, Allocator.Persistent);
                }

                voxelChunk.pendingVoxelEdit = edit;
                //voxelChunk.voxelCountersHandle = countersHandle;
                //countersHandle.pending++;
                voxelChunk.Remesh(immediate ? 0 : settings.maxFramesForDeferredEdits);
                voxelChunk.overwrite = true;
            }
        }

        if (shouldRetryNextFrame && neverForget && symmetric) {
            tempVoxelEdits.Enqueue(edit);
        }
    }

    // Get the value of a singular voxel at a world point
    public Voxel TryGetVoxel(Vector3 position) {
        float3 voxelChunk = position / ((float)VoxelUtils.Size * VoxelUtils.VoxelSizeFactor);
        int3 voxelChunkInt = new int3(math.floor(voxelChunk)) + new int3(mapChunkSize.x, mapChunkSize.y, mapChunkSize.z);
        int voxelChunkIndex = voxelChunkInt.z + voxelChunkInt.y * (mapChunkSize.z * 2) + voxelChunkInt.x * ((2 * mapChunkSize.z) * (2 * mapChunkSize.y));

        if (voxelChunkIndex < 0 || voxelChunkIndex >= totalChunks.Count)
            return Voxel.Empty;
        
        VoxelChunk chunk = totalChunks[voxelChunkIndex].GetComponent<VoxelChunk>();

        if (!chunk.voxels.IsCreated)
            return Voxel.Empty;

        float3 p = position;
        p -= new float3(chunk.transform.position);
        p /= VoxelUtils.VertexScaling;
        p += 1.5f * VoxelUtils.VoxelSizeFactor;
        p /= VoxelUtils.VoxelSizeFactor;

        
        if (math.any(p < 0.0f) | math.any(p >= (float)VoxelUtils.Size))
            return Voxel.Empty;

        int index = VoxelUtils.PosToIndex(new uint3(math.round(p)));
        return chunk.voxels[index];
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
