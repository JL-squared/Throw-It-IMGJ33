using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Game manager that contains all other managers and handles
public class GameManager : MonoBehaviour {
    [HideInInspector]
    public WeatherManager weatherManager;
    [HideInInspector]
    public DevConsole devConsole;
    public static GameManager Instance;
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
        reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        graphicsSettings = Utils.Load<GraphicsQualitySettings>("graphics.json", resave: true);
        graphicsSettings.Apply(volume.profile);

        StartCoroutine("RefreshCoroutine");
    }

    IEnumerator RefreshCoroutine() {
        while (true) {
            reflectionProbe.RenderProbe();
            DynamicGI.UpdateEnvironment();
            yield return new WaitForSeconds(1f);
        }
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
    private bool toggle;
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
