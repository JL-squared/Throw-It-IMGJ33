using UnityEngine;
using Image = UnityEngine.UI.Image;

public class VisualSlot : MonoBehaviour {
    public Image background;
    public ItemDisplay display;

    void Awake() {
        display = GetComponent<ItemDisplay>();
    }

    public void Refresh(ItemStack item) {
        display.UpdateValues(item);
    }
}
