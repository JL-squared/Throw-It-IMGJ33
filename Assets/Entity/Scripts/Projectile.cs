using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class Projectile : MonoBehaviour {
    protected ProjectileItemData data;
    protected Rigidbody rb;
    protected Vector3 shooterPosition;
    protected Collider shooterCollider;
    protected new Collider collider;

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

    protected virtual void OnHit(Collider other, Vector3 relativeVelocity) { }
    public void OnTriggerEnter(Collider other) {
        if (other.isTrigger)
            return;

        EntityMovement movement = other.gameObject.GetComponent<EntityMovement>();

        Vector3 entityVelocity = movement != null ? movement.cc.velocity : Vector3.zero;

        Rigidbody otherRb = other.gameObject.GetComponent<Rigidbody>();
        entityVelocity = otherRb != null ? otherRb.velocity : entityVelocity;

        if (otherRb != null) {
            otherRb.AddForceAtPosition(rb.velocity * 0.2f, rb.position, ForceMode.Impulse);
        }

        if (movement != null) {
            movement.AddImpulse(rb.velocity * 0.5f);
        }

        OnHit(other, entityVelocity - rb.velocity);
    }

    public void Update() {
        if (shooterCollider != null && Vector3.Distance(transform.position, shooterPosition) >= 2.0f) {
            Physics.IgnoreCollision(collider, shooterCollider, false);
        }
    }
}
