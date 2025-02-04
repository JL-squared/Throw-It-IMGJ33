using System;
using System.Collections.Generic;
using UnityEngine;

public class BrickDamaging : MonoBehaviour {
    public List<State> states;

    [Serializable]
    public struct State {
        public float percentage;
        public GameObject toggableStateMesh;
    }

    void Start() {
        states.Sort((a, b) => b.percentage.CompareTo(a.percentage));

        GetComponent<EntityHealth>().OnHealthChanged += (float percentage) => {
            for (int i = 0; i < states.Count; i++) {
                states[i].toggableStateMesh.SetActive(false);
            }
            Debug.Log(percentage);

            for (int i = 0; i < states.Count; i++) {
                if (states[i].percentage <= percentage) {
                    states[i].toggableStateMesh.SetActive(true);
                    break;
                }
            }
        };
    }
}
