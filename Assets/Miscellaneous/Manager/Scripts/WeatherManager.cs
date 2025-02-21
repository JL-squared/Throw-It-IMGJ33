using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;
using System;




#if UNITY_EDITOR
using UnityEditor;
#endif

public class WeatherManager : MonoBehaviour {
    [Header("Main")]
    public Light directionalLight;
    public float globalTimeScale = 1.0f;
    private float time;
    public bool updateWeather;

    [Header("Humidity")]
    [Range(0f, 1f)]
    public float humidity; // from 0 to 1
    public float humidityMinConstant;
    public float humidityMaxConstant;
    public float humidityIncrease;

    [Header("Cloud coverage")]
    [Range(0f, 1f)]
    public float cloudCoverage; // from 0 to 1
    public float pressureCloudMin;
    public float cloudTransferRate;
    public float humidityToCloudRatio;

    [Header("Air pressure")]
    [Range(0f, 1f)]
    public float airPressure; // this could get complicated // scale from 0 to 1
    public float proportionalityConstant;

    [Header("Temperature")]
    public float temperature; // outside temperature !! (random emoji) // scale from temperatureMin to temperatureMax
    public float temperatureMin; // constraints
    public float temperatureMax; // im touching you.....

    [Header("Snowfall/precipitation")]
    [Range(0f, 1f)]
    public float snowfall; // otherwise known as precipitation; temp and humidity // scale from 0 to 1
    public float snowfallTemperatureThreshold;
    public float snowfallToCloud;
    public float cloudPrecipitationDecay;
    public VisualEffect snowVFX;

    [Header("Wind")]
    public float windSpeed; // probably going to be determined by derivative of air pressure times some constant
    public float windMultConstant;
    [Range(0f, 360f)]
    public float windBearing; // angle of wind from north (-Z) // probably random lowkey!!!

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
        return temperature + CalculateWindchill();
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
        if(clouds.sharedMaterial != null) ApplyCloudsProperties(clouds.sharedMaterial);

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

        UpdateVisuals();
    }

    public void FixedUpdate() {
        if(updateWeather)
            UpdateValues();
    }

    private float lastAirPressure;
    public void UpdateValues() {
        temperature = (temperatureMax - temperatureMin) / 2 * (float)Math.Sin(0.00001f * Time.fixedTime) + (temperatureMax + temperatureMin) / 2;
        airPressure = proportionalityConstant * (temperature + 273.15f);

        windSpeed = (lastAirPressure - airPressure) / Time.fixedDeltaTime * windMultConstant; // take wind speed based on

        windBearing = 1.0f; // I WILL DO THIS LATER !!!!! LOL

        if (airPressure < pressureCloudMin && humidity > humidityMinConstant) {
            humidity -= cloudTransferRate * (pressureCloudMin - airPressure) * Time.fixedDeltaTime;
            cloudCoverage += cloudTransferRate * (pressureCloudMin - airPressure) * humidityToCloudRatio * Time.fixedDeltaTime;
        }

        // increase moisture based on rate of increase * temperature (moisture maximum is proportional to temperature)
        if (humidity < humidityMaxConstant * (temperature + 273.15f)) {
            humidity += humidityIncrease * Time.fixedDeltaTime;
        }

        if (cloudCoverage > 0.5f && (temperature + 273.15f) < snowfallTemperatureThreshold) {
            snowfall = humidity * (float)(Math.Pow(cloudCoverage, 0.5f) * snowfallToCloud);
            cloudCoverage -= cloudPrecipitationDecay * Time.fixedDeltaTime;
        } else {
            snowfall = 0.0f;
        }

        lastAirPressure = airPressure;
    }

    public void UpdateVisuals() {
        cloudCoverageOffset = cloudCoverage;
        // temperature off of snow creation (below 32C) = snow fall speed
        // wind & wind bearing = snow horizontal speed
        // snowfall variable = amount of snow particles
        if(snowVFX != null) {
            snowVFX.SetFloat("Rate", 20000 * snowfall);
            snowVFX.SetFloat("Speed Offset", windSpeed);
            snowVFX.SetFloat("Speed Angle", windBearing);
        }

        snowVFX.gameObject.transform.position = Player.Instance.gameObject.transform.position;
    }

    public static float CalculateWindchill(float temperature, float windSpeed) {
        float kmh = windSpeed * 3.6f;
        return 13.12f + 0.6215f * temperature - 11.37f * kmh + 0.3965f * temperature * kmh; // actual irl windchill calculation
    }

    public float CalculateWindchill() {
        return CalculateWindchill(temperature, windSpeed);
    }
}
