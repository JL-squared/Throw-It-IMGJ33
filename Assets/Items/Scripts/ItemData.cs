using UnityEngine;

// One major disatvantage of having stuff inherit from a scriptable object
// is that you can actually write mono-behavior type code (like update and start) 
// since you can't store persistent values within SOs (otherwise it would modify them in the editor)
[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/New Item Data", order = 1)]
public class ItemData : ScriptableObject {
    public int stackSize;

    public Sprite icon;
    public GameObject worldItem;
    public GameObject viewModel;
    public GameObject equippedLogic;

    public string title;
    [TextArea(15, 20)]
    public string description;

    public override string ToString() {
        return name;
    }

    public static implicit operator string(ItemData i) => i.ToString();
}
