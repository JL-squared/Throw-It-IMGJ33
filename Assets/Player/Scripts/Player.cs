using UnityEngine;

// Full Player script holding all necessary functions and variables
public class Player : MonoBehaviour, IEntitySerializer {
    public enum State {
        Default,
        Building,
        Driving,
        Dead,
    }

    public static Player Instance { get; private set; }

    [HideInInspector]
    public PlayerBobbingSway bobbing;

    [HideInInspector]
    public PlayerBuilding building;

    [HideInInspector]
    public PlayerMovement movement;

    [HideInInspector]
    public PlayerTemperature temperature;

    [HideInInspector]
    public PlayerInteractions interactions;

    [HideInInspector]
    public PlayerInventory inventory;

    [HideInInspector]
    public PlayerHealth health;

    [HideInInspector]
    public PlayerFootsteps footsteps;

    [HideInInspector]
    public PlayerCameraShake cameraShake;

    [HideInInspector]
    public PlayerInputHandler input;

    [HideInInspector]
    public MoodleManager moodleManager;

    public new Camera camera;
    public GameObject head;

    public State state;
    public AudioSource music;

    public PlayerControlsSettings settings;

    [HideInInspector]
    public RaycastHit? lookingAt;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        settings = Utils.Load<PlayerControlsSettings>("player.json");
        bobbing = GetComponent<PlayerBobbingSway>();
        building = GetComponent<PlayerBuilding>();
        movement = GetComponent<PlayerMovement>();
        temperature = GetComponent<PlayerTemperature>();
        interactions = GetComponent<PlayerInteractions>();
        health = GetComponent<PlayerHealth>();
        inventory = GetComponent<PlayerInventory>();
        footsteps = GetComponent<PlayerFootsteps>();
        cameraShake = GetComponent<PlayerCameraShake>();
        input = GetComponent<PlayerInputHandler>();
        moodleManager = GetComponent<MoodleManager>();

        foreach (var item in GetComponents<PlayerBehaviour>()) {
            item.settings = settings;
            item.player = this;
            item.bobbing = bobbing;
            item.building = building;
            item.movement = movement;
            item.temperature = temperature;
            item.interactions = interactions;
            item.health = health;
            item.inventory = inventory;
            item.footsteps = footsteps;
            item.cameraShake = cameraShake;
            item.moodleManager = moodleManager;
        }
    }

    public void Serialize(EntityData data) {
        /*
        data.inventory = items;
        data.wishHeadDir = wishHeadDir;
        */
    }

    public void Deserialize(EntityData data) {
        /*
        interaction = null;
        lastInteraction = null;
        stepValue = 0f;
        items = data.inventory;
        wishHeadDir = data.wishHeadDir.Value;
        ApplyMouseDelta(Vector2.zero);
        SelectionChanged();
        inventoryUpdateEvent?.Invoke(items);

        foreach (var item in items) {
            item.onUpdate?.AddListener((ItemStack item) => { inventoryUpdateEvent.Invoke(items); });
        }
        */
    }

    private void Update() {
        if (!music.isPlaying && !GameManager.Instance.paused) {
            PlayMusic();
        }

        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit info, 5f, ~LayerMask.GetMask("Player"))) {
            lookingAt = info;
        } else {
            lookingAt = null;
        }
    }


    public void PlayMusic() {
        var temp = Registries.music.data.Random();
        music.clip = temp.Item2;
        music.Play();
    }
}