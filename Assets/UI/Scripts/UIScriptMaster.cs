using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Static reference object for UI components.
/// Functions point here in order to access UI methods down the chain.
/// This class also holds various prefabs
/// </summary>
public class UIScriptMaster : MonoBehaviour {

    [Header("Scripts")]
    public BuildingMenu buildingMenu;
    public CraftingMenu craftingMenu;
    public HealthBar healthBar;
    public TemperatureDisplay temperatureDisplay;
    public HotbarUI hotbarUI;
    public CrosshairHints crosshairHints;
    public IngameHUDManager inGameHUD;
    public CursorItemDrag cursorItemDrag;
    public Inventory inventory;

    [Header("Prefabs")]
    public GameObject pieceEntryPrefab;
    public GameObject recipeEntryPrefab;
    public GameObject moodlePrefab;

    public static UIScriptMaster Instance;

    public UnityEvent loadCall;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        Registries.onLoaded.AddListener(Load);
    }

    private void Load() {
        loadCall.Invoke();
    }
}
