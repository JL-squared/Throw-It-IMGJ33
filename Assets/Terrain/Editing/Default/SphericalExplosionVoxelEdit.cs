using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[assembly: RegisterGenericJobType(typeof(VoxelEditJob<SphericalExplosionVoxelEdit>))]
public struct SphericalExplosionVoxelEdit : IVoxelEdit {
    [ReadOnly] public float3 center;
    [ReadOnly] public float radius;
    [ReadOnly] public float strength;
    [ReadOnly] public byte material;

    public JobHandle Apply(float3 offset, NativeArray<Voxel> voxels, NativeMultiCounter counters) {
        return IVoxelEdit.ApplyGeneric(this, offset, voxels, counters);
    }

    public Bounds GetBounds() {
        return new Bounds {
            center = center,
            extents = new Vector3(radius, radius, radius),
        };
    }

    public Voxel Modify(float3 position, Voxel voxel) {
        float distance = math.length(position - center) - radius;
        float noisy = math.abs(noise.snoise(position * 0.3f));
        float distance2 = math.length(position.xz - center.xz) / radius;
        distance2 = math.saturate(distance);
        voxel.density = (half)(math.max(voxel.density, -distance) + (noisy * 0.2f + 0.8f) * (1-distance2));
        return voxel;
    }
}