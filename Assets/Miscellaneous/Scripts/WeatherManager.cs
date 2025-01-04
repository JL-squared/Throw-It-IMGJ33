using UnityEngine;

// Simple weather system that affects each wave
// Current weather effects:
// [calm, windy, snowy, snowy windy, snowstorm]
// Reduced visibility in snowy, snowy windy, and especially snowtorm weather types
public class WeatherManager : MonoBehaviour {
    [Range(0, 1)] public float windy;
    [Range(0, 1)] public float snowy;
    [Range(0, 1)] public float stormy;
    public bool noisyParams = false;
    public float windyNoiseScale = 0.2f;
    public float snowyNoiseScale = 0.2f;
    public float stormyNoiseScale = 0.2f;
    public float globalTimeScale = 1.0f;
    [Range(0, 10000)] public int minSnowEmissionRate;
    [Range(0, 10000)] public int maxSnowEmissionRate;

    public float windyParticlesNoiseScale = 0.2f;
    public float windyParticlesNoiseFactor = 0.2f;
    public ParticleSystemForceField windParticleField;
    public ParticleSystem snowParticleSystem;

    public GameObject[] cloudLayers;
    public Vector2[] uvOffsetsCloud;

    public Vector2 cloudWindFactor;

    //public VolumetricFog fog;
    public AnimationCurve densityFogCurve;
    public AnimationCurve extinctionCoefficientCurve;
    public AnimationCurve effectsScalingFactor;

    public enum WeatherType {
        Calm,
        Windy,
        Snowy,
        SnowyWindy,
        Stormy,
    }

    public WeatherType GetWeatherType() {
        bool relativelyWindy = windy > 0.5;
        bool relativelySnowy = snowy > 0.5;
        bool relativelyStormy = stormy > 0.5;

        if (relativelyStormy) {
            return WeatherType.Stormy;
        }

        switch (relativelyWindy, relativelySnowy) {
            case (false, false):
                return WeatherType.Calm;
            case (true, false):
                return WeatherType.Windy;
            case (false, true):
                return WeatherType.Snowy;
            default:
                return WeatherType.SnowyWindy;
        }
    }

    public float GetFogLerp() {
        return windy;
    }

    public float GetOutsideTemperature() {
        float windyFactor = windy * 4.0f;
        float snowyFactor = snowy * 4.0f;
        float stormyFactor = stormy * 12.0f;
        float baseTemp = -20.0f; 
        return baseTemp - (windyFactor + snowyFactor + stormyFactor);
    }

    private void Start() {
        uvOffsetsCloud = new Vector2[cloudLayers.Length];
    }

    // https://discussions.unity.com/t/whats-the-best-way-to-rotate-a-vector2-in-unity/754872/4
    public static Vector2 Rotate(Vector2 v, float delta) {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }

    public void Update() {
        if (Player.Instance == null) return;

        // Update volumetric fog settings
        // TODO: Fix cases where it just disapears
        float x = GetFogLerp();
        //fog.density = densityFogCurve.Evaluate(x);
        //fog.extinctionCoefficient = extinctionCoefficientCurve.Evaluate(x);
        //fog.UpdateValues();

        // Calculate parameters based on game time
        float time = globalTimeScale * Time.time;
        if (noisyParams) {
            windy = effectsScalingFactor.Evaluate(Mathf.PerlinNoise1D(time * windyNoiseScale + 32.412f));
            snowy = effectsScalingFactor.Evaluate(Mathf.PerlinNoise1D(time * snowyNoiseScale + 3214.32f));
            stormy = effectsScalingFactor.Evaluate(Mathf.PerlinNoise1D(time * stormyNoiseScale - 654.12f));
        }

        // Global wind effector
        Vector2 windEffect = new Vector2(Mathf.PerlinNoise1D(time * windyParticlesNoiseScale - 43.432f), Mathf.PerlinNoise1D(time * windyParticlesNoiseScale + 243.432f));
        windEffect = windEffect * 2.0f - Vector2.one;
        windParticleField.directionX = windEffect.x * windyParticlesNoiseFactor * windy;
        windParticleField.directionZ = windEffect.y * windyParticlesNoiseFactor * windy;

        // Keep the snow particles around the player at all times
        Vector3 pos = Player.Instance.transform.position;
        windParticleField.transform.position = pos;
        snowParticleSystem.transform.position = pos;

        // Change quantity of particles based on snow amount
        float snowEmissionRate = Mathf.Lerp(minSnowEmissionRate, maxSnowEmissionRate, snowy);
        var emission = snowParticleSystem.emission;
        emission.rateOverTime = snowEmissionRate;

        // Cloud wind direction!!!
        for (int i = 0; i < cloudLayers.Length; i++) {
            float layerRng = Mathf.PerlinNoise1D(time * 0.04f + (i * 12.013f - 321.321f)) * 1.0f;



            uvOffsetsCloud[i] += Rotate(windEffect, layerRng) * cloudWindFactor * windy * Time.deltaTime * globalTimeScale;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetVector("_UV_Offset", uvOffsetsCloud[i]);
            block.SetFloat("_Coverage_Offset", stormy - 0.5f);
            cloudLayers[i].GetComponent<MeshRenderer>().SetPropertyBlock(block);
        }

    }
}
