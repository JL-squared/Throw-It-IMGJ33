using UnityEngine;
using UnityEngine.Rendering;

// Game manager that contains all other managers and handles
public class GameManager : MonoBehaviour {
    [HideInInspector]
    public WeatherManager weatherManager;
    public static GameManager Instance;
    [HideInInspector]
    public NavMeshRebuilder pathfindingRebuilder;
    private GraphicsQualitySettings graphicsSettings;
    public Volume volume;

    private void Start() {
        weatherManager = GetComponent<WeatherManager>();
        pathfindingRebuilder = GetComponent<NavMeshRebuilder>();

        if (Instance == null) {
            Instance = this;
        }

        if (VoxelTerrain.Instance != null) {
            initialized = false;
            Time.timeScale = 0.0f;
            Physics.simulationMode = SimulationMode.Script;

            VoxelTerrain.Instance.Finished += () => {
                initialized = true;
                pathfindingRebuilder.UpdateNavMesh();
                Physics.simulationMode = SimulationMode.FixedUpdate;
                Time.timeScale = 1.0f;
            };
        } else {
            initialized = true;
            pathfindingRebuilder.UpdateNavMesh();
        }

        Utils.InitSerializer();
        graphicsSettings = Utils.Load("graphics.json", new GraphicsQualitySettings());
        graphicsSettings.Apply(volume.profile);
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
    public bool initialized;

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
        if (!initialized)
            return;

        Time.timeScale = paused ? 0.0f : 1.0f;
        this.paused = paused;
        onPausedChanged?.Invoke(paused);
        AudioListener.pause = paused;
    }
}
