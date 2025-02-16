using System;
using UnityEngine;


public class PlayerBehaviour : MonoBehaviour {
    [HideInInspector]
    [NonSerialized]
    public Player player;

    [HideInInspector]
    [NonSerialized]
    public PlayerBobbingSway bobbing;

    [HideInInspector]
    [NonSerialized]
    public PlayerBuilding building;

    [HideInInspector]
    [NonSerialized]
    public PlayerMovement movement;

    [HideInInspector]
    [NonSerialized]
    public PlayerTemperature temperature;

    [HideInInspector]
    [NonSerialized]
    public PlayerInteractions interactions;

    [HideInInspector]
    [NonSerialized]
    public PlayerInventory inventory;

    [HideInInspector]
    [NonSerialized]
    public PlayerHealth health;

    [HideInInspector]
    [NonSerialized]
    public PlayerFootsteps footsteps;

    [HideInInspector]
    [NonSerialized]
    public PlayerCameraShake cameraShake;

    [HideInInspector]
    public Player.State state => player.state; 

    [NonSerialized]
    [HideInInspector]
    public new Camera camera;

    [HideInInspector]
    public PlayerControlsSettings settings;
}
