using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

// Simple weather system that affects each wave
// Current weather effects:
// [calm, windy, snowy, snowy windy, snowstorm]
// Reduced visibility in snowy, snowy windy, and especially snowtorm weather types
public class WeatherManager : MonoBehaviour {
    public float windy;
    public float snowy;
    public float stormy;

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

    public void Update() {
        
    }
}
