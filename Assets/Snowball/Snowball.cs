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

    private float CalculateDamage() {
        return 3.0f * rb.velocity.magnitude;
    }

    public void OnCollisionEnter(Collision collision) {
        EntityHealth health = collision.gameObject.GetComponent<EntityHealth>();
        if (health != null) {
            health.Damage(CalculateDamage());
        }

        Destroy(gameObject);
        GameObject prts = Instantiate(particles);
        prts.transform.position = transform.position;

        if (collision.impulse.magnitude > 0.1) {
            prts.transform.rotation = Quaternion.LookRotation(collision.impulse);

            var system = prts.GetComponent<ParticleSystem>();
            var velOverLifetime = system.velocityOverLifetime;
            Vector3 vel = rb.velocity * 0.4f;
            velOverLifetime.x = new ParticleSystem.MinMaxCurve(vel.x);
            velOverLifetime.y = new ParticleSystem.MinMaxCurve(vel.y);
            velOverLifetime.z = new ParticleSystem.MinMaxCurve(vel.z);
        }
    }

    public void Update() {
        if(snowballThrowerCollider != null && Vector3.Distance(transform.position, snowballThrowerPos) >= 2.0f) {
            Physics.IgnoreCollision(collider, snowballThrowerCollider, false);
        }
    }
}
