using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// Script added to all game objects that represent a chunk
public class VoxelChunk : MonoBehaviour {
    public NativeArray<Voxel> voxels;
    public bool hasCollisions = false;
    public NativeMultiCounter lastCounters;

    // Current voxel edits that we must execute
    public IVoxelEdit pendingVoxelEdit = default;

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

    // Call to debug this chunk using debug utils
    public void Debug() {
        DebugUtils.DrawBox(sharedMesh.bounds.center + transform.position, sharedMesh.bounds.size, Color.white);
    }

    // Remesh the chunk given the parent terrain
    public void Remesh(int maxFrames = 5) {
        if (!voxels.IsCreated) {
            return;
        }

        VoxelTerrain.Instance.GenerateMesh(this, hasCollisions, maxFrames);
    }

    public JobHandle HookHandler(MeshJobHandler handler) {
        handler.voxels.CopyFrom(voxels);

        JobHandle dep = default;
        if (pendingVoxelEdit != default)
            dep = pendingVoxelEdit.Apply(transform.position, handler.voxels, lastCounters);
        return handler.BeginJob(dep);
    }

    public VoxelMesh UnhookHandler(MeshJobHandler handler, Mesh mesh) {
        VoxelMesh voxelMesh = handler.Complete(mesh);
        voxels.CopyFrom(handler.voxels);
        return voxelMesh;
    }
}