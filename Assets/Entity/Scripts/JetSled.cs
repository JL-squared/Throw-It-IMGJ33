using TMPro;
using Tweens;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class JetSled : Vehicle {
    public float throttle;
    public float targetThrottle;
    public float targetSpeed = 20f;
    public float reachTargetSpeedTime = 0.1f;
    public float reachTargetAngleTime = 0.2f;
    public float perpendicularFactor = 0.25f;
    public float force = 700f;
    public float angle = 45f;
    private float currentAngle;
    
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

        float dotted = 1f - (Mathf.Clamp01(Vector3.Dot(rb.velocity.normalized, transform.forward)) * Mathf.Clamp01(rb.velocity.magnitude / targetSpeed));
        targetThrottle = mv.y * dotted + Mathf.Abs(mv.x) * 0.5f;

        var tween = new FloatTween {
            from = this.throttle,
            to = targetThrottle,
            duration = reachTargetSpeedTime,
            onUpdate = (instance, value) => {
                throttle = value;
            },
            easeType = EaseType.SineIn,
        };
        gameObject.AddTween(tween);

        var angleTween = new FloatTween {
            from = this.currentAngle,
            to = -mv.x * angle,
            duration = reachTargetAngleTime,
            onUpdate = (instance, value) => {
                currentAngle = value;
            },
            easeType = EaseType.SineIn,
        };
        gameObject.AddTween(angleTween);

        float effective = Mathf.Clamp01(throttle);
        rb.AddForceAtPosition(effective * thrusterRight.forward * force, thrusterRight.position);
        rb.AddForceAtPosition(effective * thrusterLeft.forward * force, thrusterLeft.position);

        float aaa = Vector3.Dot(transform.right, rb.velocity.normalized);
        rb.AddForce(-aaa * transform.right * perpendicularFactor, ForceMode.VelocityChange);


        thrusterLeft.localEulerAngles = new Vector3(0f, currentAngle, 0f);
        thrusterRight.localEulerAngles = new Vector3(0f, currentAngle, 0f);
        tmp.text = $"{rb.velocity.magnitude:F1}m/s\n{effective * 100:F1}%";
    }
}
