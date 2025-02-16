using System;
using Tweens;
using UnityEngine;

public class Cannon : MonoBehaviour {
    public Vector3 target;
    public Vector2 spread;
    public Transform bottomRotator;
    public Transform hingeRotator;
    public Transform coolThingy;
    public GameObject cratePrefab;

    public float maxHeight;
    public float targetDistance;
    public float bottomAngle;
    public float hingeAngle;
    private Vector2 forward;
    public float speed;

    public float cratePerSecond;
    private float nextTime;

    // Why was this way harder than I could've guessed? Am I fucking stupid???
    // https://www.desmos.com/calculator/3packkgce3
    public float Calc(float x) {
        float height = transform.position.y - target.y;
        float t = targetDistance;

        float m = t * (1f + Mathf.Sqrt(1 + height / (maxHeight - height))) / 2f;
        float p = m / t;
        float val = (maxHeight - height) * (-(p * x - t) * p * x) / (t*t / 4f) + height;

        return val;
    }

    public void Update() {
        target = Player.Instance.transform.position;
        forward = new Vector2(target.x - transform.position.x, target.z - transform.position.z);
        targetDistance = forward.magnitude;
        forward.Normalize();
        bottomAngle = -Vector2.SignedAngle(new Vector2(0f, 1f), forward);
        bottomRotator.localRotation = Quaternion.Euler(0f, 0f, bottomAngle);
        hingeRotator.localRotation = Quaternion.Euler(hingeAngle, 0f, 0f);

        hingeAngle = -Mathf.Rad2Deg * Mathf.Atan((Calc(0.5f) - transform.position.y + target.y) / 0.5f);

        if (Time.time > nextTime && !GameManager.Instance.paused) {
            nextTime = Time.time + 1f/cratePerSecond;

            target.x += UnityEngine.Random.Range(-1f, 1f) * spread.x;
            target.z += UnityEngine.Random.Range(-1f, 1f) * spread.y;
            forward = new Vector2(target.x - transform.position.x, target.z - transform.position.z);
            targetDistance = forward.magnitude;
            forward.Normalize();

            GameObject obj = Instantiate(cratePrefab, transform.position, Quaternion.identity);

            Vector3 flattened = new Vector3(transform.position.x, target.y, transform.position.z);
            Vector3[] points = new Vector3[32];
            for (int i = 0; i < 32; i++) {
                float dist = targetDistance * ((float)i / 32.0f);
                Vector3 tempForward = new Vector3(forward.x, 0f, forward.y).normalized;
                points[i] = flattened + Calc(dist) * Vector3.up + tempForward * dist;
            }

            obj.GetComponent<Crate>().positions = points;
            obj.GetComponent<Crate>().PlayVisual(speed);

            coolThingy.gameObject.AddTween(new LocalPositionYTween {
                from = 0f,
                to = 1f,
                duration = 0.1f,
                easeType = EaseType.ExpoIn,
                onEnd = (TweenInstance<Transform, float> instance) => {
                    coolThingy.gameObject.AddTween(new LocalPositionYTween {
                        from = 1f,
                        to = 0f,
                        duration = 0.4f,
                        easeType = EaseType.SineOut,
                    });
                },
            });

            Player.Instance.cameraShake.ShakeCamera(0.2f, 2.0f, transform.position);

        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(target, 2.0f);

        Vector3 flattened = new Vector3(transform.position.x, target.y, transform.position.z);
        Vector3[] points = new Vector3[32];
        for (int i = 0; i < 32; i++) {
            float dist = targetDistance * ((float)i / 32.0f);
            Vector3 tempForward = new Vector3(forward.x, 0f, forward.y).normalized;
            points[i] = flattened + Calc(dist) * Vector3.up + tempForward * dist;
        }
        Gizmos.DrawLineStrip(new ReadOnlySpan<Vector3>(points), false);

        Vector3 spreadNation = new Vector3(spread.x, 1, spread.y);
        Gizmos.DrawWireCube(target, spreadNation);
    }
}
