using UnityEngine;

public class Laser : BotBehaviour {
    public float catchupSpeed;
    public Transform laser;
    private Quaternion currentGlobalRotation;
    public LineRenderer lineRenderer;
    private float? targetDistance;
    private float currentDistance;
    public ParticleSystem particles;

    void Update() {
        Vector3 diff = targetPosition - transform.position;
        //diff.y *= 0.4f;
        //laser.rotation = currentGlobalRotation;
        currentGlobalRotation = Quaternion.Slerp(currentGlobalRotation, Quaternion.LookRotation(diff, Vector3.up), catchupSpeed * Time.deltaTime);
        Quaternion angles = (Quaternion.Inverse(laser.parent.rotation) * currentGlobalRotation);
        laser.localRotation = angles;

        targetDistance = null;
        if (Physics.Raycast(laser.position + laser.forward, laser.forward, out RaycastHit hit, 1000f)) {
            targetDistance = hit.distance;

            var emission = particles.emission;
            if (hit.collider.GetComponent<EntityHealth>() != null) {
                // health bar kinda doesn't like per frame damage
                hit.collider.GetComponent<EntityHealth>().Damage(3 * Time.deltaTime, new EntityHealth.DamageSourceData {
                    source = gameObject,
                    direction = laser.forward
                });
                emission.enabled = false;
            } else {
                emission.enabled = true;
            }
        } else {
            var emission = particles.emission;
            emission.enabled = false;
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance.GetValueOrDefault(1000f), Time.deltaTime * 45f);

        lineRenderer.SetPosition(0, lineRenderer.transform.InverseTransformPoint(laser.position));
        Vector3 tahiniMaxxing = laser.position + laser.forward * (1 + currentDistance);
        lineRenderer.SetPosition(0, lineRenderer.transform.InverseTransformPoint(tahiniMaxxing));

        if (targetDistance != null) {
            Vector3 tahiniRizzing = laser.position + laser.forward * (1 + targetDistance.Value);
            particles.transform.position = tahiniRizzing;
        }
    }
}
