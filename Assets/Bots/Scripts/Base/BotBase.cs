using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotBase : MonoBehaviour, IEntitySerializer {
    [Header("Main")]
    public BotData data;

    [Header("Procedural Holsters")]
    public GameObject centerHolster;
    public GameObject leftHolster;
    public GameObject rightHolster;
    public GameObject leftEyeHolster;
    public GameObject rightEyeHolster;
    public GameObject hatHolster;
    public GameObject neckHolster;
    public GameObject noseHolster;

    [Header("Head Entity")]
    public GameObject headMeshHolster;
    public GameObject healthHeadEntity;

    [Header("Faces")]
    public GameObject angryFace;
    public GameObject happyFace;

    [Header("Attributes")]
    public float movementSpeed = 0;
    public float attackSpeed = 0;
    public float bodyHealth = 0;
    public float headHealth = 0;
    public float accuracy = 0;
    public float damageResistance = 0;
    public float knockbackResistance = 0;

    private EntityMovement em;
    [HideInInspector]
    public EntityHealth _headHealth;
    [HideInInspector]
    public EntityHealth _bodyHealth;
    private BotTextToSpeech tts;

    // Both bot parts and components
    private List<BotBehaviour> botBehaviours;
    private Vector3 target;

    public GameObject neckObject;
    public GameObject headObject;
    public GameObject headMeshObject;
    public Vector3 lookTarget;
    private float timeSinceDeath;
    private bool deserialized = false;
    private SavedBotData savedBotData;

    public static GameObject Summon(BotData data, Vector3 position, Quaternion rotation) {
        GameObject bot = Instantiate(data.botPrefab, position, rotation);
        bot.GetComponent<BotBase>().data = data;
        bot.GetComponent<BotBase>().deserialized = false;
        return bot;
    }

    public static GameObject Summon(SavedBotData saved, BotData data, Vector3 position, Quaternion rotation) {
        GameObject bot = Instantiate(data.botPrefab, position, rotation);
        bot.GetComponent<BotBase>().data = data;
        bot.GetComponent<BotBase>().deserialized = true;
        bot.GetComponent<BotBase>().savedBotData = saved;
        return bot;
    }

    private void OnValidate() {
        ApplyAttributes();
    }

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
                case BotAttribute.DamageResistance:
                    damageResistance += delta;
                    break;
                case BotAttribute.KnockbackResistance:
                    knockbackResistance += delta;
                    break;
                case BotAttribute.Accuracy:
                    accuracy += delta;
                    break;
            }
        }
    }

    private void SpawnPart(GameObject holster, BotPartData part) {
        GameObject spawned = Instantiate(part.prefab, holster.transform);
        ApplyModifiers(part.modifiers);

        BotBehaviour worldPart = spawned.GetComponent<BotBehaviour>();
        if (worldPart != null) {
            botBehaviours.Add(worldPart);
        }
    }

    private BotPartData PickPartForHolsterType(GameObject holster, RngList<BotPartData> parts, ref int savedIndex) {
        int index = parts.PickRandom();

        if (deserialized) {
            index = savedIndex;
        } else {
            savedIndex = index;
        }

        BotPartData part = index >= 0 ? parts.list[index] : null;
        if (part != null)
            SpawnPart(holster, part);
        return part;
    }

    private void OnPreDamage(ref float damage, bool head) {
        damage *= (1 - damageResistance);
    }

    private void OnDamaged(float damage, bool head) {
        if (timeSinceDeath == 0) { }
            //tts.SayString(head ? "bruh" : "ouch", overwritePlaying: false);
    }

    private void OnHealed(float heal, bool head) {
        if (heal > 5f) {
            //tts.SayString("thank you");
        }
    }

    private void OnKilled(bool headshot) {
        //Destroy(gameObject);

        /*
        BotLootData loot = data.loot.PickRandom();
        if (loot != null)
            WorldItem.Spawn(loot.item, transform.position, Quaternion.identity);
        */
    }

    private void ApplyAttributes() {
        if (em == null)
            return;

        em.speed = Mathf.Max(movementSpeed, 0);
        em.knockbackResistance = Mathf.Clamp01(knockbackResistance);
        _bodyHealth.maxHealth = bodyHealth;
        _bodyHealth.health = bodyHealth;
        _headHealth.maxHealth = headHealth;
        _headHealth.health = headHealth;

        foreach (var part in botBehaviours) {
            part.AttributesUpdated();
        }
    }

    private void SpawnParts() {
        // Base weapons / attribute modifiers
        BotPartData center = PickPartForHolsterType(centerHolster, data.center, ref savedBotData.center);
        if (center == null || !center.tags.Contains("disable sides")) {
            PickPartForHolsterType(leftHolster, data.left, ref savedBotData.left);
            PickPartForHolsterType(rightHolster, data.right, ref savedBotData.right);
        };

        BotPartData head = PickPartForHolsterType(headMeshHolster, data.heads, ref savedBotData.heads);

        // Spawns at LEAST one default eye if needed
        if (data.applyFace) {
            if (data.spawnAtLeastOneDefaultEye) {
                if (Random.value > 0.5) {
                    PickPartForHolsterType(leftEyeHolster, data.leftEye, ref savedBotData.leftEye);
                    SpawnPart(rightEyeHolster, new BotPartData(data.defaultEye));
                } else {
                    SpawnPart(leftEyeHolster, new BotPartData(data.defaultEye));
                    PickPartForHolsterType(rightEyeHolster, data.rightEye, ref savedBotData.rightEye);
                }
            } else {
                PickPartForHolsterType(leftEyeHolster, data.leftEye, ref savedBotData.leftEye);
                PickPartForHolsterType(rightEyeHolster, data.rightEye, ref savedBotData.rightEye);
            }

            PickPartForHolsterType(noseHolster, data.nose, ref savedBotData.nose);
        }
        

        // Check if we have a head we can apply hats on
        if (head.tags.Contains("hattable")) {
            PickPartForHolsterType(hatHolster, data.hat, ref savedBotData.hat);
        }

        PickPartForHolsterType(neckHolster, data.neck, ref savedBotData.neck);
    }

    private void ApplyAngry() {
        bool angy = Random.value < data.angryChance;

        if (deserialized) {
            angy = savedBotData.angry;
        }

        if (data.applyFace) {
            happyFace.SetActive(!angy);
            angryFace.SetActive(angy);
        }

        if (angy) {
            ApplyModifiers(data.angryModifiers);
        }
    }

    public void Start() {
        if (!deserialized) {
            savedBotData = new SavedBotData();
        }

        botBehaviours = GetComponents<BotBehaviour>().ToList();
        em = GetComponent<EntityMovement>();
        _bodyHealth = GetComponent<EntityHealth>();
        _headHealth = healthHeadEntity.GetComponent<EntityHealth>();
        tts = GetComponent<BotTextToSpeech>();

        healthHeadEntity.transform.position = headMeshHolster.transform.position;
        Destroy(headMeshHolster.GetComponent<MeshFilter>());
        Destroy(headMeshHolster.GetComponent<MeshRenderer>());

        // I know this looks ugly but it works for now. Will need to refactor this whole class later anyways
        _bodyHealth.OnPreDamageModifier += (ref float dmg) => OnPreDamage(ref dmg, false);
        _headHealth.OnPreDamageModifier += (ref float dmg) => OnPreDamage(ref dmg, true);
        _bodyHealth.OnDamaged += (float dmg, EntityHealth.DamageSourceData source) => OnDamaged(dmg, false);
        _headHealth.OnDamaged += (float dmg, EntityHealth.DamageSourceData source) => OnDamaged(dmg, true);
        _bodyHealth.OnHealed += (float heal) => OnHealed(heal, false);
        _headHealth.OnHealed += (float heal) => OnHealed(heal, true);
        _bodyHealth.OnKilled += () => { OnKilled(false); };
        _headHealth.OnKilled += () => { OnKilled(true); };

        movementSpeed = data.baseMovementSpeed;
        attackSpeed = data.baseAttackSpeed;
        bodyHealth = data.baseBodyHealth;
        headHealth = data.baseHeadHealth;
        damageResistance = data.baseDamageResistance;
        knockbackResistance = data.baseKnockbackResistance;
        accuracy = data.baseAccuracy;

        SpawnParts();

        foreach (var item in botBehaviours) {
            item.botBase = this;
            item.movement = em;
            item.bodyHealth = _bodyHealth;
            item.headHealth = _headHealth;
            item.botTts = tts;
        }

        ApplyAngry();
        ApplyAttributes();
        em.entityMovementFlags = data.movementFlags;
    }

    float offset = 0.247f;

    public void Update() {
        GameObject player = Player.Instance.gameObject;
        Vector3 velocity = player.GetComponent<EntityMovement>().Velocity;
        target = player.transform.position;

        foreach (var part in botBehaviours) {
            part.targetPosition = target;
            part.targetVelocity = velocity;
        }

        lookTarget = Player.Instance.camera.transform.position;
        float distance = Vector3.Distance(neckObject.transform.position, lookTarget);
        Vector3 thingyMaBob = lookTarget - headObject.transform.position;

        //Debug.Log("New angle should be: " + Mathf.Acos(offset / distance));

        //neckObject.transform.eulerAngles = new Vector3(Mathf.Rad2Deg * Mathf.Acos(offset / distance) - 90f, Mathf.Rad2Deg * Mathf.Atan2(thingyMaBob.x, thingyMaBob.z));
        //headMeshObject.transform.SetPositionAndRotation(headObject.transform.position, headObject.transform.rotation);

        Debug.DrawLine(lookTarget, neckObject.transform.position);
        Debug.DrawLine(neckObject.transform.position, headObject.transform.position);
        Debug.DrawLine(lookTarget, headObject.transform.position);

        float interpolationDeathTime = 2;
        if (_bodyHealth.AlreadyKilled || _headHealth.AlreadyKilled) {
            timeSinceDeath += Time.deltaTime;

            foreach (var part in botBehaviours) {
                part.deathFactor = Mathf.Clamp01(timeSinceDeath / interpolationDeathTime);
            }

            em.speed = Mathf.Max(movementSpeed * (1 - Mathf.Clamp01(timeSinceDeath / interpolationDeathTime)), 0);
            em.rotationSmoothing = (timeSinceDeath / interpolationDeathTime) * 8;

            if (timeSinceDeath > 1) {
                em.entityMovementFlags.RemoveFlag(EntityMovementFlags.AllowedToRotate | EntityMovementFlags.ApplyMovement);
            }
        }
    }

    public void Serialize(EntityData data) {
        data.bot = savedBotData;
        data.timeSinceDeath = timeSinceDeath;
    }

    public void Deserialize(EntityData data) {
        savedBotData = data.bot;
        timeSinceDeath = data.timeSinceDeath.Value;
    }
}