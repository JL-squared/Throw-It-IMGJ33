using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMaster : MonoBehaviour {
    public IngameHUD inGameHUD;
    public GameObject menu;
    public HealthBar healthBar;
    public GameObject healthBarGroup;
    public GameObject deathScreen;
    public GameObject pauseMenu;

    public static UIMaster Instance;

    public enum MenuState {
        None,
        Crafting,
        Market,
        Paused,
        Console,
        MainMenu,
        Building
    }

    public MenuState state = MenuState.None;

    private void Awake() {
        Registries.onLoaded.AddListener(loadBuildPieces);
    }

    void Start() {
        Instance = this;
        inGameHUD.craftingMenuObject.SetActive(false);
        if (GameManager.Instance != null) {
            GameManager.Instance.onPausedChanged += (bool paused) => pauseMenu.SetActive(paused);
            GameManager.Instance.onTimeSinceDeath += (float time) => {
                deathScreen.GetComponent<CanvasGroup>().alpha = Mathf.SmoothStep(0.0f, 1.0f, time);
            };
            GameManager.Instance.onDeath += () => deathScreen.SetActive(true);
        }

        if (SceneManager.GetActiveScene().name == "Main Menu") {
            state = MenuState.MainMenu;
            Evaluate();
        }
    }

    public void loadBuildPieces() {
        var parent = inGameHUD.buildingMenuContent;
        foreach (PieceDefinition definition in Registries.pieces.data.Values) {
            var thing = Instantiate(inGameHUD.buildEntryPrefab);
            thing.transform.parent = parent.transform;
            thing.GetComponent<BuildingEntry>().Init(definition);
        }
    }

    public bool MovementPossible() {
        bool i = true;
        switch(state) {
            case MenuState.None:
            i = true; break;
            default:
            i = false; break;
        }
        return i;
    }

    // Changes the state
    public void EscPressed() {
        switch(state) {
            case MenuState.None:
                state = MenuState.Paused; 
            break;

            case MenuState.Paused or MenuState.Console:
                state = MenuState.None;
            break;

            case MenuState.MainMenu: break;

            default:
                state = MenuState.None;
            break;
        }

        Evaluate();
    }

    private void ToggleState(MenuState newState) {
        if (state == MenuState.None) {
            state = newState;
        } else if (state == newState) {
            state = MenuState.None;
        }
    }

    public void ToggleInventory() {
        ToggleState(MenuState.Crafting);
        Evaluate();
    }

    public void ToggleMarket() {
        ToggleState(MenuState.Market);
        Evaluate();
    }

    public void ToggleDevConsole() {
        ToggleState(MenuState.Console);
        Evaluate();
    }

    public void ToggleBuilding() {
        ToggleState(MenuState.Building);
        Evaluate();
    }

    public void Clear() {
        ToggleState(MenuState.None);
        Evaluate();
    }

    // THIS JUST EVALUATES THE STATE
    public void Evaluate() {
        switch(state) {
            case MenuState.None or MenuState.Console:
                pauseMenu.SetActive(false);
                healthBarGroup.SetActive(true);
                inGameHUD.SetIngame();
            break;

            case MenuState.Paused:
                pauseMenu.SetActive(true);
                healthBarGroup.SetActive(true);
            break;

            case MenuState.Crafting:
                pauseMenu.SetActive(false);
                healthBarGroup.SetActive(true);
                inGameHUD.SetCraftingMenu();
            break;

            case MenuState.Market:
                pauseMenu.SetActive(false);
                healthBarGroup.SetActive(true);
                inGameHUD.SetMarketMenu();
            break;

            case MenuState.MainMenu:
                pauseMenu.SetActive(false);
                inGameHUD.SetMenu();
                healthBarGroup.SetActive(false);
            break;

            case MenuState.Building:
                pauseMenu.SetActive(false);
                healthBarGroup.SetActive(true);
                inGameHUD.SetBuildingMenu();
            break;
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.UpdatePaused(state == MenuState.Paused);
        }

        if (MovementPossible()) {
            Cursor.lockState = CursorLockMode.Locked;
        } else {
            Player.Instance.ResetMovement();
            Cursor.lockState = CursorLockMode.None;
        }

        GameManager.Instance.devConsole.consoleNation = state == MenuState.Console;
    }
}
