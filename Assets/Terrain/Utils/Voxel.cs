using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

// CPU representation of what a voxel is. The most important value here is the density value
[StructLayout(LayoutKind.Sequential)]
public struct Voxel {
    public const int size = sizeof(int);

    // Density of the voxel as a half to save some memory
    public half density;

    // Material of the voxel that depicts its color and other parameters
    public byte material;

    // Used for extra color data on a per vertex basis
    public byte _padding;

    // Empty voxel with the empty material
    public readonly static Voxel Empty = new Voxel {
        density = half.zero,
        material = byte.MaxValue,
        _padding = 0,
    };
}

// Voxel container with custom dispose methods
// Each chunk could either have it's own unique container (after being edited once)
// Or have a temp container used from reading back the map data
public abstract class VoxelTempContainer {
    public NativeArray<Voxel> voxels;
    public VoxelChunk chunk;

    // Dispose of the voxel container
    public abstract void TempDispose();
}

// Cached voxel chunk container for chunks with their own temp voxels (for modifs)
public class UniqueVoxelChunkContainer : VoxelTempContainer {
    public override void TempDispose() {
    }
}

// Temporary voxel container for reading back map data
public class MapReadbackTempContainer : VoxelTempContainer {
    public override void TempDispose() {
    }
}