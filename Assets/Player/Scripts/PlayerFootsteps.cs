using System;
using Tweens;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Feet?? 🤤🤤🤤
public class PlayerFootsteps : PlayerBehaviour {
    public AudioSource footsteps;

    public void Start() {
        player.movement.inner.onJumping.AddListener(() => { Utils.PlaySound(footsteps, Registries.rockJump); });
    }

    public void PlayFootStep(bool run) {
        if (run) {
            Utils.PlaySound(footsteps, Registries.rockRun);
        } else {
            Utils.PlaySound(footsteps, Registries.rockRun);
        }
    }
}
