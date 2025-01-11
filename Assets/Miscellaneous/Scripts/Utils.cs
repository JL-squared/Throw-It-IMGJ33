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
using System.Linq;

public static class Utils {
    private static AddressablesRegistry<ItemData> itemRegistry;

    public static void PlaySound(AudioSource source, AddressablesRegistry<AudioClip> registry) {
        source.clip = registry.data.Random().Item2;
        source.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
        source.Play();
    }

    public static void PlaySound(Vector3 point, AddressablesRegistry<AudioClip> registry) {
        GameObject obj = new GameObject();
        obj.transform.position = point;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.spatialize = true;
        source.spatialBlend = 1.0f;
        source.clip = registry.data.Random().Item2;
        source.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
        source.Play();
        UnityEngine.Object.Destroy(obj, source.clip.length / source.pitch);
    }

    public static void BlowUp(Vector3 position, float damage=50, float force=65, float radius=2, float editStrength=30f, float editRadiusOffset=1f) {
        /*
        IVoxelEdit edit = new SphericalExplosionVoxelEdit {
            center = position,
            strength = editStrength,
            material = 0,
            radius = radius + editRadiusOffset,
        };

        if (VoxelTerrain.Instance != null) {
            VoxelTerrain.Instance.ApplyVoxelEdit(edit, neverForget: true, symmetric: false);
        }
        */
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

    public static string PersistentDir = Application.persistentDataPath;
    static JsonSerializerSettings settings2 = InitSerializer();

    public static JsonSerializerSettings InitSerializer() {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.Converters.Add(new StringEnumConverter(new KebabCaseNamingStrategy()));
        settings.Converters.Add(new ItemDataConverter());
        settings.Converters.Add(new Vector2Converter());
        settings.Converters.Add(new Vector3Converter());
        settings.Converters.Add(new Vector4Converter());
        settings.Converters.Add(new QuaternionConverter());
        settings.Formatting = Formatting.Indented;
        settings.ObjectCreationHandling = ObjectCreationHandling.Reuse;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.DefaultValueHandling = DefaultValueHandling.Include;
        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.TypeNameHandling = TypeNameHandling.Auto;
        JsonConvert.DefaultSettings = () => settings;

        return settings;
    }

    public static T Load<T>(string file, bool createIfMissing=true, bool resave=false) where T: class, new() {
        if (!File.Exists(PersistentDir + "/" + file)) {
            if (!createIfMissing) {
                return null;
            }

            Debug.LogWarning("File : " + file + " does not exist !");
            T val = new T();
            Save(file, val);
            return val;
        }


        string data = File.ReadAllText(PersistentDir + "/" + file);
        T obj = JsonConvert.DeserializeObject<T>(data);

        if (resave)
            Save(file, obj);
        return obj;
    }

    public static string Save<T>(string file, T data) {
        string stringData = JsonConvert.SerializeObject(data);
        File.WriteAllText(PersistentDir + "/" + file, stringData);
        return stringData;
    }

    // Jarvis, scan this guys balls
    public static void KillChildren(this Transform owner, Action<GameObject> method = null) {
        for (var i = owner.childCount - 1; i >= 0; i--) {
            var obj = owner.GetChild(i).gameObject;
            method?.Invoke(obj);
            UnityEngine.Object.Destroy(obj);
        }
    }

    // Jarvis, give this man testicular torsion
    public static void ExecuteChildrenRecursive(Transform root, Action<GameObject> method) {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children) {
            method?.Invoke(child.gameObject);
        }
    }
    
    // Jarvis, randomize his balls
    public static (int, V) Random<K, V>(this Dictionary<K, V> dict) {
        // very stupid but it works
        // https://stackoverflow.com/questions/1028136/random-entry-from-dictionary
        int index = UnityEngine.Random.Range(0, dict.Count);
        return (index, dict.ToList()[index].Value);
    }

    // Jarvis, randomize his balls but exclude a single number
    public static (int, V) Random<K, V>(this Dictionary<K, V> dict, int exclusionIndex) {
        if (exclusionIndex >= dict.Count || exclusionIndex < 0) {
            return Random(dict);
        }

        int lower = UnityEngine.Random.Range(0, exclusionIndex);
        int upper = UnityEngine.Random.Range(exclusionIndex+1, dict.Count);
        int rng = UnityEngine.Random.value > 0.5f ? lower : upper;

        // very stupid but it works
        // https://stackoverflow.com/questions/1028136/random-entry-from-dictionary
        return (rng, dict.ToList()[rng].Value);
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

    public static Quaternion SafeLookRotation(this Vector3 lookAt, float epsilon = 0.01f) {
        if (lookAt.magnitude < epsilon) {
            return Quaternion.identity;
        } else {
            return Quaternion.LookRotation(lookAt);
        }
    }
}