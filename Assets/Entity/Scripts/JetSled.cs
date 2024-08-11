using TMPro;
using Tweens;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class JetSled : Vehicle {
    public float throttle;
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

    public override void WishMovementChanged(Vector2 newWishMovement) {
        base.WishMovementChanged(newWishMovement);

        var angleTween = new FloatTween {
            from = currentAngle,
            to = -newWishMovement.x * angle,
            duration = reachTargetAngleTime,
            onUpdate = (instance, value) => {
                thrusterLeft.localEulerAngles = new Vector3(0f, value, 0f);
                thrusterRight.localEulerAngles = new Vector3(0f, value, 0f);
                currentAngle = value;
            },
            easeType = EaseType.Linear,
        };
        gameObject.AddTween(angleTween);
    }

    public void FixedUpdate() {
        if (!driven)
            return;
        Vector2 mv = player.localWishMovement.normalized;

        throttle = mv.y + Mathf.Abs(mv.x) * 0.5f;
        float effective = Mathf.Clamp01(throttle);
        rb.AddForceAtPosition(effective * thrusterRight.forward * force, thrusterRight.position);
        rb.AddForceAtPosition(effective * thrusterLeft.forward * force, thrusterLeft.position);

        float aaa = Vector3.Dot(transform.right, rb.velocity.normalized);
        rb.AddForce(-aaa * transform.right * perpendicularFactor, ForceMode.VelocityChange);

        tmp.text = $"{rb.velocity.magnitude:F1}m/s\n{effective * 100:F1}%";
    }
}
