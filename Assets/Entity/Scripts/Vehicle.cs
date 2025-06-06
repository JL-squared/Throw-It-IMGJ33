using UnityEngine;

public class Vehicle : MonoBehaviour, IInteraction {
    public bool Interactable => !driven;
    public bool Highlight => !driven;
    public virtual bool RotationFollows => false;
    public GameObject GameObject => gameObject;
    public Transform playerSeat;

    [HideInInspector]
    public bool driven;
    protected Player player;
    private Vector2 lastWishMovement;

    public void Interact(Player player) {
        if (player.movement.vehicle != null) {
            player.movement.ExitVehicle();
        }

        player.GetComponent<EntityMovement>().rotationIsLocal = RotationFollows;
        player.movement.vehicle = this;
        player.transform.SetParent(transform);
        player.transform.localPosition = playerSeat.localPosition;
        player.transform.localRotation = Quaternion.identity;
        player.movement.ResetMovement(true);
        ((IInteraction)this).SetHighlight(false);
        driven = true;
        player.state = Player.State.Driving;
        player.movement.inner.cc.enabled = false;
    }

    public virtual void WishMovementChanged(Vector2 newWishMovement) {
    }

    public virtual void Exit() {
        driven = false;

        if (RotationFollows) {
            player.GetComponent<EntityMovement>().rotationIsLocal = false;
        }        

        //player.ResetMovement(true);
        //player.ApplyMouseDelta(Vector2.zero);
    }

    private void Update() {
        player = Player.Instance;
        if (driven) {
            player.transform.localPosition = playerSeat.localPosition;

            Vector2 newWish = player.movement.localWishMovement.normalized;
            if (Vector2.Distance(newWish, lastWishMovement) > 0.001) {
                WishMovementChanged(newWish);
            }

            lastWishMovement = newWish;
        }
    }
}
