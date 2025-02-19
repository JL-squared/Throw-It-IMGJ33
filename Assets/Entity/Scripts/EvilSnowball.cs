using UnityEngine;

public class EvilSnowball : Snowball {
    protected override void OnHit(Collider other, Vector3 relativeVelocity) {
        base.OnHit(other, relativeVelocity);

        if (shooter.transform.tag == "PlayerTag") {
            BotBase.Summon(Registries.bots["base"], transform.position, Quaternion.identity);
        }
    }
}
