using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Snowball : Projectile {
    public SnowballItemData dataParent;

    public override void Spawned(Vector3 pos, Vector3 velocity, ProjectileShooter shooter) {
        base.Spawned(pos, velocity, shooter);
        transform.rotation = UnityEngine.Random.rotation;
        dataParent = (SnowballItemData)data;
    }

    protected override void OnHit(Collider other, Vector3 relativeVelocity) {
        base.OnHit(other, relativeVelocity);

        EntityHealth health = other.gameObject.GetComponent<EntityHealth>();
        if (health != null) {
            health.Damage(1.5f * relativeVelocity.magnitude * dataParent.damageFactor);
        }

        Destroy(gameObject);
        GameObject prts = Instantiate(dataParent.particles);
        prts.transform.position = transform.position;

        if (relativeVelocity.magnitude > 0.1) {
            //prts.transform.rotation = Quaternion.LookRotation(-lastVel);

            var system = prts.GetComponent<ParticleSystem>();
            var velOverLifetime = system.velocityOverLifetime;
            Vector3 vel = relativeVelocity * 0.4f;
            velOverLifetime.x = new ParticleSystem.MinMaxCurve(vel.x);
            velOverLifetime.y = new ParticleSystem.MinMaxCurve(vel.y);
            velOverLifetime.z = new ParticleSystem.MinMaxCurve(vel.z);
        }
    }
}
