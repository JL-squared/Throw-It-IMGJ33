using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionPropagator : MonoBehaviour, IInteraction
{
    public IInteraction owner;

    public bool Interactable => owner.Interactable;

    public bool Highlight => owner.Highlight;

    public GameObject GameObject => owner.GameObject;

    public void Interact(Player player) {
        owner.Interact(player);
    }

    public void StartHover(Player player) {
        owner.SetHighlight(Highlight);
    }
    public void StopHover(Player player) {
        owner.SetHighlight(false);
    }
}
