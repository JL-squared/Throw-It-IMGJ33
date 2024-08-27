using System.IO;
using Unity.Mathematics;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem.Utilities;

public static class Utils {
    private static AddressableRegistry<ItemData> itemRegistry;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Initialize() {
    }

    public static void BlowUp(Vector3 position, float damage=50, float force=65, float radius=2, float editStrength=30f, float editRadiusOffset=1f) {
        IVoxelEdit edit = new SphericalExplosionVoxelEdit {
            center = position,
            strength = editStrength,
            material = 0,
            radius = radius + editRadiusOffset,
        };

        if (VoxelTerrain.Instance != null) {
            VoxelTerrain.Instance.ApplyVoxelEdit(edit, neverForget: true, symmetric: false);
        }

        Collider[] colliders = Physics.OverlapSphere(position, radius);
        Utils.ApplyExplosionKnockback(position, radius, colliders, force);
        Utils.ApplyExplosionDamage(position, radius, colliders, 1, damage);
    }

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
        serializer.Converters.Add(new ItemDataConverter());
        serializer.Formatting = Formatting.Indented;
        serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
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
    public static void KillChildren(this Transform owner, Action<GameObject> method = null) {
        for (var i = owner.childCount - 1; i >= 0; i--) {
            var obj = owner.GetChild(i).gameObject;
            method?.Invoke(obj);
            UnityEngine.Object.Destroy(obj);
        }
    }

    public static float SmoothAbsClamped01(float x, float h) {
        float J(float x) {
            return Mathf.Sqrt(x*x + h) - Mathf.Sqrt(h);
        }

        float b = 1f / J(1);
        float a = J(x) * b;
        return Mathf.Clamp01(a);
    }


    // Load all the types of an Adressable using its label into memory
    public static void GetAllTypes<T>(ref Dictionary<string, T> types, string label) where T : ScriptableObject {
        var temp = new Dictionary<string, T>();
        types = temp;

        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, (x) => {
            temp.TryAdd(x.name, x);
        });

        handle.WaitForCompletion();
    }

    public static bool IsNullOrDestroyed(this object value) {
        return ReferenceEquals(value, null) || value.Equals(null);
    }
}
