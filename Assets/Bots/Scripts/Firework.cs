using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Firework : MonoBehaviour {
    public GameObject explosionParticles;
    public float lifetime = 5f;

    public float thrust;
    public float rotationSpeed;
    public float avoidanceStrength = 0.2f;
    public float driftCorrectionPow = 1.5f;
    public float driftCorrectionStrength = 0.4f;
    public float detectionRadius = 5f;

    public float voxelEditStrength = 100f;
    public float voxelEditRadius = 3f;

    public float minDamageRadius = 10f;
    public float damage = 10f;
    public float explosionRadius = 5f;
    public float explosionForce = 5f;

    public float noisy = 0.2f;
    public float noisyScale = 3.2f;
    public float stillTime = 0.2f;

    private Rigidbody rb;
    private float activeTime;
    private bool launched;
    private float localRngOffset;
    private Vector3 avoidanceOffset;
    private FireworkLauncher launcher;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        launched = false;
        rb.interpolation = RigidbodyInterpolation.None;
        localRngOffset = UnityEngine.Random.value;
        GetComponent<SphereCollider>().radius = detectionRadius;
    }

    public void Launch(float thrustPercentage, FireworkLauncher launcher) {
        this.launcher = launcher;
        launched = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        noisy = Mathf.Lerp(noisy, 0f, launcher.accuracy);
        avoidanceStrength = Mathf.Lerp(avoidanceStrength, 0f, launcher.accuracy);
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate() {
        if (!launched)
            return;
        activeTime += Time.fixedDeltaTime;

        float rotationSpeedFactor, thrustFactor;
        Vector3 forwardDir;

        if (launcher != null) {
            if (activeTime < stillTime) {
                rotationSpeedFactor = 0f;
                thrustFactor = 2f;
            } else {
                rotationSpeedFactor = 1f;
                thrustFactor = 1f;
            }
            
            forwardDir = launcher.fireworkTarget - rb.position;
        } else {
            thrustFactor = 1f;
            rotationSpeedFactor = 1f;
            forwardDir = Vector3.up;
        }
        

        // Avoidance and drift correction
        Vector3 targetFwd = (forwardDir - rb.velocity.normalized * 0.4f).normalized;
        targetFwd += avoidanceOffset * avoidanceStrength;
        float dotted = Mathf.Pow(1 - Mathf.Clamp01(Vector3.Dot(transform.forward, rb.velocity.normalized)), driftCorrectionPow);
        targetFwd += dotted * -rb.velocity.normalized * driftCorrectionStrength;

        // Noisy offset
        Vector3 localToWorldNoise = transform.TransformDirection(new Vector3(Mathf.PerlinNoise1D(noisyScale * Time.fixedTime + localRngOffset * 25.0f) - 0.5f, Mathf.PerlinNoise1D(noisyScale * Time.fixedTime + 2531.321f + localRngOffset * 25.0f) - 0.5f, 1f));
        targetFwd += localToWorldNoise * noisy;

        // Apply rotation and thrust force
        Quaternion targetRotation = Quaternion.LookRotation(targetFwd.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime * rotationSpeedFactor);
        avoidanceOffset = Vector3.zero;
        rb.AddRelativeForce(new Vector3(0, 0, thrust * thrustFactor));
    }

    private void OnTriggerStay(Collider other) {
        if (other.gameObject.GetComponent<Firework>() != null) {
            Vector3 a = (transform.position - other.transform.position);
            avoidanceOffset += a.normalized / a.magnitude;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (launcher == null)
            return;

        if (other.isTrigger || !launched || Vector3.Distance(transform.position, launcher.transform.position) < 3 || other.GetComponent<Firework>() != null)
            return;

        if (VoxelTerrain.Instance != null) {
            VoxelTerrain.Instance.ApplyVoxelEdit(new AddVoxelEdit {
                center = transform.position,
                maskMaterial = false,
                material = 0,
                radius = voxelEditRadius,
                strength = voxelEditStrength,
                writeMaterial = false,
            });
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.NameToLayer("Firework"));
        Utils.ApplyExplosionKnockback(transform.position, explosionRadius, colliders, explosionForce);
        Utils.ApplyExplosionDamage(transform.position, explosionRadius, colliders, minDamageRadius, damage);
        Instantiate(explosionParticles, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
