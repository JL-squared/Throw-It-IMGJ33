using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Icicle : Projectile {
    bool hit = false;

    public override void Spawned(Vector3 pos, Vector3 velocity, ProjectileShooter shooter) {
        base.Spawned(pos, velocity, shooter);
        rb.useGravity = false;
        hit = false;
        transform.rotation = Quaternion.LookRotation(velocity);
    }

    private void FixedUpdate() {
        rb.AddForce(Physics.gravity * 2.2f, ForceMode.Acceleration);

        if (!hit) {
            rb.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    protected override void OnHit(Collider other, Vector3 relativeVelocity) {
        if (hit)
            return;

        base.OnHit(other, relativeVelocity);

        EntityHealth health = other.gameObject.GetComponent<EntityHealth>();
        if (health != null) {
            health.Damage(2f);
            Destroy(gameObject);
        }

        Quaternion rot = rb.rotation;
        collider.isTrigger = false;
        rb.isKinematic = true;
        transform.rotation = rot;
        rb.freezeRotation = false;
        rb.rotation = rot;
        hit = true;
    }
}
