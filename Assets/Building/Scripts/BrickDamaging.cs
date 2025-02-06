using System.Collections.Generic;
using UnityEngine;

public class BrickDamaging : MonoBehaviour {
    public List<State> states;
    private int oldState;

    [System.Serializable]
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

            int newState = -1;
            for (int i = 0; i < states.Count; i++) {
                if (states[i].percentage <= percentage) {
                    newState = i;
                    break;
                }
            }

            if (oldState != newState && newState != -1) {
                Vector3 temp = states[newState].toggableStateMesh.transform.localScale;

                if (Random.value > 0.5) {
                    temp.x = -temp.x;
                }

                if (Random.value > 0.5) {
                    temp.y = -temp.z;
                }

                if (Random.value > 0.5) {
                    temp.z = -temp.z;
                }

                states[newState].toggableStateMesh.transform.localScale = temp;
                states[newState].toggableStateMesh.SetActive(true);
            }

            oldState = newState;
        };
    }
}
