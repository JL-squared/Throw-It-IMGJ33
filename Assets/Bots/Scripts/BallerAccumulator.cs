using Unity.Mathematics;
using UnityEngine;

public class BallerAccumulator : BotBehaviour {
    public Transform ball;
    public float increaseFactor;
    public float maxRadius;
    public float startingRadius;
    public float voxelEditStrength;
    public float voxelEditOffsetRadius;
    public float renderRadiusOffset;
    public float movementSpeedReduction;
    private float volume;
    private float radius;
    private float startingMovementSpeed;
    private Quaternion rotation = Quaternion.identity;
    private Quaternion angularVelocity = Quaternion.identity;
    private Quaternion currentAngularVelocity = Quaternion.identity;

    public void Start() {
        volume = (4f / 3f) * Mathf.PI * Mathf.Pow(startingRadius, 3);
        UpdateBallParams(0.0f);
    }

    public override void AttributesUpdated() {
        base.AttributesUpdated();
        startingMovementSpeed = botBase.movementSpeed;
    }

    // factor at 0 => at starting radius
    // factor at 1 => at end radius
    void UpdateBallParams(float factor) {
        // derived from dv/dt. I love calc :3
        radius = Mathf.Sqrt(volume / 4 * Mathf.PI);

        // Character controller settings
        CharacterController cc = GetComponent<CharacterController>();
        cc.Move(Vector3.up * radius * Time.deltaTime);
        cc.center = Vector3.up * (-radius);
        cc.height = 1f + 2 * radius;
        cc.radius = radius;
        
        // Update ball visuals and world settings
        ball.localScale = Vector3.one * (radius + renderRadiusOffset) * 2f;
        ball.localPosition = -Vector3.up * (radius + 0.5f);

        // Entity movement progressively gets slower (since surface area gets bigger)
        botBase.movementSpeed = startingMovementSpeed - (movementSpeedReduction * factor);
    }

    void ApplyVoxelEdit() {
        if (increaseFactor > 0.1) {
            IVoxelEdit edit = new AddVoxelEdit {
                center = ball.transform.position,
                material = 0,
                radius = radius * 0.9f + voxelEditOffsetRadius,
                strength = voxelEditStrength * increaseFactor * Time.deltaTime,
                writeMaterial = false,
                maskMaterial = true,
                falloffOffset = 0.5f,
                scale = new float3(1f),
            };

            VoxelTerrain.Instance.ApplyVoxelEdit(edit);
        }
    }

    public void Update() {
        if (VoxelTerrain.Instance == null) {
            Debug.LogWarning("Can't accumulate snow if terrain is disabled");
        }

        angularVelocity = Quaternion.identity;
        if (movement.IsGrounded) {
            // Calculate the ball's angular velocity to apply at the end of the frame
            Vector2 mov2d = new Vector2(movement.wishMovement.x, movement.wishMovement.z);
            float lateralSpeed = mov2d.magnitude;
            float rotationSpeed = Mathf.Rad2Deg * lateralSpeed / radius;
            Vector3 dir = transform.TransformDirection(Vector3.right);

            // Value that we will integrate
            angularVelocity = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, dir);

            // Check if we are on voxel terrain and if we are on the snow material
            if (movement.Ground != null && movement.Ground.GetComponent<VoxelChunk>() != null && VoxelTerrain.Instance.TryGetVoxel(ball.transform.position - Vector3.up * (radius + 2.5f)).material == 0) {
                // Increase radius, apply edit, and update params
                // factor at 0 => at starting radius
                // factor at 1 => at end radius
                float factor = math.unlerp(startingRadius, maxRadius, radius);
                volume += (1-factor) * increaseFactor * Time.deltaTime * mov2d.magnitude;
                if (mov2d.magnitude > 0.01f) {
                    UpdateBallParams(factor);
                    ApplyVoxelEdit();
                }
            }
        }

        currentAngularVelocity = Quaternion.Slerp(currentAngularVelocity, angularVelocity, Time.deltaTime * 10);
        rotation = currentAngularVelocity.normalized * rotation;

        // I tried setting global rotation to default but that didn't work idk why
        ball.localRotation = Quaternion.Inverse(transform.rotation);
        ball.rotation = rotation;
    }
}
