using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
public class CustomShadows : ScriptableRendererFeature {
    class CustomPass1 : ScriptableRenderPass {
        public Material material;
        public float generalShadowStrength;

        public CustomPass1(Shader shader, float generalShadowStrength) {
            material = CoreUtils.CreateEngineMaterial(shader);
            this.generalShadowStrength = generalShadowStrength;
        }

        private class PassData {
            internal TextureHandle target;
            internal Material material;
            internal int shadowmapID;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalLightData lights = frameData.Get<UniversalLightData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (lights.mainLightIndex == -1) {
                return;
            }

            var desc = cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            desc.depthStencilFormat = GraphicsFormat.None;
            desc.enableRandomWrite = true;
            desc.graphicsFormat = GraphicsFormat.R8_UNorm;
            var tempTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_ScreenSpaceShadowmapTexture", true);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData)) {
                passData.target = tempTexture;
                builder.SetRenderAttachment(tempTexture, 0, AccessFlags.Write);

                passData.material = material;
                passData.shadowmapID = Shader.PropertyToID("_ScreenSpaceShadowmapTexture"); ;

                builder.AllowGlobalStateModification(true);
                builder.SetGlobalTextureAfterPass(tempTexture, passData.shadowmapID);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => {
                    data.material.SetFloat("_generalShadowStrength", generalShadowStrength);
                    Matrix4x4 matrix = lights.visibleLights[lights.mainLightIndex].localToWorldMatrix;
                    data.material.SetMatrix("_sunMatrix", matrix);
                    data.material.SetMatrix("_invSunMatrix", matrix.inverse);
                    data.material.SetFloat("_generalShadowStrength", generalShadowStrength);
                    Blitter.BlitTexture(context.cmd, data.target, Vector2.one, data.material, 0);
                    context.cmd.SetKeyword(GlobalKeyword.Create("_MAIN_LIGHT_SHADOWS"), false);
                    context.cmd.SetKeyword(GlobalKeyword.Create("_MAIN_LIGHT_SHADOWS_CASCADE"), false);
                    context.cmd.SetKeyword(GlobalKeyword.Create("_MAIN_LIGHT_SHADOWS_SCREEN"), true);
                });
            }
        }
    }

    CustomPass1 customPass;
    public Shader customShader;
    public float generalShadowStrength;

    // TODO: find fix for this pls
    [Tooltip("Decided to make this toggable since the screen space shadows effect seems fucked up in prefabs??")]
    public bool allowInSceneView;

    public override void Create() {
        customPass = new CustomPass1(customShader, generalShadowStrength);
        customPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    public Material GetInternalMat() {
        return customPass.material;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (renderingData.cameraData.cameraType != CameraType.SceneView || allowInSceneView) {
            renderer.EnqueuePass(customPass);
        }
    }
}