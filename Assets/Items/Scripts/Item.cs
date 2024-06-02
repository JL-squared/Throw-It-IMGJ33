using System.Collections;
using System.Collections.Generic;
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
                emptyEvent?.Invoke();
            }
        }
    }

    UnityEvent emptyEvent;

    public ItemData data;

    public Item (int count = 0, ItemData data = null) {
        this.count = count;
        this.data = data;
    }

    public static void SpawnInWorld(Vector3 p) {

    }
}
