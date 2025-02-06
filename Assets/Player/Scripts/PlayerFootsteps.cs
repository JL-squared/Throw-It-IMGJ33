using UnityEngine;

// Feet?? 🤤🤤🤤
public class PlayerFootsteps : PlayerBehaviour {
    public AudioSource footsteps;

    public void Start() {
        player.movement.inner.onJumpStart.AddListener(() => { Utils.PlaySound(Registries.snowJump); });
        player.movement.inner.onTouchedGround.AddListener((float y) => {
            float volume = Mathf.Clamp01((-y + 2.0f) / 20.0f);
            Utils.PlaySound(Registries.rockJump, volume * 0.5f);
        });
    }

    public void PlayFootStep(bool run) {
        if (run) {
            Utils.PlaySound(Registries.rockRun);
        } else {
            Utils.PlaySound(Registries.rockWalk);
        }
    }
}
