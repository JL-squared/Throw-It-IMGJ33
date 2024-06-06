using UnityEngine;
using UnityEngine.Rendering;

public class EntityHealth : MonoBehaviour {
    public float maxHealth;

    public bool DeleteOnKill;

    [HideInInspector]
    public float health;

    public delegate void Killed();
    public event Killed onKilled;

    public delegate void HealthUpdated(float percentage);
    public event HealthUpdated onHealthUpdated;

    public void Start() {
        health = maxHealth;
        if (DeleteOnKill) onKilled += () => { Destroy(gameObject); };
    }

    public void Damage(float damage) {
        health -= damage;

        onHealthUpdated?.Invoke(health / maxHealth);
        if (health < 0) {
            onKilled?.Invoke();
        }
    }
}