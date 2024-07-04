using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotData", menuName = "ScriptableObjects/New Bot Type", order = 1)]
public class BotData : ScriptableObject {
    public List<BotPartData> center;
    public List<BotPartData> left;
    public List<BotPartData> right;
    public List<BotPartData> hat;
    public List<BotPartData> neck;
    public List<BotPartData> leftEye;
    public List<BotPartData> rightEye;
    public List<BotPartData> nose;
    public List<BotPartData> heads;

    public bool spawnAtLeastOneDefaultEye = true;
    public GameObject defaultEye;

    public float angryChance = 0.5f;
    public List<BotAttributeModifier> angryModifiers;
    public bool applyFace;

    public float baseMovementSpeed = 7;
    public float baseAttackSpeed = 5;
    public float baseBodyHealth = 50;
    public float baseHeadHealth = 20;
    public float baseDamageResistance = 0;
    public float baseKnockbackResistance = 0;
}
