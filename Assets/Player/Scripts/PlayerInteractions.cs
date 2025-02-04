using UnityEngine;

public class PlayerInteractions : PlayerBehaviour {
    [HideInInspector]
    public RaycastHit? lookingAt;
    private IInteraction interaction;
    private IInteraction lastInteraction;

    private void Update() {
        if (player.movement.vehicle == null) {
            if (Physics.Raycast(player.camera.transform.position, player.camera.transform.forward, out RaycastHit info, 5f, ~LayerMask.GetMask("Player"))) {
                GameObject other = info.collider.gameObject;
                interaction = other.GetComponent<IInteraction>();
                lookingAt = info;
            } else {
                lookingAt = null;
                interaction = null;
            }
        }

        if (!ReferenceEquals(lastInteraction, interaction) || (lastInteraction.IsNullOrDestroyed() ^ interaction.IsNullOrDestroyed())) {
            if (!interaction.IsNullOrDestroyed()) {
                interaction.StartHover(player);
            }

            if (!lastInteraction.IsNullOrDestroyed()) {
                lastInteraction.StopHover(player);
            }
        }

        UIScriptMaster.Instance.crosshairHints.SetInteractKeyHint(interaction != null && interaction.Interactable);
        lastInteraction = interaction;
    }

    public void Interact() {
        if (interaction != null && interaction.Interactable) {
            interaction.Interact(player);
        }
    }
}
