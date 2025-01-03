using System;
using UnityEngine;

[Serializable]
public class ElasticValueTweener {
    public float targetValue;
    public float factor = 1f;
    public float targetReachSpeed = 10f;
    public float velocityDampeningSpeed = 0.5f;
    public float maxAccel = 20.0f;
    public float velocity;
    public bool angular;

    // https://stackoverflow.com/questions/28036652/finding-the-shortest-distance-between-two-angles
    public float AngleDifference(float angle1, float angle2) {
        float diff = (angle2 - angle1 + 180) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }

    public void Update(float dt, ref float value) {
        float rawVel;

        if (angular) {
            rawVel = -AngleDifference(targetValue, value);
        } else {
            rawVel = targetValue - value;
        }
        velocity += Mathf.Clamp(rawVel, -maxAccel, maxAccel) * targetReachSpeed * factor;
        velocity += -velocity * Mathf.Clamp01(velocityDampeningSpeed);
        value += dt * velocity;
    }
}
