using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour, IInteraction {
    public ItemData item;
    public bool Interactable => Player.Instance.CanFitItem(new Item(item, 1));

    public GameObject GameObject => gameObject;

    public void Interact(Player player) {
        Destroy(gameObject);
        player.AddItem(new Item(item, 1));
    }

    public void StartHover(Player player) {
        var children = GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children) {
            child.gameObject.layer = LayerMask.NameToLayer("Highlight");
        }
    }
    public void StopHover(Player player) {
        var children = GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children) {
            child.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}
