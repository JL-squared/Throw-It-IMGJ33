using UnityEngine;

public class Vehicle : MonoBehaviour, IInteraction {
    public bool Interactable => !driven;
    public bool Highlight => !driven;
    public GameObject GameObject => gameObject;
    public Transform playerSeat;
    public bool driven;
    protected Player player;

    public void Interact(Player player) {
        if (player.vehicle != null) {
            player.ExitVehicle();
        }

        player.vehicle = this;
        player.transform.SetParent(transform);
        player.transform.localPosition = playerSeat.localPosition;
        player.transform.localRotation = Quaternion.identity;
        player.ResetMovement();
        ((IInteraction)this).SetHighlight(false);
    }

    private void Update() {
        player = Player.Instance;
        driven = player.vehicle == this;
        if (driven) {
            player.transform.localPosition = playerSeat.localPosition;
        }
    }
}
