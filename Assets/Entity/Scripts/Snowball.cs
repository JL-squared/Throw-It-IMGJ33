using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Snowball : MonoBehaviour {
    private Rigidbody rb;
    [HideInInspector]
    public GameObject particles;

    public void ApplySpawn(Vector3 pos, Vector3 velocity) {
        rb = GetComponent<Rigidbody>();
        rb.velocity = velocity;
        rb.position = pos;
        transform.position = pos;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.rotation = UnityEngine.Random.rotation;
        Destroy(gameObject, 15.0f);
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
}
