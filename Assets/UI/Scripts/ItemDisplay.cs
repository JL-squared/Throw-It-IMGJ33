using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
// Only used for the item itself. Background and other items should be stored in a separate component
public class ItemDisplay : MonoBehaviour {
    public Item item {
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

    public void UpdateValues(Item item_, bool modifyEnabled = true) {
        if (item_.IsEmpty() && modifyEnabled) {
            SetEnabled(false);
        } else {
            if (modifyEnabled) SetEnabled(true);
            if (countDisplay != null) countDisplay.text = item_.Count.ToString();
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

    public void SetEnabled(bool enabled = true) {
        if (nameDisplay != null) nameDisplay.gameObject.SetActive(enabled);
        if (descriptionDisplay != null) descriptionDisplay.gameObject.SetActive(enabled);
        if (icon != null) icon.gameObject.SetActive(enabled);
        if (miniIcon != null) miniIcon.gameObject.SetActive(enabled);
        if (countDisplay != null) countDisplay.gameObject.SetActive(enabled);
    }
}
