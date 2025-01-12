using System;
using Tweens;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : PlayerBehaviour {
    public float defaultFOV;

    [Header("Movement")]
    [HideInInspector]
    public EntityMovement inner;
    [HideInInspector]
    public Vector2 wishHeadDir;
    public Transform head;
    public Camera gameCamera;
    public float mouseSensitivity = 1.0f;
    [HideInInspector]
    public Vector2 localWishMovement;

    [Min(1.0f)]
    public float sprintModifier;

    public bool isSprinting;
    public bool isCrouching;

    [HideInInspector]
    public Vehicle vehicle;

    private float stepValue;
    private bool stepped;
    public AudioSource footsteps;
    public Vector2 mouseDelta;

    public void Start() {
        mouseSensitivity = player.settings.mouseSensivity;
        inner = GetComponent<EntityMovement>();
        defaultFOV = settings.fov;
        Cursor.lockState = CursorLockMode.Locked;

        gameCamera.fieldOfView = defaultFOV;
        inner.onJumping.AddListener(() => { Utils.PlaySound(footsteps, Registries.rockJump); });
    }

    private void Update() {
        Vector2 velocity2d = new Vector2(inner.Velocity.x, inner.Velocity.z);
        float targetBobbingStrength = 0f;
        if (velocity2d.magnitude > 0.01 && inner.IsGrounded) {
            stepValue += velocity2d.magnitude * Time.deltaTime;
            targetBobbingStrength = velocity2d.magnitude / inner.speed;
            targetBobbingStrength = Mathf.Clamp01(targetBobbingStrength);
        } else {
            targetBobbingStrength = 0f;
        }

        player.onSpeedPercentageUpdate?.Invoke(targetBobbingStrength);
        player.onStepUpdate?.Invoke(stepValue);

        if (verticalBobbing < 0f && !stepped) {
            if (player.controller.isSprinting) {
                Utils.PlaySound(player.controller.footsteps, Registries.rockRun);
            } else {
                Utils.PlaySound(player.controller.footsteps, Registries.rockWalk);
            }
            stepped = true;
        } else if (verticalBobbing > 0f) {
            stepped = false;
        }

        if (isSprinting) {
            inner.speedModifier = sprintModifier;
        } else {
            inner.speedModifier = 1f;
        }
    }

    public void ResetMovement(bool resetRotation = false) {
        inner.localWishMovement = Vector2.zero;
        mouseDelta = Vector2.zero;
        //SmoothedMouseDelta = Vector2.zero;

        if (resetRotation) {
            inner.localWishRotation = Quaternion.identity;
            transform.localRotation = Quaternion.identity;
            head.localRotation = Quaternion.identity;
            wishHeadDir = Vector2.zero;
        }

        ApplyMouseDelta(Vector2.zero);

        stepValue = 0f;
    }

    public void Crouch(InputAction.CallbackContext context) {
        if (!isSprinting && Performed()) {
            isCrouching = context.ReadValue<float>() > 0.5f;
        }
    }

    public void Jump(InputAction.CallbackContext context) {
        if (Performed(context)) {
            inner.Jump();
        }
    }

    public void Movement(InputAction.CallbackContext context) {
        if (Performed()) {
            localWishMovement = context.ReadValue<Vector2>();
            inner.localWishMovement = localWishMovement;
            FOVTween();
        }
    }

    public void Sprint(InputAction.CallbackContext context) {
        if (!isCrouching && Performed()) {
            isSprinting = context.ReadValue<float>() > 0.5f;
            FOVTween();
        }
    }

    public void FOVTween() {
        var tween = new FloatTween {
            from = gameCamera.fieldOfView,
            to = isSprinting && localWishMovement.magnitude > 0.0f ? defaultFOV + 10 : defaultFOV,
            duration = 0.2f,
            onUpdate = (instance, value) => {
                gameCamera.fieldOfView = value;
            },
            easeType = EaseType.QuadOut,
        };
        gameObject.AddTween(tween);
    }

    public void Look(InputAction.CallbackContext context) {
        if (Cursor.lockState != CursorLockMode.None && Performed()) {
            ApplyMouseDelta(context.ReadValue<Vector2>());
        }
    }

    public void ApplyMouseDelta(Vector2 delta) {
        mouseDelta = delta;
        wishHeadDir += delta * mouseSensitivity * 0.02f;
        wishHeadDir.y = Mathf.Clamp(wishHeadDir.y, -90f, 90f);
        head.localRotation = Quaternion.Euler(-wishHeadDir.y, 0f, 0f);
        inner.localWishRotation = Quaternion.Euler(0f, wishHeadDir.x, 0f).normalized;
    }


    public void ExitVehicle() {
        Quaternion cpy2 = transform.rotation;
        Vector2 cpy = wishHeadDir;
        vehicle.Exit();
        vehicle = null;
        //lastInteraction = null;
        transform.SetParent(null);

        ResetMovement();
        Vector3 angles = cpy2.eulerAngles;
        wishHeadDir.x = angles.y;
        ApplyMouseDelta(Vector2.zero);
    }
}
