using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PersistentSaveManager : MonoBehaviour {
    //[HideInInspector]
    public string saveName;
    //[HideInInspector]
    public bool load;

    public string GetGoodName => saveName.Trim() == "" ? "default" : saveName.Trim();
    public string Path => "saves/" + GetGoodName + "/";
    public string GlobalPath => Utils.PersistentDir + "/" + Path;
    public static PersistentSaveManager Instance;

    bool testLoad = false;

    private void Start() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(Instance);
            return;
        }

        SceneManager.sceneLoaded += SceneManager_sceneLoaded; ;
        DontDestroyOnLoad(this);
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1) {
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
    }

    private void Update() {
        if (testLoad) {
            testLoad = false;
            LoadInternal();
        }
    }

    public bool Create(string name) {
        saveName = name;
        load = false;

        if (Directory.Exists(GlobalPath)) {
            Debug.LogWarning("Save folder already exists!");
            return false;
        } else {
            return true;
        }
    }

    IEnumerator DelayedSave() {
        yield return 0;
        Save();
    }

    IEnumerator DelayedLoad() {
        yield return 0;
        LoadInternal();
    }

    public void Save() {
        load = false;
        Debug.Log("Saving...");
        Directory.CreateDirectory(GlobalPath);
        //VoxelTerrain.Instance.SaveMap(GlobalPath + "terrain", true);
        SaveState state = SaveState.Save();
        Utils.Save(Path + "save.json", state);
        Debug.Log("Saved!");
    }

    public void LoadInternal() {
        load = false;
        Debug.Log("Loading...");
        //VoxelTerrain.Instance.LoadMapSkibi(GlobalPath + "terrain");
        SaveState state = Utils.Load<SaveState>(Path + "save.json");
        state.Loaded();
        Debug.Log("Loaded!");
    }

    public void Load() {
        load = true;
        saveName = name;
        SceneManager.LoadScene("MainMenu");
    }

    public bool LoadFromMenu(string name) {
        saveName = name;

        if (SceneManager.GetActiveScene().name != "MainMenu") {
            Debug.LogWarning("Load() must be called from within the menu scene");
            return false;
        }
        
        if (!Directory.Exists(GlobalPath)) {
            Debug.LogWarning("No save folder!");
            return false;
        }

        load = true;
        SceneManager.LoadScene("SampleScene");
        return true;
    }
}
