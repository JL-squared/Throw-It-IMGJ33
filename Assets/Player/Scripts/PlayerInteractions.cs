using UnityEngine;

public class PlayerInteractions : PlayerBehaviour {

    private IInteraction interaction;
    private IInteraction lastInteraction;

    private void Update() {
        if (player.movement.vehicle == null) {
            if (player.lookingAt.HasValue) {
                interaction = player.lookingAt.Value.collider.GetComponent<IInteraction>();
            } else {
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
