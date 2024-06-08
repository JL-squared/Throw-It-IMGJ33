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

    private List<BotWorldPart> worldParts;

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

    private void SpawnPart(GameObject holster, BotPartData part) {
        GameObject spawned = Instantiate(part.prefab, holster.transform);
        ApplyModifiers(part.modifiers);

        BotWorldPart worldPart = spawned.GetComponent<BotWorldPart>();
        if (worldPart != null) {
            worldPart.botBase = this;
            worldParts.Add(worldPart);
        }
    }

    private BotPartData PickPartForHolsterType(GameObject holster, List<BotPartData> parts) {
        foreach (var part in parts) {
            if (Random.value < part.spawnChance) {
                SpawnPart(holster, part);
                return part;
            }
        }

        return null;
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
        PickPartForHolsterType(backHolster, data.back);
        PickPartForHolsterType(leftHolster, data.left);
        PickPartForHolsterType(rightHolster, data.right);

        // Spawns at LEAST one default eye if needed
        if (data.spawnAtLeastOneDefaultEye) {
            if (Random.value > 0.5) {
                PickPartForHolsterType(leftEyeHolster, data.leftEye);
                SpawnPart(rightEyeHolster, new BotPartData(data.defaultEye));
            } else {
                SpawnPart(leftEyeHolster, new BotPartData(data.defaultEye));
                PickPartForHolsterType(rightEyeHolster, data.rightEye);
            }
        } else {
            PickPartForHolsterType(leftEyeHolster, data.leftEye);
            PickPartForHolsterType(rightEyeHolster, data.rightEye);
        }

        // Cute Cosmetics :3
        PickPartForHolsterType(hatHolster, data.hat);
        PickPartForHolsterType(neckHolster, data.neck);
        PickPartForHolsterType(noseHolster, data.nose);

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
        worldParts = new List<BotWorldPart>();
        pathfinder = GetComponent<BotPathfinder>();
        entityMovement = GetComponent<EntityMovement>();

        SpawnParts();
        ApplyAngry();
        ApplyAttributes();
    }
}