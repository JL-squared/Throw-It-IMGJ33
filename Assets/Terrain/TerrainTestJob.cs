using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
struct TerrainTestJob : IJobParallelFor {
    public float3 offset;
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

        Voxel output = voxels[index];
        output.material = 0;
        output.density = (half)(position.y - noise.cellular(position * 0.07f).x * 3.0);
        voxels[index] = output;
    }
}