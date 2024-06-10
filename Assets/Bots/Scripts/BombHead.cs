using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombHead : BotWorldPart {
    public float timer = 10;
    public float boutToBlowTimer = 2;
    private bool boutaBlow = false;
    public float editStrength;
    public float force;
    public float damage;
    public float radius;
    private BotTextToSpeech tts;
    
    public void Start() {
        tts = botBase.GetComponent<BotTextToSpeech>();
    }

    public void Update() {
        timer -= Time.deltaTime;

        

        if (timer <= 0) {
            Destroy(botBase.gameObject);
            IVoxelEdit edit = new SphereVoxelEdit {
                center = transform.position,
                strength = -editStrength,
                material = 0,
                radius = radius,
                writeMaterial = false,
            };

            if (VoxelTerrain.Instance != null) {
                VoxelTerrain.Instance.ApplyVoxelEdit(edit);
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider collider in colliders) {
                var rb = collider.gameObject.GetComponent<Rigidbody>();
                var health = collider.gameObject.GetComponent<EntityHealth>();
                var movement = collider.gameObject.GetComponent<EntityMovement>();

                if (rb != null) {
                    rb.AddExplosionForce(force * 100f, transform.position, radius);
                }

                if (health != null) {
                    float dist = Vector3.Distance(transform.position, collider.transform.position);
                    float factor = -(dist - radius) / radius;
                    factor = Mathf.Clamp01(factor);
                    health.Damage(factor * damage);
                }

                if (movement != null) {
                    movement.ExplosionAt(transform.position, force);
                }
            }
        }

        if (timer < boutToBlowTimer && !boutaBlow) {
            boutaBlow = true;
            tts.SayString("I am about to blow up. Indeed so. Get ready. Kabooeoeoeoeom");
        } else if (!boutaBlow) {
            tts.SayString("laalalallaaaalalalaa", overwritePlaying: false);
        }
    }
}
