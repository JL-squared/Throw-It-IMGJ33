using System;
using UnityEngine;
using UnityEngine.UIElements;

// Rigidbody based character controller
public class EntityMovement : MonoBehaviour {
    [Header("Speed")]
    public float speed = 7f;
    [HideInInspector]
    public Vector2 localWishMovement;
    private Vector2 localMovement;
    private Vector3 movement;
    [HideInInspector]
    public Quaternion localWishRotation;

    [Header("Gravity")]
    [Min(0.01f)]
    public float airControl = 15;
    [Min(0.01f)]
    public float groundControl = 25;
    public float jump = 5.0F;
    public float gravity = -9.81f;
    [HideInInspector]
    public bool isJumping;
    [Header("Rigidbody Interaction")]
    public float pushForce = 8;
    public float maxPushForce = 20;

    [HideInInspector]
    public CharacterController cc;

    // Start is called before the first frame update
    void Start() {
        cc = GetComponent<CharacterController>();
    }

    // FixedUpdate is called each physics timestep
    void Update() {
        float control = cc.isGrounded ? groundControl : airControl;
        localMovement = Vector2.Lerp(localMovement, localWishMovement, Time.deltaTime * control);
        movement.x = speed * localMovement.x;
        movement.z = speed * localMovement.y;
        movement = transform.TransformDirection(movement);
        movement.y += gravity * Time.deltaTime;

        if (cc.isGrounded) {
            movement.y = -2.5f;

            if (isJumping) {
                movement.y = jump;
                isJumping = false;
            }
        }

        cc.Move(movement * Time.deltaTime);

        if (localWishRotation.normalized != Quaternion.identity) {
            transform.rotation = localWishRotation.normalized;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
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