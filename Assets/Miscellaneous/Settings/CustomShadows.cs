using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static Unity.Burst.Intrinsics.X86.Avx;
public class CustomShadows : ScriptableRendererFeature {
    class CustomPass1 : ScriptableRenderPass {
        private Shader shader;

        public CustomPass1(Shader shader) {
            this.shader = shader;
        }

        private class PassData {
            public int width;
            public int height;
            public Shader shader;
        }
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            
            var desc = cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            desc.depthStencilFormat = GraphicsFormat.None;
            desc.enableRandomWrite = true;
            desc.graphicsFormat = GraphicsFormat.R8_UNorm;
            var tempTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "TempTexture2", true);
            // 
            using (var builder = renderGraph.AddComputePass<PassData>("Compute Custom Shadows", out var passData)) {
                //builder.UseGlobalTexture(Shader.PropertyToID("_ScreenSpaceShadowmapTexture"), AccessFlags.Read);
                builder.UseAllGlobalTextures(true);
                /*
                Texture tex = Shader.GetGlobalTexture("_ScreenSpaceShadowmapTexture");
                RTHandle test = RTHandles.Alloc(tex);
                TextureHandle textureHandle = renderGraph.ImportTexture(test);
                */

                builder.UseTexture(resourceData.cameraDepth, AccessFlags.Read);
                builder.AllowPassCulling(false);
                builder.UseTexture(tempTexture, AccessFlags.Write);
                //builder.UseTexture(textureHandle, AccessFlags.Read);
                builder.AllowGlobalStateModification(true);
                builder.SetGlobalTextureAfterPass(tempTexture, Shader.PropertyToID("_ScreenSpaceShadowmapTexture"));


                //passData.texture2 = textureHandle;
                passData.shader = shader;
                passData.width = desc.width;
                passData.height = desc.height;

                builder.SetRenderFunc((PassData data, ComputeGraphContext context) => {
                    context.cmd.SetKeyword(GlobalKeyword.Create("_MAIN_LIGHT_SHADOWS"), false);
                    context.cmd.SetKeyword(GlobalKeyword.Create("_MAIN_LIGHT_SHADOWS_CASCADE"), false);
                    context.cmd.SetKeyword(GlobalKeyword.Create("_MAIN_LIGHT_SHADOWS_SCREEN"), true);
                    //context.cmd.DispatchCompute(data.shader, 0, Mathf.CeilToInt((float)data.width / 16.0f), Mathf.CeilToInt((float)data.height / 16.0f), 1);
                });
            }

            //renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(resourceData.cameraColor, tempTexture, blitterMat, 0));

            /*
            using (var builder = renderGraph.AddComputePass<PassData>("Compute Custom Shadows", out var passData)) {
                builder.AllowPassCulling(false);
                builder.UseGlobalTexture(Shader.PropertyToID("_ScreenSpaceShadowmapTexture"), AccessFlags.ReadWrite);
                builder.UseAllGlobalTextures(true);
                builder.AllowGlobalStateModification(true);
                builder.SetGlobalTextureAfterPass(tempTexture, Shader.PropertyToID("_ScreenSpaceShadowmapTexture"));

                Texture tex = Shader.GetGlobalTexture("_ScreenSpaceShadowmapTexture");
                RTHandle test = RTHandles.Alloc(tex);
                TextureHandle textureHandle = renderGraph.ImportTexture(test);

                builder.AllowPassCulling(false);
                builder.UseTexture(textureHandle, AccessFlags.ReadWrite);


                passData.texture = textureHandle;
                passData.texture2 = tempTexture;
                passData.shader = shader;
                
                builder.SetRenderFunc((PassData data, ComputeGraphContext context) => {
                    //context.cmd.SetComputeTextureParam(data.shader, 0, "test", data.texture);
                    //context.cmd.DispatchCompute(data.shader, 0, 1, 1, 1);
                });
            }
            */

            /*
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Compute Custom Shadows", out var passData)) {
                builder.UseGlobalTexture(Shader.PropertyToID("_ScreenSpaceShadowmapTexture"), AccessFlags.Write);
                builder.AllowPassCulling(false);
                builder.SetRenderAttachment(textureHandle, 0, );

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => {
                });
            }
            */

            /*
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Compute Custom Shadows", out var passData)) {
                builder.UseGlobalTexture(Shader.PropertyToID("_ScreenSpaceShadowmapTexture"), AccessFlags.ReadWrite);
                var texture = Shader.GetGlobalTexture("_ScreenSpaceShadowmapTexture");
                builder.
                builder.AllowPassCulling(false);
                
                passData.shader = shader;

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => {
                    //context.cmd.DispatchCompute(data.shader, 0, 16, 16, 1);
                    context.cmd.ClearRenderTarget(false, true, Color.black);
                    //Blitter.(context.cmd, Vector3.one, )
                });
            }
            */
        }
    }

    CustomPass1 customPass;
    public ComputeShader customShader;

    public override void Create() {
        //customPass = new CustomPass1(customShader);
        customPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        //renderer.EnqueuePass(customPass);
    }
}
