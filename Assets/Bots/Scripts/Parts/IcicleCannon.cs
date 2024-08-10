using UnityEngine;

public class IcicleCannon : BotBehaviour {
    public float iciclesPerSecond;
    ProjectileShooter shooter;
    float nextTimeToShoot;

    public override void AttributesUpdated() {
        base.AttributesUpdated();
        iciclesPerSecond *= botBase.attackSpeed;
    }

    void Start() {
        shooter = GetComponent<ProjectileShooter>();
    }

    void Update() {
        if (nextTimeToShoot < Time.time && deathFactor != 1) {
            shooter.Shoot();
            nextTimeToShoot = Time.time + 1.0f / (iciclesPerSecond * (1 - deathFactor));
        }
    }
}
