using UnityEngine;

public class UIMaster : MonoBehaviour {
    public IngameHUD inGameHUD;
    public GameObject menu;
    public HealthBar healthBar;
    public GameObject deathScreen;
    bool dead;
    float timeSinceDeath;

    public static UIMaster Instance;

    // Start is called before the first frame update
    void Start() {
        Instance = this;
        inGameHUD.craftingMenuObject.SetActive(false);
    }

    public void OnDeath() {
        deathScreen.SetActive(true);
        dead = true;
    }

    public void Update() {
        if (dead) {
            timeSinceDeath += Time.unscaledDeltaTime * .1f;
            Time.timeScale = Mathf.SmoothStep(1.0f, 0.0f, timeSinceDeath);

            deathScreen.GetComponent<CanvasGroup>().alpha = Mathf.SmoothStep(0.0f, 1.0f, timeSinceDeath);
        }
    }
}
