using UnityEngine;

public class PlayerCameraShake : PlayerBehaviour {
    public float damageSmoothinSpeed;
    public float damagedDirectionFactor;
    public GameObject shiverer;
    private Quaternion damageRotation;
    private Quaternion tiltStrafe;
    public float tiltStrafeStrength;
    public float tiltStrafeSmoothingSpeed;
    private float horizontal;

    private void Start() {
        player.health.health.OnDamaged += Health_OnDamaged;
        damageRotation = Quaternion.identity;
    }

    private void Health_OnDamaged(float damage, EntityHealth.DamageSourceData data) {
        Vector3 diff = data.direction;
        diff.y = 0;
        diff.Normalize();
        float xValue = -damagedDirectionFactor * Vector3.Dot(diff, transform.forward);
        float zValue = damagedDirectionFactor * Vector3.Dot(diff, transform.right);
        damageRotation *= Quaternion.Euler(xValue, 0f, zValue);
    }

    private void Update() {
        Vector3 velocity = player.movement.inner.Velocity;
        velocity.y = 0f;
        velocity = Vector3.ClampMagnitude(velocity / player.movement.inner.speed, 1);
        horizontal = Mathf.Lerp(horizontal, -Vector3.Dot(player.transform.right, velocity), Time.deltaTime * tiltStrafeSmoothingSpeed);


        tiltStrafe = Quaternion.Euler(0f, 0f, horizontal * tiltStrafeStrength);
        damageRotation = Quaternion.Lerp(damageRotation, Quaternion.identity, Time.deltaTime * damageSmoothinSpeed);
        shiverer.transform.localRotation = damageRotation * tiltStrafe;
    }
}
