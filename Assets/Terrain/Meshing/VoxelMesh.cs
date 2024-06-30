using UnityEngine;

// The generated voxel mesh that we can render to the player
public struct VoxelMesh {
    public Mesh SharedMesh { get; internal set; }
    public int[] VoxelMaterialsLookup { get; internal set; }
    public bool ComputeCollisions { get; internal set; }
    public int VertexCount { get; internal set; }
    public int TriangleCount { get; internal set; }
    public (byte, int)[] TriangleOffsetLocalMaterials { get; internal set; }

    public static VoxelMesh Empty = new VoxelMesh {
        SharedMesh = null,
        VoxelMaterialsLookup = null,
        VertexCount = 0,
        TriangleCount = 0,
        ComputeCollisions = false,
    };
}