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
    public static AddressableRegistry<ItemData> itemsData;
    public static AddressableRegistry<BotData> botsData;
    private static Dictionary<Type, Item> items = new Dictionary<Type, Item>();
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init() {
        itemsData = new AddressableRegistry<ItemData>("Items");
        botsData = new AddressableRegistry<BotData>("Bots");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RegisterItems() {
        RegisterItem<ItemData>(new Item());
        RegisterItem<ShovelItemData>(new ShovelItem());
        RegisterItem<SnowballItemData>(new SnowballItem());
    }

    public static Item GetItem(ItemData dataType) {
        if (dataType == null) return null;
        items.TryGetValue(dataType.GetType(), out var item);
        return (Item)item.Clone();
    }

    static void RegisterItem<T>(Item item) where T : ItemData {
        items.Add(typeof(T), item);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() {
        items = new Dictionary<Type, Item>();
    }
}
