using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentSaveManager : MonoBehaviour {
    /// <summary>
    /// The name of the current save that we have created. Is empty when we are not using a save
    /// </summary>
    public string CurrentSaveName { get; set; }

    /// <summary>
    /// Path relative to the local persistent di
    /// </summary>
    //public string Path => "saves/" + GetGoodName + "/";
    //public string GlobalPath => Utils.PersistentDir + "/" + Path;
    public static PersistentSaveManager Instance;

    bool testLoad = false;

    private void Start() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(Instance);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(this);
    }

    private string GetGlobalPathForSaveFolder(string save) {
        return $"{Utils.PersistentDir}/saves/{save.Trim()}/";
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) {
        /*
        string name = arg0.name;
        if (load) {
            if (name == "MainMenu") {
                LoadFromMenu(saveName);
            } else {
                testLoad = true;
                //StartCoroutine(DelayedLoad());
            }
        } else {
            if (name != "MainMenu") {
                //StartCoroutine(DelayedSave());
            }
        }
        */
    }

    private void Update() {
        if (testLoad) {
            testLoad = false;
            LoadInternal();
        }
    }

    /// <summary>
    /// Sets the save name and loads up into the SampleScene scene
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public void Create(string name) {
        /*
        saveName = name;
        load = false;

        
        if (Directory.Exists(GlobalPath)) {
            Debug.LogWarning("Save folder already exists, aborting!");
            return;
        }

        Debug.Log("Loading into SampleScene");
        SceneManager.LoadScene("SampleScene");
        */
    }

    /// <summary>
    /// Takes the current session and saves it to its respective save file
    /// </summary>
    public void Save() {
        /*
        Debug.Log("Saving...");
        Directory.CreateDirectory(GetGlobalPathForSaveFolder(curr));
        //VoxelTerrain.Instance.SaveMap(GlobalPath + "terrain", true);
        SaveState state = SaveState.Save();
        Utils.Save(Path + "save.json", state);
        Debug.Log("Saved!");
        */
    }

    /// <summary>
    /// Loads the last save for the current session. Basically loads a backup
    /// </summary>
    public void LoadInternal() {
        /*
        load = false;
        Debug.Log("Loading...");
        //VoxelTerrain.Instance.LoadMapSkibi(GlobalPath + "terrain");
        SaveState state = Utils.Load<SaveState>(Path + "save.json");
        state.Loaded();
        Debug.Log("Loaded!");
        */
    }

    public void Load() {
        /*
        load = true;
        saveName = name;
        Debug.Log($"Load: {name}");
        SceneManager.LoadScene("MainMenu");
        */
    }

    public void LoadFromMenu(string name) {
        /*
        if (SceneManager.GetActiveScene().name != "MainMenu") {
            Debug.LogWarning("Load() must be called from within the menu scene");
            return;
        }

        if (!Directory.Exists(GlobalPath)) {
            Debug.LogWarning("No save folder!");
        }

        saveName = name;
        Debug.Log($"LoadFromMenu: {name}");


        SceneManager.LoadScene("SampleScene");

        load = true;
        SceneManager.LoadScene("SampleScene");
        return true;
        */
    }
}
