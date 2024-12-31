using UnityEngine;

public class Icicle : Projectile, IEntitySerializer {
    bool hit = false;

    public override void Spawned(Vector3 pos, Vector3 velocity, ProjectileShooter shooter) {
        base.Spawned(pos, velocity, shooter);
        rb.useGravity = false;
        hit = false;
        transform.rotation = velocity.SafeLookRotation();
    }

    private void FixedUpdate() {
        if (!hit) {
            rb.AddForce(Physics.gravity * 2.2f, ForceMode.Acceleration);
            rb.rotation = rb.linearVelocity.SafeLookRotation();
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

    public void Serialize(EntityData data) {
        data.icicleHit = hit;
    }

    public void Deserialize(EntityData data) {
        hit = data.icicleHit;
    }
}
