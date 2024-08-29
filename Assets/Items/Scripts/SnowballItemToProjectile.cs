using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowballItemToProjectile : MonoBehaviour {
    Rigidbody rb;
    public float maxSpeed;


    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        if (rb.velocity.magnitude > maxSpeed) {
            Destroy(gameObject);
            ProjectileItemData data = GetComponent<WorldItem>().item.Data as ProjectileItemData;
            Projectile.Spawn(data, transform.position, rb.velocity);
        }
    }
}
