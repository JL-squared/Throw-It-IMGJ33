using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotData", menuName = "ScriptableObjects/New Bot Type", order = 1)]
public class BotData : ScriptableObject {
    public RngList<BotPartData> center;
    public RngList<BotPartData> left;
    public RngList<BotPartData> right;
    public RngList<BotPartData> hat;
    public RngList<BotPartData> neck;
    public RngList<BotPartData> leftEye;
    public RngList<BotPartData> rightEye;
    public RngList<BotPartData> nose;
    public RngList<BotPartData> heads;

    public bool spawnAtLeastOneDefaultEye = true;
    public GameObject defaultEye;

    public float angryChance = 0.5f;
    public List<BotAttributeModifier> angryModifiers;
    public bool applyFace;

    public RngList<BotLootData> loot;

    public float baseMovementSpeed = 7;
    public float baseAttackSpeed = 5;
    public float baseBodyHealth = 50;
    public float baseHeadHealth = 20;
    public float baseAccuracy = 0f;
    public float baseDamageResistance = 0;
    public float baseKnockbackResistance = 0;
}
