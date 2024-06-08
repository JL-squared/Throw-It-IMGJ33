using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class VisualSlot : MonoBehaviour
{
    public Image background;
    public Image itemIcon;
    public TextMeshProUGUI text;

    public void Refresh(Item item) {
        if(item.IsEmpty()) {
            itemIcon.enabled = false;
        } else {
            itemIcon.enabled = true;
            itemIcon.sprite = item.Data.icon;
        }
        text.text = item.IsEmpty() ? "" : item.Count.ToString();
    }
}
