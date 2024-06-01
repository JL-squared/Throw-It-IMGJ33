using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game manager that contains all other managers and handles
public class GameManager : MonoBehaviour {
    public WeatherManager weatherManager;
    public WaveManager waveManager;
    public static GameManager Singleton;

    private void Start() {
        weatherManager = GetComponent<WeatherManager>();
        waveManager = GetComponent<WaveManager>();

        if (Singleton == null) {
            Singleton = this;
        }
    }
}
