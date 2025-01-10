using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public UnityEngine.UI.Image icon;
    public UnityEngine.UI.Button button;
    PieceDefinition definition;

    public void Init(PieceDefinition definition) {
        this.definition = definition;
        icon.sprite = definition.icon;
        button.onClick.AddListener(Execute);
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIScriptMaster.Instance.buildingMenu.Display(null);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        UIScriptMaster.Instance.buildingMenu.Display(definition);
    }

    public void Execute() {
        Player.Instance.selectedPiece = definition;
        Player.Instance.SetupPlacementTarget(definition.piecePrefab);
        UIScriptMaster.Instance.inGameHUD.ToggleBuilding();
    }
}
