using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour {
    public GameObject[] slots;
    Image[] slotIcons;
    public readonly Color deselected = new Color(0f, 0f, 0f, .73f);
    public readonly Color selected = new Color(.1f, .1f, .1f, .73f);

    private void Awake() {
        
    }

    // Start is called before the first frame update
    void Start() {
        PlayerScript.singleton.selectedEvent?.AddListener(Select);
        Select(0); // might wanna make this a saved and loaded value (save scum maxxing)
        foreach(GameObject slot in slots) {
            
        }
    }

    // Update is called once per frame
    void Update() {

    }

    public void Select(int slot) {
        foreach(GameObject _slot in slots) {
            _slot.GetComponent<Image>().color = deselected;
            _slot.GetComponent<RectTransform>().sizeDelta = new Vector2(43, 43);
        }

        slots[slot].GetComponent<Image>().color = selected;
        slots[slot].GetComponent<RectTransform>().sizeDelta = new Vector2(45, 45);
    }
}
