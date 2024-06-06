using UnityEngine;

// Game manager that contains all other managers and handles
public class GameManager : MonoBehaviour {
    [HideInInspector]
    public WeatherManager weatherManager;
    [HideInInspector]
    public WaveManager waveManager;
    public static GameManager Singleton;
    [HideInInspector]
    public GameObject player;

    private void Start() {
        weatherManager = GetComponent<WeatherManager>();
        waveManager = GetComponent<WaveManager>();

        if (Singleton == null) {
            Singleton = this;
        }

        player = GameObject.FindGameObjectWithTag("PlayerTag");
    }
}
