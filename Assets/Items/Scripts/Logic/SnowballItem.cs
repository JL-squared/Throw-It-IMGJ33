using UnityEngine;
using UnityEngine.InputSystem;

public class SnowballItem : Item {
    public float timeForMaxCharge;
    public float minFactor;
    public float maxThrowDelay;

    private float time;
    private bool isCharging;
    private float throwDelay;

    public override void PrimaryAction(InputAction.CallbackContext context, Player player) {
        base.PrimaryAction(context, player);

        // check if we can charge snowball
        /* ItemData data = player.Data; */ // idk what this does honestly lol
        if (!context.canceled) {
            if (throwDelay == 0.0f) isCharging = true;
            return;
        }

        // release charge, throw snowball
        if (isCharging && context.canceled) {
            isCharging = false;

            // charge value between 0 and 1 for charge
            float charge = Mathf.Clamp01(time / timeForMaxCharge);
            
            // actual force percentage value, between minFactor and 1f
            float forcePercentage = Mathf.Lerp(minFactor, 1f, charge);

            ProjectileShooter shooter = player.GetComponent<ProjectileShooter>();
            shooter.data = (SnowballItemData)player.EquippedItem.Data;
            shooter.Shoot(forcePercentage);

            throwDelay = maxThrowDelay * charge;
            time = 0;
            player.RemoveItem(player.Equipped, 1);
            return;
        }
    }

    public override void Update(Player player) {
        if (isCharging) {
            time += Time.deltaTime;
        } else if (throwDelay > 0.0f) {
            throwDelay -= Time.deltaTime * 1.0f;
        }

        throwDelay = Mathf.Clamp(throwDelay, 0.0f, maxThrowDelay);

        float charge = Mathf.Clamp01(time / timeForMaxCharge);
        float s = 16.0f;
        //swayOffset = new Vector3(Mathf.PerlinNoise1D(Time.time * s), Mathf.PerlinNoise1D(Time.time * s - 12.31546f), Mathf.PerlinNoise1D(Time.time * s + 3.5654f)) * charge * 0.035f;
        //swayOffset += (-Vector3.forward + Vector3.right * 0.2f) * charge * 0.125f;

        UIMaster.Instance.inGameHUD.UpdateChargeMeter(isCharging ? charge : Mathf.InverseLerp(0.0f, maxThrowDelay, throwDelay));
    }

    public override void Unequipped(InputAction.CallbackContext context, Player player) {
        base.Unequipped(context, player);
        isCharging = false;
        time = 0f;
        UIMaster.Instance.inGameHUD.UpdateChargeMeter(0f);
    }
}
