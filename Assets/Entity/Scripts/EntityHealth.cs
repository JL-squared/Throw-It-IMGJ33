using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.Rendering;

public class EntityHealth : MonoBehaviour {
    public float maxHealth;
    public float health;

    public bool DeleteOnKill;
    public delegate void Killed();
    public event Killed OnKilled;

    public delegate void HealthUpdated(float percentage);
    public event HealthUpdated OnHealthUpdated;

    public delegate void PreDamageModifier(ref float damage);
    public event PreDamageModifier OnPreDamageModifier;

    private bool alrKilled;

    public void Start() {
        alrKilled = false;
        health = maxHealth;
        if (DeleteOnKill) OnKilled += () => { Destroy(gameObject); };
    }

    public void Damage(float damage) {
        OnPreDamageModifier?.Invoke(ref damage);
        health = Mathf.Clamp(health - damage, 0, maxHealth);

        OnHealthUpdated?.Invoke(health / maxHealth);
        if (health == 0 && !alrKilled) {
            alrKilled = true;
            OnKilled?.Invoke();
        }
    }
}