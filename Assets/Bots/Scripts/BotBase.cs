using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BotBase : MonoBehaviour {
    public List<BotData> datas;
    public GameObject head;
    public GameObject backHolster;
    public GameObject leftHolster;
    public GameObject rightHolster;
    public GameObject leftEyeHolster;
    public GameObject rightEyeHolster;
    public GameObject hatHolster;
    public GameObject neckHolster;

    public float movementSpeed = 7;
    public float attackSpeed = 5;
    public float bodyHealth = 50;
    public float headHealth = 20;
    public float damageResistence = 0;

    private EntityMovement entityMovement;
    private EntityHealth _headHealth;
    private EntityHealth _bodyHealth;
    private BotPathfinder pathfinder;

    private void ApplyModifiers(List<BotAttributeModifier> modifiers) {
        foreach (var modifier in modifiers) {
            float delta = modifier.delta;
            switch (modifier.attribute) {
                case BotAttribute.MovementSpeed:
                    movementSpeed += delta;
                    break;
                case BotAttribute.AttackSpeed:
                    attackSpeed += delta;
                    break;
                case BotAttribute.HeadHealth:
                    headHealth += delta;
                    break;
                case BotAttribute.BodyHealth:
                    bodyHealth += delta;
                    break;
                case BotAttribute.DamageResistence:
                    damageResistence += delta;
                    break;
            }
        }
    }

    private void SpawnPartsForHolsterType(GameObject holster, List<BotPart> parts) {
        foreach (var part in parts) {
            if (Random.value < part.spawnChance) {
                Instantiate(part.prefab, holster.transform);
                ApplyModifiers(part.modifiers);
            }
        }
    }

    private void ApplyAttributes() {
        entityMovement.speed = movementSpeed;
    }

    private void SpawnParts() {
        BotData data = datas[0];
        
        // Base weapons / attribute modifiers
        SpawnPartsForHolsterType(backHolster, data.back);
        SpawnPartsForHolsterType(leftHolster, data.left);
        SpawnPartsForHolsterType(rightHolster, data.right);
        SpawnPartsForHolsterType(leftEyeHolster, data.leftEye);
        SpawnPartsForHolsterType(rightEyeHolster, data.rightEye);

        // Cute Cosmetics :3
        SpawnPartsForHolsterType(hatHolster, data.hat);
        SpawnPartsForHolsterType(neckHolster, data.neck);
        leftHolster.transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    public void Start() {
        pathfinder = GetComponent<BotPathfinder>();
        entityMovement = GetComponent<EntityMovement>();

        SpawnParts();
        ApplyAttributes();
    }
}