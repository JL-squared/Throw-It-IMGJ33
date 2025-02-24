using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenu : MonoBehaviour {
    public CraftingRecipe selectedRecipe;
    public Button craftingButton;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public Image image;
    public GameObject content;

    private void Awake() {
        UIScriptMaster.Instance.loadCall.AddListener(Load);
    }

    void Start () {
        // load all necessary recipes
        // hook onto inventory change
        Player.Instance.inventory.container.onUpdate.AddListener(Refresh);
    }

    public void Load() {
        foreach (CraftingRecipe definition in Registries.craftingRecipes.data.Values) {
            var thing = Instantiate(UIScriptMaster.Instance.recipeEntryPrefab);
            thing.transform.SetParent(content.transform);
            thing.GetComponent<RecipeEntry>().Init(definition);
        }
    }

    void Refresh(List<ItemStack> items) {
        /*
        foreach(CraftingListEntry entry in craftingListEntries) {
            entry.Refresh(entry.recipe.CheckForRequirements(items));
        }
        */

        if(selectedRecipe != null) {
            craftingButton.interactable = Player.Instance.inventory.container.CheckForRequirements(selectedRecipe.requirements);
        }
    }

    public void SelectRecipe(CraftingRecipe recipe) {
        if(!image.gameObject.activeSelf) {
            image.gameObject.SetActive(true);
        }

        selectedRecipe = recipe;
        // refresh the thingy stuff
        title.text = recipe.m_name;
        //description.text = recipe.output.Data.description;
        //image.sprite = recipe.output.Data.icon;
        // this is when we put in requirements into the requirement slot
        // this is when we update the status of the crafting button (this will require that we scan the inventory) (we need an inventory reference probably)
        
    }

    public void CraftRecipeTemp() {
        if (selectedRecipe != null) {
            bool i = true;

            if (i) {
                foreach (ItemStack requirement in selectedRecipe.requirements) {
                    Player.Instance.inventory.container.TakeItems(requirement);
                }

                Player.Instance.inventory.container.PutItem(selectedRecipe.output);
            }
        }
    }
}