using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquippedItemLogic : MonoBehaviour {
    public Item equippedItem;
    public virtual void PrimaryAction(Player player, bool pressed) { }
    public virtual void SecondaryAction(Player player, bool pressed) { }
    public virtual void CountChanged() { }
    public virtual void Equipped(Player player) { }
    public virtual void Unequipped(Player player) { }
}
