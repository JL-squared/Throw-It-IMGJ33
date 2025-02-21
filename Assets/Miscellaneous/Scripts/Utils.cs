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

    public static void PlaySound(AudioSource source, AddressablesRegistry<AudioClip> registry, float volume = 1.0f) {
        source.clip = registry.data.Random().Item2;
        source.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
        source.Play();
    }

    public static Vector3 WarpNoise(float x) {
        return new Vector3(Mathf.PerlinNoise1D(x), Mathf.PerlinNoise1D(x - 12.31546f), Mathf.PerlinNoise1D(x + 3.5654f)) * 2f - Vector3.one;
    }

    public static void PlaySound(AddressablesRegistry<AudioClip> registry, float volume = 1.0f) {
        GameObject obj = new GameObject();
        AudioSource source = obj.AddComponent<AudioSource>();
        source.volume = volume;
        source.clip = registry.data.Random().Item2;
        source.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
        source.Play();
        UnityEngine.Object.Destroy(obj, source.clip.length / source.pitch);
    }

    public static void PlaySound(Vector3 point, AddressablesRegistry<AudioClip> registry, float volume = 1.0f) {
        GameObject obj = new GameObject();
        obj.transform.position = point;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.spatialize = true;
        source.spatialBlend = 1.0f;
        source.volume = volume;
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
                health.Damage(factor * damage, new EntityHealth.DamageSourceData {
                    source = null,
                    direction = collider.transform.position - center,
                });
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

    // Loads a string from a file, creating it if given a callback
    public static string LoadString(string file, Func<string> missingCallback = null) {
        if (!File.Exists(PersistentDir + "/" + file)) {
            if (missingCallback == null) {
                return null;
            }

            Debug.LogWarning("File : " + file + " does not exist !");
            string pt = missingCallback.Invoke();
            SaveString(file, pt);
            return pt;
        }


        string data = File.ReadAllText(PersistentDir + "/" + file);
        return data;

    }

    // Loads a general object using Json conversion
    public static T Load<T>(string file, bool createIfMissing=true, bool resave=false) where T: class, new() {
        Func<string> missingCallback = null;
        
        if (createIfMissing) {
            missingCallback = () => {
                return JsonConvert.SerializeObject(new T());
            };
        }

        string data = LoadString(file, missingCallback);
        T obj = JsonConvert.DeserializeObject<T>(data);

        if (resave)
            Save(file, obj);
        return obj;
    }

    // Saves a string as text into a file
    public static void SaveString(string file, string data) {
        File.WriteAllText(PersistentDir + "/" + file, data);
    }

    // Saves a general object using Json conversion
    public static void Save<T>(string file, T data) {
        string stringData = JsonConvert.SerializeObject(data);
        SaveString(file, stringData);
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

    // Jarvis, calculate the absolute whilst somehow smoothing and keeping the range 0-1
    public static float SmoothAbsClamped01(float x, float h) {
        float J(float x) {
            return Mathf.Sqrt(x*x + h) - Mathf.Sqrt(h);
        }

        float b = 1f / J(1);
        float a = J(x) * b;
        return Mathf.Clamp01(a);
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