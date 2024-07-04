using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworkLauncher : BotBehaviour {
    public GameObject fireworkPrefab;
    public Transform[] slots;
    public float actionsPerSecond;
    public float distTargetThrustDivider = 3f;

    private float nextActionTime;
    private bool loaded;
    public Vector3 fwLookAtPls;
    public AnimationCurve thrust;
    public AnimationCurve rotationLockin;

    private void Update() {
       if (Time.time > nextActionTime) {
            nextActionTime = Time.time + 1f / actionsPerSecond;

            if (loaded) {
                Burst();
            } else {
                Reload();
            }
       }

        fwLookAtPls = lastTargetPosition;
    }

    private void Reload() {
        loaded = true;
        foreach (var slot in slots) {
            GameObject instance = Instantiate(fireworkPrefab, slot);
        }
    }

    private void Burst() {
        loaded = false;
        foreach (var slot in slots) {
            if (slot.childCount > 0) {
                GameObject obj = slot.GetChild(0).gameObject;
                obj.transform.SetParent(null);
                Firework firework = obj.GetComponent<Firework>();
                firework.launcher = this;
                firework.launched = true;
                firework.thrust *= Mathf.Clamp(Vector3.Distance(transform.position, fwLookAtPls) / distTargetThrustDivider, 0.25f, 2);
                firework.LaunchedBruh();
            }
        }
    }
}
