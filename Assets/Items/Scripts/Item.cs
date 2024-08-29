using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Item {
    [JsonProperty]
    int count;

    [JsonIgnore]
    public int Count { 
        get { 
            return count; 
        } 
        set {
            count = value;
            if (count == 0) {
                Data = null;
                emptyEvent?.Invoke();
            }
            updateEvent.Invoke(this);
        }
    }

    [JsonIgnore]
    public UnityEvent emptyEvent = new UnityEvent();
    [JsonIgnore]
    public UnityEvent<Item> updateEvent = new UnityEvent<Item>();

    [JsonProperty]
    [JsonConverter(typeof(ItemDataConverter))]
    private ItemData data;

    [JsonIgnore]
    public ItemData Data { get { return data; } set { data = value; updateEvent.Invoke(this); } }

    public Item() {
        this.count = 0;
        this.Data = null;
    }

    public Item(ItemData data, int count) {
        this.count = count;
        this.Data = data;
    }

    public Item(string id, int count) {
        this.Data = Registries.items[id];
        this.count = count;
    }

    public void CopyItem(Item other) {
        Data = other.Data;
        Count = other.Count;
    }

    public bool IsEmpty() {
        return Data == null || Count == 0;
    }

    public bool IsFull() {
        return Data == null ? false : Count == Data.stackSize;
    }

    public Item Clone() {
        return new Item(data, count);
    }

    public bool Equals(Item other) {
        return other.Data == Data && other.count >= count;
    }

    public void MakeEmpty() {
        Count = 0;
    }

    public override string ToString() {
        string id = data == null ? "none" : data.ToString();
        return $"ID: {id}\nCount: {count}";
    }

    public static implicit operator string(Item item) {
        return item.ToString();
    }
}