using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using static EntityHealth;
using UnityEngine.InputSystem;

public class PlayerConsole : PlayerBehaviour {
    public void ToggleDevConsole(InputAction.CallbackContext context) {
        if (Pressed(context) && !GameManager.Instance.devConsole.fardNation) {
            UIScriptMaster.Instance.inGameHUD.ToggleDevConsole();
        }
    }
}