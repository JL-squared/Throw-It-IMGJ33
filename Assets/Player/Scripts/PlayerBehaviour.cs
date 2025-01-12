using UnityEngine;
using UnityEngine.InputSystem;

public class HookableActionResult {
    public int priority;
    public bool taken;
}

public class PlayerBehaviour : MonoBehaviour {
    protected Player player;
    protected PlayerControlsSettings settings;

    // Checks if the given input can be executed (using context.performed)
    protected bool Performed(InputAction.CallbackContext context) {
        return context.performed && UIScriptMaster.Instance.inGameHUD.MovementPossible() && !player.isDead && GameManager.Instance.initialized;
    }

    // Checks if we can do *any* movement
    protected bool Performed() {
        return UIScriptMaster.Instance.inGameHUD.MovementPossible() && !player.isDead && GameManager.Instance.initialized;
    }

    public void AltAction(InputAction.CallbackContext context) {
        if (context.performed) {
            altAction = true;
        } else if (context.canceled) {
            altAction = false;
        }
    }


    public void Scroll(InputAction.CallbackContext context) {
        if (Performed(context)) {
            float scroll = -context.ReadValue<float>();

            if (isBuilding) {
                placementRotation += scroll * 22.5f;
            } else {
                SelectionChanged(() => {
                    int newSelected = (Equipped + (int)scroll) % 10;
                    Equipped = (newSelected < 0) ? 9 : newSelected;
                });
            }
        }
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
}
