using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemData : ScriptableObject {
    [Tooltip("Item ID; use minecraft's snake_case")]
    public string id;

    public int stackSize;

    public Sprite itemIcon;
    public GameObject worldItem;

    public string title;
    public string description;
}
