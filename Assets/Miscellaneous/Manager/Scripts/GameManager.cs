using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Game manager that contains all other managers and handles
public class GameManager : MonoBehaviour {
    [HideInInspector]
    public WeatherManager weatherManager;
    [HideInInspector]
    public DevConsole devConsole;
    public static GameManager Instance { get; set; }
    [HideInInspector]
    private GraphicsQualitySettings graphicsSettings;
    public ReflectionProbe reflectionProbe;
    public Volume volume;

    private void Start() {
        weatherManager = GetComponent<WeatherManager>();
        devConsole = GetComponent<DevConsole>();

        if (Instance == null) {
            Instance = this;
        }

        /*
        if (VoxelTerrain.Instance != null && !VoxelTerrain.Instance.isActiveAndEnabled) {
            initialized = false;
            Time.timeScale = 0.0f;
            Physics.simulationMode = SimulationMode.Script;

            VoxelTerrain.Instance.onFinished += () => {
                initialized = true;
                Physics.simulationMode = SimulationMode.FixedUpdate;
                Time.timeScale = 1.0f;
            };
        } else {
            initialized = true;
        }
        */
        initialized = true;

        // TODO: Implement compute shader reflection probe capturing to reduce the massive amounts of stuttering that occurs due to this
        // Removed the old system that does this each second because it really should be done in compute shaders instead
        reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        reflectionProbe.RenderProbe();
        DynamicGI.UpdateEnvironment();

        Time.timeScale = 1.0f;
        Physics.simulationMode = SimulationMode.FixedUpdate;
        graphicsSettings = Utils.Load<GraphicsQualitySettings>("graphics.json", resave: true);
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
    public bool paused;
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

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1) {
        /*
        var dir = Utils.PersistentDir + "/terrain";
        //VoxelTerrain.Instance.LoadMapSkibi(dir);

        SaveState state = Utils.Load<SaveState>("save.json");
        state.Loaded();
        */
    }
}
