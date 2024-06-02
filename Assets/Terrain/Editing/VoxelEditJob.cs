using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

// Edit job that will modify the voxel chunk data DIRECTLY
[BurstCompile(CompileSynchronously = true)]
struct VoxelEditJob<T> : IJobParallelFor
    where T : struct, IVoxelEdit {
    [ReadOnly] public float3 chunkOffset;

    public T edit;
    public NativeArray<Voxel> voxels;
    
    public void Execute(int index) {
        uint3 id = VoxelUtils.IndexToPos(index);
        float3 position = (math.float3(id));

        /*
        // Needed for voxel size reduction
        position *= voxelScale;
        position -= 1.5f * voxelScale;

        //position -= math.float3(1);
        position *= vertexScaling;
        position *= scalingFactor;
        position += chunkOffset;

        // Chunk offsets + vertex scaling
        //position += math.float3((chunkOffset - (scalingFactor * size / (size - 3.0f)) * 0.5f));

        // Read, modify, write
        byte material = materials[index];
        half density = VoxelUtils.NormalizeHalf(densities[index]);
        Voxel output = edit.Modify(position, new Voxel { material = material, density = density });
        materials[index] = output.material;
        densities[index] = VoxelUtils.NormalizeHalf(output.density);
        */
    }
}