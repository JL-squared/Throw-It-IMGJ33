using System.Collections.Generic;
using TMPro;
using Tweens;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenu : MonoBehaviour {
    [Header("List")]
    public GameObject content;

    [Header("Recipe Display")]
    public CraftingRecipe selectedRecipe;
    public Button craftingButton;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public Image itemDisplay;
    public GameObject recipeRequirementsContent;
    public List<RecipeRequirement> recipeRequirements;
    public GameObject recipeRequirementPrefab;

    public float iconBumpDuration;
    public float iconBumpAmplitude;

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
            thing.transform.localScale = Vector3.one;
            thing.GetComponent<RecipeEntry>().Init(definition);
        }
    }

    void Refresh(List<ItemStack> items) { // fix this later
        var i = 0;
        foreach(RecipeRequirement entry in recipeRequirements) {
            entry.Refresh(Player.Instance.inventory.container, selectedRecipe.requirements[i]);
            i++;
        }

        if(selectedRecipe != null) {
            craftingButton.interactable = Player.Instance.inventory.container.CheckForItems(selectedRecipe.requirements);
        }
    }

    public void SelectRecipe(CraftingRecipe recipe) {
        if (recipe == selectedRecipe)
            return;

        selectedRecipe = recipe;

        if(!itemDisplay.gameObject.activeSelf) {
            itemDisplay.gameObject.SetActive(true);
        }

        // refresh the thingy stuff
        title.text = selectedRecipe.m_name;
        description.text = selectedRecipe.output.Data.description;
        itemDisplay.sprite = selectedRecipe.output.Data.icon;
        // this is when we put in requirements into the requirement slot
        // this is when we update the status of the crafting button (this will require that we scan the inventory) (we need an inventory reference probably)
        Utils.KillChildren(recipeRequirementsContent.transform);
        recipeRequirements.Clear();
        foreach (ItemStack stack in selectedRecipe.requirements) {
            var gO = Instantiate(recipeRequirementPrefab, recipeRequirementsContent.transform);
            var c = gO.GetComponent<RecipeRequirement>();
            c.Refresh(Player.Instance.inventory.container, stack);
            recipeRequirements.Add(c);
        }

        craftingButton.interactable = Player.Instance.inventory.container.CheckForItems(selectedRecipe.requirements);
    }

    public void CraftRecipeTemp() {
        if (selectedRecipe != null) {
            if (Player.Instance.inventory.container.CheckForItems(selectedRecipe.requirements)) {
                Player.Instance.inventory.container.TakeItems(selectedRecipe.requirements);

                Player.Instance.inventory.container.PutItem(selectedRecipe.output); // Shrimple as that

                Utils.Bump(itemDisplay.gameObject, iconBumpDuration, iconBumpAmplitude);
            }
        }
    }
}