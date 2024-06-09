using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct FlatMapJob : IJobParallelFor {
    public float3 offset;
    public NativeArray<Voxel> voxels;
    public int totalMats;
    
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

        float minHeight = -1f;
        float stoneHardness = (noise.snoise(position.xz * 0.01f) * 0.5f + 0.5f) * 10 + 2.0f;

        output.material = (byte)(position.y > minHeight ? 0 : 1);
        output.density = (half)(position.y * (position.y > minHeight ? 1 : stoneHardness));
        voxels[index] = output;
    }
}