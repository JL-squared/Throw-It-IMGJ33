using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Icicle : Projectile {
    public override void Spawned(Vector3 pos, Vector3 velocity, ProjectileShooter shooter) {
        base.Spawned(pos, velocity, shooter);
        rb.useGravity = false;
        transform.rotation = Quaternion.LookRotation(velocity);
    }

    private void FixedUpdate() {
        rb.AddForce(Physics.gravity * 2.2f, ForceMode.Acceleration);
        rb.rotation = Quaternion.LookRotation(rb.velocity);
    }

    protected override void OnHit(Collider other, Vector3 relativeVelocity) {
        base.OnHit(other, relativeVelocity);

        EntityHealth health = other.gameObject.GetComponent<EntityHealth>();
        if (health != null) {
            health.Damage(2f);
        }
    }
}
