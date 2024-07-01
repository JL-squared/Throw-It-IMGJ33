using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class Projectile : MonoBehaviour {
    protected ProjectileItemData data;
    protected Rigidbody rb;
    protected Vector3 velocity;
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

    private void FixedUpdate() {
        // fix for inconsistent rb.velocity during OnCollisionEnter, probably
        // because unity handles collision response before actually calling OnCollisionEnter, so the velocity
        // gets set to a lower number before OnCollisionEnter. Fix works really well
        velocity = rb.velocity;
    }

    protected virtual void OnHit(Collider other, Vector3 relativeVelocity) { }

    public void OnTriggerEnter(Collider other) {
        if (other.isTrigger)
            return;

        EntityMovement movement = other.gameObject.GetComponent<EntityMovement>();
        Vector3 entityVelocity = movement != null ? movement.cc.velocity : Vector3.zero;

        Rigidbody otherRb = other.gameObject.GetComponent<Rigidbody>();
        entityVelocity = otherRb != null ? otherRb.velocity : Vector3.zero;

        OnHit(other, velocity - entityVelocity);
    }

    public void Update() {
        if (shooterCollider != null && Vector3.Distance(transform.position, shooterPosition) >= 2.0f) {
            Physics.IgnoreCollision(collider, shooterCollider, false);
        }
    }
}
