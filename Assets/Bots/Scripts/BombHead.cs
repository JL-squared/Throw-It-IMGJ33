using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombHead : BotWorldPart {
    public float timer = 10;
    public float boutToBlowTimer = 2;
    private bool boutaBlow = false;
    public float explosionEditStrength;
    public float explosionEditRadius;
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
                strength = -explosionEditStrength,
                material = 0,
                radius = explosionEditRadius,
                writeMaterial = false,
            };

            if (VoxelTerrain.Instance != null) {
                VoxelTerrain.Instance.ApplyVoxelEdit(edit);
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
