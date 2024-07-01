using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileItemData", menuName = "ScriptableObjects/New Projectile Item Data", order = 1)]
public class ProjectileItemData : ItemData {
    public GameObject projectile;
    public float lifetime;
}
