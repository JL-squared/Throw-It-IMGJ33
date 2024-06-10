using UnityEngine;

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

    [Header("Gravity")]
    [Min(0.01f)]
    public float airControl = 15;
    [Min(0.01f)]
    public float groundControl = 25;
    public float jump = 5.0F;
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

        movement = Vector3.Lerp(movement, wishMovement, Time.deltaTime * control);
        movement.y += gravity * Time.deltaTime;
        
        if (cc.isGrounded) {
            movement.y = groundedOffsetVelocity;

            if (isJumping) {
                movement.y = jump;
                isJumping = false;
            }
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

    public void ExplosionAt(Vector3 position, float force) {
        Vector3 f = transform.position;
        explosion = f * force;
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