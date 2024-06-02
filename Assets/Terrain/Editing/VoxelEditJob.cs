using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

// Edit job that will modify the voxel chunk data DIRECTLY
[BurstCompile(CompileSynchronously = true)]
struct VoxelEditJob<T> : IJobParallelFor
    where T : struct, IVoxelEdit {
    [ReadOnly] public float3 offset;

    public T edit;
    public NativeArray<Voxel> voxels;
    
    public void Execute(int index) {
        uint3 id = VoxelUtils.IndexToPos(index);
        float3 position = (math.float3(id));

        // Needed for voxel size reduction
        position *= VoxelUtils.VoxelSizeFactor;
        position -= 1.5f * VoxelUtils.VoxelSizeFactor;

        //position -= math.float3(1);
        position *= VoxelUtils.VertexScaling;
        position += offset;

        // Read, modify, write
        Voxel voxel = voxels[index];
        voxels[index] = edit.Modify(position, voxel);
    }
}