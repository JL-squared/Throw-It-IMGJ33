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

public static class Registries {
    public static AddressablesRegistry<ItemData> items;
    public static AddressablesRegistry<BotData> bots;
    public static AddressablesRegistry<GameObject> projectiles;
    public static AddressablesRegistry<GameObject> vehicles;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init() {
        items = new AddressablesRegistry<ItemData>("Items");
        bots = new AddressablesRegistry<BotData>("Bots");
        projectiles = new AddressablesRegistry<GameObject>("Projectiles");
    }

    public static GameObject Summon(string id, EntityData data) {
        string[] args = id.Split(':');
        string basic = args[0];
        string name = args[1];

        GameObject sauce = null;
        switch (basic) {
            case "bots":
                sauce = BotBase.Summon(bots[name], data.position.Value, data.rotation.Value);
                break;
            case "projectiles":
                sauce = Projectile.Spawn((ProjectileItemData)items[name], data.position.Value, data.velocity.Value);
                break;
            case "vehicles":
                //sauce = vehicles;
                break;
            default:
                sauce = null;
                break;
        };

        return sauce;
    }
}
