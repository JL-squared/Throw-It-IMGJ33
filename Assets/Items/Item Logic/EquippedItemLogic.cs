using UnityEngine;

public abstract class EquippedItemLogic : MonoBehaviour {
    [HideInInspector]
    public ItemStack equippedItem;
    [HideInInspector]
    public GameObject viewModel;
    [HideInInspector]
    public Vector3 swayOffset;
    [HideInInspector]
    public Player player;
    public virtual void PrimaryAction(bool pressed) { }
    public virtual void SecondaryAction(bool pressed) { }
    public virtual void CountChanged() { }
    public virtual void Equipped() {
        swayOffset = Vector3.zero;
    }
    public virtual void Unequipped() {
        swayOffset = Vector3.zero;
        viewModel = null;
    }
}
