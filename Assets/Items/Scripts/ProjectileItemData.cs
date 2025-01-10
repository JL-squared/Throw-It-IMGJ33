using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileItemData", menuName = "Scriptable Objects/New Projectile Item Data", order = 1)]
public class ProjectileItemData : ItemData {
    public GameObject projectile;
    public float lifetime;
    public float knockbackFactor;
    public float rigidbodyForceFactor;
}
