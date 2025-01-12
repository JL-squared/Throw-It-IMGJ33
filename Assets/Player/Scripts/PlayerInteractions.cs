using UnityEngine;

public class PlayerInteractions : PlayerBehaviour {
    [HideInInspector]
    public RaycastHit? lookingAt;
    private IInteraction interaction;
    private IInteraction lastInteraction;

    private void Update() {
        if (vehicle == null) {
            if (Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out RaycastHit info, 5f, ~LayerMask.GetMask("Player"))) {
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
                interaction.StartHover(this);
            }

            if (!lastInteraction.IsNullOrDestroyed()) {
                lastInteraction.StopHover(this);
            }
        }

        UIScriptMaster.Instance.crosshairHints.SetInteractKeyHint(interaction != null && interaction.Interactable);
        lastInteraction = interaction;
    }
}
