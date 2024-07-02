using UnityEngine;

public interface IInteraction {
    bool Interactable { get; }
    public abstract void Interact(Player player);
    public virtual void StartHover(Player player) { }
    public virtual void StopHover(Player player) { }
    GameObject GameObject { get; }
}