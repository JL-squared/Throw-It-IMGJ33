using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraShake : PlayerBehaviour {
    struct ShakeData {
        public float nextTime;
        public float intensity;
    }

    public float damageSmoothinSpeed;
    public float damagedDirectionFactor;
    public GameObject shiverer;
    private Quaternion damageRotation;
    private Quaternion tiltStrafe;
    public float tiltStrafeStrength;
    public float tiltStrafeSmoothingSpeed;
    private float horizontal;

    public float cameraShakeIntensity;
    public float cameraShakeSmoothin;
    private Quaternion cameraShakeRot;

    [HideInInspector]
    public float shivering;
    public float shiveringStrength = 0.0002f;

    private List<ShakeData> shakes;


    private void Start() {
        player.health.health.OnDamaged += Health_OnDamaged;
        damageRotation = Quaternion.identity;
        shakes = new List<ShakeData>();
    }

    private void Health_OnDamaged(float damage, EntityHealth.DamageSourceData data) {
        Vector3 diff = data.direction;
        diff.y = 0;
        diff.Normalize();
        float xValue = -damagedDirectionFactor * Vector3.Dot(diff, transform.forward);
        float zValue = damagedDirectionFactor * Vector3.Dot(diff, transform.right);
        damageRotation *= Quaternion.Euler(xValue, 0f, zValue);
    }

    private void Update() {
        Vector3 velocity = player.movement.inner.Velocity;
        velocity.y = 0f;
        velocity = Vector3.ClampMagnitude(velocity / player.movement.inner.speed, 1);
        horizontal = Mathf.Lerp(horizontal, -Vector3.Dot(player.transform.right, velocity), Time.deltaTime * tiltStrafeSmoothingSpeed);

        Quaternion temp = Quaternion.identity;

        for (int i = shakes.Count - 1; i >= 0; i--) {
            if (Time.time > shakes[i].nextTime) {
                shakes.RemoveAt(i);
            }
        }

        for (int i = 0; i < shakes.Count; i++) {
            temp *= Quaternion.Slerp(Quaternion.identity, Random.rotation, shakes[i].intensity * cameraShakeIntensity);
        }

        cameraShakeRot = Quaternion.Slerp(temp, cameraShakeRot, Time.deltaTime * cameraShakeSmoothin);

        tiltStrafe = Quaternion.Euler(0f, 0f, horizontal * tiltStrafeStrength);
        damageRotation = Quaternion.Lerp(damageRotation, Quaternion.identity, Time.deltaTime * damageSmoothinSpeed);

        Quaternion shiverRotation = Quaternion.Lerp(Quaternion.identity, Random.rotation, shivering * shiveringStrength);

        if (!UIScriptMaster.Instance.inGameHUD.Paused) {
            shiverer.transform.localRotation = damageRotation * tiltStrafe * cameraShakeRot * shiverRotation;
        }
    }

    public void ShakeCamera(float duration, float intensity, Vector3 position) {
        shakes.Add(new ShakeData {
            nextTime = duration + Time.time,
            intensity = intensity / Vector3.Distance(position, transform.position),
        });
    }
}
