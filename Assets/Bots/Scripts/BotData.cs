using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotData", menuName = "ScriptableObjects/New Bot Type", order = 1)]
public class BotData : ScriptableObject {
    public List<BotPart> back;
    public List<BotPart> left;
    public List<BotPart> right;
    public List<BotPart> hat;
    public List<BotPart> neck;
    public List<BotPart> leftEye;
    public List<BotPart> rightEye;
    public List<BotPart> nose;

    public float angryChance = 0.5f;
    public List<BotAttributeModifier> angryModifiers;

    public float baseMovementSpeed = 7;
    public float baseAttackSpeed = 5;
    public float baseBodyHealth = 50;
    public float baseHeadHealth = 20;
    public float baseDamageResistence = 0;
}
