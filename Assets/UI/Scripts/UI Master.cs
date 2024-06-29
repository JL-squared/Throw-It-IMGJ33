using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class UIMaster : MonoBehaviour {
    public IngameHUD inGameHUD;
    public GameObject menu;
    public HealthBar healthBar;
    public GameObject deathScreen;
    public GameObject pauseMenu;

    public static UIMaster Instance;

    public enum MenuState {
        NONE,
        CRAFTING,
        PAUSED,
        MENU
    }

    public MenuState state = MenuState.NONE;

    // Start is called before the first frame update
    void Start() {
        Instance = this;
        inGameHUD.craftingMenuObject.SetActive(false);
        GameManager.Instance.onPausedChanged += (bool paused) => pauseMenu.SetActive(paused);
        GameManager.Instance.onTimeSinceDeath += (float time) => {
            deathScreen.GetComponent<CanvasGroup>().alpha = Mathf.SmoothStep(0.0f, 1.0f, time);
        };
        GameManager.Instance.onDeath += () => deathScreen.SetActive(true);
    }

    public bool MovementPossible() {
        bool i = true;
        switch(state) {
            case MenuState.NONE:
            i = true; break;
            case MenuState.CRAFTING:
            i = false; break;
            case MenuState.PAUSED:
            i = false; break;
        }
        return i;
    }

    // Changes the state
    public void EscPressed() {
        switch(state) {
            case MenuState.NONE:
                state = MenuState.PAUSED; 
            break;

            case MenuState.PAUSED:
                state = MenuState.NONE;
            break;

            case MenuState.MENU: break;

            default:
                state = MenuState.NONE;
            break;
        }

        Evaluate();
    }

    public void TabPressed() {
        switch(state) {
            case MenuState.NONE:
                state = MenuState.CRAFTING;
            break;

            case MenuState.CRAFTING:
                state = MenuState.NONE;
            break;
        }

        Evaluate();
    }

    // THIS JUST EVALUATES THE STATE
    public void Evaluate() {
        switch(state) {
            case MenuState.NONE:
                pauseMenu.SetActive(false);
                inGameHUD.SetIngame();
            break;

            case MenuState.PAUSED:
                pauseMenu.SetActive(true);
            break;

            case MenuState.CRAFTING:
                pauseMenu.SetActive(false);
                inGameHUD.SetCraftingMenu();
            break;
        }

        GameManager.Instance.UpdatePaused(state == MenuState.PAUSED);
        Cursor.lockState = MovementPossible() ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
