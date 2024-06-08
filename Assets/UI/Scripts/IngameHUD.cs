using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameHUD : MonoBehaviour {
    public GameObject craftingMenuObject;
    public GameObject rightClickHint;
    public RectTransform chargeMeter;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRightClickHint(bool on) {
        rightClickHint.SetActive(on);
    }

    public void UpdateChargeMeter(float charge) {
        float x = Mathf.InverseLerp(0.2f, 1.0f, charge);
        chargeMeter.localScale = new Vector3(x, 1.0f, 1.0f);
        chargeMeter.gameObject.SetActive(x != 0.0f);
    }
}
