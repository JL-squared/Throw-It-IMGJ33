using UnityEngine;

public class ProjectileShooter : MonoBehaviour {
    public ProjectileItemData data;
    public Transform spawnHolster;
    public float startingSpeed;
    public float offsetDistance = 0.0f;
    public Vector2 maxRandomDirection;
    [HideInInspector]
    public new Collider collider;
    [HideInInspector]
    public EntityMovement inheritVelocityMovement;
    
    void Start() {
        collider = GetComponent<Collider>();
        inheritVelocityMovement = GetComponent<EntityMovement>();
    }

    public void Shoot(float forcePercentage = 1.0f, Vector3 offsetVelocity=default) {
        Vector2 randomOffset = maxRandomDirection * new Vector2(Random.value - 0.5f, Random.value - 0.5f) * 2.0f;
        Vector3 tahini = new Vector3(randomOffset.x, randomOffset.y, 1f).normalized;
        Vector3 fwd = spawnHolster.TransformDirection(tahini);
        Vector3 startingVelocity = startingSpeed * forcePercentage * fwd;

        //Vector3 startingVelocity = startingSpeed * forcePercentage * spawnHolster.forward;
        Vector3 startingPos = spawnHolster.position + fwd * offsetDistance;

        
        if (inheritVelocityMovement != null) {
            startingVelocity += inheritVelocityMovement.Velocity;
        }

        if (Player.Instance.gameObject == gameObject) {
            if (Player.Instance.vehicle != null && Player.Instance.vehicle.GetComponent<Rigidbody>() != null) {
                startingVelocity += Player.Instance.vehicle.GetComponent<Rigidbody>().velocity;
            }
        }

        startingVelocity += offsetVelocity;
        Projectile.Spawn(data, startingPos, startingVelocity, this);
    }

    private void OnDrawGizmosSelected() {
        if (spawnHolster != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(spawnHolster.position, spawnHolster.forward * offsetDistance);
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(spawnHolster.position + spawnHolster.forward * offsetDistance, 0.2f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(spawnHolster.position + spawnHolster.forward * offsetDistance, spawnHolster.forward * 0.5f);
        }
    }
}
