using System;
using UnityEngine.Rendering;
using Newtonsoft.Json;
using UnityEngine.Rendering.Universal;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections.Generic;

[Serializable]
public class GraphicsQualitySettings {
    public int fpsLimit = -1;
    public Quality mainLightShadows = Quality.High;
    public Quality additionalLightShadows = Quality.High;

    public bool tonemapping = true;
    public bool ambientOcclusion = true;
    public bool bloom = true;
    public bool vignette = true;
    public bool whiteBalance = true;
    public bool pixelatedDitherColorCompression = true;

    public const float DEFAULT_PIXELIZATION_FACTOR = 0.4f;
    
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Quality {
        [EnumMember(Value = "disabled")]
        Disabled = 0,
        [EnumMember(Value = "low")]
        Low,
        [EnumMember(Value = "medium")]
        Medium,
        [EnumMember(Value = "high")]
        High,
    }


    public static T Select<T>(Quality quality, params T[] arr) {
        int i  = UnityEngine.Mathf.Clamp((int)quality, 0, arr.Length-1);
        return arr[i];
    }

    public void Apply(VolumeProfile profile) {
        void SetVolumeOverride<T>(bool enabled) where T : VolumeComponent {
            if (profile.TryGet<T>(out T tahini)) {
                tahini.active = enabled;
            }
        }

        UniversalRenderPipelineAsset asset = (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset);
        var res = Select(mainLightShadows, ShadowResolution._256, ShadowResolution._512, ShadowResolution._1024, ShadowResolution._4096);
        UnityGraphicsBullshitWtf.MainLightCastShadows = mainLightShadows != Quality.Disabled;
        UnityGraphicsBullshitWtf.MainLightShadowResolution = res;

        asset.maxAdditionalLightsCount = 5;
        var res2 = Select(additionalLightShadows, ShadowResolution._256, ShadowResolution._512, ShadowResolution._1024, ShadowResolution._4096);
        UnityGraphicsBullshitWtf.AdditionalLightCastShadows = additionalLightShadows != Quality.Disabled;
        UnityGraphicsBullshitWtf.AdditionalLightShadowResolution = res2;

        SetVolumeOverride<Tonemapping>(tonemapping);
        SetVolumeOverride<Bloom>(bloom);
        SetVolumeOverride<Vignette>(vignette);
        SetVolumeOverride<WhiteBalance>(whiteBalance);

        FieldInfo propertyInfo = asset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        var data = ((ScriptableRendererData[])propertyInfo?.GetValue(asset))?[0];

        Dictionary<string, ScriptableRendererFeature> features = new Dictionary<string, ScriptableRendererFeature>();
        foreach (var data2 in data.rendererFeatures) {
            features.Add(data2.name, data2);
        }

        // There seems to be a weird artefact with SSAO and items close to the camera near frustum, before we fix that imma just disable this temporarily
        features["SSAO"].SetActive(false);
        //features["SSAO"].SetActive(ambientOcclusion);
        features["DitherColorCompressionRenderFeature"].SetActive(pixelatedDitherColorCompression);
        asset.renderScale = pixelatedDitherColorCompression ? DEFAULT_PIXELIZATION_FACTOR : 1.0f;

        if (fpsLimit <= 0) {
            UnityEngine.QualitySettings.vSyncCount = 1;
            UnityEngine.Application.targetFrameRate = 10000;
        } else {
            UnityEngine.QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = fpsLimit;
        }
    }
}