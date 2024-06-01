using Andicraft.VolumetricFog;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

// Simple weather system that affects each wave
// Current weather effects:
// [calm, windy, snowy, snowy windy, snowstorm]
// Reduced visibility in snowy, snowy windy, and especially snowtorm weather types
public class WeatherManager : MonoBehaviour {
    [Range(0, 1)] public float windy;
    [Range(0, 1)] public float snowy;
    [Range(0, 1)] public float stormy;
    public float windyNoiseScale = 0.2f;
    public float snowyNoiseScale = 0.2f;
    public float stormyNoiseScale = 0.2f;
    public VolumetricFog fog;
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
        float baseTemp = 5.0f; 
        return baseTemp - (windyFactor + snowyFactor + stormyFactor);
    }

    public void Update() {
        float x = GetFogLerp();
        fog.density = densityFogCurve.Evaluate(x);
        fog.extinctionCoefficient = extinctionCoefficientCurve.Evaluate(x);
        fog.UpdateValues();

        windy = effectsScalingFactor.Evaluate(Mathf.PerlinNoise1D(Time.time * windyNoiseScale + 32.412f));
        snowy = effectsScalingFactor.Evaluate(Mathf.PerlinNoise1D(Time.time * snowyNoiseScale + 3214.32f));
        stormy = effectsScalingFactor.Evaluate(Mathf.PerlinNoise1D(Time.time * stormyNoiseScale - 654.12f));
    }
}
