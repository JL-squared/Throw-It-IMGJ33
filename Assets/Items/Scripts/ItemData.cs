using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/New Item Type", order = 1)]
public class ItemData : ScriptableObject {
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
}
