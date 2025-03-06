using UnityEngine;
using UnityEngine.Events;
using Image = UnityEngine.UI.Image;

public class VisualSlot : MonoBehaviour {
    public Image background;
    public ItemDisplay display;
    public UnityEvent onClick;
    public ItemStack itemStack = new ItemStack();

    void Awake() {
        display = GetComponent<ItemDisplay>();
        display.leftClicked.AddListener(OnClick);
        display.rightClicked.AddListener(OnRightClick);
    }

    public void Refresh(ItemStack item) {
        itemStack = item;
        display.UpdateValues(item);
    }

    public void OnClick() {
        itemStack.SwapItem(ref Player.Instance.inventory.cursorItem);
    }

    public void OnRightClick() {
        itemStack.SwapItem(ref Player.Instance.inventory.cursorItem, true);
    }
}
