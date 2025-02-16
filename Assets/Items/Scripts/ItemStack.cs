using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ItemStack {
    [JsonProperty]
    public int count;

    [JsonIgnore]
    public int Count { 
        get { 
            return count; 
        } 
        set {
            count = value;
            if (count == 0) {
                Data = null;
                onEmpty?.Invoke();
            }
            onUpdate?.Invoke();
        }
    }

    [JsonIgnore]
    [HideInInspector]
    public UnityEvent onEmpty = new UnityEvent();
    [HideInInspector]
    [JsonIgnore]
    public UnityEvent onUpdate = new UnityEvent();

    [JsonProperty(PropertyName = "id")]
    [JsonConverter(typeof(ItemDataConverter))]
    public ItemData data;

    [JsonIgnore]
    public ItemData Data { get { return data; } 
        set { 
            data = value;
            InternalRefreshItemLogic();
            onUpdate?.Invoke(); 
        } 
    }

    public ItemStack() {
        this.count = 0;
        this.Data = null;
        this.logic = null; 
    }

    // this would have to be assigned on data change?
    // yes
    public Item logic;

    private void InternalRefreshItemLogic() {
        this.logic = data != null ? Registries.GetItem(data) : null;

        if (this.logic != null) {
            this.logic.ItemReference = this;
        }
    }

    public ItemStack(ItemData data, int count = 1) {
        this.count = count;
        this.Data = data;
        InternalRefreshItemLogic();
    }

    public ItemStack(string id, int count) {
        this.Data = Registries.items[id];
        this.count = count;
        InternalRefreshItemLogic();
    }

    public void CopyItem(ItemStack other) {
        Data = other.Data;
        Count = other.Count;
    }

    public bool IsEmpty() {
        return Data == null || Count == 0;
    }

    public bool IsFull() {
        return Data == null ? false : Count == Data.stackSize;
    }

    public ItemStack Clone() {
        return new ItemStack(data, count);
    }

    public bool Equals(ItemStack other) {
        return other.Data == Data && other.count >= count;
    }

    public void MakeEmpty() {
        Count = 0;
    }

    public ItemStack NewCount(int i) {
        ItemStack clone = Clone();
        clone.count = i;
        clone.InternalRefreshItemLogic();
        return clone;
    }

    public override string ToString() {
        string id = data == null ? "none" : data.ToString();
        return $"ID: {id}\nCount: {count}";
    }

    public static implicit operator string(ItemStack item) {
        return item.ToString();
    }
}