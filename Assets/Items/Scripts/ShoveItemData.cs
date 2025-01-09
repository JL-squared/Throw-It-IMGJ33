using UnityEngine;

[CreateAssetMenu(fileName = "ShovelItemData", menuName = "ScriptableObjects/New Shovel Item Data", order = 1)]
public class ShovelItemData : ItemData {
    public float digRadius;
    public float digStrength;
    public new ShovelItem item;
    public Quaternion animationRotation;
    public Vector3 animationPosition;
}
