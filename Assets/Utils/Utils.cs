using System.IO;
using Unity.Mathematics;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;

public static class Utils {
    public static void ApplyExplosionKnockback(Vector3 center, float radius, Collider[] colliders, float force) {
        float rbExplosionFactor = 100f;

        foreach (Collider collider in colliders) {
            var rb = collider.gameObject.GetComponent<Rigidbody>();
            var movement = collider.gameObject.GetComponent<EntityMovement>();

            if (rb != null) {
                rb.AddExplosionForce(force * rbExplosionFactor, center, radius);
            }

            if (movement != null) {
                movement.ExplosionAt(center, force, radius);
            }
        }
    }

    public static void ApplyExplosionDamage(Vector3 center, float radius, Collider[] colliders, float minDamageRadius, float damage) {
        foreach (Collider collider in colliders) {
            var health = collider.gameObject.GetComponent<EntityHealth>();

            if (health != null) {
                float dist = Vector3.Distance(center, collider.transform.position);

                // 1 => closest to bomb
                // 0 => furthest from bomb 
                float factor = 1 - Mathf.Clamp01(math.unlerp(minDamageRadius, radius, dist));
                health.Damage(factor * damage);
            }
        }
    }

    static string PersistentDir = Application.persistentDataPath;
    static JsonSerializerSettings settings = InitSerializer();

    public static JsonSerializerSettings InitSerializer() {
        JsonSerializerSettings serializer = new JsonSerializerSettings();
        serializer.Converters.Add(new StringEnumConverter(new KebabCaseNamingStrategy()));
        serializer.Formatting = Formatting.Indented;
        return serializer;
    }

    public static T Load<T>(string file, T defaultValue = null) where T : class {
        if (!File.Exists(PersistentDir + "/" + file)) {
            Debug.LogWarning("File : " + file + " does not exist !");
            Save(file, defaultValue);
            return defaultValue;
        }

        object obj = defaultValue;
        string data = File.ReadAllText(PersistentDir + "/" + file);
        obj = JsonConvert.DeserializeObject<T>(data, settings);
        Save(file, obj);
        return (T)obj;
    }

    public static void Save<T>(string file, T data) where T : class {
        string stringData = JsonConvert.SerializeObject(data, settings);
        File.WriteAllText(PersistentDir + "/" + file, stringData);
    }

    // Jarvis, scan this guys balls
    public static void KillChildren(Transform owner, Action<GameObject> method = null) {
        for (var i = owner.childCount - 1; i >= 0; i--) {
            var obj = owner.GetChild(i).gameObject;
            method?.Invoke(obj);
            UnityEngine.Object.Destroy(obj);
        }
    }
}
