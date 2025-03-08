using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HotbarUI : MonoBehaviour {
    public VisualSlot[] slots;
    public readonly Color deselected = new Color(0f, 0f, 0f, .73f);
    public readonly Color selected = new Color(.1f, .1f, .1f, .73f);
    public GameObject selection;

    // Start is called before the first frame update
    void Start() {
        if (Player.Instance != null) {
            Player.Instance.inventory.selectedEvent?.AddListener(Select);
            Select(0); // might wanna make this a saved and loaded value (save scum maxxing)
            Player.Instance.inventory.initializedInventory.AddListener(() => {
                var i = 0;
                foreach (var slot in slots) {
                    var j = i;
                    Player.Instance.inventory.hotbar.items[j].onUpdate.AddListener(() => {
                        slot.Refresh(Player.Instance.inventory.hotbar.items[j]);
                    });
                    slot.display.index = i;
                    i++;
                    slot.id = "hotbar";
                    slot.Refresh(Player.Instance.inventory.hotbar.items[j]);
                }
            });
        }
    }

    public void Select(int slot) {
        selection.transform.position = slots[slot].transform.position;
    }
}
