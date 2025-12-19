using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VisualSlot : MonoBehaviour {
    public Image background;
    public ItemDisplay display;
    public ItemStack itemStack;
    public string id;
    public GenericInteractable interactable;

    public readonly Color deselectedColor = new Color(0f, 0f, 0f, .73f);
    public readonly Color highlightedColor = new Color(.5f, .5f, .5f, 1f);

    void Awake() {
        display = GetComponent<ItemDisplay>();
        interactable.onPointerDown.AddListener(OnDown);
        interactable.onPointerEnter.AddListener(OnEnter);
        interactable.onPointerExit.AddListener(OnExit);
    }

    public void Refresh(ItemStack item) {
        //Debug.Log("Refresh is being called");
        itemStack = item;
        display.UpdateValues(item);
        itemStack.visualBumpEvent.AddListener(display.Bump);
    }

    public void OnDown(PointerEventData pointerEventData) {
        if (Keyboard.current.leftShiftKey.isPressed) {
            UIScriptMaster.Instance.inGameHUD.ShiftClickedItem(id, itemStack);
        } else if (pointerEventData.button == PointerEventData.InputButton.Left) {
            itemStack.SwapItem(Player.Instance.inventory.cursorItem);
        } else if (pointerEventData.button == PointerEventData.InputButton.Right) {
            itemStack.SwapItem(Player.Instance.inventory.cursorItem, true);
        }
    }

    public void OnEnter(PointerEventData pointerEventData) {
        background.color = highlightedColor;
        /* // not sure why i implemented this lol we don't have normal crafting
        if (Mouse.current != null && Mouse.current.rightButton.isPressed) {
            itemStack.SwapItem(Player.Instance.inventory.cursorItem, true, true);
        }
        */
    }

    public void OnExit() {
        background.color = deselectedColor;
    }
}
