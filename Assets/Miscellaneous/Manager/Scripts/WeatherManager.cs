using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Unity.Collections.AllocatorManager;

public class WeatherManager : MonoBehaviour {
    [Header("Main")]
    public Light directionalLight;
    public float globalTimeScale = 1.0f;
    private float time;

    [Header("Clouds")]
    public MeshRenderer clouds;
    public float[] cloudLayersSpeeds;
    public float cloudCoverageOffset = 0.0f;
    public AnimationCurve coverageCurve;
    public Color baseSunColor = Color.white;
    public Color overcastSunColor = Color.white / 2.0f;
    public float baseSunIntensity = 1.5f;
    public float overcastSunIntensity = 0.5f;

    [HideInInspector]
    public Vector2[] uvOffsetsCloud;
    public Vector2 cloudWindFactor;

    [Header("Skybox")]
    public Material skybox;

    public enum WeatherType {
        Calm,
        Wind,
        Snowing,
        Overcast,
        Storm,
    }

    public WeatherType status = WeatherType.Calm;

    public float GetOutsideTemperature() {
        return -20.0f;
    }

    private void Start() {
        uvOffsetsCloud = new Vector2[cloudLayersSpeeds.Length];
    }

    // Accumulates the wind UV offset directions and sets the coverage offset
    // We use this for both the visible clouds and cloud shadows
    public void ApplyCloudsProperties(Material material) {
        for (int i = 0; i < cloudLayersSpeeds.Length; i++) {
            material.SetVector($"_baseOffset{i}", uvOffsetsCloud[i]);
        }
        material.SetFloat("_coverageOffset", cloudCoverageOffset - 0.5f);
    }

    public void Update() {
        if (Player.Instance == null) return;
        time += Time.deltaTime * globalTimeScale;

        // Accumulate!!
        for (int i = 0; i < cloudLayersSpeeds.Length; i++) {
            uvOffsetsCloud[i] += Time.deltaTime * globalTimeScale * Vector2.one * cloudWindFactor * cloudLayersSpeeds[i];
        }

        // Set visible property blocks
        ApplyCloudsProperties(clouds.material);


        UniversalRenderPipelineAsset asset = (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset);
        FieldInfo propertyInfo = asset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        var data = ((ScriptableRendererData[])propertyInfo?.GetValue(asset))?[0];

        Dictionary<string, ScriptableRendererFeature> features = new Dictionary<string, ScriptableRendererFeature>();
        foreach (var data2 in data.rendererFeatures) {
            features.Add(data2.name, data2);
        }
        CustomShadows shadows = (CustomShadows)features["CustomShadows"];
        ApplyCloudsProperties(shadows.GetInternalMat());

        float basic = coverageCurve.Evaluate(cloudCoverageOffset);
        float invert = 1 - coverageCurve.Evaluate(cloudCoverageOffset);
        // TODO: Update render feature shadow strength instead!!
        //directionalLight.shadowStrength = invert;
        directionalLight.color = Color.Lerp(baseSunColor, overcastSunColor, basic);
        directionalLight.intensity = Mathf.Lerp(baseSunIntensity, overcastSunIntensity, basic);
        skybox.SetFloat("_Cloud_Coverage", Mathf.Pow(basic, 2f));
    }
}
