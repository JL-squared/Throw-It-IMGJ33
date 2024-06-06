using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// Script added to all game objects that represent a chunk
public class VoxelChunk : MonoBehaviour {
    public NativeArray<Voxel> voxels;
    public bool hasCollisions = false;
    public NativeMultiCounter lastCounters;

    // Custom pending job that must be completed before we can mesh the chunk
    // Either voxel edit job, base terrain job, or decompression job
    public JobHandle dependency = default;

    // Callback that we must invoke when we finish meshing this voxel chunk
    public VoxelTerrain.VoxelEditCountersHandle voxelCountersHandle;

    // Shared generated mesh
    public Mesh sharedMesh;
    public int[] voxelMaterialsLookup;

    // Get the AABB world bounds of this chunk
    public Bounds GetBounds() {
        return new Bounds {
            min = transform.position,
            max = transform.position + Vector3.one * VoxelUtils.Size * VoxelUtils.VoxelSizeFactor,
        };
    }

    // Remesh the chunk given the parent terrain
    public void Remesh(int maxFrames = 5) {
        if (!voxels.IsCreated) {
            return;
        }

        VoxelTerrain.Instance.GenerateMesh(this, hasCollisions, maxFrames);
    }
}