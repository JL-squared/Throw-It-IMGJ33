using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BombHead : BotBehaviour {
    public float timer = 10;
    public float boutToBlowTimer = 2;
    private bool boutaBlow = false;
    public float editStrength;
    public float force;
    public float damage;
    public float radius;
    public float minDamageRadius;
    public float editRadiusOffset;
    public GameObject particles;
    public AnimationCurve explosionProfile;

    // one must assume that the array profile is something constant across all instances (please let it be so)
    private static NativeArray<float> arrayProfile;

    public void Start() {
        /*
        botTts.tts.onSpeechCutoff += (string a, out string b) => {
            b = "Nevermind lol";
        };
        */

        if (arrayProfile == null || !arrayProfile.IsCreated) {
            arrayProfile = new NativeArray<float>(256, Allocator.Persistent);

            for (int i = 0; i < 256; i++) {
                float x = (float)i / 256.0f;
                arrayProfile[i] = explosionProfile.Evaluate(x);
            }
        }
    }

    public void Stop() {
        if (arrayProfile != null && arrayProfile.IsCreated) {
            arrayProfile.Dispose();
        }
    }

    public void Update() {
        timer -= Time.deltaTime;

        if (timer <= 0) {
            Destroy(botBase.gameObject);
            IVoxelEdit edit = new ProfiledExplosionVoxelEdit {
                center = transform.position,
                strength = editStrength,
                material = 0,
                radius = radius + editRadiusOffset,
                values = arrayProfile
            };

            if (VoxelTerrain.Instance != null) {
                VoxelTerrain.Instance.ApplyVoxelEdit(edit, neverForget: true, symmetric: false, immediate: true);
            }

            Vector3 explosionCenter = botBase.transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionCenter, radius);
            foreach (Collider collider in colliders) {
                var rb = collider.gameObject.GetComponent<Rigidbody>();
                var health = collider.gameObject.GetComponent<EntityHealth>();
                var movement = collider.gameObject.GetComponent<EntityMovement>();

                if (rb != null) {
                    rb.AddExplosionForce(force * 100f, explosionCenter, radius);
                }

                if (health != null) {
                    float dist = Vector3.Distance(explosionCenter, collider.transform.position);

                    // 1 => closest to bomb
                    // 0 => furthest from bomb 
                    float factor = 1 - Mathf.Clamp01(math.unlerp(minDamageRadius, radius, dist));
                    health.Damage(factor * damage);
                }

                if (movement != null) {
                    movement.ExplosionAt(explosionCenter, force, radius);
                }
            }

            DebugUtils.DrawSphere(explosionCenter, radius, Color.red, 1000);

            Instantiate(particles, transform.position, Quaternion.identity);
        }

        if (timer < boutToBlowTimer && !boutaBlow) {
            boutaBlow = true;
            botTts.SayString("I am about to blow up. Indeed so. Get ready. Kabooeoeoeoeom");
        } else if (!boutaBlow) {
            botTts.SayString("laalalallaaaalalalaa", overwritePlaying: false);
        }
    }
}
