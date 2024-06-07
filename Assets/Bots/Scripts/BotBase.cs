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
    public GameObject noseHolster;

    public GameObject angryFace;
    public GameObject happyFace;

    public float movementSpeed = 0;
    public float attackSpeed = 0;
    public float bodyHealth = 0;
    public float headHealth = 0;
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
                return;
            }
        }
    }

    private void ApplyAttributes() {
        entityMovement.speed = movementSpeed;
    }

    private void SpawnParts() {
        BotData data = datas[0];

        movementSpeed = data.baseMovementSpeed;
        attackSpeed = data.baseAttackSpeed;
        bodyHealth = data.baseBodyHealth;
        headHealth = data.baseHeadHealth;
        damageResistence = data.baseDamageResistence;
        
        // Base weapons / attribute modifiers
        SpawnPartsForHolsterType(backHolster, data.back);
        SpawnPartsForHolsterType(leftHolster, data.left);
        SpawnPartsForHolsterType(rightHolster, data.right);
        SpawnPartsForHolsterType(leftEyeHolster, data.leftEye);
        SpawnPartsForHolsterType(rightEyeHolster, data.rightEye);

        // Cute Cosmetics :3
        SpawnPartsForHolsterType(hatHolster, data.hat);
        SpawnPartsForHolsterType(neckHolster, data.neck);
        SpawnPartsForHolsterType(noseHolster, data.nose);

        leftHolster.transform.localScale = new Vector3(-1f, 1f, 1f);
        leftEyeHolster.transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void ApplyAngry() {
        bool angy = Random.value < datas[0].angryChance;
        happyFace.SetActive(!angy);
        angryFace.SetActive(angy);

        if (angy) {
            ApplyModifiers(datas[0].angryModifiers);
        }
    }

    public void Start() {
        pathfinder = GetComponent<BotPathfinder>();
        entityMovement = GetComponent<EntityMovement>();

        SpawnParts();
        ApplyAngry();
        ApplyAttributes();
    }
}