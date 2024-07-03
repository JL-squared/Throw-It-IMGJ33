using UnityEngine;

public class Projectile : MonoBehaviour {
    protected ProjectileItemData data;
    protected Rigidbody rb;
    protected Vector3 shooterPosition;
    protected Collider shooterCollider;
    protected new Collider collider;
    protected bool justSpawnedVehicle;

    public virtual void Spawned(Vector3 pos, Vector3 velocity, ProjectileShooter shooter) {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        rb.velocity = velocity;
        transform.position = pos;

        data = shooter.data;

        Destroy(gameObject, data.lifetime);

        if (shooter.collider != null) {
            shooterPosition = shooter.transform.position;
            shooterCollider = shooter.collider;

            Physics.IgnoreCollision(collider, shooterCollider, true);
        }
    }

    public void Start() {
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    protected virtual bool ShouldCollideWith(Collider other) {
        if (other.isTrigger)
            return false;

        if (shooterCollider != null) {
            Player player = shooterCollider.GetComponent<Player>();
            if (player != null && player.vehicle != null && player.vehicle.gameObject == other.gameObject) {
                return false;
            }
        }

        return true;
    }
    protected virtual void OnHit(Collider other, Vector3 relativeVelocity) { }
    public void OnTriggerEnter(Collider other) {
        if (!ShouldCollideWith(other))
            return;

        EntityMovement movement = other.gameObject.GetComponent<EntityMovement>();

        Vector3 entityVelocity = movement != null ? movement.Velocity : Vector3.zero;

        Rigidbody otherRb = other.gameObject.GetComponent<Rigidbody>();
        entityVelocity = otherRb != null ? otherRb.velocity : entityVelocity;

        Vector3 relativeVelocity = entityVelocity - rb.velocity;
        if (otherRb != null) {
            otherRb.AddForceAtPosition(-relativeVelocity * rb.mass * data.rigidbodyForceFactor, rb.position, ForceMode.Impulse);
        }

        if (movement != null) {
            movement.AddImpulse(-relativeVelocity * 0.5f * data.knockbackFactor);
        }

        OnHit(other, relativeVelocity);
    }

    public void Update() {
        if (shooterCollider != null && Vector3.Distance(transform.position, shooterPosition) >= 2.0f) {
            Physics.IgnoreCollision(collider, shooterCollider, false);
            collider.excludeLayers = 0;
        }
    }
}
