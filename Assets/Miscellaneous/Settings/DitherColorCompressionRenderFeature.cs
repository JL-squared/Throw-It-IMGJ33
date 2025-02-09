using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
public class DitherColorCompressionRenderFeature : ScriptableRendererFeature {
    class CustomPass : ScriptableRenderPass {
        private ComputeShader shader;

        public CustomPass(ComputeShader shader) {
            this.shader = shader;
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

            using (var builder = renderGraph.AddComputePass<PassData>(passName, out var passData)) {
                passData.src = tempTexture;
                passData.shader = shader;
                passData.width = desc.width;
                passData.height = desc.height;
                builder.UseTexture(passData.src, AccessFlags.ReadWrite);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData data, ComputeGraphContext context) => {
                    context.cmd.SetComputeTextureParam(data.shader, 0, "test", data.src);
                    context.cmd.DispatchCompute(data.shader, 0, Mathf.CeilToInt((float)data.width / 16.0f), Mathf.CeilToInt((float)data.height / 16.0f), 1);
                });
            }

            renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(tempTexture, resourceData.cameraColor, blitterMat, 0));
        }
    }

    CustomPass customPass;
    public ComputeShader customShader;

    public override void Create() {
        customPass = new CustomPass(customShader);
        customPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (renderingData.cameraData.cameraType == CameraType.Game) {
            renderer.EnqueuePass(customPass);
        }
    }
}