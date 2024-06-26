using System.Collections;
using System.Collections.Generic;
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

    
    public void Start() {
        botTts.tts.onSpeechCutoff += (string a, out string b) => {
            b = "Nevermind lol";
        };
    }

    public void Update() {
        timer -= Time.deltaTime;

        if (timer <= 0) {
            Destroy(botBase.gameObject);
            IVoxelEdit edit = new ExplosionVoxelEdit {
                center = transform.position,
                strength = editStrength,
                material = 0,
                radius = radius,
                jParam = 4f,
                hParam = 0.2f,
            };

            if (VoxelTerrain.Instance != null) {
                VoxelTerrain.Instance.ApplyVoxelEdit(edit);
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
                    float factor = 1 - Mathf.Clamp01(math.unlerp(0, radius, dist));
                    health.Damage(factor * damage);
                }

                if (movement != null) {
                    movement.ExplosionAt(explosionCenter, force, radius);
                }
            }

            DebugUtils.DrawSphere(explosionCenter, radius, Color.red, 1000);
        }

        if (timer < boutToBlowTimer && !boutaBlow) {
            boutaBlow = true;
            botTts.SayString("I am about to blow up. Indeed so. Get ready. Kabooeoeoeoeom");
        } else if (!boutaBlow) {
            botTts.SayString("laalalallaaaalalalaa", overwritePlaying: false);
        }
    }
}
