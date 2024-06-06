using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Snowball : MonoBehaviour {
    private Rigidbody rb;

    public void ApplySpawn(Vector3 pos, Vector3 velocity) {
        rb = GetComponent<Rigidbody>();
        rb.velocity = velocity;
        rb.position = pos;
        transform.position = pos;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        Destroy(gameObject, 15.0f);
    }
}
