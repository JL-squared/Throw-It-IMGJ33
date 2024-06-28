using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

// Rigidbody based character controller
public class EntityMovement : MonoBehaviour {
    [Header("Speed")]
    public float speed = 7f;
    [HideInInspector]
    public Vector2 localWishMovement;
    private Vector3 movement;
    [HideInInspector]
    public Vector3 wishMovement;
    [HideInInspector]
    public Quaternion localWishRotation;

    [Header("Control")]
    [Min(0.01f)]
    public float airControl = 15;
    [Min(0.01f)]
    public float groundControl = 25;
    public float maxAcceleration = 5;
    public float jump = 5.0F;
    public float coyoteTime = 0.0f;
    public float jumpBufferTime = 0.0f;
    public float gravity = -9.81f;
    public float groundedOffsetVelocity = -2.5f;
    [HideInInspector]
    public bool isJumping;
    [Header("Rigidbody Interaction")]
    public float pushForce = 8;
    public float maxPushForce = 20;

    [HideInInspector]
    public CharacterController cc;
    private Vector3 explosion;
    private float lastGroundedTime = 0;
    private float nextJumpTime = 0;
    private int jumpCounter = 0;
    private bool buffered;
    [HideInInspector]
    public GameObject groundObject;
    private bool groundJustExploded;

    // Start is called before the first frame update
    void Start() {
        cc = GetComponent<CharacterController>();
    }

    // FixedUpdate is called each physics timestep
    void Update() {
        float control = cc.isGrounded ? groundControl : airControl;

        Vector2 normalized = localWishMovement.normalized;
        wishMovement.x = speed * normalized.x;
        wishMovement.z = speed * normalized.y;
        wishMovement = transform.TransformDirection(wishMovement);

        // bypass y value as that must remain unchanged
        wishMovement.y = movement.y;

        //movement = Vector3.Lerp(movement, wishMovement, Time.deltaTime * control);
        movement += Vector3.ClampMagnitude(wishMovement - movement, maxAcceleration) * Time.deltaTime * control;
        
        movement.y += gravity * Time.deltaTime;

        if (Time.time < nextJumpTime && buffered && cc.isGrounded) {
            nextJumpTime = 0;
            isJumping = true;
            buffered = false;
        }

        if (cc.isGrounded && !groundJustExploded) {
            movement.y = groundedOffsetVelocity;
            lastGroundedTime = Time.time;
            jumpCounter = 0;
        } else {
            groundObject = null;
        }

        if (groundJustExploded)
            groundJustExploded = false;

        // could change the restriction on jumpCounter to enable double jumping
        if (isJumping && jumpCounter == 0) {
            movement.y = jump;
            isJumping = false;
            jumpCounter++;
        }

        
        CollisionFlags flags = cc.Move((movement + explosion) * Time.deltaTime);

        if (flags == CollisionFlags.CollidedAbove && movement.y > 0.0) {
            movement.y = 0;
        }

        if (localWishRotation.normalized != Quaternion.identity) {
            transform.rotation = localWishRotation.normalized;
        }

        // TODO: Actually write acceleration and integrate it instead of doing this goofy stuff
        explosion = Vector3.Lerp(explosion, Vector3.zero, Time.deltaTime * 10);
    }

    public void ExplosionAt(Vector3 position, float force, float radius) {
        Vector3 f = transform.position - position;

        // 1 => closest to explosion
        // 0 => furthest from explosion
        float factor = 1 - Mathf.Clamp01(math.unlerp(0, radius, f.magnitude));
        factor = Mathf.Sqrt(1f - Mathf.Pow(factor - 1, 2f));

        if (f.normalized.y > 0.9) {
            groundJustExploded = true;
        }

        movement += f.normalized * factor * force;
    }

    public void Jump() {
        if ((Time.time - lastGroundedTime) <= coyoteTime && jumpCounter == 0) {
            isJumping = true;
        } else if (jumpCounter == 1) {
            nextJumpTime = Time.time + jumpBufferTime;
            buffered = true;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        if (hit.normal.y > 0.9f) {
            groundObject = hit.gameObject;
        }

        if (hit.rigidbody != null) {
            Vector3 scaled = hit.moveDirection * hit.rigidbody.mass;
            scaled = Vector3.ClampMagnitude(scaled, maxPushForce);
            hit.rigidbody.AddForce(scaled * pushForce);

            // Ok IDFK what im doing here ngl
            if(hit.rigidbody.isKinematic) {
               // cc.attachedRigidbody.AddForce(-scaled * pushForce, ForceMode.Impulse);
            }
        }
    }
}