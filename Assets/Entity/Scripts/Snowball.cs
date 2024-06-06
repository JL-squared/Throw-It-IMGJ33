using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Snowball : MonoBehaviour {
    private Rigidbody rb;
    private float lifetime = 0.0f;


    public void ApplySpawn(Vector3 pos, Vector3 velocity) {
        rb = GetComponent<Rigidbody>();
        rb.velocity = velocity;
        rb.position = pos;
        transform.position = pos;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void FixedUpdate() {
        lifetime += Time.fixedDeltaTime;
        if(lifetime > 100.0f)
            Destroy(this);
    }
}
