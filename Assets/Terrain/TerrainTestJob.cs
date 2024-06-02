using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
struct TerrainTestJob : IJobParallelFor {
    public NativeArray<Voxel> voxels;
    
    public void Execute(int index) {
        uint3 id = VoxelUtils.IndexToPos(index);
        float3 position = (math.float3(id));

        Voxel output = voxels[index];
        output.material = 0;
        output.density = (half)(position.y - 50 - noise.cellular(position * 0.07f).x * 3.0);
        voxels[index] = output;
    }
}