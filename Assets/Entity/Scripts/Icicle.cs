using UnityEngine;

public class Icicle : Projectile {
    bool hit = false;

    public override void Spawned(Vector3 pos, Vector3 velocity, ProjectileShooter shooter) {
        base.Spawned(pos, velocity, shooter);
        rb.useGravity = false;
        hit = false;
        if (velocity.magnitude > 0.01) {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }

    private void FixedUpdate() {
        if (!hit) {
            rb.AddForce(Physics.gravity * 2.2f, ForceMode.Acceleration);
            rb.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    protected override bool ShouldCollideWith(Collider other) {
        if (other.gameObject.GetComponent<Icicle>() != null) {
            return false;
        }

        return base.ShouldCollideWith(other);
    }

    protected override void OnHit(Collider other, Vector3 relativeVelocity) {
        if (hit)
            return;

        base.OnHit(other, relativeVelocity);

        Quaternion rot = rb.rotation;
        rb.isKinematic = true;
        transform.rotation = rot;
        rb.freezeRotation = false;
        rb.rotation = rot;
        hit = true;

        EntityHealth health = other.gameObject.GetComponent<EntityHealth>();
        if (health != null) {
            health.Damage(2f);
            rb.interpolation = RigidbodyInterpolation.None;
            transform.SetParent(health.transform, true);
        }
    }
}
