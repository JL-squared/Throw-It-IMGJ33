using UnityEngine;

public class EvilSnowball : SnowballProjectile {
    protected override void OnHit(Collider other, Vector3 relativeVelocity) {
        base.OnHit(other, relativeVelocity);

        if (shooter.transform.tag == "PlayerTag") {
            BotBase.Summon(Registries.bots["base"], lastPosition, Quaternion.identity);
        }
    }
}
