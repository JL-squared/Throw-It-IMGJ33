using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Tweens;
using System;
using System.Linq;

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

    public new Camera camera;

    public State state;
    public AudioSource music;

    public PlayerControlsSettings settings;
    [HideInInspector]
    public bool PrimaryHeld = false;
    [HideInInspector]
    public bool SecondaryHeld = false;
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

        foreach (var item in GetComponents<PlayerBehaviour>()) {
            item.settings = settings;
            item.player = this;
        }
    }

    // Checks if a button has been pressed in the current frame
    public bool Pressed(InputAction.CallbackContext context) {
        return context.performed && !context.canceled && CanPerform();
    }

    // Checks if we can do *any* movement
    public bool CanPerform() {
        return UIScriptMaster.Instance.inGameHUD.MovementPossible() && state != State.Dead && GameManager.Instance.initialized;
    }

    public void ExitButton(InputAction.CallbackContext context) {
        UIScriptMaster.Instance.inGameHUD.EscPressed();
    }

    /*
    public void AltAction(InputAction.CallbackContext context) {
        if(context.performed) {
            altAction = true;
        } else if (context.canceled) {
            altAction = false;
        }
    }
    */


    public void Scroll(InputAction.CallbackContext context) {
        if (!Pressed(context))
            return;

        float scroll = -context.ReadValue<float>();
        if (state == State.Building) {
            building.Scroll(scroll);
        } else {
            inventory.Scroll(scroll);
        }
    }

    public void PrimaryAction(InputAction.CallbackContext context) {
        if (!CanPerform())
            return;

        PrimaryHeld = !context.canceled && state == State.Default;

        if (state == State.Building) {
            building.PrimaryAction(context);
        } else {
            inventory.PrimaryAction(context);
        }
    }

    public void InteractAction(InputAction.CallbackContext context) {
        if (!Pressed(context))
            return;

        if (state == State.Driving) {
            movement.ExitVehicle();
        } else {
            interactions.Interact();
        }
    }

    public void SecondaryAction(InputAction.CallbackContext context) {
        if (!CanPerform())
            return;

        SecondaryHeld = !context.canceled && state == State.Default;

        if (state == State.Building) {
            building.SecondaryAction(context);
        } else {
            inventory.SecondaryAction(context);
        }
    }

    public void TertiaryAction(InputAction.CallbackContext context) {
        if (!CanPerform())
            return;

        if (state == State.Building) {
            building.TertiaryAction(context);
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
    }


    int lastPlayedMusicIndex = -1;
    public void PlayMusic() {
        /*
        var temp = Registries.music.data.Random(lastPlayedMusicIndex);
        music.clip = temp.Item2;
        lastPlayedMusicIndex = temp.Item1;
        music.Play();
        */
    }
}