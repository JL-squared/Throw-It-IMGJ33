using UnityEngine;

public class Crate : MonoBehaviour {
    private float lastMagnitude;
    private Vector3 lastVelocity;
    private Vector3 lastAngularVelocity;
    public Vector3[] positions;


    public void FixedUpdate() {
        var rb = GetComponent<Rigidbody>();
        lastMagnitude = rb.linearVelocity.magnitude;
        float factorino = Vector3.Distance(lastVelocity, rb.linearVelocity) + Vector3.Distance(lastAngularVelocity, rb.angularVelocity) * 0.25f;

        lastVelocity = rb.linearVelocity;
        lastAngularVelocity = rb.angularVelocity;

        if (factorino > 1) {
            float effector = Mathf.Clamp01(factorino / 30f);
            Player.Instance.cameraShake.ShakeCamera(0.5f * effector, effector, transform.position);
        }

        if (factorino > 10) {
            BotBase.Summon(Registries.bots["tall"], transform.position, Quaternion.identity);
            BotBase.Summon(Registries.bots["base"], transform.position, Quaternion.identity);
            Destroy(gameObject);
            //Registries.Summon("bots:base", null);
        }
    }

}
