using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class JetSled : Vehicle {
    public float throttle;
    public float targetThrottle;
    public float targetSpeed = 20f;
    public float force = 400f;
    public float angle = 60f;
    
    Rigidbody rb;
    public Transform thrusterLeft;
    public Transform thrusterRight;
    public Vector2 summed;
    public TextMeshProUGUI tmp;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }

    public void FixedUpdate() {
        if (!driven)
            return;
        Vector2 mv = player.localWishMovement.normalized;

        float targetThrottle = mv.y;
        float dotted = 1f - (Mathf.Clamp01(Vector3.Dot(rb.velocity.normalized, transform.forward)) * Mathf.Clamp01(rb.velocity.magnitude / targetSpeed));
        targetThrottle = Mathf.Clamp01(targetThrottle * dotted);
        throttle = Mathf.Lerp(throttle, targetThrottle, Time.fixedDeltaTime * 1.0f);

        rb.AddForceAtPosition(throttle * thrusterRight.forward * force, thrusterRight.position);
        rb.AddForceAtPosition(throttle * thrusterLeft.forward * force, thrusterLeft.position);

        thrusterLeft.localEulerAngles = new Vector3(0f, -mv.x, 0f) * angle;
        thrusterRight.localEulerAngles = new Vector3(0f, -mv.x, 0f) * angle;
        tmp.text = $"{rb.velocity.magnitude:F1}m/s\n{throttle*100:F1}%";
    }
}
