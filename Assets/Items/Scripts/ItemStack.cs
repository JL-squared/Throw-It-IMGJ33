using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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

    public void SwapItem(ref ItemStack other, bool partial = false) { // Other is assumed to be the cursor since the slot is the one receiving events // Partial is essentially right click
        if (IsEmpty() && other.IsEmpty())
            return; // nothing burger ahh interaction

        // logic for if item types aren't the same
        if ((other.IsEmpty() && !IsEmpty() || !other.IsEmpty() && IsEmpty()) && partial) { // Initiate swap
            if(IsEmpty()) { // This is just completely mimicking minecraft's item slot controls
                Debug.Log("Slot is empty, sigma");
                CopyItem(new ItemStack(other.Data, 1));
                other.Count = other.Count - 1;
            } else {
                int half = Count / 2;
                int otherHalf = Count - half;
                var data = Data;
                Count = half;
                other.CopyItem(new ItemStack(data, otherHalf));
            }
        } else if (other.Data == this.Data) { // otherwise
            // sigma sigma boy sigma boy sigma boy
            if(partial && !IsFull()) {
                other.Count -= 1;
                Count += 1;
            } else if (!IsFull()) { // if we can fit anything, take as much possible from other
                var amountWeCanPutIn = Data.stackSize - Count;
                amountWeCanPutIn = Math.Min(amountWeCanPutIn, other.Count);
                other.Count -= amountWeCanPutIn;
                Count += amountWeCanPutIn;
            }
        } else { // Generic swap
            var temp = Clone();
            CopyItem(other);
            other.CopyItem(temp);
        }
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