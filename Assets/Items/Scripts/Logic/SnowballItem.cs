using UnityEngine;
using UnityEngine.InputSystem;

public class SnowballItem : Item {
    public float timeForMaxCharge = 0.2f;
    public float minFactor = 0.2f;
    public float maxThrowDelay = 0.5f;

    private float time = 0.0f;
    private bool isCharging;
    private float throwDelay;

    public override void PrimaryAction(InputAction.CallbackContext context, Player player) {
        base.PrimaryAction(context, player);

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

    public override void EquippedUpdate(Player player) {
        if (isCharging) {
            time += Time.deltaTime;
        }

        throwDelay = Mathf.Clamp(throwDelay, 0.0f, maxThrowDelay);

        float charge = Mathf.Clamp01(time / timeForMaxCharge);
        float s = 16.0f;
        //swayOffset = new Vector3(Mathf.PerlinNoise1D(Time.time * s), Mathf.PerlinNoise1D(Time.time * s - 12.31546f), Mathf.PerlinNoise1D(Time.time * s + 3.5654f)) * charge * 0.035f;
        //swayOffset += (-Vector3.forward + Vector3.right * 0.2f) * charge * 0.125f;

        UIScriptMaster.Instance.crosshairHints.UpdateChargeMeter(isCharging ? charge : Mathf.InverseLerp(0.0f, maxThrowDelay, throwDelay));

        if (player.input.PrimaryHeld && !isCharging) {
            if (throwDelay == 0.0f) isCharging = true;
        }
    }

    public override void Update(Player player) {
        base.Update(player);
        if(!isCharging && throwDelay > 0.0f) {
            throwDelay -= Time.deltaTime * 1.0f;
        }
    }

    public override void OnWorldItemSpawned(WorldItem wi) {
        var thingy = wi.gameObject.AddComponent<SnowballItemToProjectile>();
        thingy.maxSpeed = 7;
    }

    public override void Unequipped(Player player) {
        base.Unequipped(player);
        isCharging = false;
        time = 0f;
        UIScriptMaster.Instance.crosshairHints.UpdateChargeMeter(0f);
    }
}
