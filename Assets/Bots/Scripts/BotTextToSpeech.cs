using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static TextToSpeech;

public class BotTextToSpeech : MonoBehaviour {
    public TextToSpeech tts;
    public float jitterFactor;
    public Transform[] mouthBeads;
    private Vector3[] startingPositions;

    public void Start() {
        startingPositions = new Vector3[mouthBeads.Length];
        for (int i = 0; i < mouthBeads.Length; i++) {
            startingPositions[i] = mouthBeads[i].localPosition;
        }

        tts.OnRawVolumeUpdate += OnVolumeUpdate;
    }

    public void SayString(string s, DeltaAttribs deltas = new DeltaAttribs(), float srcVolume = 1.0f, float srcPitch = 1.0f) {
        tts.SayString(s, deltas, srcVolume, srcPitch);
    }

    private void OnVolumeUpdate(float volume) {
        for (int i = 0; i < mouthBeads.Length; i++) {
            mouthBeads[i].localPosition = startingPositions[i] + Vector3.Scale(new Vector3(0.1f, 0.1f, 1f), Random.insideUnitSphere * volume * jitterFactor);
        }
    }
}
