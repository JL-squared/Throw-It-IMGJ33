using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ElasticValueTweener {
    public float targetValue;
    public float targetReachSpeed = 10f;
    public float velocityDampeningSpeed = 0.5f;
    public float maxAccel = 20.0f;
    private float velocity;

    public void Update(float dt, ref float value) {
        velocity += Mathf.Clamp(targetValue - value, -maxAccel, maxAccel) * targetReachSpeed;
        velocity += -velocity * Mathf.Clamp01(velocityDampeningSpeed);
        value += dt * velocity;
    }
}
