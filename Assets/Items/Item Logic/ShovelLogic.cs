using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelLogic : EquippedItemLogic {
    public override void PrimaryAction(PlayerScript player, bool pressed) {
        base.PrimaryAction(player, pressed);

        if (player.canPickupSnow && pressed) {
            player.AddItem(new Item("snowball", 1));
        }
    }
}
