using UnityEngine;

public class Firework : MonoBehaviour {
    public Transform mesh;
    public GameObject explosionParticles;
    public float lifetime = 5f;

    public float thrust;
    public float rotationSpeed;
    public float avoidanceStrength = 0.2f;
    public float avoidanceRadius = 3f;
    public float driftCorrectionPow = 1.5f;
    public float driftCorrectionStrength = 0.4f;
    public float targetDetectionRadius = 2f;
    public float maxRadiusNearLauncherDontBlow = 4f;

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
    private FireworkLauncher launcher;
    private bool primed;

    private float thrustFactor;
    private float rotationSpeedFactor;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        launched = false;
        primed = false;
        rb.interpolation = RigidbodyInterpolation.None;
        localRngOffset = UnityEngine.Random.value;
        GetComponent<SphereCollider>().enabled = false;
    }

    public void Launch(float thrustPercentage, FireworkLauncher launcher) {
        this.launcher = launcher;
        launched = true;
        primed = false;
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

        // TODO: I know this is bad for perf but idrc rn...
        Collider[] colliders = Physics.OverlapSphere(transform.position, avoidanceRadius);
        Vector3 avoidanceOffset = Vector3.zero;
        foreach (var item in colliders) {
            if (item.GetComponent<Firework>() != null) {
                Vector3 a = (transform.position - item.transform.position);
                if (a.magnitude > 0.001) {
                    avoidanceOffset += a.normalized / a.magnitude;
                }
            }
        }

        // Avoidance and drift correction
        Vector3 targetFwd = (forwardDir).normalized;
        targetFwd += avoidanceOffset * avoidanceStrength;
        float dotted = Mathf.Pow(1 - Mathf.Clamp01(Vector3.Dot(transform.forward, rb.linearVelocity.normalized)), driftCorrectionPow);
        targetFwd += dotted * -rb.linearVelocity.normalized * driftCorrectionStrength;

        // Noisy offset
        Vector3 localToWorldNoise = transform.TransformDirection(new Vector3(Mathf.PerlinNoise1D(noisyScale * Time.fixedTime + localRngOffset * 25.0f) - 0.5f, Mathf.PerlinNoise1D(noisyScale * Time.fixedTime + 2531.321f + localRngOffset * 25.0f) - 0.5f, 0f));
        targetFwd += localToWorldNoise * noisy;

        // Apply rotation and thrust force
        Quaternion targetRotation = targetFwd.normalized.SafeLookRotation();
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime * rotationSpeedFactor);
        rb.AddRelativeForce(new Vector3(0, 0, thrust * thrustFactor));
    }

    private void Update() {
        if (!launched)
            return;

        if (rb.linearVelocity.magnitude > 0.01) {
            mesh.rotation = rb.linearVelocity.normalized.SafeLookRotation() * Quaternion.Euler(90f, 0f, 0f);
        }

        if (launcher != null) {
            if (Vector3.Distance(transform.position, launcher.fireworkTarget) < targetDetectionRadius) {
                GoKaboom();
            }

            if (Vector3.Distance(transform.position, launcher.transform.position) > maxRadiusNearLauncherDontBlow) {
                primed = true;
                GetComponent<SphereCollider>().enabled = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (launcher == null || !launched || !primed)
            return;

        GoKaboom();
    }

    private void GoKaboom() {
        /*
        if (VoxelTerrain.Instance != null) {
            VoxelTerrain.Instance.ApplyVoxelEdit(new AddVoxelEdit {
                center = transform.position,
                maskMaterial = false,
                material = 0,
                radius = voxelEditRadius,
                strength = voxelEditStrength,
                writeMaterial = false,
                scale = new float3(1f, 1f, 1f),
            }, neverForget: true);
        }
        */

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, ~LayerMask.GetMask("Firework"));
        Utils.ApplyExplosionKnockback(transform.position, explosionRadius, colliders, explosionForce);
        Utils.ApplyExplosionDamage(transform.position, explosionRadius, colliders, minDamageRadius, damage);
        Instantiate(explosionParticles, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
