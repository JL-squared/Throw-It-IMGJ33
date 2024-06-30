using UnityEngine;

[CreateAssetMenu(fileName = "SnowballItemData", menuName = "ScriptableObjects/New Snowball Item Data", order = 1)]
public class SnowballItemData : ItemData {
    public GameObject snowball;
    public GameObject particles;
    public float damageFactor;
    public float speedFactor;
    public float lifetime;
}
