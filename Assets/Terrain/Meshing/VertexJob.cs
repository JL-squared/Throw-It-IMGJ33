using System.Drawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


// Surface mesh job that will generate the isosurface mesh vertices
[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
public struct VertexJob : IJobParallelFor {
    // Positions of the first vertex in edges
    [ReadOnly]
    static readonly uint3[] edgePositions0 = new uint3[] {
        new uint3(0, 0, 0),
        new uint3(1, 0, 0),
        new uint3(1, 1, 0),
        new uint3(0, 1, 0),
        new uint3(0, 0, 1),
        new uint3(1, 0, 1),
        new uint3(1, 1, 1),
        new uint3(0, 1, 1),
        new uint3(0, 0, 0),
        new uint3(1, 0, 0),
        new uint3(1, 1, 0),
        new uint3(0, 1, 0),
    };

    // Positions of the second vertex in edges
    [ReadOnly]
    static readonly uint3[] edgePositions1 = new uint3[] {
        new uint3(1, 0, 0),
        new uint3(1, 1, 0),
        new uint3(0, 1, 0),
        new uint3(0, 0, 0),
        new uint3(1, 0, 1),
        new uint3(1, 1, 1),
        new uint3(0, 1, 1),
        new uint3(0, 0, 1),
        new uint3(0, 0, 1),
        new uint3(1, 0, 1),
        new uint3(1, 1, 1),
        new uint3(0, 1, 1),
    };

    // Voxel native array
    [ReadOnly]
    public NativeArray<Voxel> voxels;

    // Used for fast traversal
    [ReadOnly]
    public NativeArray<byte> enabled;

    // Contains 3D data of the indices of the vertices
    [WriteOnly]
    public NativeArray<int> indices;

    // Vertices that we generated
    [WriteOnly]
    [NativeDisableParallelForRestriction]
    public NativeArray<float3> vertices;

    [WriteOnly]
    [NativeDisableParallelForRestriction]
    public NativeArray<float2> uvs;

    // Vertex Counter
    public NativeCounter.Concurrent counter;

    // Excuted for each cell within the grid
    public void Execute(int index) {
        uint3 position = VoxelUtils.IndexToPos(index);
        indices[index] = int.MaxValue;

        // Idk bruh
        if (math.any(position > math.uint3(VoxelUtils.Size - 2)))
            return;

        float3 vertex = float3.zero;

        // Fetch the byte that contains the number of corners active
        uint enabledCorners = enabled[index];
        bool empty = enabledCorners == 0 || enabledCorners == 255;

        // Early check to quit if the cell if full / empty
        if (empty) return;

        // Doing some marching cube shit here
        uint code = VoxelUtils.EdgeMasks[enabledCorners];
        int count = math.countbits(code);

        // Use linear interpolation when smoothing
        if (!empty) {
            // Create the smoothed vertex
            // TODO: Test out QEF or other methods for smoothing here
            for (int edge = 0; edge < 12; edge++) {
                // Continue if the edge isn't inside
                if (((code >> edge) & 1) == 0) continue;

                uint3 startOffset = edgePositions0[edge];
                uint3 endOffset = edgePositions1[edge];

                int startIndex = VoxelUtils.PosToIndex(startOffset + position);
                int endIndex = VoxelUtils.PosToIndex(endOffset + position);

                // Get the Voxels of the edge
                Voxel startVoxel = voxels[startIndex];
                Voxel endVoxel = voxels[endIndex];

                // Create a vertex on the line of the edge
                float value = math.unlerp(startVoxel.density, endVoxel.density, 0);
                vertex += math.lerp(startOffset, endOffset, value) - math.float3(0.5);
            }
        } else {
            count = 1;
        }

        // Must be offset by vec3(1, 1, 1)
        int vertexIndex = counter.Increment();
        indices[index] = vertexIndex;


        // Calculate per vertex ambient occlusion and apply it
        const float aoSpread = 3f;
        const float aoGlobalOffset = 1.5f;

        // Output vertex in object space
        float3 offset = (vertex / (float)count);
        float3 outputVertex = (offset - 1.0F) + position;
        float ambientOcclusion = VoxelUtils.CalculateVertexAmbientOcclusion(outputVertex, ref voxels, aoSpread, aoGlobalOffset);

        if (float.IsNaN(ambientOcclusion)) {
            ambientOcclusion = 1;
        }


        vertices[vertexIndex] = outputVertex * VoxelUtils.VertexScaling * VoxelUtils.VoxelSizeFactor;
        uvs[vertexIndex] = new float2(ambientOcclusion, 0.0f);
    }
}