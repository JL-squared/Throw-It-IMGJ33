using UnityEngine;
using UnityEngine.InputSystem;

public class EvilSnowballItem : SnowballItem {
    public override void PrimaryAction(InputAction.CallbackContext context, Player player) {
        // release charge, throw snowball
        if (isCharging && context.canceled) {
            isCharging = false;

            // charge value between 0 and 1 for charge
            float charge = Mathf.Clamp01(time / timeForMaxCharge);
            
            // actual force percentage value, between minFactor and 1f
            float forcePercentage = Mathf.Lerp(minFactor, 1f, charge);

            ProjectileShooter shooter = player.GetComponent<ProjectileShooter>();
            shooter.data = (SnowballItemData)player.inventory.EquippedItem.Data;

            // flick maxxing
            Vector3 bruh = (player.transform.right * player.movement.mouseDelta.x + player.transform.up * player.movement.mouseDelta.y) * 0.05f;
            //Vector3 bruh = Vector3.zero;
            shooter.Shoot(forcePercentage, bruh);

            throwDelay = maxThrowDelay * charge;
            time = 0;
            player.inventory.container.RemoveItem(player.inventory.Equipped, 1);
            return;
        }
    }
}
