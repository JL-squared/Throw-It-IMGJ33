using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class UIMaster : MonoBehaviour {
    public IngameHUD inGameHUD;
    public GameObject menu;
    public HealthBar healthBar;
    public GameObject deathScreen;
    public GameObject pauseMenu;


    public static UIMaster Instance;

    // Start is called before the first frame update
    void Start() {
        Instance = this;
        inGameHUD.craftingMenuObject.SetActive(false);
        GameManager.Instance.timeManager.onPausedChanged += (bool paused) => pauseMenu.SetActive(paused);
        GameManager.Instance.timeManager.onTimeSinceDeath += (float time) => {
            deathScreen.GetComponent<CanvasGroup>().alpha = Mathf.SmoothStep(0.0f, 1.0f, time);
        };
        GameManager.Instance.timeManager.onDeath += () => deathScreen.SetActive(true);
    }
}
