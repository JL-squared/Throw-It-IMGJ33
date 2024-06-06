// simple icon and name thingerator

using UnityEngine;
using UnityEngine.UI;

public class CraftingListEntry : MonoBehaviour {
    public Sprite sprite;
    public Text title;
    public CraftingRecipe recipe;

    public void Initialize(CraftingRecipe recipe) {
        sprite = recipe.output.data.itemIcon;
        title.text = recipe.output.data.name;
    }

    public void Refresh(bool active) {
        title.color = active ? Color.white : Color.gray;
    }
}