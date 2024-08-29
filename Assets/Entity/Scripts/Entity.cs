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
    public EntityFlags flags = EntityFlags.Default;
    public EntityUnityFlags unityFlags = EntityUnityFlags.Rigidbody;

    public void Deserialize(EntityData data) {
        var rb = GetComponent<Rigidbody>();
        if (rb != null && unityFlags.HasFlag(EntityUnityFlags.Rigidbody)) {
            rb.position = data.position.Value;
            rb.rotation = data.rotation.Value;

            if (!rb.isKinematic) {
                rb.angularVelocity = data.angularVelocity.Value;
                rb.velocity = data.velocity.Value;
            }
        }

        if (unityFlags.HasFlag(EntityUnityFlags.Transform)) {
            transform.position = data.position.Value;
            transform.rotation = data.rotation.Value;
        }
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
    }
}

[Flags]
public enum EntityFlags {
    None,
    Serialize,
    Spawn,
    DestroyExistingOnDeserialize,
    Default = Spawn | Serialize | DestroyExistingOnDeserialize,
}

[Flags]
public enum EntityUnityFlags {
    None,
    Rigidbody,
    Transform,
}