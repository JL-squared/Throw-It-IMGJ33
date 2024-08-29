using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEditor;
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
    public List<ItemStack> inventory;
    public ItemStack itemStack;
    public Vector2? wishHeadDir;
    public SavedBotData bot;
    public bool icicleHit;
    public Guid guid;
    public List<EntityData> children;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public string name;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public bool spawn;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public bool recurse;
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

        List<GameObject> temp = UnityEngine.Object.FindObjectsOfType<Entity>().AsEnumerable()
            .Where(x => {
                Transform obj = x.gameObject.transform;
                while (obj != null) {
                    obj = obj.transform.parent;
                    if (obj != null && obj.GetComponent<Entity>() != null) {
                        return false;
                    }
                }

                return true;
            })
            .Select(x => x.gameObject)
            .ToList();

        void Ohio(GameObject go, EntityData data, int depth) {
            Entity entity = go.GetComponent<Entity>();

            if (depth > 10) {
                return;
            }

            if (entity.flags.HasFlag(EntityFlags.Serialize)) {

                foreach (var serializer in go.GetComponents<IEntitySerializer>()) {
                    serializer.Serialize(data);
                }
            }

            if (entity.flags.HasFlag(EntityFlags.Recurse)) {
                Entity[] children = entity.GetComponentsInChildren<Entity>();
                EntityData[] childrenDatas = new EntityData[children.Length-1];

                for (int i = 0; i < children.Length - 1; i++) {
                    childrenDatas[i] = new EntityData();
                }

                int index = 0;
                foreach (var item in children) {
                    if (item.gameObject != go) {
                        Ohio(item.gameObject, childrenDatas[index], depth + 1);
                        index++;
                    }
                }

                data.children = childrenDatas.ToList();

                if (data.children.Count == 0) {
                    data.children = null;
                }
            } else {
                data.children = null;
            }

            if (entity.flags.HasFlag(EntityFlags.Serialize)) {
                state.entities.Add(data);
            }
        }

        state.entities = new List<EntityData>();
        foreach (GameObject go in temp) {
            Ohio(go, new EntityData(), 0);
        }

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

        void Skibi(EntityData data, GameObject go=null) {
            if (data.spawn) {
                go = Registries.Summon(data.name, data);
            } else {
                if (data.name == "player") {
                    go = Player.Instance.gameObject;
                }
            }

            // if this is a parent node, then add its children as objects as well
            if (data.children != null && data.recurse) {
                Dictionary<string, Entity> entities = go.GetComponentsInChildren<Transform>().AsEnumerable().Where(x => x.gameObject != go).Select(x => x.GetComponent<Entity>()).Where(x => x != null).ToDictionary(x => x.identifier);
                foreach (var item in data.children) {
                    if (!item.spawn) {
                        Skibi(item, entities[item.name].gameObject);
                    }
                }
            }

            objects.Add(data, go);
        }

        foreach (var data in entities) {
            Skibi(data);
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
}
