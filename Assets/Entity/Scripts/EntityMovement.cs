using System;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

// Rigidbody based character controller
public class EntityMovement : MonoBehaviour, IEntitySerializer {
    [Header("Speed")]
    public float speed = 7f;

    [HideInInspector]
    public float speedModifier = 1f;
    [HideInInspector]
    public Vector2 localWishMovement;
    [HideInInspector]
    public Vector3 movement;
    [HideInInspector]
    public Vector3 wishMovement;
    [HideInInspector]
    public Quaternion localWishRotation;

    [Header("Control")]
    [Min(0.01f)]
    public float airControl = 15;
    [Min(0.01f)]
    public float groundControl = 0;
    public float rotationSmoothing = 0;
    public float maxAcceleration = 5;
    public float jump = 5.0F;
    public float coyoteTime = 0.0f;
    public float jumpBufferTime = 0.0f;
    public float gravity = -9.81f;
    public float knockbackResistance = 0.0f;
    public float groundedOffsetVelocity = -2.5f;
    public bool rotationIsLocal = false;
    [HideInInspector]
    public bool isJumping;
    [Header("Rigidbody Interaction")]
    public float pushForce = 8;
    public float maxPushForce = 20;

    [HideInInspector]
    public CharacterController cc;
    private float lastGroundedTime = 0;
    private float nextJumpTime = 0;
    private int jumpCounter = 0;
    private bool buffered;
    private bool groundJustExploded;
    public EntityMovementFlags entityMovementFlags = EntityMovementFlags.Default;

    public Vector3 Velocity {
        get {
            if (cc.enabled) {
                return cc.velocity;
            } else {
                return Vector3.zero;
            }
        }
    }

    public bool IsGrounded {
        get {
            if (cc.enabled) {
                return cc.isGrounded && !groundJustExploded;
            } else {
                return false;
            }
        }
    }

    public GameObject Ground { get; private set; }

    // Start is called before the first frame update
    void Start() {
        cc = GetComponent<CharacterController>();
    }

    // TODO: Will eventually need to migrate all frame based systems to fixed step to keep logic consistent across FPSes (because even if we use deltaTime, it would always be better to use a fixed tick system instead)
    // Will need to figure out how to handle character interpolation though....
    void Update() {
        if (!GameManager.Instance.initialized)
            return;

        float control = cc.isGrounded ? groundControl : airControl;

        // Transform local wish movement to global world movement direction
        Vector2 normalized = localWishMovement.normalized;
        wishMovement.x = speed * speedModifier * normalized.x;
        wishMovement.z = speed * speedModifier * normalized.y;
        if (entityMovementFlags.HasFlag(EntityMovementFlags.ApplyMovement)) {
            wishMovement = transform.TransformDirection(wishMovement);
        } else {
            wishMovement = Vector3.zero;
        }

        // Bypass y value as that must remain unchanged through the acceleration diff
        wishMovement.y = movement.y;
        movement += Vector3.ClampMagnitude(wishMovement - movement, maxAcceleration) * Time.deltaTime * control;
        movement.y += 2 * gravity * Time.deltaTime;
    

        // When we hit the ground and the input is buffered
        if (Time.time < nextJumpTime && buffered && cc.isGrounded) {
            nextJumpTime = 0;
            isJumping = true;
            buffered = false;
        }

        // Handles being grounded (resets jump buffer and coyote time thing)
        if (cc.isGrounded && !groundJustExploded) {
            movement.y = groundedOffsetVelocity;
            lastGroundedTime = Time.time;
            jumpCounter = 0;
        } else {
            Ground = null;
        }

        // Sometimes coyte time shits itself with explosions
        if (groundJustExploded)
            groundJustExploded = false;

        // Could change the restriction on jumpCounter to enable double jumping
        if (isJumping && jumpCounter == 0) {
            movement.y = jump;
            isJumping = false;
            jumpCounter++;
        }

        // Move the character and fix head bump problem
        if (cc.enabled) {
            Debug.DrawRay(transform.position, movement);
            CollisionFlags flags = cc.Move((movement) * Time.deltaTime);
            if (flags == CollisionFlags.CollidedAbove && movement.y > 0.0) {
                movement.y = 0;
            }
        }

        if (entityMovementFlags.HasFlag(EntityMovementFlags.AllowedToRotate)) {
            Quaternion q = Quaternion.identity;
            if (rotationSmoothing == 0f) {
                q = localWishRotation;
            } else {
                q = Quaternion.Lerp(transform.rotation, localWishRotation, (1f / rotationSmoothing) * Time.deltaTime);
            }

            if (rotationIsLocal) {
                transform.localRotation = q;
            } else {
                transform.rotation = q;
            }
        }
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

        AddImpulse(f.normalized * factor * force * 3);
    }

    public void AddImpulse(Vector3 force) {
        if (force.normalized.y > 0.9) {
            groundJustExploded = true;
        }

        force *= (1 - knockbackResistance);
        movement += force;
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
            Ground = hit.gameObject;
            return;
        }

        // We have to multiply by delta time since OnControllerColliderHit is called every frame (since we call move on the cc every frame)

        // TODO: Rewrite the entity movement using a kinematic rigidbody instead so we can handle proper rigidbody interactions and entity to entity interactions
        if (hit.rigidbody != null) {
            Vector3 scaled = hit.moveDirection * hit.rigidbody.mass * Time.deltaTime * 160;
            scaled = Vector3.ClampMagnitude(scaled, maxPushForce);
            hit.rigidbody.AddForceAtPosition(scaled * pushForce, hit.point);
        }

        EntityMovement em = hit.gameObject.GetComponent<EntityMovement>();
        if (em != null) {
            Vector3 a = hit.moveDirection * hit.moveLength * 10 * Time.deltaTime * 100;
            a.y = 0f;
            em.movement += a;
        }
    }

    public void Serialize(EntityData data) {
        data.position = transform.position;
        data.rotation = transform.rotation;
        data.velocity = movement;
    }

    public void Deserialize(EntityData data) {
        cc.enabled = false;
        localWishMovement = Vector2.zero;
        localWishRotation = Quaternion.identity;
        transform.position = data.position.Value;
        transform.rotation = data.rotation.Value;
        movement = data.velocity.Value;
        localWishRotation = data.rotation.Value;
        cc.enabled = true;
    }
}

[Flags]
public enum EntityMovementFlags {
    None,

    // if the entity can rotate using the localWishRotation
    AllowedToRotate,

    // if the entity can move using the localWishDir (does not count impulse or knockback)
    ApplyMovement,

    Default = ApplyMovement | AllowedToRotate,
}

public static class EntityMovementFlagsExt {
    public static void AddFlag(this ref EntityMovementFlags myFlags, EntityMovementFlags flag) {
        myFlags |= flag;
    }

    public static void RemoveFlag(this ref EntityMovementFlags myFlags, EntityMovementFlags flag) {
        myFlags &= ~flag;
    }
}

/*
public class EntityMovementConverter : JsonConverter {
    public override bool CanRead => true;
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        EntityMovement movement = (EntityMovement)value;
        Transform transform = movement.transform;
        writer.WriteStartObject();
        writer.WritePropertyName("position");
        serializer.Serialize(writer, transform.position);
        
        writer.WritePropertyName("velocity");
        serializer.Serialize(writer, movement.Velocity);
        
        writer.WritePropertyName("rotation");
        serializer.Serialize(writer, transform.rotation);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        Debug.Log("wtf");
        JObject jo = JObject.Load(reader);
        EntityMovement movement = (EntityMovement)existingValue;
        Debug.Log(movement == null);
        movement.transform.position = JsonConvert.DeserializeObject<Vector3>(jo["position"].ToString());
        movement.transform.rotation = JsonConvert.DeserializeObject<Quaternion>(jo["rotation"].ToString());
        movement.movement = JsonConvert.DeserializeObject<Vector3>(jo["velocity"].ToString());
        Debug.Log("aaa");
        return movement;
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(EntityMovement);
    }
}
*/