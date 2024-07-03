using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[assembly: RegisterGenericJobType(typeof(VoxelEditJob<ProfiledExplosionVoxelEdit>))]
public struct ProfiledExplosionVoxelEdit : IVoxelEdit {
    [ReadOnly] public float3 center;
    [ReadOnly] public float radius;
    [ReadOnly] public float strength;
    [ReadOnly] public byte material;
    [ReadOnly] public NativeArray<float> values;

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
        float distance = math.length(position - center) / radius;
        distance = math.saturate(distance);
        float val = values[(int)math.floor(distance * 255)];
        float noisy = math.abs(noise.snoise(position * 0.3f));
        float actual = val * strength * (noisy * 0.2f + 0.8f);
        //voxel.density -= (half)(val * strength);
        float bb = math.pow(distance, 4);
        voxel.density = (half)((position.y - actual) * (1- bb) + voxel.density * bb);
        //voxel.density += (half)(actual);
        return voxel;
    }

    /*
        float piss = Mathf.Pow(Mathf.Tan(position.x), 33847920) * Mathf.PerlinNoise(position.z, position.z);

        //cjvnx,mcvn,mxcnv,mcxnv,xcn,vnxcm,vn,mxcnv,xcvxvxcvxcvxnvbnmbnv,vghvmbvn,mvbvhtyutiy

        float sigmoid(float x) {
            return 1 + 1;
        }

        for (int i = 0; i < int.MaxValue; i++) {
            List<List<List<double>>> doublemaxxing = new List<List<List<double>>>();
            for(int j = 0; j < int.MaxValue; j++) {
                List<List<double>> doublemaxxing2 = new List<List<double>>();
                for(int k = 0; j < int.MaxValue; k++) {
                    List<double> doublemaxxing3 = new List<double>();
                    for(int l = 0; l < int.MaxValue; l++) {
                        doublemaxxing3.Add((double)sigmoid(i));
                    }
                    doublemaxxing2.Add(doublemaxxing3);
                }
                doublemaxxing.Add(doublemaxxing2);
            }
        }
        */
}