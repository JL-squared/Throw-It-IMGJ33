using Unity.Mathematics;
using UnityEngine;

public class Crate : MonoBehaviour {
    private Vector3 lastVelocity;
    private Vector3 lastAngularVelocity;
    public Vector3[] positions;
    private float time;
    private float time2;
    private float speed;
    private Vector3 velocity;
    private Vector3 next;
    private Vector3 current;
    private Vector3 angularVelocity;
    private Rigidbody rb;
    float test1;
    float test2;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        angularVelocity = UnityEngine.Random.onUnitSphere * 360;
    }

    public void Update() {
        if (positions != null && rb.isKinematic) {
            time2 += Time.deltaTime;

            float a = math.unlerp(test1, test2, time2);
            rb.position = Vector3.Lerp(current, next, a);
        }
    }

    public void FixedUpdate() {
        if (positions != null && rb.isKinematic) {
            time += Time.fixedDeltaTime;
            int index = (int)(time * speed);

            test1 = (float)index / speed;
            test2 = (float)(index+1) / speed;
            rb.rotation *= Quaternion.Euler(angularVelocity * Time.fixedDeltaTime);
            //Debug.Log(index);

            if (index < 30 && positions.Length > 0) {
                current = positions[index];
                next = positions[index + 1];
                //rb.position = current;
            } else if (index > 29) {
                rb.isKinematic = false;
                velocity = positions[31] - positions[30];
                rb.linearVelocity = velocity * speed;
                lastVelocity = rb.linearVelocity;
                rb.AddForce(velocity, ForceMode.VelocityChange);
            }
        }

        if (!rb.isKinematic) {
            float factorino = Vector3.Distance(lastVelocity, rb.linearVelocity) + Vector3.Distance(lastAngularVelocity, rb.angularVelocity) * 0.25f;

            lastVelocity = rb.linearVelocity;
            lastAngularVelocity = rb.angularVelocity;

            if (factorino > 1) {
                float effector = Mathf.Clamp01(factorino / 30f);
                Player.Instance.cameraShake.ShakeCamera(0.5f * effector, effector, transform.position);
            }

            if (factorino > 10) {
                BreakOpen();
            }
        }
    }

    public void BreakOpen() {
        BotBase.Summon(Registries.bots["tall"], transform.position, Quaternion.identity);
        BotBase.Summon(Registries.bots["momohsin"], transform.position, Quaternion.identity);
        BotBase.Summon(Registries.bots["belbelbel"], transform.position, Quaternion.identity);
        BotBase.Summon(Registries.bots["base"], transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void PlayVisual(float speed) {
        rb = GetComponent<Rigidbody>();
        this.speed = speed;
        rb.isKinematic = true;
    }
}
