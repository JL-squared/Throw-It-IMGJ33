using UnityEngine;

public class PlayerBobbingSway : PlayerBehaviour {
    public float baseCameraHeight = 0.8f;
    public GameObject viewModelHolster;
    private Vector3 rotationLocalOffset;
    private Vector3 positionLocalOffset;
    public float holsterSwaySmoothingSpeed = 25f;
    public float holsterSwayMouseAccelSmoothingSpeed = 20f;
    public float viewModelRotationClampMagnitude = 0.2f;
    public float viewModelRotationStrength = -0.001f;
    public float viewModelPositionStrength = 0.2f;
    public float bobbingStrength = 0.05f;
    public float bobbingSpeed = 2.5f;
    public float bobbingSteppiness = 20f;
    public float viewModelBobbingStrength = 3.0f;
    private float bobbingStrengthCurrent = 0f;
    private bool stepped;
    [HideInInspector]
    public Vector2 smoothedMouseDelta;

    public void Update() {
        // Calculate bobbing strength based on player velocity relative to their max speed
        EntityMovement inner = player.movement.inner;
        float stepValue = player.movement.stepValue;
        float targetBobbingStrength = Mathf.Clamp01(player.movement.currentVelocityPercentage);

        // Interpolated so it doesn't come to an abrupt stop
        bobbingStrengthCurrent = Mathf.Lerp(bobbingStrengthCurrent, targetBobbingStrength, 25f * Time.deltaTime);

        // Vertical and horizontal bobbing values
        float effectiveBobbingStrength = bobbingStrength * bobbingStrengthCurrent;

        // https://www.desmos.com/calculator/bvzhohw3cu
        //float verticalBobbing = Mathf.Sin(stepValue * bobbingSpeed) * effectiveBobbingStrength;
        float suace = Mathf.Sin(0.5f * stepValue * bobbingSpeed);
        float horizontalBobbing = Mathf.Pow(Mathf.Abs(suace), 1f / 1.5f) * Mathf.Sign(suace) * effectiveBobbingStrength;
        float verticalBobbing = (Utils.SmoothAbsClamped01(Mathf.Sin((0.5f * stepValue + Mathf.PI / 4f) * bobbingSpeed), 1f / bobbingSteppiness) * 2f - 1f) * effectiveBobbingStrength;

        if (verticalBobbing < 0.0f && !stepped) {
            player.footsteps.PlayFootStep(player.movement.isSprinting);
            stepped = true;
        } else if (verticalBobbing > 0f) {
            stepped = false;
        }

        if (!settings.bobbing) {
            viewModelHolster.transform.localPosition = Vector3.zero;
        } else {
            if (player.state != Player.State.Dead)
                player.movement.head.transform.localPosition = new Vector3(horizontalBobbing, baseCameraHeight + verticalBobbing, 0);
            ApplyHandSway(verticalBobbing);
        }

        smoothedMouseDelta = Vector2.Lerp(smoothedMouseDelta, player.movement.mouseDelta, Time.deltaTime * holsterSwayMouseAccelSmoothingSpeed);
    }


    private void ApplyHandSway(float bobbing) {
        rotationLocalOffset = Vector3.ClampMagnitude((new Vector3(smoothedMouseDelta.x, smoothedMouseDelta.y, 0) / (Time.deltaTime + 0.001f)) * 0.01f, viewModelRotationClampMagnitude) * viewModelRotationStrength;
        positionLocalOffset = transform.InverseTransformDirection(-player.movement.inner.Velocity) * viewModelPositionStrength;
        Vector3 current = viewModelHolster.transform.localPosition;
        Vector3 target = rotationLocalOffset + positionLocalOffset;
        target += Vector3.up * bobbing * viewModelBobbingStrength;

        //if (itemLogic != null) {
        //    target += itemLogic.swayOffset;
        //}

        Vector3 localPosition = Vector3.Lerp(current, target, Time.deltaTime * holsterSwaySmoothingSpeed);
        viewModelHolster.transform.localPosition = Vector3.ClampMagnitude(localPosition, 1f);
    }
}
