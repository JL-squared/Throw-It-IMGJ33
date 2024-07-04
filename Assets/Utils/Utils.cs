using Unity.Mathematics;
using UnityEngine;

public static class Utils {
    public static void ApplyExplosionKnockback(Vector3 center, float radius, Collider[] colliders, float force) {
        float rbExplosionFactor = 100f;

        foreach (Collider collider in colliders) {
            var rb = collider.gameObject.GetComponent<Rigidbody>();
            var movement = collider.gameObject.GetComponent<EntityMovement>();

            if (rb != null) {
                rb.AddExplosionForce(force * rbExplosionFactor, center, radius);
            }

            if (movement != null) {
                movement.ExplosionAt(center, force, radius);
            }
        }
    }

    public static void ApplyExplosionDamage(Vector3 center, float radius,Collider[] colliders, float minDamageRadius, float damage) {
        foreach (Collider collider in colliders) {
            var health = collider.gameObject.GetComponent<EntityHealth>();
            
            if (health != null) {
                float dist = Vector3.Distance(center, collider.transform.position);

                // 1 => closest to bomb
                // 0 => furthest from bomb 
                float factor = 1 - Mathf.Clamp01(math.unlerp(minDamageRadius, radius, dist));
                health.Damage(factor * damage);
            }
        }
    }
}
