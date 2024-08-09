using System;
using UnityEngine;

public class EntityHealth : MonoBehaviour {
    public float maxHealth;
    public float health;

    public bool DeleteOnKill;
    public delegate void Killed();
    public event Killed OnKilled;

    public delegate void HealthChanged(float percentage);
    public event HealthChanged OnHealthChanged;

    public delegate void HealthDamaged(float damage);
    public event HealthDamaged OnDamaged;

    public delegate void HealthHealed(float healing);
    public event HealthDamaged OnHealed;

    public delegate void PreDamageModifier(ref float damage);
    public event PreDamageModifier OnPreDamageModifier;

    [HideInInspector]
    public bool AlreadyKilled { get; private set; }

    public void Start() {
        AlreadyKilled = false;
        health = maxHealth;
        if (DeleteOnKill) OnKilled += () => { Destroy(gameObject); };
    }

    public void Damage(float damage) {
        if (AlreadyKilled)
            return;

        OnPreDamageModifier?.Invoke(ref damage);
        health = Mathf.Clamp(health - damage, 0, maxHealth);
         
        OnDamaged?.Invoke(damage);
        OnHealthChanged?.Invoke(health / maxHealth);
        if (health == 0) {
            AlreadyKilled = true;
            OnKilled?.Invoke();
        }
    }

    public bool Heal(float healing) {
        float healthCpy = health;
        health = Mathf.Clamp(health + healing, 0, maxHealth);

        float effectiveHealing = health - healthCpy;

        if (effectiveHealing > 0) {
            OnHealed?.Invoke(effectiveHealing);
            OnHealthChanged?.Invoke(health / maxHealth);
        }

        return effectiveHealing > 0;
    }
}