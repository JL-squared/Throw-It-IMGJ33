using UnityEngine;

public abstract class BotBehaviour : MonoBehaviour {
    [HideInInspector] public BotBase botBase;
    [HideInInspector] public EntityMovement movement;
    [HideInInspector] public EntityHealth bodyHealth;
    [HideInInspector] public EntityHealth headHealth;
    [HideInInspector] public BotTextToSpeech botTts;
    public virtual void AttributesUpdated() { }

    protected Vector3 lastTargetPosition;
    protected Vector3 lastTargetVelocity;
    public virtual void TargetChanged(Vector3 target, Vector3 velocity) {
        lastTargetPosition = target;
        lastTargetVelocity = velocity;
    }
}
