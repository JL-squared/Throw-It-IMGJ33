using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Create New Item", order = 1)]
public class ItemType : ScriptableObject {
    [Tooltip("Item ID; use minecraft's snake_case")]
    public string id;

    public int stackSize;

    public Sprite icon;
    public GameObject worldItem;

    [Header("View Model")]
    public GameObject viewModel;

    public string title;
    [TextArea(15, 20)]
    public string description;

    public override string ToString() {
        return name;
    }

    public static implicit operator string(ItemType i) => i.ToString();
}
