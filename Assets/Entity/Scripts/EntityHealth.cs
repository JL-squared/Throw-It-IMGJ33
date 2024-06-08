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

    private bool alrKilled;

    public void Start() {
        alrKilled = false;
        health = maxHealth;
        if (DeleteOnKill) onKilled += () => { Destroy(gameObject); };
    }

    public void Damage(float damage) {
        health = Mathf.Clamp(health - damage, 0, maxHealth);

        onHealthUpdated?.Invoke(health / maxHealth);
        if (health == 0 && !alrKilled) {
            alrKilled = true;
            onKilled?.Invoke();
        }
    }
}