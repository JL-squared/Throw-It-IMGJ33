using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


// https://www.desmos.com/calculator/ufsc6rllsk
[assembly: RegisterGenericJobType(typeof(VoxelEditJob<ParametricExplosionVoxelEdit>))]
public struct ParametricExplosionVoxelEdit : IVoxelEdit {
    [ReadOnly] public float3 center;
    [ReadOnly] public float radius;
    [ReadOnly] public float strength;
    [ReadOnly] public byte material;
    [ReadOnly] public float jParam;
    [ReadOnly] public float hParam;

    public JobHandle Apply(float3 offset, NativeArray<Voxel> voxels, NativeMultiCounter counters) {
        return IVoxelEdit.ApplyGeneric(this, offset, voxels, counters);
    }

    public Bounds GetBounds() {
        return new Bounds {
            center = center,
            extents = new Vector3(radius, radius, radius),
        };
    }

    float k(float x) {
        return math.pow(10, -x);
    }

    float g(float x) {
        return math.sqrt(x*x + 0.001f);
    }

    float n(float x) {
        float d = math.min(x, 1f);
        return math.sin(math.PI * d + math.PI / 2f) * 0.5f + 0.5f;
    }

    public Voxel Modify(float3 position, Voxel voxel) {
        float scaleParam = math.rcp(radius);

        float distance = math.length(position.xz - center.xz) * scaleParam;
        float crater = (hParam - g(1 / (jParam * distance*distance + 1) - hParam));
        //crater = math.saturate(crater);
        voxel.density -= (half)(crater * strength * n(distance));

        return voxel;
    }
}