using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Thanks to kurtdekker for converting this to c# and making it work with Unity
// Original repo: https://github.com/kurtdekker/UnitySAM
public class TextToSpeech: MonoBehaviour {
    public int speed = 72;
    public int pitch = 64;
    public int mouth = 128;
    public int throat = 128;
    public bool sing = false;
    private AudioSource source;

    public delegate void RawVolumeUpdate(float volume);
    public RawVolumeUpdate OnRawVolumeUpdate;

    public struct DeltaAttribs {
        public int speed;
        public int pitch;
        public int mouth;
        public int throat;
    }

    private float updateStep = 0.1f;
    private int sampleDataLength = 256;
    private float currentUpdateTime = 0f;
    private float clipLoudness;
    private float[] clipSampleData;

    public void Start() {
        source = GetComponent<AudioSource>();
        clipSampleData = new float[sampleDataLength];
    }

    public void Update() {
        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep && source.clip != null) {
            if (source.timeSamples + sampleDataLength > source.clip.samples) {
                return;
            }

            currentUpdateTime = 0f;

            // TODO: Error coming from here. Dunno why...
            //Debug.Log("L: " + clipSampleData.Length + ", TS: " + source.timeSamples + "S: " + source.clip.samples);
            source.clip.GetData(clipSampleData, source.timeSamples);

            if (source.timeSamples > 0) {
                clipLoudness = 0f;
                foreach (var sample in clipSampleData) {
                    clipLoudness += Mathf.Abs(sample);
                }
                clipLoudness /= sampleDataLength;
            } else {
                clipLoudness = 0f;
            }

            OnRawVolumeUpdate?.Invoke(clipLoudness);
        }
    }

    public void SayString(string s, DeltaAttribs deltas = new DeltaAttribs(), float srcVolume = 1.0f, float srcPitch = 1.0f, bool overwritePlaying = true) {
        if (source.isPlaying && overwritePlaying) {
            source.Stop();
            source.timeSamples = 0;
        } else if (source.isPlaying) {
            return;
        }

        UnitySAM.SetSpeed(speed + deltas.speed);
        UnitySAM.SetPitch(pitch + deltas.pitch);
        UnitySAM.SetMouth(mouth + deltas.mouth);
        UnitySAM.SetThroat(throat + deltas.throat);
        UnitySAM.SetSingMode(sing);

        if (!s.EndsWith('.')) {
            s += ".";
        }

        int[] ints = null;
        string output = UnitySAM.TextToPhonemes(s + "[", out ints);
        UnitySAM.SetInput(ints);

        var buf = UnitySAM.SAMMain();
        if (buf == null) {
            Debug.LogError("Buffer was null");
            return;
        }

        AudioClip clip = AudioClip.Create("SamClip", buf.GetSize(), 1, 22050, false);
        clip.SetData(buf.GetFloats(), 0);
        source.clip = clip;
        source.volume = srcVolume;
        source.pitch = srcPitch;
        source.Play();
    }
}
