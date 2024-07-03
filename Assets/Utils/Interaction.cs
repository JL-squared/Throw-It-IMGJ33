using UnityEngine;

public interface IInteraction {
    bool Interactable { get; }
    bool Highlight { get; }
    public abstract void Interact(Player player);
    public virtual void StartHover(Player player) {
        SetHighlight(Highlight);
    }
    public virtual void StopHover(Player player) {
        SetHighlight(false);
    }

    public void SetHighlight(bool highlight) {
        int hl = LayerMask.NameToLayer("Highlight");
        int def = LayerMask.NameToLayer("Interaction");

        var children = GameObject.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children) {
            if (def == child.gameObject.layer || hl == child.gameObject.layer)
                child.gameObject.layer = highlight ? hl : def;
        }
    }

    GameObject GameObject { get; }
}