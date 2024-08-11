using UnityEngine;

public class Vehicle : MonoBehaviour, IInteraction {
    public bool Interactable => !driven;
    public bool Highlight => !driven;
    public GameObject GameObject => gameObject;
    public Transform playerSeat;

    [HideInInspector]
    public bool driven;
    protected Player player;
    private Vector2 lastWishMovement;

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
        driven = true;
    }

    public virtual void WishMovementChanged(Vector2 newWishMovement) {
    }

    public virtual void Exit() {
        driven = false;
    }

    private void Update() {
        player = Player.Instance;
        if (driven) {
            player.transform.localPosition = playerSeat.localPosition;

            Vector2 newWish = player.localWishMovement.normalized;
            if (Vector2.Distance(newWish, lastWishMovement) > 0.001) {
                WishMovementChanged(newWish);
            }

            lastWishMovement = newWish;
        }
    }
}
