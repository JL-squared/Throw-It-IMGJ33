using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;



[Serializable]
public class EntityData {
    public Vector3? position;
    public Vector3? velocity;
    public Vector3? angularVelocity;
    public Quaternion? rotation;
    public float? health;
    public float? timeSinceDeath;
    public List<Item> inventory;
    public ItemData item;
    public Vector2? wishHeadDir;
    public SavedBotData bot;
    public bool icicleHit;
    public Guid guid;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public string name;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public bool spawn;
}

[Serializable]
public class SavedBotData {
    public int center;
    public int left;
    public int right;
    public int hat;
    public int neck;
    public int leftEye;
    public int rightEye;
    public int nose;
    public int heads;
    public bool angry;
}

interface IEntitySerializer {
    public void Serialize(EntityData data);
    public void Deserialize(EntityData data);
}

[Serializable]
public class SaveState {
    public List<EntityData> entities;

    [JsonIgnore]
    public Dictionary<EntityData, GameObject> objects;

    public static SaveState Save() {
        SaveState state = new SaveState();

        List<(GameObject, EntityData)> temp = UnityEngine.Object.FindObjectsOfType<Entity>().AsEnumerable()
            .Where(x => x.flags.HasFlag(EntityFlags.Serialize))
            .Select((x) => (x.gameObject, new EntityData()))
            .ToList();

        state.entities = new List<EntityData>();
        foreach ((GameObject go, EntityData data) in temp) {
            Entity entity = go.GetComponent<Entity>();

            if (!entity.flags.HasFlag(EntityFlags.Serialize)) {
                continue;
            }

            data.name = entity.addressablesPrefabName;
            data.spawn = entity.flags.HasFlag(EntityFlags.Spawn);

            foreach (var serializer in go.GetComponents<IEntitySerializer>()) {
                serializer.Serialize(data);
            }

            state.entities.Add(data);
        }

        /*
        int counter = 0;
        foreach (var entity in entities) {
            Rigidbody rb = entity.GetComponent<Rigidbody>();
            EntityMovement movement = entity.GetComponent<EntityMovement>();
            Player player = entity.GetComponent<Player>();

            if (movement != null) {
                EntityMovementData data = new EntityMovementData {
                    position = movement.transform.position,
                    velocity = movement.movement,
                    rotation = movement.transform.rotation,
                    entity = counter,
                };

                state.movements.Add(data);
            }

            if (rb != null) {
                RigidbodyData data = new RigidbodyData {
                    position = rb.position,
                    velocity = rb.velocity,
                    angularVelocity = rb.angularVelocity,
                    rotation = rb.rotation,
                    entity = counter,
                };

                state.rigidbodies.Add(data);
            }

            if (player != null) {

            }

            counter++;
        }

        */
        return state;
    }


    public void Loaded() {
        objects = new Dictionary<EntityData, GameObject>();
        Entity[] alrEntities = UnityEngine.Object.FindObjectsOfType<Entity>();
        foreach (var entity in alrEntities) {
            if (entity.flags.HasFlag(EntityFlags.DestroyExistingOnDeserialize)) {
                UnityEngine.Object.Destroy(entity.gameObject);
            }
        }

        Dictionary<Guid, Entity> lookup = new Dictionary<Guid, Entity>();

        foreach (var entity in alrEntities) {
            lookup.Add(entity.guid, entity);
        }

        foreach (var data in entities) {
            GameObject go = null;
            if (data.spawn) {
                go = Registries.Summon(data.name, data);
            } else {
                go = lookup[data.guid].gameObject;
            }

            objects.Add(data, go);
        }

        GameManager.Instance.StartCoroutine(Start());
    }

    IEnumerator Start() {
        yield return 0;

        foreach (var item in objects) {
            if (item.Value != null) {
                foreach (var serializer in item.Value.GetComponents<IEntitySerializer>()) {
                    serializer.Deserialize(item.Key);
                }
            }
        }
    }

    public void AfterLoaded() {

    }
}
