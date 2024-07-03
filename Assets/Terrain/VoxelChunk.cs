using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// Script added to all game objects that represent a chunk
public class VoxelChunk : MonoBehaviour {
    [HideInInspector] public NativeArray<Voxel> voxels;
    [HideInInspector] public bool hasCollisions = false;
    [HideInInspector] public NativeMultiCounter lastCounters;

    // Current voxel edits that we must execute
    [HideInInspector] public IVoxelEdit pendingVoxelEdit = default;

    // Callback that we must invoke when we finish meshing this voxel chunk
    [HideInInspector] public VoxelTerrain.VoxelEditCountersHandle voxelCountersHandle;

    // Shared generated mesh
    [HideInInspector] public Mesh sharedMesh;
    [HideInInspector] public int[] voxelMaterialsLookup;
    [HideInInspector] public (byte, int)[] triangleOffsetLocalMaterials;

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

    // Check the global material type of a hit triangle index
    public byte GetTriangleIndexMaterialType(int triangleIndex) {
        if (triangleOffsetLocalMaterials == null) {
            return byte.MaxValue;
        }

        for (int i = triangleOffsetLocalMaterials.Length-1; i >= 0; i--) {
            (byte localMaterial, int offset) = triangleOffsetLocalMaterials[i];
            if (triangleIndex > offset) {
                return (byte)voxelMaterialsLookup[i];
            }
        }

        return byte.MaxValue;
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