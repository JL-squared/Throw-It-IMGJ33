using UnityEngine;

public class LootableBotScrap : MonoBehaviour, IInteraction {
    public bool Interactable => true;
    public bool Highlight => true;
    public GameObject GameObject => gameObject;

    public void Interact(Player player) {
    }
}
