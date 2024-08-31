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
                if (!Directory.Exists(Utils.PersistentDir + "/" + Path)) {
                    Debug.LogError("Sorry bro, no save file for you!!!");
                    return;
                }

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

    public void Create(string name) {
        saveName = name;
        load = false;
        Directory.CreateDirectory(Path);
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
        VoxelTerrain.Instance.SaveMap(Utils.PersistentDir + "/" + Path + "terrain", true);
        SaveState state = SaveState.Save();
        Utils.Save(Path + "save.json", state);
        Debug.Log("Saved!");
    }

    public void LoadInternal() {
        load = false;
        Debug.Log("Loading...");
        VoxelTerrain.Instance.LoadMapSkibi(Utils.PersistentDir + "/" + Path + "terrain");
        SaveState state = Utils.Load<SaveState>(Path + "save.json");
        state.Loaded();
        Debug.Log("Loaded!");
    }

    public void Load() {
        load = true;
        saveName = name;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadFromMenu(string name) {
        if (SceneManager.GetActiveScene().name != "MainMenu") {
            Debug.LogWarning("Load() must be called from within the menu scene");
            return;
        }

        load = true;
        saveName = name;
        SceneManager.LoadScene("SampleScene");
    }
}
