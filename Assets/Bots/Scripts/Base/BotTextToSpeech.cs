using System.Collections.Generic;
using UnityEngine;
using static TextToSpeech;

public class BotTextToSpeech : MonoBehaviour {
    public TextToSpeech tts;
    public float jitterFactor;
    private List<Transform> mouthBeads;
    private Vector3[] startingPositions;

    private void GetMouthBeads(Transform face) {
        for (int i = 0; i < face.childCount; i++) {
            Transform child = face.GetChild(i);
            if (child.name.Contains("Mouth")) {
                mouthBeads.Add(child);
            }
        }
    }

    public void Start() {
        mouthBeads = new List<Transform>();
        BotBase bot = GetComponent<BotBase>();
        GetMouthBeads(bot.happyFace.transform);
        GetMouthBeads(bot.angryFace.transform);

        startingPositions = new Vector3[mouthBeads.Count];
        for (int i = 0; i < mouthBeads.Count; i++) {
            startingPositions[i] = mouthBeads[i].localPosition;
        }

        tts.OnRawVolumeUpdate += OnVolumeUpdate;
    }

    public void SayString(string s, DeltaAttribs deltas = new DeltaAttribs(), float srcVolume = 1.0f, float srcPitch = 1.0f, bool overwritePlaying = true) {
        //Debug.Log("say: " + s);
        deltas.pitch = Random.Range(-10, 10);
        deltas.speed = Random.Range(-10, 10);
        tts.Say(s, deltas, srcVolume, srcPitch, overwritePlaying);
    }

    private void OnVolumeUpdate(float volume) {
        for (int i = 0; i < mouthBeads.Count; i++) {
            mouthBeads[i].localPosition = startingPositions[i] + Vector3.Scale(new Vector3(0.1f, 0.1f, 1f), Random.insideUnitSphere * volume * jitterFactor);
        }
    }
}
