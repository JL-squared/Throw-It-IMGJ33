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
    public float hypothermiaShivering;
    public float hypothermiaShiveringStrength = 0.0002f;

    private List<ShakeData> shakes;


    private void Start() {
        player.health.health.OnDamaged += OnDamaged;
        damageRotation = Quaternion.identity;
        shakes = new List<ShakeData>();
    }

    private void OnDamaged(float damage, EntityHealth.DamageSourceData data) {
        if (data == null)
            return;

        Vector3 diff = data.direction;
        diff.y = 0;
        diff.Normalize();
        float xValue = -damagedDirectionFactor * Vector3.Dot(diff, transform.forward);
        float zValue = damagedDirectionFactor * Vector3.Dot(diff, transform.right);
        damageRotation *= Quaternion.Euler(xValue, 0f, zValue);
    }

    private void Update() {
        if (!UIScriptMaster.Instance.inGameHUD.Paused) {
            CalculateAndApply();
        }
    }

    private void CalculateAndApply() {
        // Get rid of old shakes that have completely elapsed their duration
        for (int i = shakes.Count - 1; i >= 0; i--) {
            if (Time.time > shakes[i].nextTime) {
                shakes.RemoveAt(i);
            }
        }

        // Calculate accumulated camera shake from all sources
        Quaternion temp = Quaternion.identity;
        for (int i = 0; i < shakes.Count; i++) {
            temp *= Quaternion.Slerp(Quaternion.identity, Random.rotation, shakes[i].intensity * cameraShakeIntensity);
        }
        cameraShakeRot = Quaternion.Slerp(temp, cameraShakeRot, Time.deltaTime * cameraShakeSmoothin);

        // Calculate some tilt-strafe rotation based on horizontal movement
        Vector3 velocity = player.movement.inner.Velocity;
        velocity.y = 0f;
        velocity = Vector3.ClampMagnitude(velocity / player.movement.inner.speed, 1);
        horizontal = Mathf.Lerp(horizontal, -Vector3.Dot(player.transform.right, velocity), Time.deltaTime * tiltStrafeSmoothingSpeed);
        tiltStrafe = Quaternion.Euler(0f, 0f, horizontal * tiltStrafeStrength);

        // We can convert MoodleStrength to an integer (knowing that Bad=8)
        // We can use this as an indicator for hypothermia shake
        hypothermiaShivering = (int)moodleManager.bodyTempMoodleStrength > 7 ? 1f : 0f;
        Quaternion shiverRotation = Quaternion.Lerp(Quaternion.identity, Random.rotation, hypothermiaShivering * hypothermiaShiveringStrength);

        // Damage rotation calculation (that brings it back to the default value)
        damageRotation = Quaternion.Lerp(damageRotation, Quaternion.identity, Time.deltaTime * damageSmoothinSpeed);
            
            
        // Apply all rotation
        shiverer.transform.localRotation = damageRotation * tiltStrafe * cameraShakeRot * shiverRotation;
    }

    public void ShakeCamera(float duration, float intensity, Vector3 position) {
        shakes.Add(new ShakeData {
            nextTime = duration + Time.time,
            intensity = intensity / Vector3.Distance(position, transform.position),
        });
    }
}
