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
        snowballThrowerPos = snowballThrower.transform.position;
        snowballThrowerCollider = snowballThrower.collider;
        Physics.IgnoreCollision(collider, snowballThrowerCollider, true);
    }

    private float CalculateDamage() {
        return 10.0f;
    }

    public void OnCollisionEnter(Collision collision) {
        EntityHealth health = collision.gameObject.GetComponent<EntityHealth>();
        if (health != null) {
            health.Damage(CalculateDamage());
        }

        Destroy(gameObject);
        GameObject prts = Instantiate(particles);
        prts.transform.position = transform.position;
        prts.transform.rotation = Quaternion.LookRotation(collision.impulse, Vector3.up);
    }

    public void Update() {
        if(Vector3.Distance(transform.position, snowballThrowerPos) >= 2.0f) {
            Physics.IgnoreCollision(collider, snowballThrowerCollider, false);
        }
    }
}
