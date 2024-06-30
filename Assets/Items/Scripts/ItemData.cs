using UnityEngine;

public abstract class ItemData : ScriptableObject {
    public int stackSize;

    public Sprite icon;
    public GameObject worldItem;

    [Header("View Model")]
    public GameObject viewModel;

    public string title;
    [TextArea(15, 20)]
    public string description;

    public override string ToString() {
        return name;
    }

    public static implicit operator string(ItemData i) => i.ToString();

    public virtual bool Interactable() { return false;  }
    public virtual void PrimaryAction(PlayerScript player) { }
    public virtual void SecondaryAction(PlayerScript player) { }
}
