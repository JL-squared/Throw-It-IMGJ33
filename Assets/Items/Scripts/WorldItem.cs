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
}
