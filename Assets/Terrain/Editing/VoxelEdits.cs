using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

// Handles keeping track of voxel edits and dynamic edits in the world
public class VoxelEdits : VoxelBehaviour {
    // Reference that we can use to fetch modified data of a voxel edit
    internal class VoxelEditCountersHandle {
        internal int[] changed;
        internal int pending;
        internal VoxelEditCounterCallback callback;
    } 

    // Callback that we can pass to the voxel edit functions to allow us to check how many voxels were added/removed
    public delegate void VoxelEditCounterCallback(int[] changed);

    public bool debugGizmos = false;
    
    // Temporary place for voxel/dynamic edits that have not been applied yet
    internal Queue<IVoxelEdit> tempVoxelEdits = new Queue<IVoxelEdit>();
    
    // Initialize the voxel edits handler
    internal override void Init() {
    }

    // Dispose of any memory
    internal override void Dispose() {
    }

    private void Update() {
    }

    // Apply a voxel edit to the terrain world
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

    // Check if a chunk contains voxel edits
    public bool IsChunkAffectedByVoxelEdits(VoxelChunk chunk) {
        return false;
    }

    // Create an apply job dependeny for a chunk that has voxel edits
    public JobHandle TryGetApplyVoxelEditJobDependency(VoxelChunk chunk, ref NativeArray<Voxel> voxels, NativeMultiCounter counters, JobHandle dependency) {
        return default;
        /*
        if (!IsChunkAffectedByVoxelEdits(chunk)) {
            if (chunk.voxelCountersHandle != null) {
                chunk.voxelCountersHandle.pending--;
                chunk.voxelCountersHandle = null;
            }
            
            return dependency;
        }

        VoxelEditOctreeNode.RawNode raw = new VoxelEditOctreeNode.RawNode {
            position = chunk.node.position,
            depth = chunk.node.depth,
            size = chunk.node.size,
        };

        int index = chunkLookup[raw];
        SparseVoxelDeltaData data = sparseVoxelData[index];

        JobHandle newDep = JobHandle.CombineDependencies(dependency, data.applyJobHandle);

        VoxelEditApplyJob job = new VoxelEditApplyJob {
            data = data,
            voxels = voxels,
            counters = counters,
        };
        return job.Schedule(VoxelUtils.Volume, 2048 * 8, newDep);
        */
    }    

    // Update the modified voxel counters of a chunk after finishing meshing
    internal void UpdateCounters(MeshJobHandler handler, VoxelChunk voxelChunk)
    {
        /*
        VoxelEditCountersHandle handle = voxelChunk.voxelCountersHandle;
        if (handle != null)
        {
            handle.pending--;
            int lookup = chunkLookup[new VoxelEditOctreeNode.RawNode
            {
                position = voxelChunk.node.position,
                depth = voxelChunk.node.depth,
                size = voxelChunk.node.size,
            }];

            // Check current values, update them
            SparseVoxelDeltaData data = sparseVoxelData[lookup];
            NativeArray<int> lastValues = data.lastCounters;

            // Store the data back into the sparse voxel array
            for (int i = 0; i < terrain.VoxelMesher.voxelMaterials.Length; i++) {
                int newValue = handler.voxelCounters[i];
                int delta = newValue - lastValues[i];
                handle.changed[i] += delta;
                data.lastCounters[i] = newValue;
            }

            // Call the callback when we finished modifiying all requested chunks
            if (handle.pending == 0)
                handle.callback?.Invoke(handle.changed);            
        }
        */
    }
}
