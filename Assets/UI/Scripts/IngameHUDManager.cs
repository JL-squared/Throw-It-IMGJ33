using UnityEngine;

/// <summary>
/// State manager for ingame menu objects.
/// </summary>
public class IngameHUDManager : MonoBehaviour {
    public GameObject FullHUDGroup;

    [Header("Panel Objects")]
    public GameObject craftingMenuObject;
    public GameObject buildingMenuObject;

    [Header("Screen Objects")]
    public GameObject hotbarGroup;
    public GameObject deathScreen;
    public GameObject pauseMenu;
    public GameObject screenGraphics;

    public enum PanelState {
        None,
        Crafting,
        Building
    }

    private PanelState panelState = PanelState.None;

    public enum ScreenState {
        Default,
        Paused,
        GameOver,
    }

    private ScreenState screenState = ScreenState.Default;

    public bool HUDActivated = false;
    public bool consoleActivated = false;

    private bool paused;
    public bool Paused {
        get { return paused; }
        set {
            pauseMenu.SetActive(value);
            paused = value;

            if (paused && screenState == ScreenState.Default) screenState = ScreenState.Paused;
            if (!paused && screenState == ScreenState.Paused) screenState = ScreenState.Default;
        }
    }

    void Start() {
        SetDefault();

        if (GameManager.Instance != null) {
            GameManager.Instance.onPausedChanged += (bool paused) => Paused = paused;
            GameManager.Instance.onTimeSinceDeath += (float time) => {
                deathScreen.GetComponent<CanvasGroup>().alpha = Mathf.SmoothStep(0.0f, 1.0f, time);
            };
            GameManager.Instance.onDeath += () => deathScreen.SetActive(true);
        }
    }

    // Group of methods for when you're ingame
    public void SetIngame() {
        craftingMenuObject.SetActive(false);
        buildingMenuObject.SetActive(false);
        screenGraphics.SetActive(true);
    }

    // Group of common purpose method calls for when you're not ingame
    public void SetMenu() {
        screenGraphics.SetActive(false);
    }
    
    public void SetDefault() {
        craftingMenuObject.SetActive(false);
        buildingMenuObject.SetActive(false);
        hotbarGroup.SetActive(true);
        deathScreen.SetActive(false);
        screenGraphics.SetActive(true);
        pauseMenu.SetActive(false);
    }

    public bool IsInState(PanelState planelState, ScreenState screenState) {
        return this.panelState == planelState && this.screenState == screenState && !consoleActivated;
    }

    // Makes a single change to the state and breaks
    public void EscPressed() {
        // Remove console
        if (consoleActivated) {
            consoleActivated = false;
            return;
        }

        // Remove panel
        if (panelState != PanelState.None) {
            panelState = PanelState.None;
            return;
        }

        // Pause
        Paused = !paused;

        Evaluate();
    }

    private void TogglePanel(PanelState newState) {
        if (panelState == PanelState.None) {
            panelState = newState;
        } else if (panelState == newState) {
            panelState = PanelState.None;
        }
    }

    public void ToggleInventory() {
        TogglePanel(PanelState.Crafting);
        Evaluate();
    }

    public void ToggleDevConsole() {
        consoleActivated = !consoleActivated;
        Evaluate();
    }

    public void ToggleBuilding() {
        TogglePanel(PanelState.Building);
        Evaluate();
    }

    public void ClearPanels() {
        buildingMenuObject.SetActive(false);
        craftingMenuObject.SetActive(false);
    }

    // THIS JUST EVALUATES THE STATE
    public void Evaluate() {
        hotbarGroup.SetActive(true);
        screenGraphics.SetActive(false);
        ClearPanels();

        switch (panelState) {
            case(PanelState.Building):
                buildingMenuObject.SetActive(true);
                break;

            case(PanelState.Crafting):
                craftingMenuObject.SetActive(true);
                break;

            case(PanelState.None):
                screenGraphics.SetActive(true);
                break;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.UpdatePaused(screenState == ScreenState.Paused);

        if (IsInState(PanelState.None, ScreenState.Default)) {
            Cursor.lockState = CursorLockMode.Locked;
        } else {
            Player.Instance.movement.ResetMovement();
            Cursor.lockState = CursorLockMode.None;
        }

        GameManager.Instance.devConsole.fardNation = consoleActivated;
    }
}
