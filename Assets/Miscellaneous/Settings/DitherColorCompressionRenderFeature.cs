using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
public class DitherColorCompressionRenderFeature : ScriptableRendererFeature {
    class DitherColorCompressionPass : ScriptableRenderPass {
        private ComputeShader shader;
        private int compressionDepth = 7;
        private float multiplier = 128f;
        private float tightness = 0.0f;

        public DitherColorCompressionPass(ComputeShader shader, int compressionDepth, float multiplier, float tightness) {
            this.shader = shader;
            this.compressionDepth = compressionDepth;
            this.multiplier = multiplier;
            this.tightness = tightness;
        }

        private class PassData {
            public int width;
            public int height;
            public TextureHandle src;
            public ComputeShader shader;
        }
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            var desc = cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            desc.enableRandomWrite = true;
            var tempTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "TempTexture", false);

            Material blitterMat = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
            renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(resourceData.cameraColor, tempTexture, blitterMat, 0));

            using (var builder = renderGraph.AddComputePass<PassData>("Compute Dithering", out var passData)) {
                passData.src = tempTexture;
                passData.shader = shader;
                passData.width = desc.width;
                passData.height = desc.height;
                builder.UseTexture(passData.src, AccessFlags.ReadWrite);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData data, ComputeGraphContext context) => {
                    context.cmd.SetComputeTextureParam(data.shader, 0, "screenTexture", data.src);
                    context.cmd.SetComputeIntParam(data.shader, "cDepth", compressionDepth);
                    context.cmd.SetComputeFloatParam(data.shader, "multiplier", multiplier);
                    context.cmd.SetComputeFloatParam(data.shader, "tightness", tightness);
                    context.cmd.DispatchCompute(data.shader, 0, Mathf.CeilToInt((float)data.width / 16.0f), Mathf.CeilToInt((float)data.height / 16.0f), 1);
                });
            }

            renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(tempTexture, resourceData.cameraColor, blitterMat, 0));
        }
    }

    DitherColorCompressionPass pass;

    [Range(1, 8)]
    public int compressionDepth = 7;

    [Min(0)]
    public float multiplier = 128f;
    public float tightness = 0.0f;
    public ComputeShader customShader;

    public override void Create() {
        pass = new DitherColorCompressionPass(customShader, compressionDepth, multiplier, tightness);
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (renderingData.cameraData.cameraType == CameraType.Game) {
            renderer.EnqueuePass(pass);
        }
    }
}