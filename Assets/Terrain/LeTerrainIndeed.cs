using Unity.Mathematics;

public class LeTerrainIndeed : VoxelGraph {
    public Inject<float> scale;
    public Inject<float> amplitude;

    public override void Execute(Variable<float3> position, out Variable<float> density, out Variable<float3> color) {
        var y = position.Swizzle<float>("y");
        var xz = position.Swizzle<float2>("xz");
        density = y + Noise.Simplex(xz, scale, amplitude);
        color = float3.zero;
    }
}