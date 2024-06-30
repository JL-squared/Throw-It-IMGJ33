using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowballLogic : EquippedItemLogic {
    private float currentCharge;
    private bool isCharging;
    public float chargeSpeed;
    public float minCharge;
    public float maxThrowDelay;
    public float throwDelay;


    public override void PrimaryAction(PlayerScript player, bool pressed) {
        base.PrimaryAction(player, pressed);

        // check if we can charge snowball
        ItemData data = equippedItem.Data;
        if (pressed) {
            if (throwDelay == 0.0f) isCharging = true;
            return;
        }

        // release charge, throw snowball
        if (isCharging && !pressed) {
            isCharging = false;
            
            SnowballThrower thrower = player.GetComponent<SnowballThrower>();
            thrower.itemData = (SnowballItemData)equippedItem.Data;
            thrower.Throw(currentCharge);

            throwDelay = maxThrowDelay * currentCharge;
            currentCharge = minCharge;
            player.RemoveItem(player.Selected, 1);
            return;
        }
    }

    private void Update() {
        if (isCharging) {
            currentCharge += Time.deltaTime * chargeSpeed;
            currentCharge = Mathf.Clamp01(currentCharge);
        } else if (throwDelay > 0.0f) {
            throwDelay -= Time.deltaTime * 1.0f;
        }

        throwDelay = Mathf.Clamp(throwDelay, 0.0f, maxThrowDelay);

        UIMaster.Instance.inGameHUD.UpdateChargeMeter(isCharging ? Mathf.InverseLerp(.2f, 1.0f, currentCharge) : Mathf.InverseLerp(0.0f, maxThrowDelay, throwDelay));
    }

    public override void Unequipped(PlayerScript player) {
        base.Unequipped(player);
        isCharging = false;
        currentCharge = minCharge;
        UIMaster.Instance.inGameHUD.UpdateChargeMeter(0f);
    }

    public override void Equipped(PlayerScript player) {
        base.Equipped(player);
        currentCharge = minCharge;
    }
}
