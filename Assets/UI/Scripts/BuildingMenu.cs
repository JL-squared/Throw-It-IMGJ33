using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingMenu : MonoBehaviour {
    public TextMeshProUGUI m_name;
    public TextMeshProUGUI description;
    public Image icon;
    public ItemDisplay requirement1;
    public ItemDisplay requirement2;
    public ItemDisplay requirement3;

    private void Awake() {
        Display(null);
    }

    public void Display(PieceDefinition definition) {
        if (definition != null) {
            SetEnabled(true);
            m_name.text = definition.m_name != null ? definition.m_name : "";
            //description.text = definition.description != null ? definition.description : "";
            requirement1.item = definition.requirement1;
            requirement2.item = definition.requirement2;
            requirement3.item = definition.requirement3;
        } else {
            SetEnabled(false);
        }
    }
    
    public void SetEnabled(bool enabled) {
        m_name.gameObject.SetActive(enabled);
        if (description != null) description.gameObject.SetActive(enabled);
        if (icon != null) icon.gameObject.SetActive(enabled);
        requirement1.SetEnabled(enabled);
        requirement2.SetEnabled(enabled);
        requirement3.SetEnabled(enabled);
    }
}
