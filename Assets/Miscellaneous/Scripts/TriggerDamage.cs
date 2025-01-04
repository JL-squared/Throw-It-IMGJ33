using UnityEngine;

public class TriggerDamage : MonoBehaviour {
    public float damage;

    private void OnTriggerEnter(Collider other) {
        EntityHealth health = other.gameObject.GetComponent<EntityHealth>();
        if (health != null) {
            health.Damage(damage);
        }
    }
}
