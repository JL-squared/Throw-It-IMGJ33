using UnityEngine;

public abstract class BotBehaviour : MonoBehaviour {
    [HideInInspector] public BotBase botBase;
    [HideInInspector] public EntityMovement movement;
    [HideInInspector] public EntityHealth bodyHealth;
    [HideInInspector] public EntityHealth headHealth;
    [HideInInspector] public BotTextToSpeech botTts;
    public virtual void AttributesUpdated() { }

    public Vector3 targetPosition;
    public Vector3 targetVelocity;
    public float deathFactor;
}
