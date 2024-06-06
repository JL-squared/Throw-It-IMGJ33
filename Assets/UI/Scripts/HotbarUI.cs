using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour {
    public GameObject[] slots;
    public readonly Color deselected = new Color(0f, 0f, 0f, .73f);
    public readonly Color selected = new Color(.25f, .25f, .25f, .73f);

    private void Awake() {
        
    }

    // Start is called before the first frame update
    void Start() {
        PlayerScript.singleton.selectedEvent?.AddListener(Select);
        Select(0);
    }

    // Update is called once per frame
    void Update() {
    }

    public void Select(int slot) {
        //Debug.Log("Hotbar shit is being called!!");

        foreach(GameObject _slot in slots) {
            _slot.GetComponent<Image>().color = deselected;
            _slot.GetComponent<RectTransform>().sizeDelta = new Vector2(43, 43);
        }

        slots[slot].GetComponent<Image>().color = selected;
        slots[slot].GetComponent<RectTransform>().sizeDelta = new Vector2(45, 45);
    }
}
