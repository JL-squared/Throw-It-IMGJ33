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
    public static AddressableRegistry<ItemData> items;
    public static AddressableRegistry<BotData> bots;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init() {
        items = new AddressableRegistry<ItemData>("Items");
        //bots = new AddressableRegistry<BotData>("Bots");
    }
}
