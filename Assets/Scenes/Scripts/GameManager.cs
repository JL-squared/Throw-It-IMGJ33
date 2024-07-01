using UnityEngine;

// Game manager that contains all other managers and handles
public class GameManager : MonoBehaviour {
    [HideInInspector]
    public WeatherManager weatherManager;
    public static GameManager Instance;
    [HideInInspector]
    public GameObject player;

    private void Start() {
        weatherManager = GetComponent<WeatherManager>();

        if (Instance == null) {
            Instance = this;
        }

        player = GameObject.FindGameObjectWithTag("PlayerTag");
    }

    bool dead;
    float timeSinceDeath;
    public delegate void OnTimeSinceDeathChanged(float timeSinceDeath);
    public event OnTimeSinceDeathChanged onTimeSinceDeath;
    public delegate void OnDeath();
    public event OnDeath onDeath;
    public delegate void OnPauseChanged(bool paused);
    public event OnPauseChanged onPausedChanged;
    bool paused;

    private void Update() {
        if (dead) {
            timeSinceDeath += Time.unscaledDeltaTime * .1f;
            Time.timeScale = Mathf.SmoothStep(1.0f, 0.0f, timeSinceDeath);
            onTimeSinceDeath?.Invoke(timeSinceDeath);
        }
    }

    public void PlayerKilled() {
        dead = true;
        onDeath?.Invoke();
    }

    public void UpdatePaused(bool paused) {
        Time.timeScale = paused ? 0.0f : 1.0f;
        this.paused = paused;
        onPausedChanged?.Invoke(paused);
        AudioListener.pause = paused;
    }
}
