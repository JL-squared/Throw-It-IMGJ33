using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Only used as a means to tag any game object that should be saved
// Whenever we save the game, each gameobject with this tag will be converted to a simple unique ID
// Other data will simply refer to this "ID"
public class Entity : MonoBehaviour, IEntitySerializer {
    public string addressablesPrefabName;
    public EntityFlags flags = EntityFlags.Serialize | EntityFlags.Spawn | EntityFlags.DestroyExistingOnDeserialize;
    public EntityUnityFlags unityFlags = EntityUnityFlags.Rigidbody;
    [HideInInspector]
    public Guid guid;

    public void Start() {
        guid = Guid.NewGuid();
    }

    public void Deserialize(EntityData data) {
        var rb = GetComponent<Rigidbody>();
        if (rb != null && unityFlags.HasFlag(EntityUnityFlags.Rigidbody)) {
            RigidbodyInterpolation interpolation = rb.interpolation;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.position = data.position.Value;
            rb.rotation = data.rotation.Value;

            if (!rb.isKinematic) {
                rb.angularVelocity = data.angularVelocity.Value;
                rb.velocity = data.velocity.Value;
            }

            rb.interpolation = interpolation;
        }

        if (unityFlags.HasFlag(EntityUnityFlags.Transform)) {
            transform.position = data.position.Value;
            transform.rotation = data.rotation.Value;
        }

        guid = data.guid;
    }

    public void Serialize(EntityData data) {
        var rb = GetComponent<Rigidbody>();
        if (rb != null && unityFlags.HasFlag(EntityUnityFlags.Rigidbody)) {
            data.position = rb.position;
            data.rotation = rb.rotation;
            data.angularVelocity = rb.angularVelocity;
            data.velocity = rb.velocity;
        }

        if (unityFlags.HasFlag(EntityUnityFlags.Transform)) {
            data.position = transform.position;
            data.rotation = transform.rotation;
        }

        data.guid = guid;
    }
}

[Flags]
public enum EntityFlags {
    None = 0,
    Serialize = 1,
    Spawn = 2,
    DestroyExistingOnDeserialize = 4,
}

[Flags]
public enum EntityUnityFlags {
    None = 0,
    Rigidbody = 1,
    Transform = 2,
}