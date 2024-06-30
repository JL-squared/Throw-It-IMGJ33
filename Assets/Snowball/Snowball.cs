using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Snowball : MonoBehaviour {
    private Rigidbody rb;
    [HideInInspector]
    public SnowballItemData dataParent;
    Vector3 snowballThrowerPos;
    Collider snowballThrowerCollider;
    Vector3 lastVel;
    new Collider collider;

    public void ApplySpawn(Vector3 pos, Vector3 velocity, SnowballThrower snowballThrower) {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        rb.velocity = velocity;
        rb.position = pos;
        transform.position = pos;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.rotation = UnityEngine.Random.rotation;
        Destroy(gameObject, dataParent.lifetime);
        if (snowballThrower.collider != null) {
            snowballThrowerPos = snowballThrower.transform.position;
            snowballThrowerCollider = snowballThrower.collider;
            Physics.IgnoreCollision(collider, snowballThrowerCollider, true);
        }
    }

    public void FixedUpdate() {
        // fix for inconsistent rb.velocity during OnCollisionEnter, probably
        // because unity handles collision response before actually calling OnCollisionEnter, so the velocity
        // gets set to a lower number before OnCollisionEnter. Fix works really well
        lastVel = rb.velocity;
    }

    public void OnTriggerEnter(Collider other) {
        EntityHealth health = other.gameObject.GetComponent<EntityHealth>();

        Vector3 entityVelocity = Vector3.zero;
        EntityMovement movement = other.gameObject.GetComponent<EntityMovement>();
        if (movement != null) {
            entityVelocity = movement.cc.velocity;
        }

        if (health != null) {
            health.Damage(1.5f * (lastVel - entityVelocity).magnitude * dataParent.damageFactor);
        }

        Destroy(gameObject);
        GameObject prts = Instantiate(dataParent.particles);
        prts.transform.position = transform.position;

        if (lastVel.magnitude > 0.1) {
            //prts.transform.rotation = Quaternion.LookRotation(-lastVel);

            var system = prts.GetComponent<ParticleSystem>();
            var velOverLifetime = system.velocityOverLifetime;
            Vector3 vel = lastVel * 0.4f;
            velOverLifetime.x = new ParticleSystem.MinMaxCurve(vel.x);
            velOverLifetime.y = new ParticleSystem.MinMaxCurve(vel.y);
            velOverLifetime.z = new ParticleSystem.MinMaxCurve(vel.z);
        }
    }

    public void Update() {
        if (snowballThrowerCollider != null && Vector3.Distance(transform.position, snowballThrowerPos) >= 2.0f) {
            Physics.IgnoreCollision(collider, snowballThrowerCollider, false);
        }
    }
}
