using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WorldItem : MonoBehaviour, IInteraction {
    public ItemData item;
    public bool Interactable => Player.Instance.CanFitItem(new Item(item, 1));
    public bool Highlight => true;
    public GameObject GameObject => gameObject;

    public void Interact(Player player) {
        Destroy(gameObject);
        player.AddItem(new Item(item, 1));
    }

    public static void Spawn(ItemData item, Vector3 position, Quaternion rotation) {
        GameObject spawned = Instantiate(item.worldItem, position, rotation);
    }
}
