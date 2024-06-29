using Unity.VisualScripting.Antlr3.Runtime;
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
        Paused,
        MainMenu
    }

    public MenuState state = MenuState.None;

    // Start is called before the first frame update
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

    public bool MovementPossible() {
        bool i = true;
        switch(state) {
            case MenuState.None:
            i = true; break;
            case MenuState.Crafting or MenuState.Paused or MenuState.MainMenu:
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

            case MenuState.Paused:
                state = MenuState.None;
            break;

            case MenuState.MainMenu: break;

            default:
                state = MenuState.None;
            break;
        }

        Evaluate();
    }

    public void TabPressed() {
        switch(state) {
            case MenuState.None:
                state = MenuState.Crafting;
            break;

            case MenuState.Crafting:
                state = MenuState.None;
            break;
        }

        Evaluate();
    }

    // THIS JUST EVALUATES THE STATE
    public void Evaluate() {
        switch(state) {
            case MenuState.None:
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
            
            case MenuState.MainMenu:
                pauseMenu.SetActive(false);
                inGameHUD.SetMenu();
                healthBarGroup.SetActive(false);
            break;
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.UpdatePaused(state == MenuState.Paused);
        }
        Cursor.lockState = MovementPossible() ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
