using jedjoud.VoxelTerrain.Generation;
using Unity.Mathematics;

public class TerrainTest : VoxelGenerator {
    public InlineTransform transformed;
    public Inject<float> amplitude;
    public Inject<float> scale;
    public override void Execute(Variable<float3> position, out Variable<float> density, out Variable<float3> color) {
        position = new ApplyTransformation(transformed).Transform(position);

        var xz = position.Swizzle<float2>("xz");
        var y = position.Swizzle<float>("y");

        var simplex = Noise.Simplex(xz, scale, amplitude);
        var box = new SdfBox(new float3(30f, 10f, 30f));
        var blended = SdfOps.Subtraction(-(simplex+y), -box.Evaluate(position));


        density = (Variable<float>) blended;

        color = float3.zero;
    }
}
