using UnityEngine;

public class UIMaster : MonoBehaviour {
    public IngameHUD inGameHUD;
    public GameObject menu;
    public HealthBar healthBar;
    public GameObject deathScreen;
    bool dead;

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
            deathScreen.GetComponent<CanvasGroup>().alpha += Time.deltaTime;
        }
    }
}
