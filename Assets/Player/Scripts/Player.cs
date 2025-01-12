using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Tweens;
using System;
using System.Linq;

// Full Player script holding all necessary functions and variables
public class Player : MonoBehaviour, IEntitySerializer {
    public static Player Instance { get; private set; }
    public PlayerBobbingSway bobbing;
    public PlayerBuilding building;
    public PlayerController controller;
    public PlayerTemperature temperature;
    public PlayerInteractions interactions;
    public PlayerInventory inventory;
    public PlayerHealth health;

    public new Camera camera;
    
    public bool isDead;
    public AudioSource music;

    public PlayerControlsSettings settings;
    [HideInInspector]
    public bool PrimaryHeld = false;
    [HideInInspector]
    public bool SecondaryHeld = false;

    public UnityEvent<float> onSpeedPercentageUpdate;
    public UnityEvent<float> onStepUpdate;

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
        controller = GetComponent<PlayerController>();
        temperature = GetComponent<PlayerTemperature>();
        interactions = GetComponent<PlayerInteractions>();
        health = GetComponent<PlayerHealth>();
        inventory = GetComponent<PlayerInventory>();
    }

    private void Update() {
        if(!music.isPlaying && !GameManager.Instance.paused) {
            PlayMusic();
        }
    }

    public void ExitButton(InputAction.CallbackContext context) {
        if (context.performed) {
            UIScriptMaster.Instance.inGameHUD.EscPressed();
        }
    }

    public void ToggleDevConsole(InputAction.CallbackContext context) {
        if (Performed(context) && !GameManager.Instance.devConsole.fardNation) {
            UIScriptMaster.Instance.inGameHUD.ToggleDevConsole();
        }
    }

    /*
    public void ToggleInventory(InputAction.CallbackContext context) {
        if (context.performed && !isDead) {
            UIScriptMaster.Instance.inGameHUD.ToggleInventory();
        }
    }

    public void ExitButton(InputAction.CallbackContext context) {
        if (context.performed) {
            UIScriptMaster.Instance.inGameHUD.EscPressed();
        }
    }


    public void AltAction(InputAction.CallbackContext context) {
        if(context.performed) {
            altAction = true;
        } else if (context.canceled) {
            altAction = false;
        }
    }



    public void ToggleDevConsole(InputAction.CallbackContext context) {
        if (Performed(context) && !GameManager.Instance.devConsole.fardNation) {
            UIScriptMaster.Instance.inGameHUD.ToggleDevConsole();
        }
    }



    public void Scroll(InputAction.CallbackContext context) {
        if (Performed(context)) {
            float scroll = -context.ReadValue<float>();

            if (isBuilding) {
                placementRotation += scroll * 22.5f;
            } else {
                SelectionChanged( () => {
                    int newSelected = (Equipped + (int)scroll) % 10;
                    Equipped = (newSelected < 0) ? 9 : newSelected;
                });
            }
        }
    }

    public void SelectSlot(InputAction.CallbackContext context) {
        if (Performed(context))
            SelectionChanged(() => { Equipped = (int)context.ReadValue<float>(); });
    }

    public void PrimaryAction(InputAction.CallbackContext context) {
        if (!Performed() || context.performed)
            return;

        PrimaryHeld = !context.canceled && !isBuilding;

        if (isBuilding && placementStatus && !context.canceled) {
            BuildActionPrimary();
        } else if (!EquippedItem.IsEmpty() && !isBuilding) {
            EquippedItem.logic.PrimaryAction(context, this);
        }
    }

    public void InteractAction(InputAction.CallbackContext context) {
        if (!Performed(context) || !lookingAt.HasValue)
            return;

        if (interaction != null && interaction.Interactable && vehicle == null) {
            interaction.Interact(this);
        } else if (vehicle != null) {
            ExitVehicle();
        }
    }

    public void SecondaryAction(InputAction.CallbackContext context) {
        if (context.performed)
            return;

        bool pressed = !context.canceled;
        if (isBuilding && pressed) {
            UIScriptMaster.Instance.inGameHUD.ToggleBuilding();
        } else if (!EquippedItem.IsEmpty()) {
            EquippedItem.logic.SecondaryAction(context, this);
        }
    }

    public void TertiaryAction(InputAction.CallbackContext context) {
        if (context.performed)
            return;

        bool pressed = !context.canceled;
        if (isBuilding && pressed) {
            DestroySelectedBuilding();
        }
    }

    public void TempActivateBuildingMode(InputAction.CallbackContext context) {
        if (Performed(context)) {
            isBuilding = !isBuilding;
            placementTarget.SetActive(false);
            if (!isBuilding) {
                ClearOutline();
            }
        }
    }
    */

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


    int lastPlayedMusicIndex = -1;
    public void PlayMusic() {
        var temp = Registries.music.data.Random(lastPlayedMusicIndex);
        music.clip = temp.Item2;
        lastPlayedMusicIndex = temp.Item1;
        music.Play();
    }
}