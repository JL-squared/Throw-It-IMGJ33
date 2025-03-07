using TMPro;
using Tweens;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
// Only used for the item itself. Background and other items should be stored in a separate component
public class ItemDisplay : MonoBehaviour, IPointerClickHandler {
    public ItemStack item {
        private get { return null; }
        set {
            if (value != null) {
                UpdateValues(value);
            }
        }
    }

    public TextMeshProUGUI nameDisplay = null;
    public TextMeshProUGUI descriptionDisplay = null;
    public Image icon = null;
    public Image miniIcon = null;
    public TextMeshProUGUI countDisplay = null;
    public bool interactable = false;
    public UnityEvent leftClicked = new UnityEvent();
    public UnityEvent rightClicked = new UnityEvent();
    public int index;

    // sigma
    public void UpdateValues(ItemStack item_, bool modifyEnabled = true) {
        //Debug.Log("Update values being called on " + index);
        if (item_.IsEmpty() && modifyEnabled) {
            SetEnabled(false);
        } else {
            Debug.Log(item_);
            if (modifyEnabled) SetEnabled(true);
            if (countDisplay != null) countDisplay.text = item_.Count > 1 ? item_.Count.ToString() : "";
            UpdateValues(item_.Data);
        }
    }

    public void UpdateValues(ItemData data) {
        SetEnabled(true);
        if (nameDisplay != null) nameDisplay.text = data.name;
        if (descriptionDisplay != null) descriptionDisplay.text = data.description;
        if (icon != null) icon.sprite = data.icon;
        miniIcon = null;
    }

    public void UpdateValues(ItemData data, string customNumberText) {
        UpdateValues(data);
        if (countDisplay != null) {
            countDisplay.text = customNumberText;
        }
    }

    public void SetEnabled(bool enabled = true) {
        //Debug.Log("Item display content becoming " + enabled + " at " + index);
        if (nameDisplay != null) nameDisplay.gameObject.SetActive(enabled);
        if (descriptionDisplay != null) descriptionDisplay.gameObject.SetActive(enabled);
        if (icon != null) icon.gameObject.SetActive(enabled);
        if (miniIcon != null) miniIcon.gameObject.SetActive(enabled);
        if (countDisplay != null) countDisplay.gameObject.SetActive(enabled);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(interactable) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                leftClicked.Invoke();
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                rightClicked.Invoke();
            }
        }
    }

    public void Bump() {
        Utils.Bump(icon.gameObject, 0.2f, 0.3f);
    }
}
