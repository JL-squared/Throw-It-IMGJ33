using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firework : MonoBehaviour {
    [HideInInspector]
    public FireworkLauncher launcher;
    private Rigidbody rb;
    public float thrust;
    public bool launched;
    public float rotationSpeed;
    private float activeTime;
    public float lifetime = 5f;
    private Vector3 avoidanceOffset;
    public float avoidanceStrength = 0.2f;
    public float dottedPow = 1.5f;
    public float dottedStrength = 0.4f;


    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
    }

    public void LaunchedBruh() {
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate() {
        if (!launched)
            return;

        float damn = launcher.rotationLockin.Evaluate(activeTime);
        float trhs = launcher.thrust.Evaluate(activeTime);
        activeTime += Time.fixedDeltaTime;
        Vector3 forwardDir = launcher.fwLookAtPls - rb.position;

        Vector3 targetFwd = forwardDir.normalized + avoidanceOffset * avoidanceStrength;

        float dotted = Mathf.Pow(1 - Mathf.Clamp01(Vector3.Dot(transform.forward, rb.velocity.normalized)), dottedPow);
        targetFwd += dotted * -rb.velocity.normalized * dottedStrength;


        Quaternion targetRotation = Quaternion.LookRotation(targetFwd.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime * damn);
        avoidanceOffset = Vector3.zero;
        
        rb.AddRelativeForce(new Vector3(0, 0, thrust * trhs));
    }

    private void OnTriggerStay(Collider other) {
        if (other.gameObject.GetComponent<Firework>() != null) {
            Vector3 a = (transform.position - other.transform.position);
            avoidanceOffset += a.normalized / a.magnitude;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.isTrigger || !launched || Vector3.Distance(transform.position, launcher.transform.position) < 3 || other.GetComponent<Firework>() != null)
            return;

        if (VoxelTerrain.Instance != null) {
            VoxelTerrain.Instance.ApplyVoxelEdit(new AddVoxelEdit {
                center = transform.position,
                maskMaterial = true,
                material = 0,
                radius = 3f,
                strength = 100f,
                writeMaterial = false,
            });
        }

        if (other.GetComponent<EntityHealth>() != null) {
            //other.GetComponent<EntityHealth>().Damage(5);
        }

        Destroy(gameObject);
    }
}
