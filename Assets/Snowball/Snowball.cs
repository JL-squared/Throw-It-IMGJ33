using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Snowball : MonoBehaviour {
    private Rigidbody rb;
    [HideInInspector]
    public GameObject particles;
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
        Destroy(gameObject, 15.0f);
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

    public void OnCollisionEnter(Collision collision) {
        EntityHealth health = collision.gameObject.GetComponent<EntityHealth>();

        Vector3 entityVelocity = Vector3.zero;
        EntityMovement movement = collision.gameObject.GetComponent<EntityMovement>();
        if (movement != null) {
            entityVelocity = movement.cc.velocity;
        }

        if (health != null) {
            health.Damage(1.5f * (lastVel - entityVelocity).magnitude);
        }

        Destroy(gameObject);
        GameObject prts = Instantiate(particles);
        prts.transform.position = transform.position;

        if (collision.impulse.magnitude > 0.1) {
            prts.transform.rotation = Quaternion.LookRotation(collision.impulse);

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
