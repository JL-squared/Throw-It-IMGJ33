using System.Collections.Generic;
using UnityEngine;

public class HotbarUI : MonoBehaviour {
    public VisualSlot[] slots;
    public readonly Color deselected = new Color(0f, 0f, 0f, .73f);
    public readonly Color selected = new Color(.1f, .1f, .1f, .73f);

    // Start is called before the first frame update
    void Start() {
        if (Player.Instance != null) {
            Player.Instance.inventory.selectedEvent?.AddListener(Select);
            Select(0); // might wanna make this a saved and loaded value (save scum maxxing)
            var i = 0;
            foreach (var slot in slots) {
                var j = i;
                Player.Instance.inventory.container.items[j].onUpdate?.AddListener(() => {
                    var k = j;
                    slot.Refresh(Player.Instance.inventory.container.items[k]); 
                });
                i++;
            }
        }
    }

    public void Select(int slot) {
        foreach(VisualSlot _slot in slots) {
            _slot.background.color = deselected;
        }

        slots[slot].background.color = selected;
    }
}
