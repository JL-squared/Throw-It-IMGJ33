using UnityEngine;
using Image = UnityEngine.UI.Image;

public class VisualSlot : MonoBehaviour {
    public Image background;
    public ItemDisplay display;
    public ItemStack itemStack; // NOT A REFERENCE, CLONING, OTHERWISE KNOWN AS "OTHER"

    void Awake() {
        display = GetComponent<ItemDisplay>();
        display.leftClicked.AddListener(OnClick);
        display.rightClicked.AddListener(OnRightClick);
    }

    public void Refresh(ItemStack item) {
        //Debug.Log("Refresh is being called");
        itemStack = item;
        display.UpdateValues(item);
    }

    public void OnClick() {
        itemStack.SwapItem(Player.Instance.inventory.cursorItem);
    }

    public void OnRightClick() {
        itemStack.SwapItem(Player.Instance.inventory.cursorItem, true);
    }
}
