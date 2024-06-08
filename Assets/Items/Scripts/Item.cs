using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;

public class Item {
    [SerializeField]
    int count;
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

    public UnityEvent emptyEvent = new UnityEvent();
    public UnityEvent<Item> updateEvent = new UnityEvent<Item>();

    private ItemData data;
    public ItemData Data { get {return data; } set { data = value; updateEvent.Invoke(this); } }


    public Item (int count = 0, ItemData data = null) {
        this.count = count;
        this.Data = data;
    }

    public void CopyItem(Item other) {
        Data = other.Data;
        Count = other.Count;
    }

    public static void SpawnInWorld(Vector3 p) {
        // search up prefab
    }

    public bool IsEmpty() {
        return Data == null || Count == 0;
    }

    public bool IsFull() {
        return Data == null ? false : Count == Data.stackSize;
    }

    public Item Clone() {
        return new Item(count, data);
    }

    public bool Equals(Item other) {
        return other.Data == Data && other.count >= count;
    }

    public void MakeEmpty() {
        Count = 0;
    }

    public override string ToString() {
        string id = data == null ? "none" : data.id;
        return $"ID: {id}\nCount: {count}";
    }

    public static implicit operator string(Item item) {
        return item.ToString();
    }
}
