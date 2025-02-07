using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

// Full Player script holding all necessary functions and variables
public class PlayerInputHandler : PlayerBehaviour {

    [HideInInspector]
    public bool PrimaryHeld = false;
    [HideInInspector]
    public bool SecondaryHeld = false;

    // Checks if a button has been pressed in the current frame
    public bool Pressed(InputAction.CallbackContext context) {
        return !context.canceled && context.performed;
    }

    // Checks if a button has been released in the current frame
    public bool Released(InputAction.CallbackContext context) {
        return context.canceled && !context.performed;
    }

    // Checks if we can do *any* movement (or if we are in a specific panel state)
    public bool Performed(InputAction.CallbackContext context, IngameHUDManager.PanelState panelState = IngameHUDManager.PanelState.None) {
        return (context.performed ^ context.canceled) && UIScriptMaster.Instance.inGameHUD.IsInState(panelState, IngameHUDManager.ScreenState.Default) && state != Player.State.Dead && GameManager.Instance.initialized;
    }

    public void ExitButton(InputAction.CallbackContext context) {
        if (Pressed(context)) {
            UIScriptMaster.Instance.inGameHUD.EscPressed();
        }
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
        if (Performed(context) && Pressed(context)) {
            float scroll = -context.ReadValue<float>();
            if (state == Player.State.Building) {
                building.Scroll(scroll);
            } else {
                inventory.Scroll(scroll);
            }
        }
    }

    public void ToggleInventory(InputAction.CallbackContext context) {
        if ((Performed(context) || Performed(context, IngameHUDManager.PanelState.Crafting)) && Pressed(context)) {
            inventory.ToggleInventory();
        }
    }

    public void SelectSlot(InputAction.CallbackContext context) {
        if (Performed(context) && Pressed(context)) {
            int value = (int)context.ReadValue<float>();
            inventory.SelectSlot(value);
        }
    }

    public void PrimaryAction(InputAction.CallbackContext context) {
        if (Performed(context)) {
            PrimaryHeld = !context.canceled && state == Player.State.Default;

            if (state == Player.State.Building && Pressed(context)) {
                building.PrimaryAction();
            } else {
                inventory.PrimaryAction(context);
            }
        }
    }

    public void InteractAction(InputAction.CallbackContext context) {
        if (!(Pressed(context) && Performed(context)))
            return;

        if (state == Player.State.Driving) {
            movement.ExitVehicle();
        } else {
            interactions.Interact();
        }
    }

    public void SecondaryAction(InputAction.CallbackContext context) {
        if (Performed(context)) {
            SecondaryHeld = !context.canceled && state == Player.State.Default;
        }

        if (state == Player.State.Building && Pressed(context) && (Performed(context, IngameHUDManager.PanelState.Building) || Performed(context, IngameHUDManager.PanelState.None))) {
            building.SecondaryAction();
        } else if (Performed(context)) {
            inventory.SecondaryAction(context);
        }
    }

    public void TertiaryAction(InputAction.CallbackContext context) {
        if (Performed(context) && Pressed(context)) {
            if (state == Player.State.Building) {
                building.TertiaryAction();
            }
        }
    }

    public void DropItem(InputAction.CallbackContext context) {
        if (Performed(context) && Pressed(context)) {
            inventory.DropItem();
        }
    }

    public void TempActivateBuildingMode(InputAction.CallbackContext context) {
        if (Performed(context) && Pressed(context)) {
            building.TempActivateBuildingMode();
        }
    }

    public void ToggleDevConsole(InputAction.CallbackContext context) {
        if (Pressed(context) && Performed(context)) {
            UIScriptMaster.Instance.inGameHUD.ToggleDevConsole();
        }
    }

    public void Crouch(InputAction.CallbackContext context) {
        if (Performed(context)) {
            movement.Crouch(context.ReadValue<float>() > 0.5f);
        }
    }

    public void Jump(InputAction.CallbackContext context) {
        if (Performed(context) && Pressed(context)) {
            movement.Jump();
        }
    }

    public void Movement(InputAction.CallbackContext context) {
        if (Performed(context)) {
            movement.Movement(context.ReadValue<Vector2>());
        }
    }

    public void Sprint(InputAction.CallbackContext context) {
        if (Performed(context)) {
            movement.Sprint(context.ReadValue<float>() > 0.5f);
        }
    }

    public void Look(InputAction.CallbackContext context) {
        if (Performed(context)) {
            movement.Look(context.ReadValue<Vector2>());
        }
    }
}