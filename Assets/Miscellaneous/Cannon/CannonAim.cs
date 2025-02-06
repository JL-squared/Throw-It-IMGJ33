using JetBrains.Annotations;
using System;
using UnityEngine;

public class CannonAim : MonoBehaviour {
    public Vector3 target;
    public Transform bottomRotator;
    public Transform hingeRotator;
    public GameObject cratePrefab;

    public float targetHeight;
    public float targetDistance;
    public float bottomAngle;
    public float hingeAngle;
    private Vector2 forward;

    public float cratePerSecond;
    private float nextTime;
    
    public float Calc(float x) {
        float Internal(float x) {
            return -(x - targetDistance) * (x);
        }

        return targetHeight * Internal(x) / Internal(targetDistance / 2);
    }

    public void Update() {
        forward = new Vector2(target.x - transform.position.x, target.z - transform.position.z).normalized;
        bottomAngle = -Vector2.SignedAngle(new Vector2(0f, 1f), forward);
        targetDistance = Vector3.Distance(target, transform.position);
        bottomRotator.localRotation = Quaternion.Euler(0f, 0f, bottomAngle);
        hingeRotator.localRotation = Quaternion.Euler(hingeAngle, 0f, 0f);

        hingeAngle = -Mathf.Rad2Deg * Mathf.Atan(Calc(0.5f) / 0.5f);

        if (Time.time > nextTime) {
            nextTime = Time.time + cratePerSecond;
            //GameObject obj = Instantiate(cratePrefab);

            /*
            Vector3[] points = new Vector3[32];
            float timeStep = 0.2f;
            for (int i = 0; i < 32; i++) {
                float dist = targetDistance * timeStep;
                Vector3 tempForward = new Vector3(forward.x, 0f, forward.y).normalized;
                points[i] = transform.position + Calc(dist) * Vector3.up + tempForward * dist;
            }

            obj.GetComponent<Crate>().positions = points;
            */
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(target, 2.0f);


        Vector3[] points = new Vector3[32];
        for (int i = 0; i < 32; i++) {
            float dist = targetDistance * ((float)i / 32.0f);
            Vector3 tempForward = new Vector3(forward.x, 0f, forward.y).normalized;
            points[i] = transform.position + Calc(dist) * Vector3.up + tempForward * dist;
        }
        Gizmos.DrawLineStrip(new ReadOnlySpan<Vector3>(points), false);
    }
}
