using System;
using Unity.Collections;
using UnityEngine;

// Script added to all game objects that represent a chunk
public class VoxelChunk : MonoBehaviour {
    // Either temporary data when loading map from disk
    // Or permamnent chunk data for modified chunks
    public NativeArray<Voxel>? voxels;
    public bool memoryTypeTemp = false;
    public bool hasCollisions = false;

    // Callback that we must invoke when we finish meshing this voxel chunk
    internal VoxelTerrain.VoxelEditCountersHandle voxelCountersHandle;

    // Shared generated mesh
    public Mesh sharedMesh;
    internal int[] voxelMaterialsLookup;

    // Get the AABB world bounds of this chunk
    public Bounds GetBounds() {
        return new Bounds {
            min = transform.position,
            max = transform.position + Vector3.one * VoxelUtils.Size,
        };
    }

    // Remesh the chunk given the parent terrain
    public void Remesh(VoxelTerrain terrain, int maxFrames = 5) {
        if (!voxels.HasValue) {
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