using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Scooper : BotWorldPart {
    public Transform fakeSnowball;
    public Transform origin;
    private float angle;
    public ElasticValueTweener tweener;
    public float speed;
    public bool repeating;
    public float animCharge;
    public float fakeSnowballSize;
    public float groundPickupAngle = 270;
    public float groundPickupAngleSpread = 60;
    private Quaternion startingRot;
    private SnowballThrower thrower;
    public bool thrown;
    private float time;

    public void Start() {
        thrower = GetComponent<SnowballThrower>();
        startingRot = origin.rotation;
    }

    // zero degrees is when the scoop is horizontal, facing down (in front of snowman, ready to pickup snow)
    // 90 degrees would be straight into the ground
    public void Update() {
        // Handle angle stuff
        if (repeating) {
            angle += Time.deltaTime * speed;
        } else {
            time += Time.deltaTime;
            // scoop up snow, overshoot a bit
            // ratchet back 2-3 times
            tweener.Update(Time.deltaTime, ref angle);
        }

        float normalized = (angle + 180) % 360.0f;

        // Handle throwing only
        if (repeating) {
            if (normalized > 270f && !thrown) {
                thrower.Throw();
                thrown = true;
            }
        } else {
            // badoing... throw that shit
        }

        // Sizing the fake snowball size based on current angle
        float startPickupAngle = groundPickupAngle - groundPickupAngleSpread;
        float endPickupAngle = groundPickupAngle + groundPickupAngleSpread;
        if (normalized > startPickupAngle && normalized < endPickupAngle) {
            float state = math.unlerp(startPickupAngle, endPickupAngle, normalized);
            fakeSnowballSize = state;
            thrown = false;
        } else {
            fakeSnowballSize = 1.0f;
        }

        origin.localRotation = Quaternion.AngleAxis(angle, Vector3.right) * startingRot;

        if (thrown) {
            fakeSnowball.localScale = Vector3.zero;
        } else {
            fakeSnowball.localScale = Vector3.one * fakeSnowballSize * 0.5f;
        }
    }
}
