using UnityEngine;
using UnityEngine.UI;

public class BuildingEntry : MonoBehaviour {
    Image icon;
    Button button;
    PieceDefinition definition;

    public void Init(PieceDefinition definition) {
        this.definition = definition;
        icon.sprite = definition.icon;
        button.onClick.AddListener(Thing);
    }

    public void Thing() {
        Player.Instance.SetupPlacementTarget(definition.piecePrefab);
        UIMaster.Instance.ToggleBuilding();
    }
}
