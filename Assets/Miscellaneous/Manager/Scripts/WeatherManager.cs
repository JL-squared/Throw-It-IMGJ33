using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeatherManager : MonoBehaviour {
    [Header("Main")]
    public Light directionalLight;
    public float globalTimeScale = 1.0f;

    [Header("Clouds")]
    public GameObject[] cloudLayers;
    public float cloudCoverageOffset = 0.0f;
    public AnimationCurve coverageCurve;
    public Color baseSunColor = Color.white;
    public Color overcastSunColor = Color.white / 2.0f;
    public float baseSunIntensity = 1.5f;
    public float overcastSunIntensity = 0.5f;

    [HideInInspector]
    public Vector2[] uvOffsetsCloud;
    public Vector2 cloudWindFactor;

    public enum WeatherType {
        Calm,
        Windy,
        Snowy,
        Overcast,
        Stormy,
    }

    public float GetOutsideTemperature() {
        return -20.0f;
    }

    private void Start() {
        uvOffsetsCloud = new Vector2[cloudLayers.Length];
    }

    public void Update() {
        if (Player.Instance == null) return;
        float time = globalTimeScale * Time.time;

        // Accumulates the wind UV offset directions and applies them to the cloud materials
        for (int i = 0; i < cloudLayers.Length; i++) {
            // Accumulate wind offset
            uvOffsetsCloud[i] += Time.deltaTime * globalTimeScale * Vector2.one * cloudWindFactor;
            
            // Apply the UV offsets and overcast values to the clouds
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetVector("_UV_Offset", uvOffsetsCloud[i]);
            block.SetFloat("_Coverage_Offset", cloudCoverageOffset - 0.5f);
            cloudLayers[i].GetComponent<MeshRenderer>().SetPropertyBlock(block);
        }

        float basic = coverageCurve.Evaluate(cloudCoverageOffset);
        float invert = 1 - coverageCurve.Evaluate(cloudCoverageOffset);
        directionalLight.shadowStrength = invert;
        directionalLight.color = Color.Lerp(baseSunColor, overcastSunColor, basic);
        directionalLight.intensity = Mathf.Lerp(baseSunIntensity, overcastSunIntensity, basic);
    }
}
