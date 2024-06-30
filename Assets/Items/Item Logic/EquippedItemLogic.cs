using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquippedItemLogic : MonoBehaviour {
    public Item equippedItem;
    public virtual void PrimaryAction(PlayerScript player, bool pressed) { }
    public virtual void SecondaryAction(PlayerScript player, bool pressed) { }
    public virtual void CountChanged() { }
    public virtual void Equipped(PlayerScript player) { }
    public virtual void Unequipped(PlayerScript player) { }
}
