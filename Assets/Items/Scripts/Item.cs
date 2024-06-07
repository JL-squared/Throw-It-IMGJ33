using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
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
                data = null;
                emptyEvent?.Invoke();
            }
        }
    }

    public UnityEvent emptyEvent;
    public UnityEvent<Item> updateEvent;

    public ItemData data;

    public Item (int count = 0, ItemData data = null) {
        this.count = count;
        this.data = data;
    }

    public static void SpawnInWorld(Vector3 p) {
        // search up prefab
    }

    public bool IsEmpty() {
        return (data == null || Count == 0) ? true : false;
    }

    public bool IsFull() {
        return Count == data.stackSize;
    }

    public Item Clone() {
        return (Item)MemberwiseClone();
    }

    public bool Equals(Item other) {
        return other.data == data && other.count >= count;
    }

    public void MakeEmpty() {
        Count = 0;
    }
}
