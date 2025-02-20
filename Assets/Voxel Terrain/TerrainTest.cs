using jedjoud.VoxelTerrain.Generation;
using Unity.Mathematics;

public class TerrainTest : VoxelGenerator {
    public InlineTransform transformed;
    public Inject<float> amplitude;
    public Inject<float> scale;
    public override void Execute(Variable<float3> position, out Variable<float> density, out Variable<float3> color) {
        var shifted = new ApplyTransformation(transformed).Transform(position);

        var xz = shifted.Swizzle<float2>("xz");
        var y = shifted.Swizzle<float>("y");

        var fractal = new Fractal<float2>(new Simplex(scale, amplitude), FractalMode.Sum, 3, 1.7f, 0.4f);
        var simplex = fractal.Evaluate(xz);
        var box = new SdfBox(new float3(30f, 10f, 30f));
        var blended = SdfOps.Subtraction(-(simplex+y), -box.Evaluate(position));

        density = (Variable<float>) blended;

        color = float3.zero;
    }
}
