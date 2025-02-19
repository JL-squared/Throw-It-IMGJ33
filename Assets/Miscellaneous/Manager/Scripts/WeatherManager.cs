using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class WeatherManager : MonoBehaviour {
    [Header("Main")]
    public Light directionalLight;
    public float globalTimeScale = 1.0f;
    private float time;

    public float humidity; // this determines cloud size idk what the metrics are for this // but it is random
    public float windSpeed; // probably going to be determined by derivative of temperature function times some constant
    public float windMultConstant;
    public float windBearing; // angle of wind from north (-Z) // probably random lowkey!!!

    public float temperature; // outside temperature !! (random emoji)
    public float temperatureMin; // constraints
    public float temperatureMax; // im touching you.....

    public float snowfall; // temp and humidity

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

    public float GetOutsideTemperature() {
        return -20.0f;
    }

    private void Start() {
        uvOffsetsCloud = new Vector2[cloudLayersSpeeds.Length];
    }

    private void OnValidate() {
        UpdateCloudProperties();
    }

    // Accumulates the wind UV offset directions and sets the coverage offset
    // We use this for both the visible clouds and cloud shadows
    public void ApplyCloudsProperties(Material material) {
        if (uvOffsetsCloud != null && uvOffsetsCloud.Length > 0) {
            for (int i = 0; i < cloudLayersSpeeds.Length; i++) {
                material.SetVector($"_baseOffset{i}", uvOffsetsCloud[i]);
            }
        }
        material.SetFloat("_coverageOffset", cloudCoverageOffset);
    }

    // Called with OnValidate in the editor!!
    private void UpdateCloudProperties() {
        ApplyCloudsProperties(clouds.sharedMaterial);

        // Most cursed thing ever but honestly whatever
        UniversalRenderPipelineAsset asset = (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset);
        FieldInfo propertyInfo = asset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        var data = ((ScriptableRendererData[])propertyInfo?.GetValue(asset))?[0];

        if (data != null) {
            Dictionary<string, ScriptableRendererFeature> features = new Dictionary<string, ScriptableRendererFeature>();
            foreach (var data2 in data.rendererFeatures) {
                features.Add(data2.name, data2);
            }
            CustomShadows shadows = (CustomShadows)features["CustomShadows"];
            ApplyCloudsProperties(shadows.GetInternalMat());
        }
    }

    public void Update() {
        if (Player.Instance == null) return;
        time += Time.deltaTime * globalTimeScale;

        // Accumulate!!
        for (int i = 0; i < cloudLayersSpeeds.Length; i++) {
            uvOffsetsCloud[i] += Time.deltaTime * globalTimeScale * Vector2.one * cloudWindFactor * cloudLayersSpeeds[i];
        }

        UpdateCloudProperties();

        float basic = coverageCurve.Evaluate(cloudCoverageOffset);
        float invert = 1 - coverageCurve.Evaluate(cloudCoverageOffset);
        directionalLight.color = Color.Lerp(baseSunColor, overcastSunColor, basic);
        directionalLight.intensity = Mathf.Lerp(baseSunIntensity, overcastSunIntensity, basic);
        skybox.SetFloat("_Cloud_Coverage", Mathf.Pow(basic, 2f));
    }

    
}
