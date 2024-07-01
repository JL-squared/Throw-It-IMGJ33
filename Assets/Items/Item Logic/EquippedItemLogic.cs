using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquippedItemLogic : MonoBehaviour {
    [HideInInspector]
    public Item equippedItem;
    [HideInInspector]
    public Vector3 swayOffset;
    public virtual void PrimaryAction(Player player, bool pressed) { }
    public virtual void SecondaryAction(Player player, bool pressed) { }
    public virtual void CountChanged() { }
    public virtual void Equipped(Player player) {
        swayOffset = Vector3.zero;
    }
    public virtual void Unequipped(Player player) {
        swayOffset = Vector3.zero;
    }
}
