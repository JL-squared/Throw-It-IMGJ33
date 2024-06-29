using UnityEngine;

// Game manager that contains all other managers and handles
public class GameManager : MonoBehaviour {
    [HideInInspector]
    public WeatherManager weatherManager;
    [HideInInspector]
    public WaveManager waveManager;
    [HideInInspector]
    public TimeManager timeManager;
    public static GameManager Instance;
    [HideInInspector]
    public GameObject player;

    private void Start() {
        weatherManager = GetComponent<WeatherManager>();
        waveManager = GetComponent<WaveManager>();
        timeManager = GetComponent<TimeManager>();

        if (Instance == null) {
            Instance = this;
        }

        player = GameObject.FindGameObjectWithTag("PlayerTag");
    }
}
