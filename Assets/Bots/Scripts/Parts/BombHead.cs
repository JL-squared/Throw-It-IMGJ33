using Unity.Collections;
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


    public void Start() {
        /*
        botTts.tts.onSpeechCutoff += (string a, out string b) => {
            b = "Nevermind lol";
        };
        */
    }

    public void Update() {
        timer -= Time.deltaTime;

        if (timer <= 0 && deathFactor == 0) {
            Destroy(botBase.gameObject);

            /*
            IVoxelEdit edit = new ProfiledExplosionVoxelEdit {
                center = transform.position,
                strength = editStrength,
                material = 0,
                radius = radius + editRadiusOffset,
                values = arrayProfile
            };

            if (VoxelTerrain.Instance != null) {
                VoxelTerrain.Instance.ApplyVoxelEdit(edit, neverForget: true, symmetric: false);
            }
            */

            Vector3 center = botBase.transform.position;
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            Utils.ApplyExplosionKnockback(center, radius, colliders, force);
            Utils.ApplyExplosionDamage(center, radius, colliders, minDamageRadius, damage);

            Instantiate(particles, transform.position, Quaternion.identity);
        }

        if (timer < boutToBlowTimer && !boutaBlow) {
            boutaBlow = true;
            //botTts.SayString("I am about to blow up. Indeed so. Get ready. Kabooeoeoeoeom");
        } else if (!boutaBlow) {
            //botTts.SayString("laalalallaaaalalalaa", overwritePlaying: false);
        }
    }
}
