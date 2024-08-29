using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public List<Item> inventory;
    public ItemData data;
    public Vector2? wishHeadDir;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public string name;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public bool spawn;
}

interface IEntitySerializer {
    public void Serialize(EntityData data);
    public void Deserialize(EntityData data);
}

[Serializable]
public class SaveState {
    public List<EntityData> entities;

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
        foreach (var data in entities) {
            GameObject go = null;
            if (!data.spawn && data.name == "player") {
                go = Player.Instance.gameObject;
            }

            // do summoning shit here idk vro

            if (go != null) {
                foreach (var serializer in go.GetComponents<IEntitySerializer>()) {
                    serializer.Deserialize(data);
                }
            }
        }
        
        /*
        List<GameObject> objects = new List<GameObject>();
        foreach (var entity in entities) {
            if (entity.flags.HasFlag(EntitySerializationFlags.Spawn)) {
                // spawn stuff
            }
        }

        foreach (var movement in movements) {
            Debug.Log(entities[movement.entity].name);
            if (entities[movement.entity].name == "player") {
                Debug.Log("bruh");
                
            }
        }
        */
    }
}
