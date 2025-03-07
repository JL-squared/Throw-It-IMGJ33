using UnityEngine;
using UnityEngine.UI;

public class RecipeRequirement : MonoBehaviour {
    public Image background;
    public ItemDisplay itemDisplay;
    Color notEnoughColor = new Color(150, 0, 0, 100);
    Color enoughColor = new Color(0, 0, 0, 100);
    bool firstTime = false;

    public void Refresh(ItemContainer container, ItemStack requirement) {
        var amount = container.GetItemAmount(requirement);
        if(amount < requirement.Count) {
            background.color = notEnoughColor;
        } else {
            background.color = enoughColor;
        }
        itemDisplay.UpdateValues(requirement.Data, $"{amount}/{requirement.Count}");
        if (!firstTime) {
            itemDisplay.Bump();
            firstTime = true;
        }
    }
}
