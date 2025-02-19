using UnityEngine;

public class Projectile : MonoBehaviour {
    protected ProjectileItemData data;
    protected Rigidbody rb;
    protected Vector3 shooterPosition;
    protected Collider shooterCollider;
    protected new Collider collider;
    protected bool justSpawnedVehicle;
    protected bool ignoringSpawner;
    protected ProjectileShooter shooter;

    public virtual void Spawned(Vector3 pos, Vector3 velocity, ProjectileShooter shooter = null) {
        if (shooter != null && shooter.collider != null) {
            shooterPosition = shooter.transform.position;
            shooterCollider = shooter.collider;            
            Physics.IgnoreCollision(collider, shooterCollider, true);
            ignoringSpawner = true;
        }

        if(shooter != null)
            this.shooter = shooter;

        gameObject.SetActive(true);
    }

    public static GameObject Spawn(ProjectileItemData data, Vector3 pos, Vector3 velocity, ProjectileShooter shooter = null) {
        GameObject spawned = Instantiate(data.projectile);
        spawned.SetActive(false);
        Projectile projectile = spawned.GetComponent<Projectile>();
        projectile.rb = projectile.GetComponent<Rigidbody>();
        projectile.rb.interpolation = RigidbodyInterpolation.Interpolate;
        projectile.collider = projectile.GetComponent<Collider>();
        projectile.rb.linearVelocity = velocity;
        projectile.transform.position = pos;
        projectile.data = data;
        Destroy(spawned, data.lifetime);
        projectile.Spawned(pos, velocity, shooter);
        return spawned;
    }

    protected virtual bool ShouldCollideWith(Collider other) {
        if (other.isTrigger)
            return false;

        if (shooterCollider != null) {
            Player player = shooterCollider.GetComponent<Player>();
            if (player != null && player.movement.vehicle != null && player.movement.vehicle.gameObject == other.gameObject) {
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
        entityVelocity = otherRb != null ? otherRb.linearVelocity : entityVelocity;

        Vector3 relativeVelocity = entityVelocity - rb.linearVelocity;
        if (otherRb != null) {
            otherRb.AddForceAtPosition(-relativeVelocity * rb.mass * data.rigidbodyForceFactor, rb.position, ForceMode.Impulse);
        }

        if (movement != null) {
            movement.AddImpulse(-relativeVelocity * 0.5f * data.knockbackFactor);
        }

        OnHit(other, relativeVelocity);
    }

    public void Update() {
        if (shooterCollider != null && Vector3.Distance(transform.position, shooterPosition) >= 2.0f && ignoringSpawner) {
            Physics.IgnoreCollision(collider, shooterCollider, false);
            collider.excludeLayers = 0;
            ignoringSpawner = false;
        }
    }
}
