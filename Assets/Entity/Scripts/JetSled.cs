using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;

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
        targetThrottle = targetThrottle * dotted + Mathf.Abs(mv.x) * 0.5f;
        throttle = Mathf.Lerp(throttle, targetThrottle, Time.fixedDeltaTime * 1.0f);

        float effective = Mathf.Clamp01(throttle);
        rb.AddForceAtPosition(effective * thrusterRight.forward * force, thrusterRight.position);
        rb.AddForceAtPosition(effective * thrusterLeft.forward * force, thrusterLeft.position);

        float aaa = Vector3.Dot(transform.right, rb.velocity.normalized);
        rb.AddForce(-aaa * transform.right * 0.1f, ForceMode.VelocityChange);

        thrusterLeft.localEulerAngles = new Vector3(0f, -mv.x, 0f) * angle;
        thrusterRight.localEulerAngles = new Vector3(0f, -mv.x, 0f) * angle;
        tmp.text = $"{rb.velocity.magnitude:F1}m/s\n{effective * 100:F1}%";
    }
}
