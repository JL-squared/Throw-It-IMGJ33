using UnityEngine.InputSystem;

public class PlayerConsole : PlayerBehaviour {
    public void ToggleDevConsole(InputAction.CallbackContext context) {
        if (Pressed(context) && !GameManager.Instance.devConsole.fardNation) {
            UIScriptMaster.Instance.inGameHUD.ToggleDevConsole();
        }
    }
}