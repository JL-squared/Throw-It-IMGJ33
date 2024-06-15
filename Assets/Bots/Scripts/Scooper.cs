using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Scooper : BotWorldPart {

    [Header("Main Params")]
    public Transform origin;
    public Transform spawnHolster;
    public float secondsBetweenThrows;
    public bool repeating;
    
    [Header("Snow Pickup Fake Snowball Params")]
    public Transform fakeSnowball;
    public float groundPickupAngle = 270;
    public float groundPickupAngleSpread = 60;    

    [Header("Procedural Charge Back Params")]
    public ElasticValueTweener tweener;
    public float maxTime;
    public float curveThrowTime;
    public AnimationCurve curve;

    private Quaternion startingRot;
    private SnowballThrower thrower;
    private float time;
    private bool thrown;
    private float fakeSnowballSize;
    private float angle;

    public override void AttributesUpdated() {
        base.AttributesUpdated();
        secondsBetweenThrows /= botBase.attackSpeed;
    }

    public override void TargetChanged(Vector3 target, Vector3 velocity) {
        base.TargetChanged(target, velocity);
        // TODO: Actually predict time of flight using projectile motion?
        float lookAhead = 0.2f;
        
        Vector3 newTarget = target + velocity * lookAhead;
        spawnHolster.rotation = Quaternion.LookRotation((newTarget - spawnHolster.position).normalized);
        
        if (Quaternion.Angle(spawnHolster.localRotation, Quaternion.LookRotation(Vector3.forward)) > 20) {
            spawnHolster.localRotation = Quaternion.identity;
        }
    }

    public void Start() {
        thrower = GetComponent<SnowballThrower>();
        startingRot = origin.rotation;
    }

    // zero degrees is when the scoop is horizontal, facing down (in front of snowman, ready to pickup snow)
    // 90 degrees would be straight into the ground
    public void Update() {
        // Handle angle stuff
        if (repeating) {
            angle += Time.deltaTime * 360f / secondsBetweenThrows;
        } else {
            time += Time.deltaTime * maxTime / secondsBetweenThrows;
            // scoop up snow, overshoot a bit
            // ratchet back 2-3 times
            float localized = (time % maxTime);
            tweener.targetValue = curve.Evaluate(localized);
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
            if ((time % maxTime) > curveThrowTime && !thrown) {
                thrower.Throw();
                thrown = true;
            }
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
