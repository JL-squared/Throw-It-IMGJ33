using UnityEngine;

public class Snowball : Projectile {
    [HideInInspector]
    public SnowballItemData dataParent;

    public override void Spawned(Vector3 pos, Vector3 velocity, ProjectileShooter shooter) {
        base.Spawned(pos, velocity, shooter);
        transform.rotation = UnityEngine.Random.rotation;
        dataParent = (SnowballItemData)data;
    }

    protected override bool ShouldCollideWith(Collider other) {
        return base.ShouldCollideWith(other) || other.gameObject.GetComponent<Snowball>() != null;
    }

    protected override void OnHit(Collider other, Vector3 relativeVelocity) {
        base.OnHit(other, relativeVelocity);

        EntityHealth health = other.gameObject.GetComponent<EntityHealth>();
        if (health != null) {
            health.Damage(relativeVelocity.magnitude * dataParent.speedFactor + dataParent.baseDamage);
        }

        Destroy(gameObject);
        GameObject prts = Instantiate(dataParent.particles);
        prts.transform.position = transform.position;

        if (relativeVelocity.magnitude > 0.1) {
            var system = prts.GetComponent<ParticleSystem>();
            var velOverLifetime = system.velocityOverLifetime;
            Vector3 vel = -relativeVelocity * 0.4f;
            velOverLifetime.x = new ParticleSystem.MinMaxCurve(vel.x);
            velOverLifetime.y = new ParticleSystem.MinMaxCurve(vel.y);
            velOverLifetime.z = new ParticleSystem.MinMaxCurve(vel.z);
        }

        Utils.PlaySound(transform.position, Registries.snowJump);
    }
}
