using System.Reflection;
using UnityEngine.Rendering.Universal;
using GraphicsSettings = UnityEngine.Rendering.GraphicsSettings;

public static class UnityGraphicsBullshitWtf {
    private static FieldInfo MainLightCastShadows_FieldInfo;
    private static FieldInfo MainLightCastShadows_Distance;
    private static FieldInfo AdditionalLightCastShadows_FieldInfo;
    private static FieldInfo MainLightShadowmapResolution_FieldInfo;
    private static FieldInfo AdditionalLightShadowmapResolution_FieldInfo;
    private static FieldInfo SoftShadowsEnabled_FieldInfo;

    static UnityGraphicsBullshitWtf() {
        var pipelineAssetType = typeof(UniversalRenderPipelineAsset);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;

        foreach (var x in pipelineAssetType.GetFields(flags)) {
            //UnityEngine.Debug.Log(x.Name);
        }
        MainLightCastShadows_FieldInfo = pipelineAssetType.GetField("m_MainLightShadowsSupported", flags);
        MainLightCastShadows_Distance = pipelineAssetType.GetField("m_ShadowDistance", flags);
        AdditionalLightCastShadows_FieldInfo = pipelineAssetType.GetField("m_AdditionalLightShadowsSupported", flags);
        MainLightShadowmapResolution_FieldInfo = pipelineAssetType.GetField("m_MainLightShadowmapResolution", flags);
        AdditionalLightShadowmapResolution_FieldInfo = pipelineAssetType.GetField("m_AdditionalLightsShadowmapResolution", flags);
        SoftShadowsEnabled_FieldInfo = pipelineAssetType.GetField("m_SoftShadowsSupported", flags);
    }


    public static bool MainLightCastShadows {
        get => (bool)MainLightCastShadows_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => MainLightCastShadows_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public static float MainLightShadowDistance {
        get => (float)MainLightCastShadows_Distance.GetValue(GraphicsSettings.currentRenderPipeline);
        set => MainLightCastShadows_Distance.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public static bool AdditionalLightCastShadows {
        get => (bool)AdditionalLightCastShadows_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => AdditionalLightCastShadows_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public static ShadowResolution MainLightShadowResolution {
        get => (ShadowResolution)MainLightShadowmapResolution_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => MainLightShadowmapResolution_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public static ShadowResolution AdditionalLightShadowResolution {
        get => (ShadowResolution)AdditionalLightShadowmapResolution_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => AdditionalLightShadowmapResolution_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public static bool SoftShadowsEnabled {
        get => (bool)SoftShadowsEnabled_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => SoftShadowsEnabled_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }
}