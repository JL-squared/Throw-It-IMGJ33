using UnityEngine;

[CreateAssetMenu(fileName = "SnowballItemData", menuName = "Scriptable Objects/New Snowball Item Data", order = 1)]
public class SnowballItemData : ProjectileItemData {
    public GameObject particles;
    public float baseDamage;
    public float speedFactor;
}
