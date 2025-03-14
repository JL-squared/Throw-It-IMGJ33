using Unity.Mathematics;
using UnityEngine;

public class FireworkLauncher : BotBehaviour {
    public GameObject fireworkPrefab;
    public Transform[] slots;
    public float fireworksPerSecond;
    public float reloadTime;
    public float distTargetThrustDivider = 3f;

    public float accuracy;
    private float nextActionTime;
    private uint loaded;
    public Vector3 fireworkTarget;

    public override void AttributesUpdated() {
        base.AttributesUpdated();
        accuracy = Mathf.Clamp01(botBase.accuracy * accuracy);
        fireworksPerSecond *= botBase.attackSpeed;
        reloadTime /= botBase.attackSpeed;
    }

    private void Update() {
       if (Time.time > nextActionTime && deathFactor == 0f) {
            if (loaded == 0) {
                nextActionTime = Time.time + reloadTime;
                Reload();
            } else if (Vector3.Distance(fireworkTarget, transform.position) > 3f) {
                nextActionTime = Time.time + 1f / fireworksPerSecond;
                LaunchOne(math.tzcnt(loaded));
            }
       }

        fireworkTarget = targetPosition;
    }

    private void Reload() {
        loaded = 0b1111;
        foreach (var slot in slots) {
            GameObject instance = Instantiate(fireworkPrefab, slot);
        }
    }

    private void Burst() {
        loaded = 0;

        for (int i = 0; i < slots.Length; i++) {
            LaunchOne(i);
        }
    }

    private void LaunchOne(int index) {
        if (slots[index].childCount > 0) {
            loaded &= ~((uint)1 << index);
            GameObject obj = slots[index].GetChild(0).gameObject;
            obj.transform.SetParent(null);
            Firework firework = obj.GetComponent<Firework>();
            firework.Launch(Mathf.Clamp(Vector3.Distance(transform.position, fireworkTarget) / distTargetThrustDivider, 0.25f, 2), this);
        }
    }
}
