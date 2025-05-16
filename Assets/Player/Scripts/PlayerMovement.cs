using Tweens;
using UnityEngine;

public class PlayerMovement : PlayerBehaviour {
    public float defaultFOV;

    [Header("Movement")]
    [HideInInspector]
    public EntityMovement inner;
    [HideInInspector]
    public Vector2 wishHeadDir;
    public Transform head;
    public float mouseSensitivity = 1.0f;
    [HideInInspector]
    public Vector2 localWishMovement;

    [Min(1.0f)]
    public float sprintModifier;

    public bool isSprinting;
    public bool isCrouching;

    [HideInInspector]
    public Vehicle vehicle;

    [HideInInspector]
    public float stepValue;
    [HideInInspector]
    public float currentVelocityPercentage;
    public Vector2 mouseDelta;

    public void Start() {
        mouseSensitivity = player.settings.mouseSensivity;
        inner = GetComponent<EntityMovement>();
        defaultFOV = settings.fov;
        Cursor.lockState = CursorLockMode.Locked;
        player.camera.fieldOfView = defaultFOV;
    }

    private void Update() {
        Vector2 velocity2d = new Vector2(inner.Velocity.x, inner.Velocity.z);
        if (velocity2d.magnitude > 0.01 && inner.IsGrounded) {
            stepValue += velocity2d.magnitude * Time.deltaTime;
            currentVelocityPercentage = velocity2d.magnitude / inner.speed;
        } else {
            currentVelocityPercentage = 0f;
        }

        if (isSprinting) {
            inner.speedModifier = sprintModifier;
        } else {
            inner.speedModifier = 1f;
        }

        FOVTween();
    }

    public void ResetMovement(bool resetRotation = false) {
        inner.localWishMovement = Vector2.zero;
        localWishMovement = Vector2.zero;
        mouseDelta = Vector2.zero;
        player.bobbing.smoothedMouseDelta = Vector2.zero;

        if (resetRotation) {
            inner.localWishRotation = Quaternion.identity;
            transform.localRotation = Quaternion.identity;
            head.localRotation = Quaternion.identity;
            wishHeadDir = Vector2.zero;
        }

        ApplyMouseDelta(Vector2.zero);

        stepValue = 0f;
    }

    public void Crouch(bool crouching) {
        isCrouching = crouching;
    }

    public void Jump() {
        inner.Jump();
    }

    public void Movement(Vector2 dir) {
        localWishMovement = dir;
        inner.localWishMovement = localWishMovement;
        //FOVTween();
    }

    public void Sprint(bool sprinting) {
        isSprinting = sprinting;
        //FOVTween();
    }

    public void Look(Vector2 looking) {
        ApplyMouseDelta(looking);
    }

    public void FOVTween() {
        bool actuallyMovingHorizontally = new Vector2(inner.Velocity.x, inner.Velocity.z).magnitude > (sprintModifier * 0.80f * inner.speed);
        bool inputMovingHorizontally = localWishMovement.magnitude > 0.5f;

        var tween = new FloatTween {
            from = player.camera.fieldOfView,
            to = isSprinting && actuallyMovingHorizontally && inputMovingHorizontally ? defaultFOV + 10 : defaultFOV,
            duration = 0.2f,
            onUpdate = (instance, value) => {
                player.camera.fieldOfView = value;
            },
            easeType = EaseType.QuadOut,
        };
        gameObject.AddTween(tween);
    }

    public void ApplyMouseDelta(Vector2 delta) {
        mouseDelta = delta;
        wishHeadDir += delta * mouseSensitivity * 0.1f;
        wishHeadDir.y = Mathf.Clamp(wishHeadDir.y, -90f, 90f);
        head.localRotation = Quaternion.Euler(-wishHeadDir.y, 0f, 0f);
        inner.localWishRotation = Quaternion.Euler(0f, wishHeadDir.x, 0f).normalized;
    }


    public void ExitVehicle() {
        player.state = Player.State.Default;
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
        inner.cc.enabled = true;
    }
}
