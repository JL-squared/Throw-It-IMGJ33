using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// Script added to all game objects that represent a chunk
public class VoxelChunk : MonoBehaviour {
    // Either temporary data when loading map from disk
    // Or permamnent chunk data for modified chunks
    public NativeArray<Voxel> voxels;
    public bool memoryTypeTemp = false;
    public bool hasCollisions = false;
    public NativeMultiCounter lastCounters;

    // Pending voxel edit job occuring on this chunk
    public JobHandle pendingVoxelEditJob = default;

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
    public void Remesh(VoxelTerrain terrain, int maxFrames = 5) {
        if (!voxels.IsCreated) {
            return;
        }

        if (memoryTypeTemp) {
            // Simply fetch map data again and regenerate it
            // Just do it????
            //throw new NotImplementedException();
        } else {
            // Regenerate the mesh based on the unique voxel container
            terrain.GenerateMesh(this, hasCollisions, maxFrames);
        }
    }
}