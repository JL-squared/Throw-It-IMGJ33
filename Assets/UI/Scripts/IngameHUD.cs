using System;
using TMPro;
using UnityEngine;

public class IngameHUD : MonoBehaviour {
    public GameObject craftingMenuObject;
    public GameObject marketMenuObject;
    public GameObject rightClickHint;
    public GameObject interactHint;
    public RectTransform chargeMeter;
    public GameObject crosshairGroup;
    public TextMeshProUGUI marketCurrencyText;

    // Start is called before the first frame update
    void Start() {
        SetRightClickHint(false);
        SetInteractHint(false);
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void SetRightClickHint(bool on) {
        rightClickHint.SetActive(on);
    }

    public void SetInteractHint(bool on) {
        interactHint.SetActive(on);
    }

    public void UpdateChargeMeter(float charge) {
        float x = Mathf.InverseLerp(0.2f, 1.0f, charge);
        chargeMeter.localScale = new Vector3(x, 1.0f, 1.0f);
        chargeMeter.gameObject.SetActive(x != 0.0f);
    }

    public void SetCraftingMenu() {
        SetMenu();
        craftingMenuObject.SetActive(true);
    }

    internal void SetMarketMenu() {
        SetMenu();
        marketMenuObject.SetActive(true);
    }

    public void SetMarketCurrency(float currency) {
        marketCurrencyText.text = currency.ToString();
    }

    // Group of methods for when you're ingame
    public void SetIngame() {
        marketMenuObject.SetActive(false);
        craftingMenuObject.SetActive(false);
        crosshairGroup.SetActive(true);
    }

    // Group of common purpose method calls for when you're not ingame
    public void SetMenu() {
        crosshairGroup.SetActive(false);
        chargeMeter.gameObject.SetActive(false);
    }
}
