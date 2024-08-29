using Newtonsoft.Json;
using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

// One major disadvantage of having stuff inherit from a scriptable object
// is that you can actually write mono-behavior type code (like update and start) 
// since you can't store persistent values within SOs (otherwise it would modify them in the editor)
[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/New Item Data", order = 1)]
public class ItemData : ScriptableObject {
    public int stackSize;

    public Sprite icon;
    public GameObject worldItem;
    public GameObject viewModel;
    public Type equippedLogic;

    public string name;
    [TextArea(15, 20)]
    public string description;

    public int marketLimit;
    public int marketBuyCost;
    public int marketSellCost;

    public override string ToString() {
        return base.name;
    }

    public static implicit operator string(ItemData i) => i.ToString();
}

public class ItemDataConverter : JsonConverter {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        writer.WriteValue(((ItemData)value).name);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        var t = Registries.items[reader.ReadAsString()];
        return t;
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(ItemData);
    }
}