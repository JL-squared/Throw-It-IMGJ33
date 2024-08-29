using Newtonsoft.Json;
using System;
using UnityEngine;

// One major disadvantage of having stuff inherit from a scriptable object
// is that you can actually write mono-behavior type code (like update and start) 
// since you can't store persistent values within SOs (otherwise it would modify them in the editor)
[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/New Item Data", order = 1)]
public class ItemData : ScriptableObject {
    [Header("Settings")]
    public int stackSize;
    public int marketLimit;
    public int marketBuyCost;
    public int marketSellCost;

    [Header("Visual Assets")]
    public Sprite icon;
    public ModelOffset viewModelOffset;
    public ModelOffset worldModelOffset;
    public GameObject model;

    [Header("Language Assets (Locale WIP)")]
    public string title;
    [TextArea(15, 20)]
    public string description;

    public override string ToString() {
        return base.name;
    }

    public static implicit operator string(ItemData i) => i.ToString();
}

public class ItemDataConverter : JsonConverter {
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        return Registries.items[serializer.Deserialize<string>(reader)];
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        ItemData item = (ItemData)value;
        writer.WriteValue(item.name);
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(ItemData);
    }
}