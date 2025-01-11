using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeatherManager : MonoBehaviour {
    [Header("Main")]
    public Light directionalLight;
    public float globalTimeScale = 1.0f;

    [Header("Clouds")]
    public GameObject[] cloudLayers;
    public float cloudCoverageOffset = 0.0f;
    public AnimationCurve shadowStrengthCloudCoverage;

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
        return 0.0f;
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

        directionalLight.shadowStrength = shadowStrengthCloudCoverage.Evaluate(cloudCoverageOffset);
    }
}
